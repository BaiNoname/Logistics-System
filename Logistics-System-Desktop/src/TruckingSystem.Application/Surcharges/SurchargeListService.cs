using Microsoft.EntityFrameworkCore;
using System.Data;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Surcharges;

public record SurchargeListDto(int Id, string Name, DateOnly EffectiveDate, bool Published, int Revision,
    decimal FragilePercent, decimal HazmatPercent, bool IsActive = false, bool IsPending = false);
public record SaveDraft(string Name, DateOnly EffectiveDate, decimal FragilePercent, decimal HazmatPercent, int Revision = 0);
public record PublishDraft(int Revision);

public interface ISurchargeListService
{
    Task<List<SurchargeListDto>> ListAsync();
    Task<SurchargeListDto> CreateAsync(SaveDraft input);
    Task<SurchargeListDto> UpdateAsync(int id, SaveDraft input);
    Task<SurchargeListDto> PublishAsync(int id, PublishDraft input);
}

public sealed class SurchargeListService(IApplicationDbContext db) : ISurchargeListService
{
    private static SurchargeListDto Dto(SurchargeList list, int? activeId)
    {
        var rates = list.Rates();
        return new(list.Id, list.Name, list.EffectiveDate, list.Published, list.Revision, rates.FragilePercent, rates.HazmatPercent, list.Id == activeId,
            list.Published && list.EffectiveDate > DateOnly.FromDateTime(VnClock.Today));
    }

    public async Task<List<SurchargeListDto>> ListAsync()
    {
        var lists = await db.SurchargeLists.AsNoTracking().Include(p => p.Lines)
            .OrderByDescending(p => p.Id).ToListAsync();
        var today = DateOnly.FromDateTime(VnClock.Today);
        var activeId = lists.FirstOrDefault(p => p.Published && p.EffectiveDate <= today)?.Id;
        return lists.Select(p => Dto(p, activeId)).ToList();
    }

    private Task<int?> ActiveIdAsync() => db.SurchargeLists.AsNoTracking()
        .Where(p => p.Published && p.EffectiveDate <= DateOnly.FromDateTime(VnClock.Today))
        .OrderByDescending(p => p.Id)
        .Select(p => (int?)p.Id).FirstOrDefaultAsync();

    private async Task<SurchargeList> LoadAsync(int id) =>
        await db.SurchargeLists.Include(p => p.Lines).SingleOrDefaultAsync(p => p.Id == id)
        ?? throw new KeyNotFoundException("Không tìm thấy bảng phụ phí.");

    public async Task<SurchargeListDto> CreateAsync(SaveDraft input)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var prefix = $"PP-{VnClock.Today.Year}-";
        var name = DocumentNumber.Next(prefix, await db.SurchargeLists.AsNoTracking()
            .Where(p => p.Name.StartsWith(prefix)).Select(p => p.Name).ToListAsync());
        var list = new SurchargeList();
        list.SetDraft(name, input.EffectiveDate, input.FragilePercent, input.HazmatPercent);
        Apply(list);
        db.SurchargeLists.Add(list);
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return Dto(list, await ActiveIdAsync());
    }

    public async Task<SurchargeListDto> UpdateAsync(int id, SaveDraft input)
    {
        var list = await LoadAsync(id);
        CheckRevision(list, input.Revision);
        list.SetDraft(input.Name, input.EffectiveDate, input.FragilePercent, input.HazmatPercent);
        Apply(list);
        await db.SaveChangesAsync();
        return Dto(list, await ActiveIdAsync());
    }

    public async Task<SurchargeListDto> PublishAsync(int id, PublishDraft input)
    {
        var list = await LoadAsync(id);
        CheckRevision(list, input.Revision);
        Apply(list);
        await db.SaveChangesAsync();
        return Dto(list, await ActiveIdAsync());
    }

    private static void Apply(SurchargeList list)
        => list.Publish(DateOnly.FromDateTime(VnClock.Today));

    private static void CheckRevision(SurchargeList list, int revision)
    {
        if (list.Revision != revision)
            throw new InvalidOperationException("Bảng vừa thay đổi. Hãy tải lại trước khi thao tác.");
    }
}
