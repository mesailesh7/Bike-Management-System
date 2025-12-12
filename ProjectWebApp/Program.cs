using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using ProjectWebApp.Components;
using ProjectWebApp.Components.Account;
using ProjectWebApp.Data;
using ProjectWebApp.Services;
using SalesReturnsSystem.BLL;
using SalesReturnsSystem.DAL;





var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<eBike_2025Context>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("eBike_2025")));

builder.Services.AddScoped<SalesReturnsSystem.BLL.ReturnService>();
builder.Services.AddScoped<SalesReturnsSystem.BLL.ReturnTransactionService>();


// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

//builder.Services.AddAuthorization();

// Main connection string
var connectionStringEBike = builder.Configuration.GetConnectionString("eBikeDB");
// Auth Connection String
var connectionStringAuth = builder.Configuration.GetConnectionString("AuthDB");

// Register services for PurchasingSystem
PurchasingSystem.eBikeServiceExtension.AddBackendDependencies(builder.Services,
    options => options.UseSqlServer(connectionStringEBike));

// Register services for ReceivingSystem
ReceivingSystem.EBikeServiceExtension.AddBackendDependencies(builder.Services,
    options => options.UseSqlServer(connectionStringEBike));

// Register services for SalesReturnsSystem
SalesReturnsSystem.eBikeServiceExtension.AddBackendDependencies(builder.Services,
    options => options.UseSqlServer(connectionStringEBike));

// Register services for ServicingSystem
ServicingSystem.eBikeServiceExtension.AddBackendDependencies(builder.Services,
    options => options.UseSqlServer(connectionStringEBike));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionStringAuth));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    //Adding the role manager must come before the AddEntityFrameworkStores or this will fail
    //Modified for DMIT2018
    .AddRoles<IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    //.AddRoleStore<RoleStore<IdentityRole, ApplicationDbContext>>()
    .AddClaimsPrincipalFactory<eBikeClaimsPrincipalFactory>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddScoped<IClaimsTransformation, ProjectWebApp.Security.CustomClaimsTransformer>();

// Custom services for Sales subsystem
builder.Services.AddScoped<ProjectWebApp.Services.SalesService>();
builder.Services.AddScoped<ProjectWebApp.Services.SalesStateService>();

builder.Services.AddAuthorization(options =>
{
    // Receiving: Admin or Receiving or Store Staff or Parts Manager 
    options.AddPolicy("ReceivingPolicy", p =>
        p.RequireRole("Admin", "Receiving", "Store Staff", "Parts Manager"));

    // Sales & Returns: Admin or Sales Manager or Store Staff or Salesperson
    options.AddPolicy("SalesReturnsPolicy", p =>
        p.RequireRole("Admin", "Sales Manager", "Store Staff", "Salesperson"));

});



var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
