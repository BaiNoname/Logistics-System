using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;
using TruckingSystem.Application.Payroll;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Domain.Attendance;
using TruckingSystem.Domain.Payroll;
using TruckingSystem.Infrastructure.Persistence;
using TruckingSystem.Desktop.GUI_PayrollManagement;

var checks = 0;
var previewSlips = new List<TruckingSystem.Desktop.DTO_PayrollManagement.PayrollSlipDto>();
void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }
void Ok(PayrollResult result, string name) => Check(result.Success, name + ": " + result.Message);
AppDbContext Fixture(bool withOffice = true)
{
    var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options);
    if (withOffice) db.Employees.Add(new Employee { Id = 1, Code = "NV1", FullName = "Office", HireDate = new(2020,1,1) });
    db.Employees.Add(new Employee { Id = 2, Code = "TX2", FullName = "Driver", HireDate = new(2020,1,1),
        TerminationDate = new(2024,2,15), DriverProfile = new DriverProfile { EmployeeId = 2 } });
    if (withOffice) db.EmployeeSalaries.Add(new EmployeeSalary { EmployeeId = 1, EffectiveFrom = new(2024,1,1), MonthlySalary = 10_000_000 });
    db.EmployeeSalaries.Add(new EmployeeSalary { EmployeeId = 2, EffectiveFrom = new(2024,1,1), MonthlySalary = 10_000_000 });
    db.SaveChanges(); return db;
}
SaveKpiPolicyRequest Policy(decimal target = 5) => new(new(2024,1,1), 200, target,
    new() { new() { VehicleType = "A", Factor = 1 }, new() { VehicleType = "B", Factor = 2 } });
foreach (var (year, month, days, standard) in new[] { (2024,2,29,25),(2025,2,28,24),(2024,6,30,25),(2024,12,31,26),(2025,1,31,27) })
{
    var range = PayrollRules.PeriodRange(year,month);
    Check(range.From.Day == 1 && range.To.Day == days, "Calendar month");
    Check(PayrollRules.StandardDays(year,month) == standard, "Mon-Sat standard");
}
var start = new DateTime(2024,2,1,8,0,0);
foreach (var (hours, days) in new[] { (3.99,0m),(4,0.5m),(7.99,0.5m),(8,1m),(12,1m) })
    Check(AttendanceRules.PaidDays(start,start.AddHours(hours)) == days, "Attendance threshold");
Check(PayrollRules.DriverSalary(10_000_000, 30, 20) == 15_000_000, "Uncapped KPI 150%");
Check(PayrollRules.DriverSalary(10_000_000, 0, 20) == 0, "Zero KPI");
Check(PayrollRules.TripPoints(200,1) + PayrollRules.TripPoints(400,2) == 6.5m, "Mixed vehicle types with common K");
Check(PayrollRules.SalaryStatus(new(2024,2,1),null,new(2024,1,31)) == "Pending", "Salary pending");
Check(PayrollRules.SalaryStatus(new(2024,2,1),new(2024,2,29),new(2024,2,29)) == "Active", "Salary inclusive end");
Check(PayrollRules.SalaryStatus(new(2024,2,1),new(2024,2,29),new(2024,3,1)) == "Expired", "Salary expired");

