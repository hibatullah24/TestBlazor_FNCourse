using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using TestBlazor_FNCourse;
using TestBlazor_FNCourse.Components;
using TestBlazor_FNCourse.Data;
using TestBlazor_FNCourse.Data.Services;
using TestBlazor_FNCourse.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddCors();

builder.Services.AddSingleton<AppAuthState>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<AppAuthState>());
builder.Services.AddAuthenticationCore();



builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped(typeof(LoggingService<>));

var app = builder.Build();

app.UseCors(policy => policy
   .AllowAnyOrigin()
   .AllowAnyMethod()
   .AllowAnyHeader());

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
