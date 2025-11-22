using Azure.Identity;
using IndoorBookingSystem.Data;
using IndoorBookingSystem.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new ManagedIdentityCredential());
}

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BlobStorageService>();
builder.Services.AddRazorPages();

// Add DbContext
var cosmos = builder.Configuration.GetSection("Cosmos");
var accountEndpoint = cosmos["AccountEndpoint"] ?? throw new InvalidOperationException("Cosmos AccountEndpoint missing");
var accountKey = builder.Configuration["CosmosDB:AccountKey"] ?? throw new InvalidOperationException("Cosmos AccountKey missing");
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
app.UseAuthorization();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapRazorPages();
await app.RunAsync();
