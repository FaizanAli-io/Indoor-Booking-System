using Azure.Identity;
using IndoorBookingSystem.Data;
using IndoorBookingSystem.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
Console.WriteLine($"KeyVault URI: {keyVaultUri}");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

if (!string.IsNullOrEmpty(keyVaultUri))
{
    try
    {
        // Use ManagedIdentityCredential in Azure, DefaultAzureCredential for local dev
        Azure.Core.TokenCredential credential = builder.Environment.IsDevelopment() 
            ? new DefaultAzureCredential() 
            : new ManagedIdentityCredential();
        
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            credential);
        Console.WriteLine("Key Vault loaded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Key Vault error: {ex.Message}");
    }
}

// Add Azure AD Authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.GetSection("AzureAd").Bind(options);
        // Get ClientSecret from Key Vault
        options.ClientSecret = builder.Configuration["AzureAd-ClientSecret"];
    });

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BlobStorageService>();
builder.Services.AddRazorPages();

// Add DbContext
var cosmos = builder.Configuration.GetSection("Cosmos");
var accountEndpoint = cosmos["AccountEndpoint"] ?? throw new InvalidOperationException("Cosmos AccountEndpoint missing");

// Debug: Check all configuration keys
Console.WriteLine("All configuration keys containing 'Cosmos':");
foreach (var key in builder.Configuration.AsEnumerable())
{
    if (key.Key.Contains("Cosmos", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"  {key.Key} = {(key.Value?.Length > 0 ? "***" : "null")}");
    }
}

var accountKey = builder.Configuration["CosmosDB-AccountKey"] ?? throw new InvalidOperationException("Cosmos AccountKey missing");
Console.WriteLine($"CosmosDB:AccountKey found: {!string.IsNullOrEmpty(accountKey)}");
var dbName = cosmos["DatabaseName"] ?? throw new InvalidOperationException("Cosmos DatabaseName missing");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseCosmos(accountEndpoint, accountKey, dbName));

// ✅ Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(1);
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
await app.RunAsync();
