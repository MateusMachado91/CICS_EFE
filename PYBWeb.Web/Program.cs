using PYBWeb.Web.Components;
using Microsoft.EntityFrameworkCore;
using PYBWeb.Infrastructure.Data;
using PYBWeb.Infrastructure.Services;
using PYBWeb.Domain.Entities;
using PYBWeb.Infrastructure;
using PYBWeb.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using PYBWeb.Web.Authorization;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// SISTEMA DE SEGURANÇA - AUTENTICAÇÃO WINDOWS + AUTORIZAÇÃO POR BD
// =====================================================================
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.Requirements.Add(new AdminRequirement()));
});

builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, AdminAuthorizationHandler>();

// 🔐 Serviços de Autenticação Blazor
builder.Services.AddSingleton<IBlazorAuthenticationService, BlazorAuthenticationService>();

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ICurrentUserService>(sp => 
    new UserService(
        sp.GetRequiredService<IHttpContextAccessor>(),
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<IBlazorAuthenticationService>()
    ));

builder.Services.AddInfrastructure(builder.Configuration); 
builder.Services.AddScoped<ColaboradorService>();
builder.Services.AddScoped<SuporteService>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ⚡ Registrar CircuitHandler para capturar autenticação no WebSocket
builder.Services.AddScoped<AuthenticationCircuitHandler>();
builder.Services.AddScoped<IJclGeneratorService, JclGeneratorService>();

// =====================================================================
// ⚡ ADICIONAR INFRAESTRUTURA (DbContexts + Serviços) ⚡
// =====================================================================


// Configurar logging
builder.Services. AddLogging();

var app = builder.Build();

// --- APLICAR MIGRATIONS / CRIAR ESQUEMA AQUI, ANTES DO app.Run() ---
using (var scope = app.Services. CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetService<ILogger<Program>>();

    // 📝 BANCO DE LOGS
    try
    {
        var logDb = services.GetRequiredService<LogDbContext>();
        logger?. LogInformation("🔍 Verificando banco de logs...");
        logDb.Database.EnsureCreated();
        logger?.LogInformation("✅ Banco de logs OK.");
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "❌ Erro no banco de logs:  {Message}", ex.Message);
    }

    // 👥 BANCO DE COLABORADORES
    try
    {
        var colDb = services.GetRequiredService<ColaboradoresDbContext>();
        logger?.LogInformation("🔍 Verificando banco de colaboradores...");
        colDb.Database.EnsureCreated();
        logger?.LogInformation("✅ Banco de colaboradores OK.");
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "❌ Erro no banco de colaboradores: {Message}", ex.Message);
    }
}
// -----------------------------------------------------

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 🔐 Middleware de Preservação de Autenticação para WebSocket
app.UseMiddleware<AuthenticationPreservationMiddleware>();

app.UseAntiforgery();
app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();