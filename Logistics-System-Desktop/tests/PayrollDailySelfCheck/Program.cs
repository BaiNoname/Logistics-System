using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Payroll;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Payroll;
using TruckingSystem.Infrastructure.Hosting;
using TruckingSystem.Infrastructure.Persistence;

Check(PayrollRules.TripPoints(100, 1, 100) == 1m, "A 100 km trip earns one point with K=100 and H=1.");
Check(PayrollRules.TripPoints(50, 1, 100) * 4 == PayrollRules.TripPoints(200, 1, 100),
    "Splitting the same distance across trips does not add points.");
Check(PayrollRules.TripPoints(200, 1) + PayrollRules.TripPoints(400, 2) == 5m,
    "Mixed vehicle factors apply to distance without a per-trip bonus.");
Check(PayrollRules.DriverSalary(10_000_000, PayrollRules.TripPoints(200, 2, 100), 20) == 2_000_000,
    "Driver salary uses distance-based points.");
foreach (var input in new[] { (0m, 1m, 100m), (100m, 0m, 100m), (100m, 1m, 0m), (-1m, 1m, 100m) })
{
    var rejected = false;
    try { PayrollRules.TripPoints(input.Item1, input.Item2, input.Item3); }
    catch (ArgumentOutOfRangeException) { rejected = true; }
    Check(rejected, "Invalid trip distance, vehicle factor or km per point is rejected.");
}
if (args.Contains("--rules-only"))
{
    Console.WriteLine("Payroll distance-based KPI rules self-check passed.");
    return;
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var connectionString = Environment.GetEnvironmentVariable("PAYROLL_SELFCHECK_DB")
    ?? throw new InvalidOperationException("Set PAYROLL_SELFCHECK_DB to a disposable, empty PostgreSQL database.");
var connection = new NpgsqlConnectionStringBuilder(connectionString);
Check(connection.Database?.StartsWith("payroll_selfcheck_", StringComparison.Ordinal) == true,
    "Use a dedicated payroll_selfcheck_* database.");
var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options;
using var services = new ServiceCollection().AddScoped(_ => new AppDbContext(options))
    .AddScoped<IPayrollService>(sp => new PayrollService(sp.GetRequiredService<AppDbContext>()))
    .BuildServiceProvider();

var latestRunMethod = typeof(PayrollCalculationWorker).GetMethod("LatestScheduledRunUtc", BindingFlags.Static | BindingFlags.NonPublic)!;
DateTime LatestRun(DateTime utc) => (DateTime)latestRunMethod.Invoke(null, [utc])!;
var boundary = new DateTime(2026, 12, 31, 23, 0, 0, DateTimeKind.Utc);
Check(LatestRun(boundary.AddSeconds(-1)) == boundary.AddDays(-1), "Before 06:00 VN, use the previous slot.");
Check(LatestRun(boundary) == boundary && LatestRun(boundary.AddHours(12)) == boundary,
    "At and after 06:00 VN, use today's slot, including at New Year.");

var slot = LatestRun(DateTime.UtcNow);
var localSlot = VnClock.ToVnWallClock(slot)!.Value;
var prior = localSlot.AddMonths(-1);
await using (var setup = new AppDbContext(options))
{
    Check(!(await setup.Database.GetAppliedMigrationsAsync()).Any(), "The disposable database must be empty.");
    await setup.Database.MigrateAsync();
    var employee = new Employee { Code = "NV-CHECK", FullName = "Payroll Self Check", HireDate = new DateTime(prior.Year, prior.Month, 1) };
    setup.EmployeeSalaries.Add(new EmployeeSalary { Employee = employee, EffectiveFrom = employee.HireDate, MonthlySalary = 10_000_000 });
    setup.PayrollPeriods.Add(new PayrollPeriod { Year = prior.Year, Month = prior.Month, CreatedBy = "fixture" });
    await setup.SaveChangesAsync();
}

using (var worker = new PayrollCalculationWorker(services.GetRequiredService<IServiceScopeFactory>(),
    NullLogger<PayrollCalculationWorker>.Instance))
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
    await worker.StartAsync(timeout.Token);
    await using var read = new AppDbContext(options);
    while (await read.PayrollPeriods.CountAsync(p => p.LastAutomaticCalculationAt != null, timeout.Token) < 2)
    {
        await Task.Delay(100, timeout.Token);
    }
    await worker.StopAsync(timeout.Token);
}