using(var db = Fixture())
{
    var service = new PayrollService(db);
    Check(!(await service.SaveKpiPolicyAsync(Policy(0),"admin")).Success, "Zero target rejected");
    Ok(await service.SaveKpiPolicyAsync(Policy(),"admin"),"Save common K and target");
    Ok(await service.SaveKpiPolicyAsync(Policy(),"admin"),"Update same effective date");
    Check(await db.PayrollKpiPolicies.CountAsync() == 1,"Unique effective date upsert");
    var a = new Vehicle { Id=1, VehicleType="A", PlateNumber="TEST-A" };
    var b = new Vehicle { Id=2, VehicleType="B", PlateNumber="TEST-B" };
    db.Vehicles.AddRange(a,b);
    db.Trips.AddRange(new Trip { DriverEmployeeId=2, Vehicle=a, Status=TripStatus.Completed, StartTime=new(2024,2,9), EndTime=new(2024,2,10), EstimatedDistanceKm=200 },
        new Trip { DriverEmployeeId=2, Vehicle=b, Status=TripStatus.Completed, StartTime=new(2024,2,13), EndTime=new(2024,2,14), EstimatedDistanceKm=400 },
        new Trip { DriverEmployeeId=2, Vehicle=b, Status=TripStatus.Planned, StartTime=new(2024,2,13), EndTime=new(2024,2,14), EstimatedDistanceKm=999 },
        new Trip { DriverEmployeeId=2, Vehicle=b, Status=TripStatus.Completed, EndTime=new(2024,2,14), EstimatedDistanceKm=999 });
    await db.SaveChangesAsync();
    Ok(await service.AddMonthlyAdjustmentAsync(new(2,2024,2,"Allowance",500_000,"Phụ cấp"),"accountant"),"Allowance before period exists");
    Check(await db.PayrollPeriods.CountAsync() == 0,"No explicit open period");
    var failed = await service.GenerateMonthAsync(new(2024,2,0),"accountant");
    Check(!failed.Success && failed.Message!.Contains("NV1"),"Missing office attendance stops whole month");
    Check(await db.PayrollPeriods.CountAsync() == 0 && await db.PayrollSlips.CountAsync() == 0,"Failure creates no period or partial slips");
    var r1 = new AttendanceRecord { EmployeeId=1, WorkDate=new(2024,2,1), CheckInAt=start, CheckOutAt=start.AddHours(8) };
    var r2 = new AttendanceRecord { EmployeeId=1, WorkDate=new(2024,2,2), CheckInAt=start.AddDays(1) };
    db.AttendanceRecords.AddRange(r1,r2); await db.SaveChangesAsync();
    Check(!(await service.GenerateMonthAsync(new(2024,2,0),"accountant")).Success,"Incomplete scan blocks generation");
    r2.CheckOutAt = r2.CheckInAt.Value.AddHours(4);
    var pending = new AttendanceAdjustmentRequest { EmployeeId=1, WorkDate=r2.WorkDate, Status=AttendanceAdjustmentStatus.Pending };
    db.AttendanceAdjustmentRequests.Add(pending); await db.SaveChangesAsync();
    Check(!(await service.GenerateMonthAsync(new(2024,2,0),"accountant")).Success,"Pending correction blocks generation");
    pending.Status = AttendanceAdjustmentStatus.Rejected; await db.SaveChangesAsync();
    Ok(await service.GenerateMonthAsync(new(2024,2,0),"accountant"),"Generate whole month");
    var period = await db.PayrollPeriods.SingleAsync();
    var slips = await db.PayrollSlips.OrderBy(s=>s.EmployeeId).ToListAsync();
    Check(slips.Count==2 && slips.All(s=>s.CalculationVersion==2),"Complete new calculation set");
    Check(slips[0].WorkedDays==1.5m && slips[0].EarnedSalary==600_000,"Office salary from 8h and 4h records");
    Check(slips[1].KpiPoints==6.5m && slips[1].EarnedSalary==13_000_000,"Driver left mid-month, no daily proration");
    Check(slips[1].NetPay==13_500_000 && slips[1].Items.Count==1,"Allowance is separate, KPI not added twice");
    Check(await db.MonthlyAttendances.CountAsync()==0,"No duplicate manual payroll attendance");
    var revision=period.Revision;
    Ok(await service.AddMonthlyAdjustmentAsync(new(2,2024,2,"Deduction",100_000,"Khấu trừ"),"accountant"),"Add deduction");
    Check(!(await service.GenerateMonthAsync(new(2024,2,revision),"accountant")).Success,"Stale revision rejected");
    Check(!(await service.ApproveAsync(period.Id,new(period.Revision),"admin")).Success,"Stale ledger blocks approval");
    Ok(await service.GenerateMonthAsync(new(2024,2,period.Revision),"accountant"),"Recalculate preserves ledger");
    Check(await db.PayrollSlips.CountAsync()==2 && await db.PayrollMonthlyAdjustments.CountAsync()==2,"No duplicate slips or lost adjustments");
    Check((await db.PayrollSlips.SingleAsync(s=>s.EmployeeId==2)).NetPay==13_400_000,"Deduction included");
    var oldNet=(await db.PayrollSlips.SingleAsync(s=>s.EmployeeId==1)).NetPay;
    r1.CheckOutAt=null; await db.SaveChangesAsync();
    Check(!(await service.GenerateMonthAsync(new(2024,2,period.Revision),"accountant")).Success,"Failed recalculation");
    Check((await db.PayrollSlips.SingleAsync(s=>s.EmployeeId==1)).NetPay==oldNet,"Failed recalculation preserves existing draft");
    r1.CheckOutAt=start.AddHours(8); r2.AdjustedCheckOutAt=r2.CheckInAt.Value.AddHours(8); await db.SaveChangesAsync();
    Check(!(await service.ApproveAsync(period.Id,new(period.Revision),"admin")).Success,"Changed corrected attendance blocks approval");
    Ok(await service.GenerateMonthAsync(new(2024,2,period.Revision),"accountant"),"Use approved corrected time");
    Check((await db.PayrollSlips.SingleAsync(s=>s.EmployeeId==1)).EarnedSalary==800_000,"Correction affects office salary");
    Ok(await service.ApproveAsync(period.Id,new(period.Revision),"admin"),"Separate close");
    previewSlips = System.Text.Json.JsonSerializer.Deserialize<List<TruckingSystem.Desktop.DTO_PayrollManagement.PayrollSlipDto>>(
        System.Text.Json.JsonSerializer.Serialize(await service.GetSlipsAsync(period.Id)))!;
    var driver=await db.PayrollSlips.SingleAsync(s=>s.EmployeeId==2);
    var html=await service.ExportSlipAsync(driver.Id);
    Check(html!.Contains("LCB × (P / điểm chuẩn)") && !html.Contains("Ngày công tài xế"),"Export uses new formula and stored trip evidence");
    Ok(await service.SaveKpiPolicyAsync(Policy(10),"admin"),"Update settings after close");
    Check(driver.NetPay==13_400_000,"Closed salary unchanged");
    Check(!(await service.GenerateMonthAsync(new(2024,2,period.Revision),"accountant")).Success,"Closed month cannot recalculate");
    Check(!(await service.AddMonthlyAdjustmentAsync(new(2,2024,2,"Allowance",1,"Test"),"accountant")).Success,"Closed ledger locked");
    Check(!(await service.DeleteMonthlyAdjustmentAsync((await db.PayrollMonthlyAdjustments.FirstAsync()).Id,"accountant")).Success,"Closed ledger cannot delete");
    Check(!(await service.AddItemAsync(driver.Id,new(period.Revision,"Bonus",1,false,"Test"),true,"admin")).Success,"Old adjustment route cannot mutate new closed slip");
    Check(!(await service.GenerateSlipAsync(period.Id,2,new(period.Revision),"accountant")).Success,"Old one-employee calculation retired");
}
using(var db=Fixture(false))
{
    var service=new PayrollService(db);
    Check(!(await service.GenerateMonthAsync(new(2024,2,0),"test")).Success,"Missing policy blocks driver");
    Ok(await service.SaveKpiPolicyAsync(Policy(),"admin"),"Policy for zero trips");
    Ok(await service.GenerateMonthAsync(new(2024,2,0),"test"),"Driver needs no registered workdays");
    Check((await db.PayrollSlips.SingleAsync()).NetPay==0,"No trips means zero KPI salary");
}
using(var db=Fixture(false))
{
    var period=new PayrollPeriod { Year=2024,Month=2,ApprovedAt=DateTime.UtcNow };
    db.PayrollPeriods.Add(period);
    var legacy=new PayrollSlip { EmployeeId=2,PayrollPeriod=period,NetPay=123,WorkFrom=new(2024,1,26),WorkTo=new(2024,2,25),DriverWorkDates="01/02/2024" };
    db.PayrollSlips.Add(legacy);await db.SaveChangesAsync();
    var service=new PayrollService(db);
    Check((await service.ExportSlipAsync(legacy.Id))!.Contains("26/01/2024–25/02/2024"),"Legacy approved period preserved");
    Check(!(await service.GenerateMonthAsync(new(2024,2,0),"test")).Success && legacy.NetPay==123,"Legacy approved money preserved");
    period.ApprovedAt=null;await db.SaveChangesAsync();
    var oldDraft=await service.GenerateMonthAsync(new(2024,2,0),"test");
    Check(!oldDraft.Success && oldDraft.Message!.Contains("cơ chế cũ") && legacy.NetPay==123,"Legacy draft requires explicit reconciliation, never silently overwritten");
}
using(var db=Fixture(false))
{
    var service=new PayrollService(db);
    Ok(await service.SaveKpiPolicyAsync(Policy(),"admin"),"Boundary policy");
    var vehicle=new Vehicle { VehicleType="A", PlateNumber="BOUNDARY" };db.Vehicles.Add(vehicle);
    db.Trips.AddRange(new Trip { DriverEmployeeId=2,Vehicle=vehicle,Status=TripStatus.Completed,StartTime=new(2024,1,31,10,0,0),EndTime=new(2024,1,31,17,0,0),EstimatedDistanceKm=200 },
        new Trip { DriverEmployeeId=2,Vehicle=vehicle,Status=TripStatus.Completed,StartTime=new(2024,2,29,10,0,0),EndTime=new(2024,2,29,17,0,0),EstimatedDistanceKm=999 });
    await db.SaveChangesAsync();
    Ok(await service.GenerateMonthAsync(new(2024,2,0),"test"),"Vietnam month boundaries");
    Check((await db.PayrollSlips.SingleAsync()).KpiPoints==1.5m,"Includes VN first midnight, excludes following midnight");
    var period=await db.PayrollPeriods.SingleAsync();
    (await db.Trips.OrderBy(t=>t.Id).FirstAsync()).EstimatedDistanceKm=null;await db.SaveChangesAsync();
    Check(!(await service.GenerateMonthAsync(new(2024,2,period.Revision),"test")).Success,"Missing trip km blocks recalculation");
    Check((await db.PayrollSlips.SingleAsync()).KpiPoints==1.5m,"Bad trip leaves old draft intact");
}
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);
using(var db=new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql("Host=localhost;Database=offline_check").Options))
{
    Check(!db.Database.HasPendingModelChanges(),"Npgsql migration model matches");
    var sql=db.GetService<IMigrator>().GenerateScript("20261102111437_AddMobileQrAttendance", "20261102115836_RebuildMonthlyPayroll");
    Check(sql.Contains("PayrollKpiPolicies") && sql.Contains("PayrollMonthlyAdjustments") && !sql.Contains("DROP TABLE"),"Additive migration preserves history");
    File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"monthly-payroll-migration.sql"),sql);
}
Exception? uiError=null;
var thread=new Thread(()=>
{
    try
    {
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        using var control=new PayrollControl();
        Check(typeof(PayrollControl).GetField("_openPeriodButton",flags)==null && typeof(PayrollControl).GetField("_attendanceGrid",flags)==null,"No manual period or attendance UI");
        using var host=new Form { Size=new Size(1250,750), ShowInTaskbar=false, StartPosition=FormStartPosition.Manual,Location=new Point(-20000,-20000) };
        var payrollGrid = (DataGridView)typeof(PayrollControl).GetField("_slipGrid",flags)!.GetValue(control)!;
        payrollGrid.DataSource = previewSlips;
        host.Controls.Add(control);host.Show();System.Windows.Forms.Application.DoEvents();
        var driverRow = payrollGrid.Rows.Cast<DataGridViewRow>().Single(r => ((TruckingSystem.Desktop.DTO_PayrollManagement.PayrollSlipDto)r.DataBoundItem).IsDriver);
        Check(driverRow.Cells["KpiRatio"].FormattedValue.ToString()!.Contains("130"),"Uncapped KPI percent displayed in month grid");
        using(var image=new Bitmap(host.Width,host.Height)) {host.DrawToBitmap(image,new Rectangle(Point.Empty,host.Size));image.Save(Path.Combine(AppContext.BaseDirectory,"monthly-payroll-preview.png"));}
        host.Close();
        using var policy=new PayrollKpiPolicyForm(new(),new(){"Xe tải","Container"},_=>Task.FromResult(new TruckingSystem.Desktop.DTO_PayrollManagement.PayrollResult(true)));
        policy.ShowInTaskbar=false;policy.StartPosition=FormStartPosition.Manual;policy.Location=new Point(-20000,-20000);policy.Show();System.Windows.Forms.Application.DoEvents();
        IEnumerable<Control> All(Control root) => root.Controls.Cast<Control>().SelectMany(c=>new[]{c}.Concat(All(c)));
        var grid=All(policy).OfType<DataGridView>().Single();
        Check(!grid.ReadOnly && grid.Columns.Count==2 && grid.Columns["VehicleType"].ReadOnly,"Policy H grid editable without money-per-point");
        Check(All(policy).OfType<NumericUpDown>().Count()==2,"Common K and common target controls");
        using(var image=new Bitmap(policy.Width,policy.Height)) {policy.DrawToBitmap(image,new Rectangle(Point.Empty,policy.Size));image.Save(Path.Combine(AppContext.BaseDirectory,"monthly-kpi-preview.png"));}
        policy.Close();
    }
    catch(Exception ex){uiError=ex;}
});thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();if(uiError!=null)throw uiError;
Console.WriteLine($"PASS: {checks} monthly payroll checks. InMemory does not validate PostgreSQL transactions or HTTP authorization.");
