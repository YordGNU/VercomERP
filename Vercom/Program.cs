using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vercom.Models;
using Vercom.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Vercom.Filters;
using Microsoft.AspNetCore.Authorization;
using Vercom.Security;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IFinancialReportService, FinancialReportService>();
builder.Services.AddScoped<IFixedAssetService, FixedAssetService>();
builder.Services.AddScoped<IReceivablesPayablesService, ReceivablesPayablesService>();
builder.Services.AddScoped<IHRService, HRService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IHRReportService, HRReportService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IProductionService, ProductionService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IBIService, BIService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IClosureService, ClosureService>();
builder.Services.AddScoped<ICashBankService, CashBankService>();
builder.Services.AddScoped<IParametroSistemaService, ParametroSistemaService>();
builder.Services.AddScoped<IConsecutivoService, ConsecutivoService>();

// Workers de Fondo
builder.Services.AddHostedService<PosSyncBackgroundWorker>();

// Seguridad avanzada
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<MustChangePasswordFilter>();
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
