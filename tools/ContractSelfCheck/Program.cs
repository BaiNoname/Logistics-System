using System.Reflection;
using TruckingSystem.Desktop.GUI.GUI_SurchargeManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using TruckingSystem.Application.Contracts;
using TruckingSystem.Application.Pricing;
using TruckingSystem.Application.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;
using TruckingSystem.Desktop.GUI.GUI_PricingManagement;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Infrastructure.Persistence;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;
using TruckingSystem.Desktop.GUI.GUI_ContractManagement;

internal static class Program
{
    private static int checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        Console.WriteLine("PASS " + name); checks++;
    }
    private static async Task Reject(Func<Task> action, string name)
    {
        try { await action(); } catch (ArgumentException) { Check(true, name); return; }
        throw new Exception("Expected rejection: " + name);
    }
    private static SaveContractRequest Request(string code = "C1", string? type = "OneTime", decimal? discount = null, int? price = 1)
        => new() { Code = code, CustomerId = 1, StartDate = new(2026, 1, 1), PriceListId = price, Type = type, DiscountPercent = discount };

    [STAThread]
    private static void Main()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        RunAsync().GetAwaiter().GetResult();
        CheckPricingAsync().GetAwaiter().GetResult();
        CheckForms();
        Console.WriteLine($"{checks} checks passed.");
    }
    private static async Task RunAsync()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Customers.AddRange(new Customer { Id = 1, Name = "Customer" }, new Customer { Id = 2, Name = "Other" });
        db.PriceLists.AddRange(new PriceList { Id = 1, Name = "Active" }, new PriceList { Id = 2, Name = "Inactive", Status = PriceListStatus.Inactive });
        db.TariffRules.AddRange(new TariffRule { Id = 1, PriceListId = 1, VehicleType = "Truck" }, new TariffRule { Id = 2, PriceListId = 2, VehicleType = "Truck" });
        await db.SaveChangesAsync();
        var service = new ContractService(db);
        var lookups = await service.GetLookupsAsync();
        Check(lookups.PriceLists.Count == 1 && lookups.TariffRules.Count == 1, "Active lookups only");
        foreach (var price in new int?[] { null, 0, 999, 2 })
            await Reject(() => service.CreateAsync(Request(price: price)), "Reject missing/invalid/inactive price " + price);
        foreach (var type in new string?[] { null, "", "Unknown", "0", "onetime" })
            await Reject(() => service.CreateAsync(Request(type: type)), "Reject invalid type " + type);
        foreach (var discount in new decimal?[] { null, -1, 100.01m, 1.001m })
            await Reject(() => service.CreateAsync(Request(type: "LongTerm", discount: discount)), "Reject invalid long-term discount " + discount);
        await Reject(() => service.CreateAsync(Request(discount: 1)), "One-time rejects discount");
        var mismatch = Request(); mismatch.TariffRuleId = 2;
        await Reject(() => service.CreateAsync(mismatch), "Tariff must belong to price list");
        var invalidCustomer = Request(); invalidCustomer.CustomerId = 999;
        await Reject(() => service.CreateAsync(invalidCustomer), "Missing customer rejected");
        var invalidPeriod = Request(); invalidPeriod.EndDate = invalidPeriod.StartDate.AddDays(-1);
        await Reject(() => service.CreateAsync(invalidPeriod), "End before start rejected");
        var one = await service.CreateAsync(Request());
        Check(one.Type == "OneTime" && one.DiscountPercent == null, "One-time round trip");
        await Reject(() => service.CreateAsync(Request()), "Duplicate code rejected");
        foreach (var discount in new[] { 0m, 12.25m, 100m })
        {
            var saved = await service.CreateAsync(Request("L" + discount, "LongTerm", discount));
            Check(saved.Type == "LongTerm" && saved.DiscountPercent == discount, "Long-term round trip " + discount);
        }
        var update = Request(type: "LongTerm", discount: 5.25m);
        Check(await service.UpdateAsync(one.Id, update), "Update type and discount");
        await service.UpdateAsync(one.Id, Request(type: null));
        Check((await service.GetByIdAsync(one.Id))!.DiscountPercent == 5.25m, "Old update preserves new fields");
        await Reject(() => service.UpdateAsync(one.Id, Request(type: "LongTerm")), "Update requires long-term discount");
        await service.UpdateAsync(one.Id, Request());
        Check((await service.GetByIdAsync(one.Id))!.DiscountPercent == null, "Switch to one-time clears discount");
        db.Contracts.Add(new Contract { Id = 100, Code = "Legacy", CustomerId = 1, StartDate = new(2026, 1, 1) });
        await db.SaveChangesAsync();
        await service.UpdateAsync(100, Request("Legacy", null, price: null));
        Check((await service.GetByIdAsync(100))!.Type == null, "Legacy remains unclassified");
        var withTariff = Request("ExistingTariff"); withTariff.TariffRuleId = 1;
        var existingTariff = await service.CreateAsync(withTariff);
        await service.UpdateAsync(existingTariff.Id, Request("ExistingTariff"));
        Check((await service.GetByIdAsync(existingTariff.Id))!.TariffRule?.Id == 1,
            "Editing without tariff selector preserves the historical tariff");
        db.PriceLists.Local.Single(p => p.Id == 1).Status = PriceListStatus.Inactive;
        await db.SaveChangesAsync();
        await service.UpdateAsync(one.Id, Request());
        Check((await service.GetByIdAsync(one.Id))!.PriceListId == 1, "Keep historical inactive price on edit");
        await Reject(() => service.CreateAsync(Request("New")), "Stale lookup rejected at save");
        db.TransportOrders.Add(new TransportOrder { Id = 1, Code = "Order", CustomerId = 1, ContractId = one.Id });
        await db.SaveChangesAsync();
        var changeCustomer = Request(); changeCustomer.CustomerId = 2;
        await Reject(() => service.UpdateAsync(one.Id, changeCustomer), "Transport order blocks customer change");
        try { await service.DeleteAsync(one.Id); throw new Exception("Delete should fail"); }
        catch (InvalidOperationException) { Check(true, "Transport order blocks deletion"); }
        Check(await service.DeleteAsync(100), "Unused contract can be deleted");
        using var relational = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql("Host=127.0.0.1;Port=1;Database=offline_contract_design;Username=unused").Options);
        Check(!relational.Database.HasPendingModelChanges(), "Migration snapshot matches Npgsql model");
        var migrations = relational.GetService<IMigrationsAssembly>();
        var entry = migrations.Migrations.Single(m => m.Key.EndsWith("_AddContractTypeAndDiscount"));
        var migration = migrations.CreateMigration(entry.Value, relational.Database.ProviderName!);
        Check(entry.Key.EndsWith("AddContractTypeAndDiscount") && migration.UpOperations.Count == 2 &&
            migration.UpOperations.All(o => o is AddColumnOperation { Table: "Contracts", IsNullable: true }), "Migration only adds two nullable contract columns");
        var sql = relational.GetService<IMigrator>().GenerateScript(migrations.Migrations.Keys.TakeWhile(id => id != entry.Key).Last(), entry.Key);
        Check(!sql.Contains("DROP ") && !sql.Contains("UPDATE "), "Forward SQL does not delete or rewrite data");
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "contract-migration.sql"), sql);
    }
    private static async Task CheckPricingAsync()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var service = new PricingService(db);
        var active = await service.CreatePriceListAsync(new("Current", VnClock.Today));
        Check(await service.UpdatePriceListAsync(active.Id, new("Updated", VnClock.Today)), "Active price list can be edited");
        Check(await service.UpdatePriceListAsync(active.Id, new("Updated", VnClock.Today, "Inactive")), "Active price list can be stopped");
        foreach (var status in new string?[] { null, "Active", "Inactive" })
        {
            var rejected = false;
            try { await service.UpdatePriceListAsync(active.Id, new("Forbidden", VnClock.Today, status)); }
            catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "Inactive price list rejects edits/reactivation " + status);
        }
        var stored = await db.PriceLists.FindAsync(active.Id);
        Check(stored!.Name == "Updated" && stored.Status == PriceListStatus.Inactive, "Rejected edits leave inactive data intact");
        var rejectedRule = false;
        try { await service.CreateTariffRuleAsync(active.Id, new("Truck", 1, 1, 0, 0)); }
        catch (InvalidOperationException) { rejectedRule = true; }
        Check(rejectedRule, "Inactive price list still rejects tariff changes");
        var copy = await service.CopyPriceListAsync(active.Id, new("Replacement"));
        Check(copy?.Status == "Active" && copy.Id != active.Id && stored.Status == PriceListStatus.Inactive, "Inactive price list can be copied to a new active list");
        var oldRule = new TariffRule { PriceListId = copy!.Id, VehicleType = "Legacy", PricePerKm = 10, SurchargeFragile = 75000 };
        db.TariffRules.Add(oldRule);
        await db.SaveChangesAsync();
        Check(await service.UpdateTariffRuleAsync(copy.Id, oldRule.Id, new("Legacy", 12, 20))
            && oldRule.SurchargeFragile == 75000, "Rate-only edit preserves historical fixed fee");
        try
        {
            await service.UpdateTariffRuleAsync(copy.Id, oldRule.Id, new("Legacy", 12, 20, 0, 0));
            throw new Exception("Unexpected legacy fee edit");
        }
        catch (InvalidOperationException) { Check(oldRule.SurchargeFragile == 75000, "Cannot overwrite historical fee"); }
        Check(!await service.UpdatePriceListAsync(999, new("Missing", VnClock.Today)), "Missing price list returns not found");
    }
    private static T Field<T>(object form, string name) => (T)form.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(form)!;
    private static void CheckForms()
    {
        var api = new ApiServices("http://127.0.0.1:1");
        foreach (var role in new[] { "Admin", "Accountant", "Dispatcher" })
        {
            api.Connection.SetToken("test", [role]);
            using var fees = new SurchargeDetailForm(api);
            fees.Show();
            Check(Field<Button>(fees, "_save").Visible == (role != "Dispatcher"), "Surcharge form write access " + role);
            Check(Field<DateTimePicker>(fees, "_date").Value.Date == VnClock.Today.AddDays(1), "Default surcharge date");
            using var locked = new SurchargeDetailForm(api, new(3, "Published", VnClock.Today, true, 2, 3, 5));
            locked.Show();
            Check(!Field<Button>(locked, "_save").Visible && !Field<NumericUpDown>(locked, "_hazmat").Enabled, "Published detail read only");
            locked.Close();
            fees.Close();
        }
        using var form = new ContractForm(api);
        Check(typeof(ContractForm).GetField("_tariffRule", BindingFlags.NonPublic | BindingFlags.Instance) is null,
            "Contract form no longer requires a tariff row");
        var inactiveList = new PriceListItem(2, "Stopped", VnClock.Today, 0, 0, "Inactive");
        using var stoppedForm = new PriceListForm(api, inactiveList);
        Check(!Field<Button>(stoppedForm, "_save").Enabled && !Field<FlowLayoutPanel>(stoppedForm, "_fields").Enabled,
            "Inactive price-list form cannot be edited");
        using var copyForm = new PriceListForm(api, inactiveList, copy: true);
        Check(Field<Button>(copyForm, "_save").Enabled, "Copy inactive price list remains available");
        using var tariffEditor = new TariffRuleForm();
        Check(typeof(TariffRuleForm).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Count(f => f.FieldType == typeof(NumericUpDown)) == 2, "Tariff editor has only two prices");
        using var pricing = new PricingControl();
        Check(!Field<DataGridView>(pricing, "_rulesGrid").Columns.Cast<DataGridViewColumn>()
            .Any(c => c.DataPropertyName.StartsWith("Surcharge")), "Tariff grid has no fees");
        pricing.BindingContext = new BindingContext();
        var listsGrid = Field<DataGridView>(pricing, "_listsGrid");
        listsGrid.DataSource = new[] { inactiveList };
        listsGrid.CurrentCell = listsGrid.Rows[0].Cells[0];
        typeof(PricingControl).GetMethod("UpdateActions", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(pricing, null);
        Check(!Field<Button>(pricing, "_edit").Enabled && Field<Button>(pricing, "_copy").Enabled,
            "Pricing toolbar disables edit for an inactive list");
        listsGrid.DataSource = new[] { inactiveList with { Status = "Active" } };
        listsGrid.CurrentCell = listsGrid.Rows[0].Cells[0];
        typeof(PricingControl).GetMethod("UpdateActions", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(pricing, null);
        Check(Field<Button>(pricing, "_edit").Enabled, "Pricing toolbar allows editing an active list");
        var type = Field<ComboBox>(form, "_type");
        var discount = Field<TextBox>(form, "_discount");
        Check(type.Items.Count == 2 && type.SelectedIndex == -1 && !discount.Enabled, "Form has two explicit choices and disabled discount");
        type.SelectedIndex = 1;
        Check(discount.Enabled, "Long-term enables discount");
        discount.Text = "12.25"; type.SelectedIndex = 0;
        Check(!discount.Enabled && discount.Text == "", "One-time disables and clears discount");
        form.BindingContext = new BindingContext();
        Field<TextBox>(form, "_code").Text = "New";
        var customer = Field<ComboBox>(form, "_customer");
        customer.DisplayMember = nameof(ContractOption.Name); customer.ValueMember = nameof(ContractOption.Id);
        customer.DataSource = new[] { new ContractOption { Id = 1, Name = "Customer" } };
        customer.SelectedValue = 1;
        type.SelectedIndex = 1;
        foreach (var invalid in new[] { "", "abc", "101", "-1", 1.001m.ToString() })
        {
            discount.Text = invalid;
            ((Task)typeof(ContractForm).GetMethod("SaveAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(form, null)!).GetAwaiter().GetResult();
            Check(Field<Label>(form, "_status").Text.Contains("100%"), "Form rejects invalid/pasted discount " + invalid);
        }
        type.SelectedIndex = 0;
        ((Task)typeof(ContractForm).GetMethod("SaveAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(form, null)!).GetAwaiter().GetResult();
        Check(Field<Label>(form, "_status").Text.Contains("bảng giá đang áp dụng"), "Form rejects absent price list");
        using var details = new ContractForm(api, new ContractListItem { Id = 1, Code = "HD-001", CustomerId = 1, CustomerName = "Khách hàng", StartDate = DateTime.Today, Type = "LongTerm", DiscountPercent = 12.25m, PriceListId = 1, PriceListName = "Bảng giá công ty" }, true);
        details.Show(); details.PerformLayout();
        Check(!Field<ComboBox>(details, "_type").Enabled && !Field<TextBox>(details, "_discount").Enabled, "Details are read-only");
        Check(Field<TextBox>(details, "_discount").Text == 12.25m.ToString("0.##"), "Details show persisted discount");
        using var bitmap = new Bitmap(details.Width, details.Height);
        details.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
        bitmap.Save(Path.Combine(AppContext.BaseDirectory, "contract-preview.png"));
        Field<ComboBox>(details, "_type").SelectedIndex = 0;
        Check(Field<TextBox>(details, "_discount").Visible && !Field<TextBox>(details, "_discount").Enabled &&
            Field<TextBox>(details, "_discount").Text == "", "Discount remains visible, disabled and empty for one-time contracts");
        details.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
        bitmap.Save(Path.Combine(AppContext.BaseDirectory, "contract-discount-disabled-preview.png"));
        details.Close();
    }
}
