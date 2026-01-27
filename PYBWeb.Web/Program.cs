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
    options. AddPolicy("RequireAdmin", policy =>
        policy.Requirements.Add(new AdminRequirement()));
});

builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, AdminAuthorizationHandler>();

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ColaboradorService>();
builder.Services.AddScoped<SuporteService>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registrar ICurrentUserService após o pipeline de componentes do Blazor
builder.Services.AddScoped<ICurrentUserService, UserService>();

builder.Services.AddScoped<IJclGeneratorService, JclGeneratorService>();

// =====================================================================
// ⚡ ADICIONAR INFRAESTRUTURA (DbContexts + Serviços) ⚡
// =====================================================================
builder.Services.AddInfrastructure(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();