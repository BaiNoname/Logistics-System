using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Locations;
using TruckingSystem.Application.Pricing;
using TruckingSystem.Application.Routing;
using TruckingSystem.Application.Warehouses;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.FreightChat;

/// <summary>
/// Chatbot báo giá theo đơn hàng: server điền từng ô (hàng, khối lượng, loại hàng, nơi lấy/giao, cách lấy hàng, xe,
/// địa điểm, kho) theo thứ tự cố định rồi tính cước như hóa đơn; AI chỉ trích dữ liệu và viết lời mở đầu.
/// </summary>
public class FreightChatService(IApplicationDbContext db, IFreightChatAi ai,
    IFreightVectorStore vectors, IFreightChatContext contexts,
    IPlaceSearchProvider places, IRouteProvider routes)
{
    public const string Notice = "Cước tham khảo, chưa gồm VAT, cầu đường, bốc xếp và khoản ngoài bảng giá; không áp dụng ưu đãi hợp đồng. Tuyến ô tô tham khảo chưa bảo đảm phù hợp đường cấm tải.";
    private const string Company = "company", Self = "self";

    private sealed record Turn(string Message, List<ChatOption> Options, FreightQuote? Quote, string? Appended,
        List<string> Facts, List<ChatTable> Tables, PendingQuestion? Pending);

    /// <summary>Server quyết định bước tiếp theo; AI chỉ diễn đạt lại và bị kiểm tra trước khi gửi khách.</summary>
    public async Task<FreightChatResponse> ChatAsync(FreightChatRequest request, CancellationToken ct)
    {
        var (state, turn) = await DecideAsync(request, ct);
        var message = await ComposeAsync(state, turn, ct);
        state.Pending = turn.Pending;
        state.History.Add(new("assistant", message));
        state.History = state.History.TakeLast(20).ToList();
        state.ExpiresUtc = DateTime.UtcNow.AddMinutes(30);
        return new(message, contexts.Protect(state), turn.Options, turn.Quote, turn.Tables);
    }

    private async Task<string> ComposeAsync(ChatState state, Turn turn, CancellationToken ct)
    {
        var fallback = turn.Appended is null ? turn.Message : $"{turn.Appended}\n{turn.Message}";
        var facts = turn.Facts.Append(DescribeDraft(state.Draft)).ToList();
        var brief = new FreightReplyBrief(fallback, turn.Appended, turn.Options.Select(o => o.Label).ToList(), facts, turn.Tables.Count > 0);
        string? reply;
        try { reply = FreightReplyGuard.Clean(await ai.ReplyAsync(state.History, brief, ct), brief, state.History); }
        catch (FreightServiceUnavailableException) { return fallback; } // AI diễn đạt lỗi không làm hỏng lượt chat
        // AI chỉ viết lời mở đầu; câu hỏi bắt buộc của server luôn được nối nguyên văn phía sau.
        if (!FreightReplyGuard.Accept(reply, brief, state.History)) return fallback;
        return $"{reply}\n{fallback}";
    }

    private async Task<(ChatState, Turn)> DecideAsync(FreightChatRequest request, CancellationToken ct)
    {
        var facts = new List<string>();
        var tables = new List<ChatTable>();
        if (request.Message is null || request.Message.Length > 2000 || request.Context?.Length > 100000
            || request.Selection?.Length > 80)
            throw new ArgumentException("Nội dung yêu cầu không hợp lệ hoặc vượt giới hạn.");
        var state = string.IsNullOrEmpty(request.Context) ? new ChatState() : contexts.Unprotect(request.Context);
        var selection = string.IsNullOrEmpty(request.Selection) ? null : request.Selection;
        var asks = new List<string>();
        if (selection is null)
        {
            if (string.IsNullOrWhiteSpace(request.Message)) throw new ArgumentException("Vui lòng nhập câu hỏi.");
            var history = state.History.Append(new ChatMessage("user", request.Message)).TakeLast(20).ToList();
            var draft = await ai.ExtractAsync(state.Draft, history, state.Pending, ct)
                ?? throw new FreightServiceUnavailableException("AI trả kết quả không đúng cấu trúc. Vui lòng gửi lại.");
            Normalize(draft);
            ValidateDraft(draft);
            ReconcileAnswers(draft, state.Draft, state.Pending, request.Message);
            asks = (draft.Asks ?? []).Concat(FreightTables.DetectAsks(request.Message)).ToList();
            draft.Asks = null; // ý định chỉ thuộc tin nhắn này
            state = Carry(state, draft, history);
        }

        var list = await PriceListLookup.Current(db)
            .OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.Id).FirstOrDefaultAsync(ct);
        if (list is null) return Reply("Hiện chưa có bảng giá đang áp dụng. Vui lòng liên hệ nhân viên.");
        var rules = await db.TariffRules.AsNoTracking().Where(r => r.PriceListId == list.Id).ToListAsync(ct);
        if (rules.Count == 0) return Reply("Bảng giá hiện chưa có loại xe để báo cước.");
        var policy = await SurchargeLookup.AtAsync(db, VnClock.Today, ct);
        // Bảng giá đang hiệu lực là dữ liệu công khai của endpoint này; AI dùng để trả lời câu hỏi phụ của khách.
        facts.AddRange(rules.OrderBy(r => r.Id).Take(15).Select(r => FreightDocuments.Create(r, list, includeFixedFees: policy is null).Text));
        facts.Add(FormulaFact(policy));
        foreach (var kind in new[] { FreightTables.PriceListKind, FreightTables.FormulaKind, FreightTables.GuideKind }.Where(asks.Contains))
            tables.Add(kind switch
            {
                FreightTables.PriceListKind => FreightTables.PriceList(list, rules, policy),
                FreightTables.FormulaKind => FreightTables.Formula(policy),
                _ => FreightTables.Guide()
            });

        if (selection is not null) ApplySelection(state, selection, rules);

        var tariff = rules.FirstOrDefault(r => r.Id == state.TariffId);
        if (tariff != null && state.TariffHash != null && FreightDocuments.Create(tariff, list).Hash != state.TariffHash)
        {
            state.TariffId = null;
            state.AwaitingConfirmation = false;
            tariff = null;
        }

        var d = state.Draft;
        if (string.IsNullOrWhiteSpace(d.Cargo))
            return Reply(d.Weight is null && tables.Count > 0 ? "Bạn muốn gửi hàng gì, khoảng bao nhiêu kg hoặc tấn?" : "Bạn muốn gửi hàng gì?", slot: "cargo");
        if (d.Weight is null || string.IsNullOrWhiteSpace(d.Unit)) return Reply("Hàng nặng khoảng bao nhiêu kg hoặc tấn?", slot: "weight");
        var kg = FreightQuotePricing.ToKilograms(d.Weight.Value, d.Unit);
        if (d.IsFragile is null || d.IsHazmat is null)
            return Reply("Hàng có dễ vỡ hoặc nguy hiểm (dễ cháy, hóa chất…) không?",
                [new("cargo:normal", "Hàng thường"), new("cargo:fragile", "Dễ vỡ"), new("cargo:hazmat", "Nguy hiểm")], slot: "cargoType");
        if (string.IsNullOrWhiteSpace(d.From)) return Reply("Hàng lấy ở đâu (hoặc bạn ở khu vực nào)?", slot: "from");
        if (string.IsNullOrWhiteSpace(d.To)) return Reply("Giao hàng đến đâu?", slot: "to");
        if (d.PickupMethod is null)
            return Reply("Bạn muốn kho cử xe tới lấy hàng hay tự mang hàng đến kho?",
                [new("pickup:company", "Kho cử xe tới lấy"), new("pickup:self", "Tự mang đến kho")], slot: "pickupMethod");

        if (tariff is null)
        {
            state.AwaitingConfirmation = false;
            var weightLabel = $"{d.Weight.Value.ToString("0.###", CultureInfo.GetCultureInfo("vi-VN"))} {d.Unit}";
            List<TariffRule> offered = [];
            if (!string.IsNullOrWhiteSpace(d.Vehicle))
            {
                var matches = await vectors.SearchAsync(list.Id, d.Vehicle, ct);
                offered = matches.Select(m => rules.FirstOrDefault(r => r.Id == m.Id && FreightDocuments.Create(r, list).Hash == m.Hash))
                    .Where(r => r != null).Cast<TariffRule>().Take(VehicleSuggestion.MaxChoices).ToList();
            }
            var suggested = offered.Count == 0;
            if (suggested) offered = VehicleSuggestion.Rank(rules, kg);
            state.TariffChoices = offered.Select(r => r.Id).ToList();
            return Reply(suggested
                    ? $"Mình gợi ý {offered[0].VehicleType} cho {weightLabel}. Bấm để chọn hoặc đổi loại xe:"
                    : "Vui lòng chọn loại xe phù hợp trong bảng giá:",
                offered.Select(r => new ChatOption($"vehicle:{r.Id}", r.VehicleType)).ToList(), slot: "vehicle");
        }
        state.TariffHash = FreightDocuments.Create(tariff, list).Hash;
        facts.Add($"Xe đã chọn: {tariff.VehicleType}.");

        if (state.PickupPlace is null) return await PlaceStep(d.From, "nơi lấy hàng", "from");
        if (state.DropoffPlace is null) return await PlaceStep(d.To, "nơi giao", "to");

        if (state.Warehouse is null)
        {
            var needed = WarehouseZoneRules.Required(d.IsHazmat.Value, d.IsFragile.Value);
            var stored = await db.Warehouses.AsNoTracking().Where(w => w.Lat != null && w.Lng != null)
                .Select(w => new { w.Id, w.Name, w.Address, Lat = w.Lat!.Value, Lng = w.Lng!.Value }).ToListAsync(ct);
            var candidates = new List<WarehouseCandidate>();
            foreach (var w in stored)
                candidates.Add(new(w.Id, w.Name, w.Address, w.Lat, w.Lng, (await WarehouseZoneRules.AvailableAsync(db, w.Id)).Contains(needed)));
            var nearest = WarehouseLocator.Nearest(candidates, state.PickupPlace.Lat, state.PickupPlace.Lng);
            if (nearest is null) return Reply("Hiện chưa có kho để báo giá. Vui lòng liên hệ nhân viên.");
            state.Warehouse = nearest.Value.Warehouse with { Suitable = nearest.Value.Suitable };
        }
        var warehouse = state.Warehouse;
        facts.Add($"Kho trung chuyển gần nơi lấy hàng nhất: {warehouse.Name}.");

        if (selection != "confirm" || !state.AwaitingConfirmation || state.SurchargeDate != policy?.EffectiveDate)
        {
            state.SurchargeDate = policy?.EffectiveDate;
            state.AwaitingConfirmation = true;
            tables.Add(FreightTables.Summary(new(d.Cargo, $"{d.Weight.Value.ToString("0.###", CultureInfo.GetCultureInfo("vi-VN"))} {d.Unit}",
                d.IsFragile.Value, d.IsHazmat.Value, tariff.VehicleType, PickupLabel(d.PickupMethod), state.PickupPlace.Address,
                warehouse.Name, warehouse.Address, warehouse.Suitable, state.DropoffPlace.Address)));
            return Reply("Vui lòng kiểm tra thông tin đơn hàng rồi bấm Xác nhận để tính cước, hoặc gõ phần cần sửa.",
                [new("confirm", "Xác nhận và tính cước")], slot: "confirm");
        }

        var depot = new ConfirmedPlace(warehouse.Address ?? warehouse.Name, warehouse.Lat, warehouse.Lng);
        var legs = new List<(string Name, ConfirmedPlace From, ConfirmedPlace To)>();
        if (d.PickupMethod == Company) legs.Add(("Lấy hàng", state.PickupPlace, depot));
        legs.Add(("Giao hàng", depot, state.DropoffPlace));
        var quoted = new List<QuotedLeg>();
        foreach (var leg in legs)
        {
            ct.ThrowIfCancellationRequested();
            var route = await routes.GetRouteAsync(new(leg.From.Lat, leg.From.Lng, leg.To.Lat, leg.To.Lng));
            if (!route.Success || !double.IsFinite(route.DistanceKm) || route.DistanceKm <= 0 || route.DistanceKm > 100000)
                return Reply($"Không tìm được tuyến đường chặng {leg.Name.ToLowerInvariant()}. Vui lòng kiểm tra địa điểm hoặc thử lại sau.");
            quoted.Add(new(leg.Name, leg.Name == "Lấy hàng" ? leg.From.Address : warehouse.Name,
                leg.Name == "Lấy hàng" ? warehouse.Name : leg.To.Address, decimal.Round((decimal)route.DistanceKm, 2)));
        }
        // Giá có thể thay đổi trong thời gian chờ dịch vụ bản đồ.
        var latest = await PriceListLookup.Current(db)
            .OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.Id).FirstOrDefaultAsync(ct);
        var current = await db.TariffRules.AsNoTracking().FirstOrDefaultAsync(r => r.Id == tariff.Id, ct);
        if (latest?.Id != list.Id || current == null || FreightDocuments.Create(current, latest).Hash != state.TariffHash)
        {
            state.TariffId = null; state.AwaitingConfirmation = false;
            return Reply("Bảng giá vừa thay đổi. Vui lòng gửi lại yêu cầu để xác nhận giá mới.");
        }
        var currentPolicy = await SurchargeLookup.AtAsync(db, VnClock.Today, ct);
        if (currentPolicy?.EffectiveDate != policy?.EffectiveDate)
        {
            state.AwaitingConfirmation = false;
            return Reply("Phụ phí vừa thay đổi. Vui lòng xác nhận lại thông tin đơn hàng.");
        }
        var lines = FreightQuotePricing.Calculate(new FreightQuoteInput(quoted.Select(l => l.DistanceKm).ToList(), kg,
            d.IsFragile.Value, d.IsHazmat.Value), current, currentPolicy).ToList();
        var total = lines.Sum(l => l.Amount);
        var notice = Notice + (warehouse.Suitable ? "" : " Kho trung chuyển chưa có khu phù hợp loại hàng; nhân viên sẽ xác nhận kho khi nhận đơn.");
        var quote = new FreightQuote(list.Name, list.EffectiveDate, DateTime.UtcNow, current.VehicleType, d.PickupMethod,
            warehouse.Name, quoted, lines, total, notice);
        state.AwaitingConfirmation = false;
        facts.AddRange(quoted.Select(l => $"Chặng {l.Name.ToLowerInvariant()}: {l.DistanceKm:N2} km."));
        facts.AddRange(lines.Select(l => $"{l.Name}: {l.Detail} = {l.Amount:N2} đồng."));
        return Reply("Bạn có thể nhập yêu cầu mới hoặc chọn Cuộc trò chuyện mới để tra lô hàng khác.", quote: quote,
            appended: $"Tổng cước tham khảo: {total:N2} đồng.\n{notice}");

        async Task<(ChatState, Turn)> PlaceStep(string address, string label, string slot)
        {
            var suggestions = await places.SearchAsync(address);
            state.PlaceChoices = suggestions.Where(p => WarehouseLocator.Valid(p.Lat, p.Lng))
                .Take(5).Select(p => new ConfirmedPlace(p.Address, p.Lat, p.Lng)).ToList();
            if (state.PlaceChoices.Count == 0)
                return Reply($"Chưa tra được địa điểm “{address}”. Vui lòng nhập địa chỉ cụ thể hơn hoặc thử lại sau.", slot: slot);
            return Reply($"Xác nhận {label} “{address}”:",
                state.PlaceChoices.Select((p, i) => new ChatOption($"place:{i}", p.Address)).ToList(), slot: "place");
        }

        (ChatState, Turn) Reply(string message, List<ChatOption>? options = null, FreightQuote? quote = null,
            string? appended = null, string? slot = null)
        {
            if (tables.Count > 0)
                facts.Add("Bảng đang hiển thị cho khách lượt này: " + string.Join(", ", tables.Select(t => t.Title)) + ".");
            return (state, new Turn(message, options ?? [], quote, appended, facts, tables,
                slot is null ? null : new PendingQuestion(slot, message)));
        }
    }

    /// <summary>Bấm nút: gán thẳng vào trạng thái, không gọi AI. Lựa chọn phải thuộc danh sách server vừa đưa ra.</summary>
    private static void ApplySelection(ChatState state, string selection, IReadOnlyList<TariffRule> rules)
    {
        string label;
        var parts = selection.Split(':', 2);
        switch (parts[0])
        {
            case "pickup" when parts.Length == 2 && parts[1] is Company or Self:
                state.Draft = state.Draft with { PickupMethod = parts[1] };
                label = PickupLabel(parts[1]);
                break;
            case "cargo" when parts.Length == 2 && parts[1] is "normal" or "fragile" or "hazmat":
                state.Draft = state.Draft with { IsFragile = parts[1] == "fragile", IsHazmat = parts[1] == "hazmat" };
                state.Warehouse = null;
                label = parts[1] switch { "fragile" => "Dễ vỡ", "hazmat" => "Nguy hiểm", _ => "Hàng thường" };
                break;
            case "vehicle":
                if (parts.Length != 2 || !int.TryParse(parts[1], out var id) || !state.TariffChoices.Contains(id))
                    throw new ArgumentException("Lựa chọn xe không hợp lệ. Vui lòng hỏi lại.");
                state.TariffId = id;
                state.TariffChoices.Clear();
                state.TariffWeightKg = Kilograms(state.Draft);
                label = rules.FirstOrDefault(r => r.Id == id)?.VehicleType ?? "Loại xe";
                break;
            case "place":
                if (parts.Length != 2 || !int.TryParse(parts[1], out var index) || index < 0 || index >= state.PlaceChoices.Count)
                    throw new ArgumentException("Lựa chọn địa điểm không hợp lệ.");
                var place = state.PlaceChoices[index];
                if (state.PickupPlace is null) { state.PickupPlace = place; state.Warehouse = null; }
                else state.DropoffPlace = place;
                state.PlaceChoices.Clear();
                label = place.Address;
                break;
            case "confirm" when parts.Length == 1:
                label = "Xác nhận và tính cước";
                break;
            default:
                throw new ArgumentException("Lựa chọn không hợp lệ. Vui lòng hỏi lại.");
        }
        if (selection != "confirm") state.AwaitingConfirmation = false;
        state.History = state.History.Append(new ChatMessage("user", $"(chọn) {label}")).TakeLast(20).ToList();
    }

    /// <summary>
    /// Đối chiếu kết quả AI với câu khách: cách lấy hàng chỉ nhận khi khách nhắc tới (từ khóa), khi server đang hỏi
    /// đúng câu đó, hoặc giữ giá trị cũ — AI hay tự đoán ô này. AI không điền ô đang hỏi thì đọc câu trả lời cụt bằng từ khóa.
    /// </summary>
    private static void ReconcileAnswers(FreightDraft draft, FreightDraft previous, PendingQuestion? pending, string message)
    {
        var said = SlotAnswers.PickupMethod(message);
        if (said is not null) draft.PickupMethod = said; // lời khách nói rõ luôn thắng giá trị AI
        else if (pending?.Slot != "pickupMethod" || SlotAnswers.IsBareReply(message)) draft.PickupMethod = previous.PickupMethod;
        // "hàng", "đồ"… chưa nói là hàng gì: không để AI tự kết luận là hàng thường, server sẽ hỏi.
        if (SlotAnswers.IsGenericCargo(draft.Cargo) && previous.IsFragile is null && previous.IsHazmat is null
            && pending?.Slot != "cargoType" && SlotAnswers.CargoType(message) is null)
            draft.IsFragile = draft.IsHazmat = null;
        if (pending?.Slot == "cargoType" && (draft.IsFragile is null || draft.IsHazmat is null)
            && SlotAnswers.CargoType(message) is var (fragile, hazmat))
        {
            draft.IsFragile ??= fragile;
            draft.IsHazmat ??= hazmat;
        }
    }

    /// <summary>Giữ xe/địa điểm/kho đã xác nhận khi phần tương ứng không đổi; mọi tin nhắn mới đều bỏ xác nhận cũ.</summary>
    private static ChatState Carry(ChatState old, FreightDraft draft, List<ChatMessage> history)
    {
        var next = new ChatState { Draft = draft, History = history, Pending = old.Pending, SurchargeDate = old.SurchargeDate };
        if (Same(old.Draft.Vehicle, draft.Vehicle) && old.TariffWeightKg == Kilograms(draft))
        {
            next.TariffId = old.TariffId;
            next.TariffHash = old.TariffHash;
            next.TariffWeightKg = old.TariffWeightKg;
            next.TariffChoices = old.TariffChoices;
        }
        else if (Same(old.Draft.Vehicle, draft.Vehicle) && old.TariffId is null)
            next.TariffChoices = old.TariffChoices; // đang chờ chọn xe, khách chưa đổi gì liên quan
        var sameFrom = Same(old.Draft.From, draft.From);
        if (sameFrom) next.PickupPlace = old.PickupPlace;
        if (Same(old.Draft.To, draft.To)) next.DropoffPlace = old.DropoffPlace;
        if (sameFrom && next.PickupPlace != null && old.Draft.IsFragile == draft.IsFragile && old.Draft.IsHazmat == draft.IsHazmat)
            next.Warehouse = old.Warehouse;
        return next;

        static bool Same(string? a, string? b) => string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static decimal? Kilograms(FreightDraft draft)
    {
        if (draft.Weight is not decimal weight || string.IsNullOrWhiteSpace(draft.Unit)) return null;
        try { return FreightQuotePricing.ToKilograms(weight, draft.Unit); }
        catch (ArgumentException) { return null; }
    }

    private static void Normalize(FreightDraft draft)
    {
        draft.Cargo = Clean(draft.Cargo); draft.From = Clean(draft.From); draft.To = Clean(draft.To); draft.Vehicle = Clean(draft.Vehicle);
        draft.Unit = FreightText.Fold(draft.Unit) switch
        {
            "" => null,
            "kg" or "ki" or "kilogram" or "kilo" or "can" => "kg",
            "tan" or "ton" or "t" => "tấn",
            _ => draft.Unit
        };
        draft.PickupMethod = draft.PickupMethod?.Trim().ToLowerInvariant() is Company or Self ? draft.PickupMethod.Trim().ToLowerInvariant() : null;
        draft.Asks = draft.Asks?.Where(a => a is FreightTables.PriceListKind or FreightTables.FormulaKind or FreightTables.GuideKind).ToList();

        static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ValidateDraft(FreightDraft draft)
    {
        if (draft.Cargo?.Length > 200 || draft.From?.Length > 500 || draft.To?.Length > 500 || draft.Vehicle?.Length > 200)
            throw new ArgumentException("Thông tin lô hàng quá dài. Vui lòng mô tả ngắn gọn hơn.");
        if (draft.Unit is not (null or "kg" or "tấn")) throw new ArgumentException("Đơn vị khối lượng phải là kg hoặc tấn.");
        if (draft.Weight <= 0) throw new ArgumentException("Khối lượng hàng phải lớn hơn 0.");
        if (draft.Weight is decimal w && draft.Unit is not null) _ = FreightQuotePricing.ToKilograms(w, draft.Unit);
    }

    private static string PickupLabel(string? method) => method == Company ? "Kho cử xe tới lấy" : "Tự mang đến kho";

    private static string FormulaFact(SurchargePolicy? policy)
        => "Cách tính cước: tổng km (chặng lấy hàng từ nơi lấy đến kho gần nhất, chỉ khi kho cử xe tới lấy, cộng chặng giao từ kho đến nơi giao) × giá/km của loại xe, "
           + "cộng khối lượng (tấn) × giá/tấn tính một lần cho cả lô, cộng phụ phí "
           + (policy is null
               ? "dễ vỡ/nguy hiểm cố định theo loại xe, mỗi loại một lần."
               : $"dễ vỡ {policy.FragilePercent:0.##}% và nguy hiểm {policy.HazmatPercent:0.##}% trên (cước km + cước tấn), mỗi loại một lần.")
           + " Km tính theo tuyến đường thực tế sau khi khách xác nhận địa điểm.";

    private static string DescribeDraft(FreightDraft d)
        => $"Lô hàng đã ghi nhận: hàng {d.Cargo ?? "chưa rõ"}; khối lượng {(d.Weight is null ? "chưa rõ" : $"{d.Weight} {d.Unit}")}; "
           + $"dễ vỡ: {Flag(d.IsFragile)}; nguy hiểm: {Flag(d.IsHazmat)}; lấy ở: {d.From ?? "chưa rõ"}; giao đến: {d.To ?? "chưa rõ"}; "
           + $"cách lấy hàng: {(d.PickupMethod is null ? "chưa rõ" : PickupLabel(d.PickupMethod))}; xe khách nêu: {d.Vehicle ?? "chưa nêu"}.";

    private static string Flag(bool? value) => value switch { true => "có", false => "không", _ => "chưa rõ" };
}