await using var db = new AppDbContext(options);
var service = new PayrollService(db);
var period = await db.PayrollPeriods.SingleAsync(p => p.Year == prior.Year && p.Month == prior.Month);
var current = await db.PayrollPeriods.SingleAsync(p => p.Year == localSlot.Year && p.Month == localSlot.Month);
Check(period.Revision == 0 && period.AutomaticCalculationError!.Contains("NV-CHECK"),
    "The worker records validation errors for an existing unapproved period.");
Check(current.AutomaticCalculationError!.Contains("NV-CHECK") && !await db.PayrollSlips.AnyAsync(),
    "A first failed run creates a period with useful details and no partial slips.");
var previousAttempt = period.LastAutomaticCalculationAt;
await service.RunAutomaticCalculationAsync(period.Year, period.Month, slot, default);
Check(period.LastAutomaticCalculationAt == previousAttempt, "Repeating the same slot is a no-op.");

var checkIn = new DateTime(prior.Year, prior.Month, 1, 1, 0, 0, DateTimeKind.Utc);
var attendance = new AttendanceRecord
{
    EmployeeId = await db.Employees.Select(e => e.Id).SingleAsync(),
    WorkDate = checkIn.Date,
    CheckInAt = checkIn,
    CheckOutAt = checkIn.AddHours(8)
};
db.AttendanceRecords.Add(attendance);
await db.SaveChangesAsync();
await RunNextSlotAsync();
var original = await db.PayrollSlips.AsNoTracking().SingleAsync();
var originalRevision = period.Revision;
Check(period.AutomaticCalculationError is null && original.WorkedDays == 1 && original.NetPay > 0,
    "The next run builds a draft from corrected attendance.");

await RunNextSlotAsync();
Check((await db.PayrollSlips.AsNoTracking().SingleAsync()).Id == original.Id && period.Revision == originalRevision,
    "An unchanged draft retains its slip ID and revision on the next run.");

attendance.CheckOutAt = null;
await db.SaveChangesAsync();
await RunNextSlotAsync();
Check(period.AutomaticCalculationError!.Contains("thiếu hoặc sai giờ vào/ra")
    && (await db.PayrollSlips.AsNoTracking().SingleAsync()).Id == original.Id && period.Revision == originalRevision,
    "A later invalid scan leaves the existing draft intact.");

attendance.CheckOutAt = checkIn.AddHours(4);
await db.SaveChangesAsync();
Check(!(await service.ApproveAsync(period.Id, new(period.Revision), "admin")).Success, "Stale drafts cannot be approved.");
await RunNextSlotAsync();
var revised = await db.PayrollSlips.AsNoTracking().SingleAsync();
Check(period.AutomaticCalculationError is null && revised.WorkedDays == 0.5m && revised.NetPay < original.NetPay
    && period.Revision == originalRevision + 1, "Changed source data atomically replaces the draft.");
Check((await service.ApproveAsync(period.Id, new(period.Revision), "admin")).Success, "Admin can approve the refreshed draft.");
var lastSuccessfulCalculation = period.LastAutomaticCalculationAt;
await service.RunAutomaticCalculationAsync(period.Year, period.Month, slot.AddDays(1), default);
Check(period.ApprovedAt is not null && period.LastAutomaticCalculationAt == lastSuccessfulCalculation
    && (await db.PayrollSlips.AsNoTracking().SingleAsync()).Id == revised.Id, "An approved payroll period is skipped.");
Console.WriteLine("Daily payroll PostgreSQL self-check passed.");

async Task RunNextSlotAsync()
{
    period.LastAutomaticCalculationAt = slot.AddDays(-1);
    await db.SaveChangesAsync();
    await service.RunAutomaticCalculationAsync(period.Year, period.Month, slot, default);
}

static void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException(description);
}
