using RetireBuddy.Components;
using RetireBuddy.Context;
using RetireBuddy.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register planning components
builder.Services.AddScoped<PlanningContext>();
builder.Services.AddScoped<TaxCalculationService>();
builder.Services.AddScoped<SocialSecurityService>();
builder.Services.AddScoped<IrmaaService>();
builder.Services.AddScoped<PlanningProtocol>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
