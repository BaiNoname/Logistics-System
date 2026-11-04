using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Surcharges;

public record SurchargeListDto(int Id, string Name, DateOnly EffectiveDate, bool Published, int Revision,
    decimal FragilePercent, decimal HazmatPercent);
public record SaveDraft(string Name, DateOnly EffectiveDate, decimal FragilePercent, decimal HazmatPercent, int Revision = 0);
public record CopyDraft(string Name, DateOnly EffectiveDate);
public record PublishDraft(int Revision);

public interface ISurchargeListService
{
    Task<List<SurchargeListDto>> ListAsync();
    Task<SurchargeListDto> CreateAsync(SaveDraft input);
    Task<SurchargeListDto> CopyAsync(int id, CopyDraft input);
    Task<SurchargeListDto> UpdateAsync(int id, SaveDraft input);
    Task<SurchargeListDto> PublishAsync(int id, PublishDraft input);
}

public sealed class SurchargeListService(IApplicationDbContext db) : ISurchargeListService
{
    private static SurchargeListDto Dto(SurchargeList list)
    {
        var rates = list.Rates();
        return new(list.Id, list.Name, list.EffectiveDate, list.Published, list.Revision, rates.FragilePercent, rates.HazmatPercent);
    }

    public async Task<List<SurchargeListDto>> ListAsync() =>
        (await db.SurchargeLists.AsNoTracking().Include(p => p.Lines)
            .OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.Id).ToListAsync()).Select(Dto).ToList();

    private async Task<SurchargeList> LoadAsync(int id) =>
        await db.SurchargeLists.Include(p => p.Lines).SingleOrDefaultAsync(p => p.Id == id)
        ?? throw new KeyNotFoundException("Không tìm thấy bảng phụ phí.");

    public async Task<SurchargeListDto> CreateAsync(SaveDraft input)
    {
        var list = new SurchargeList();
        list.SetDraft(input.Name, input.EffectiveDate, input.FragilePercent, input.HazmatPercent);
        db.SurchargeLists.Add(list);
        await db.SaveChangesAsync();
        return Dto(list);
    }

    public async Task<SurchargeListDto> CopyAsync(int id, CopyDraft input)
    {
        var source = (await LoadAsync(id)).Rates();
        return await CreateAsync(new(input.Name, input.EffectiveDate, source.FragilePercent, source.HazmatPercent));
    }

    public async Task<SurchargeListDto> UpdateAsync(int id, SaveDraft input)
    {
        var list = await LoadAsync(id);
        CheckRevision(list, input.Revision);
        list.SetDraft(input.Name, input.EffectiveDate, input.FragilePercent, input.HazmatPercent);
        await db.SaveChangesAsync();
        return Dto(list);
    }

    public async Task<SurchargeListDto> PublishAsync(int id, PublishDraft input)
    {
        var list = await LoadAsync(id);
        CheckRevision(list, input.Revision);
        if (await db.SurchargeLists.AnyAsync(p => p.Published && p.EffectiveDate == list.EffectiveDate))
            throw new InvalidOperationException("Ngày hiệu lực này đã có bảng công bố.");
        list.Publish(DateOnly.FromDateTime(VnClock.Today));
        await db.SaveChangesAsync();
        return Dto(list);
    }

    private static void CheckRevision(SurchargeList list, int revision)
    {
        if (list.Revision != revision)
            throw new InvalidOperationException("Bảng vừa thay đổi. Hãy tải lại trước khi thao tác.");
    }
}
