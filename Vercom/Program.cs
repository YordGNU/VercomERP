using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vercom.Filters;
using Vercom.Models;
using Vercom.Security;
using Vercom.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEntidadProvider, HttpContextEntidadProvider>();
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IFixedAssetService, FixedAssetService>();
builder.Services.AddScoped<IReceivablesPayablesService, ReceivablesPayablesService>();
builder.Services.AddScoped<IHRService, HRService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IHRReportService, HRReportService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IProductionService, ProductionService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<ICommercialService, CommercialService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IIntelligenceService, IntelligenceService>();
builder.Services.AddScoped<IPosService, PosService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IClosureService, ClosureService>();
builder.Services.AddScoped<ICashBankService, CashBankService>();
builder.Services.AddScoped<IParametroSistemaService, ParametroSistemaService>();
builder.Services.AddScoped<IConsecutivoService, ConsecutivoService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IExportService, ExportService>();

// Servicios POS (JWT / API móvil)
builder.Services.AddScoped<PosAuthService>();
builder.Services.AddScoped<PosCatalogoService>();
builder.Services.AddScoped<PosCajaService>();
builder.Services.AddScoped<PosSincronizacionService>();
builder.Services.AddScoped<PosSeguridadService>();
builder.Services.AddScoped<PosConfigService>();

builder.Services.AddSignalR();

// Workers de Fondo
builder.Services.AddHostedService<PosSyncBackgroundWorker>();
builder.Services.AddHostedService<BackupBackgroundWorker>();

// Seguridad avanzada
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

var jwtSecret = builder.Configuration["Jwt:SecretKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("Jwt:SecretKey debe tener al menos 32 bytes.");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("pos_gestionar_usuarios", policy => policy.RequireClaim("Permission", "gestionar_usuarios"));
    options.AddPolicy("pos_configuracion", policy => policy.RequireClaim("Permission", "configuracion"));
});

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<MustChangePasswordFilter>();
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.Initialize(services);
    await PosApiSeed.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<Vercom.Hubs.NotificationHub>("/notificationHub");

app.MapHealthChecks("/health");

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
