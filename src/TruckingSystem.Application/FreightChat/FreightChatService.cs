using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Pricing;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Locations;
using TruckingSystem.Application.Routing;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.FreightChat;

public class FreightChatService(IApplicationDbContext db, IFreightChatAi ai,
    IFreightVectorStore vectors, IFreightChatContext contexts,
    IPlaceSearchProvider places, IRouteProvider routes)
{
    public const string Notice = "Cước tham khảo, chưa gồm VAT, cầu đường, bốc xếp và khoản ngoài bảng giá; không áp dụng ưu đãi hợp đồng. Tuyến ô tô tham khảo chưa bảo đảm phù hợp đường cấm tải.";

    private sealed record Turn(string Message, List<ChatOption> Options, FreightQuote? Quote, string? Appended, List<string> Facts);

    /// <summary>Server quyết định bước tiếp theo; AI chỉ diễn đạt lại và bị kiểm tra trước khi gửi khách.</summary>
    public async Task<FreightChatResponse> ChatAsync(FreightChatRequest request, CancellationToken ct)
    {
        var (state, turn) = await DecideAsync(request, ct);
        var message = await ComposeAsync(state, turn, ct);
        state.History.Add(new("assistant", message));
        state.History = state.History.TakeLast(20).ToList();
        state.ExpiresUtc = DateTime.UtcNow.AddMinutes(30);
        return new(message, contexts.Protect(state), turn.Options, turn.Quote);
    }

    private async Task<string> ComposeAsync(ChatState state, Turn turn, CancellationToken ct)
    {
        var fallback = turn.Appended is null ? turn.Message : $"{turn.Appended}\n{turn.Message}";
        var facts = turn.Facts.Append(DescribeDraft(state.Draft)).ToList();
        var brief = new FreightReplyBrief(fallback, turn.Appended, turn.Options.Select(o => o.Label).ToList(), facts);
        string? reply;
        try { reply = FreightReplyGuard.Clean(await ai.ReplyAsync(state.History, brief, ct), brief, state.History); }
        catch (FreightServiceUnavailableException) { return fallback; } // AI diễn đạt lỗi không làm hỏng lượt chat
        // AI chỉ viết lời mở đầu (trả lời câu hỏi phụ/từ chối/đáp lời); câu hỏi bắt buộc của server luôn được nối nguyên văn phía sau.
        if (!FreightReplyGuard.Accept(reply, brief, state.History)) return fallback;
        return $"{reply}\n{fallback}";
    }

    private static string DescribeDraft(FreightDraft draft)
        => "Hành trình đã ghi nhận: xe " + (draft.Vehicle ?? "chưa rõ") + "; " + (draft.Legs.Count == 0 ? "chưa có chặng." :
            string.Join("; ", draft.Legs.Select((l, i) => $"chặng {i + 1}: {l.From ?? "?"} → {l.To ?? "?"}, " + l.IsEmpty switch
            {
                true => "xe rỗng",
                false => $"hàng {l.Weight?.ToString() ?? "?"} {l.Unit}, dễ vỡ: {Flag(l.IsFragile)}, nguy hiểm: {Flag(l.IsHazmat)}",
                _ => "chưa rõ có hàng hay rỗng"
            })));

    private static string Flag(bool? value) => value switch { true => "có", false => "không", _ => "chưa rõ" };

    private async Task<(ChatState, Turn)> DecideAsync(FreightChatRequest request, CancellationToken ct)
    {
        var facts = new List<string>();
        if (request.Message is null || request.Message.Length > 2000 || request.Context?.Length > 100000
            || request.Selection?.Length > 80)
            throw new ArgumentException("Nội dung yêu cầu không hợp lệ hoặc vượt giới hạn.");
        var state = string.IsNullOrEmpty(request.Context) ? new ChatState() : contexts.Unprotect(request.Context);
        var selecting = !string.IsNullOrEmpty(request.Selection);
        if (!selecting)
        {
            if (string.IsNullOrWhiteSpace(request.Message)) throw new ArgumentException("Vui lòng nhập câu hỏi.");
            var history = state.History.Append(new ChatMessage("user", request.Message)).TakeLast(20).ToList();
            var draft = await ai.ExtractAsync(state.Draft, history, ct);
            ValidateDraft(draft);
            state = Carry(state, draft, history);
        }
        var list = await db.PriceLists.AsNoTracking().Where(p => p.Status == PriceListStatus.Active && p.EffectiveDate <= VnClock.Today)
            .OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.Id).FirstOrDefaultAsync(ct);
        if (list is null) return Reply("Hiện chưa có bảng giá đang áp dụng. Vui lòng liên hệ nhân viên.");
        var rules = await db.TariffRules.AsNoTracking().Where(r => r.PriceListId == list.Id).ToListAsync(ct);
        if (rules.Count == 0) return Reply("Bảng giá hiện chưa có loại xe để báo cước.");
        // Bảng giá đang hiệu lực là dữ liệu công khai của endpoint này; AI dùng để trả lời câu hỏi phụ của khách.
        var policy = await SurchargeLookup.AtAsync(db, VnClock.Today, ct);
        facts.AddRange(rules.OrderBy(r => r.Id).Take(15).Select(r =>
            FreightDocuments.Create(r, list, includeFixedFees: policy is null).Text));
        if (policy != null)
            facts.Add($"Phụ phí chung trên tổng cước km và tấn toàn đơn: dễ vỡ {policy.FragilePercent}%, nguy hiểm {policy.HazmatPercent}%. Mỗi loại tính một lần.");
        else
            facts.Add("Cách tính cước mỗi chặng: số km × đơn giá/km + số tấn × đơn giá/tấn + phụ phí hàng dễ vỡ hoặc nguy hiểm; chặng xe rỗng chỉ tính theo km. Km được tính theo tuyến đường thực tế sau khi khách xác nhận địa điểm.");
        if (selecting && request.Selection!.StartsWith("vehicle:"))
        {
            if (!int.TryParse(request.Selection[8..], out var id) || !state.TariffChoices.Contains(id))
                throw new ArgumentException("Lựa chọn xe không hợp lệ. Vui lòng hỏi lại.");
            state.TariffId = id;
            state.TariffChoices.Clear();
        }
        var tariff = rules.FirstOrDefault(r => r.Id == state.TariffId);
        if (tariff != null && state.TariffHash != null && FreightDocuments.Create(tariff, list).Hash != state.TariffHash)
        {
            state.TariffId = null;
            state.AwaitingConfirmation = false;
            tariff = null;
        }
        if (tariff is null)
        {
            state.AwaitingConfirmation = false;
            if (string.IsNullOrWhiteSpace(state.Draft.Vehicle))
            {
                // Khách thường chưa biết tên xe: đưa luôn các loại xe trong bảng giá để chọn.
                var offered = rules.OrderBy(r => r.Id).Take(5).ToList();
                state.TariffChoices = offered.Select(r => r.Id).ToList();
                return Reply("Bạn muốn dùng loại xe nào? Chọn trong bảng giá hoặc nhập loại xe:",
                    offered.Select(r => new ChatOption($"vehicle:{r.Id}", r.VehicleType)).ToList());
            }
            var matches = await vectors.SearchAsync(list.Id, state.Draft.Vehicle, ct);
            var valid = matches.Select(m => rules.FirstOrDefault(r => r.Id == m.Id && FreightDocuments.Create(r, list).Hash == m.Hash))
                .Where(r => r != null).Cast<TariffRule>().Take(5).ToList();
            if (valid.Count == 0) return Reply("Dữ liệu tra giá đang cập nhật hoặc chưa sẵn sàng. Vui lòng thử lại sau.");
            state.TariffChoices = valid.Select(r => r.Id).ToList();
            return Reply("Vui lòng chọn loại xe phù hợp trong bảng giá:", valid.Select(r => new ChatOption($"vehicle:{r.Id}", r.VehicleType)).ToList());
        }
        state.TariffHash = FreightDocuments.Create(tariff, list).Hash;
        facts.Add($"Xe đã chọn: {tariff.VehicleType}.");
        if (state.Draft.Legs.Count == 0) return Reply("Bạn gửi hàng từ đâu đến đâu? Nếu khứ hồi, vui lòng mô tả cả chiều về.");
        for (var i = 0; i < state.Draft.Legs.Count; i++)
        {
            var leg = state.Draft.Legs[i];
            if (string.IsNullOrWhiteSpace(leg.From) || string.IsNullOrWhiteSpace(leg.To)) return Reply($"Vui lòng cho biết điểm đi và điểm đến chặng {i + 1}.");
            if (leg.IsEmpty is null) return Reply($"Chặng {i + 1} có hàng hay xe rỗng?");
            if (leg.IsEmpty == false && (leg.Weight is null || string.IsNullOrWhiteSpace(leg.Unit))) return Reply($"Chặng {i + 1} chở bao nhiêu kg hoặc tấn?");
            if (leg.IsEmpty == false && (leg.IsFragile is null || leg.IsHazmat is null)) return Reply($"Hàng chặng {i + 1} có dễ vỡ hoặc nguy hiểm không?");
            if (leg.IsEmpty == false) _ = FreightQuotePricing.ToKilograms(leg.Weight!.Value, leg.Unit!);
        }
        var addresses = state.Draft.Legs.SelectMany(l => new[] { l.From!, l.To! }).ToList();
        if (selecting && request.Selection!.StartsWith("place:"))
        {
            if (!int.TryParse(request.Selection[6..], out var index) || index < 0 || index >= state.PlaceChoices.Count)
                throw new ArgumentException("Lựa chọn địa điểm không hợp lệ.");
            state.Places.Add(state.PlaceChoices[index]);
            state.PlaceChoices.Clear();
        }
        if (state.Places.Count < addresses.Count)
        {
            var address = addresses[state.Places.Count];
            var suggestions = await places.SearchAsync(address);
            state.PlaceChoices = suggestions.Where(p => double.IsFinite(p.Lat) && double.IsFinite(p.Lng)
                && Math.Abs(p.Lat) <= 90 && Math.Abs(p.Lng) <= 180 && (p.Lat != 0 || p.Lng != 0))
                .Take(5).Select(p => new ConfirmedPlace(p.Address, p.Lat, p.Lng)).ToList();
            if (state.PlaceChoices.Count == 0) return Reply($"Chưa tra được địa điểm “{address}”. Vui lòng nhập địa chỉ cụ thể hơn hoặc thử lại sau.");
            return Reply($"Xác nhận địa điểm “{address}”: ", state.PlaceChoices.Select((p, i) => new ChatOption($"place:{i}", p.Address)).ToList());
        }
        var summary = $"Xe: {tariff.VehicleType}\n" + string.Join("\n", state.Draft.Legs.Select((l, i) =>
            $"Chặng {i + 1}: {state.Places[i * 2].Address} → {state.Places[i * 2 + 1].Address}; " +
            (l.IsEmpty == true ? "xe rỗng" : $"{l.Weight} {l.Unit}; dễ vỡ: {Flag(l.IsFragile)}; nguy hiểm: {Flag(l.IsHazmat)}")));
        if (policy != null)
            summary += $"\nPhụ phí trên tổng cước: dễ vỡ {policy.FragilePercent:0.##}%, nguy hiểm {policy.HazmatPercent:0.##}%.";
        if (request.Selection != "confirm" || !state.AwaitingConfirmation || state.SurchargeDate != policy?.EffectiveDate)
        {
            state.SurchargeDate = policy?.EffectiveDate;
            state.AwaitingConfirmation = true;
            return Reply("Vui lòng xác nhận hành trình để tính cước, hoặc nhập nội dung cần sửa.", [new("confirm", "Xác nhận và tính cước")], appended: summary);
        }
        var inputs = new List<FreightQuoteLegInput>();
        for (var i = 0; i < state.Draft.Legs.Count; i++)
        {
            var from = state.Places[i * 2]; var to = state.Places[i * 2 + 1];
            ct.ThrowIfCancellationRequested();
            var route = await routes.GetRouteAsync(new(from.Lat, from.Lng, to.Lat, to.Lng));
            if (!route.Success || !double.IsFinite(route.DistanceKm) || route.DistanceKm <= 0 || route.DistanceKm > 100000)
                return Reply($"Không tìm được tuyến đường chặng {i + 1}. Vui lòng kiểm tra địa điểm hoặc thử lại sau.");
            var leg = state.Draft.Legs[i];
            inputs.Add(new((decimal)route.DistanceKm, leg.IsEmpty == true ? 0 : FreightQuotePricing.ToKilograms(leg.Weight!.Value, leg.Unit!),
                leg.IsEmpty!.Value, leg.IsEmpty == false && leg.IsFragile == true, leg.IsEmpty == false && leg.IsHazmat == true));
        }
        // Giá có thể thay đổi trong thời gian chờ dịch vụ bản đồ.
        var latest = await db.PriceLists.AsNoTracking().Where(p => p.Status == PriceListStatus.Active && p.EffectiveDate <= VnClock.Today)
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
            return Reply("Phụ phí vừa thay đổi. Vui lòng xác nhận lại hành trình.");
        }
        var amounts = FreightQuotePricing.Calculate(inputs, current, currentPolicy);
        var notice = Notice;
        if (currentPolicy != null)
        {
            var basis = amounts.Sum(a => a.DistanceCharge + a.WeightCharge);
            notice += $" Phụ phí tính một lần trên {basis:N2} đồng cước toàn đơn. Dễ vỡ {currentPolicy.FragilePercent:0.##}%: {amounts.Sum(a => a.FragileCharge):N2} đồng. Nguy hiểm {currentPolicy.HazmatPercent:0.##}%: {amounts.Sum(a => a.HazmatCharge):N2} đồng.";
        }
        var quote = new FreightQuote(list.Name, list.EffectiveDate, DateTime.UtcNow, current.VehicleType,
            inputs.Select((l, i) => new QuotedLeg(state.Places[i * 2].Address, state.Places[i * 2 + 1].Address,
                l.DistanceKm, l.WeightKg, amounts[i])).ToList(), amounts.Sum(a => a.Total), notice);
        state.AwaitingConfirmation = false;
        facts.AddRange(quote.Legs.Select((l, i) => $"Báo giá chặng {i + 1}: {l.DistanceKm:N2} km, {l.WeightKg:N2} kg, tổng {l.Price.Total:N2} đồng."));
        return Reply("Bạn có thể nhập yêu cầu mới hoặc chọn Cuộc trò chuyện mới để tra hành trình khác.", quote: quote,
            appended: $"Tổng cước tham khảo: {quote.Total:N2} đồng.\n{notice}");

        (ChatState, Turn) Reply(string message, List<ChatOption>? options = null, FreightQuote? quote = null, string? appended = null)
            => (state, new Turn(message, options ?? [], quote, appended, facts));
    }

    /// <summary>Giữ xe/địa điểm đã xác nhận khi phần hành trình tương ứng không đổi; mọi tin nhắn mới đều bỏ xác nhận cũ.</summary>
    private static ChatState Carry(ChatState old, FreightDraft draft, List<ChatMessage> history)
    {
        var next = new ChatState { Draft = draft, History = history };
        // Gồm cả xe chọn bằng nút khi khách chưa nêu tên xe (cả hai đều null): giữ đến khi khách nêu loại xe khác.
        if (Same(old.Draft.Vehicle, draft.Vehicle))
        {
            next.TariffId = old.TariffId;
            next.TariffHash = old.TariffHash;
            next.TariffChoices = old.TariffChoices;
        }
        var before = Addresses(old.Draft);
        var after = Addresses(draft);
        var kept = 0;
        while (kept < old.Places.Count && kept < after.Count && kept < before.Count && Same(before[kept], after[kept])) kept++;
        next.Places = old.Places.Take(kept).ToList();
        return next;

        static List<string?> Addresses(FreightDraft d) => d.Legs.SelectMany(l => new[] { l.From, l.To }).ToList();
        static bool Same(string? a, string? b) => string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static void ValidateDraft(FreightDraft draft)
    {
        if (draft is null || draft.Legs is null || draft.Legs.Count > 10 || draft.Vehicle?.Length > 200)
            throw new ArgumentException("Hành trình không hợp lệ hoặc vượt 10 chặng.");
        foreach (var leg in draft.Legs)
        {
            if (leg is null || leg.From?.Length > 500 || leg.To?.Length > 500 || leg.Weight < 0 || leg.Unit?.Length > 20)
                throw new ArgumentException("Thông tin chặng không hợp lệ.");
            if (leg.IsEmpty == true && (leg.Weight > 0 || leg.IsFragile == true || leg.IsHazmat == true))
                throw new ArgumentException("Chặng xe rỗng không được khai hàng hóa.");
            if (leg.IsEmpty == false && leg.Weight == 0) throw new ArgumentException("Khối lượng hàng phải lớn hơn 0.");
        }
    }
}
