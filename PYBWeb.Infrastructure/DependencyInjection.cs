using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PYBWeb.Domain.Interfaces;
using PYBWeb.Infrastructure.Data;
using PYBWeb.Infrastructure.Services;

namespace PYBWeb.Infrastructure;

/// <summary>
/// Configuração da injeção de dependência da infraestrutura
/// ⚡ PROJETO CONFIGURADO PARA USAR SQLITE NA PASTA DATA ⚡
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // =====================================================================
        // 🗄️ CONFIGURAÇÃO SQLITE - CAMINHO RELATIVO À APLICAÇÃO
        // =====================================================================
        
        // Obter caminho da pasta DATA do appsettings.json
        var pastaDataConfig = configuration.GetValue<string>("PastaData") ?? ".. \\DATA_PYB";
        
        // Resolver caminho relativo a partir do diretório base da aplicação
        var pastaDataCompleta = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, pastaDataConfig)
        );
        
        // Garantir que as pastas existam
        if (!Directory.Exists(pastaDataCompleta))
        {
            Console.WriteLine($"⚠️ Pasta DATA não encontrada: {pastaDataCompleta}");
            Console.WriteLine($"📁 Criando pasta DATA:  {pastaDataCompleta}");
            Directory.CreateDirectory(pastaDataCompleta);
        }

        // Garantir que a pasta LOG existe
        var pastaLog = Path.Combine(pastaDataCompleta, "LOG");
        if (!Directory.Exists(pastaLog))
        {
            Console.WriteLine($"📁 Criando pasta LOG: {pastaLog}");
            Directory.CreateDirectory(pastaLog);
        }

        // Garantir que a pasta JCL existe
        var pastaJcl = Path.Combine(pastaDataCompleta, "JCL");
        if (!Directory.Exists(pastaJcl))
        {
            Console.WriteLine($"📁 Criando pasta JCL: {pastaJcl}");
            Directory.CreateDirectory(pastaJcl);
        }

        // Connection strings para cada banco SQLite
        var dados2025ConnectionString = $"Data Source={Path.Combine(pastaDataCompleta, "dados2025.db")}";
        var ambienteConnectionString = $"Data Source={Path. Combine(pastaDataCompleta, "ambiente.db")}";
        var colaboradoresConnectionString = $"Data Source={Path. Combine(pastaDataCompleta, "colaboradores.db")}";
        var logsConnectionString = $"Data Source={Path. Combine(pastaLog, "logs.db")}";

        // =====================================================================
        // ⚡ CONFIGURAR CONNECTION STRINGS NO CONFIGURATION (PRIMEIRO!)
        // =====================================================================
        configuration["ConnectionStrings:Dados2025"] = dados2025ConnectionString;
        configuration["ConnectionStrings: Ambiente"] = ambienteConnectionString;
        configuration["ConnectionStrings:Colaboradores"] = colaboradoresConnectionString;
        configuration["ConnectionStrings:Logs"] = logsConnectionString;

        // Log das connection strings para debug
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"📁 Diretório Base:  {AppContext.BaseDirectory}");
        Console.WriteLine($"📁 Configuração PastaData: {pastaDataConfig}");
        Console.WriteLine($"📁 Pasta DATA Resolvida: {pastaDataCompleta}");
        Console.WriteLine($"📊 Dados2025 DB: {dados2025ConnectionString}");
        Console.WriteLine($"🌍 Ambiente DB: {ambienteConnectionString}");
        Console.WriteLine($"👥 Colaboradores DB: {colaboradoresConnectionString}");
        Console.WriteLine($"📝 Logs DB: {logsConnectionString}");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        // =====================================================================
        // 📦 REGISTRO DOS DbContexts
        // =====================================================================
                // Registro do DbContext de Logs
        services.AddDbContext<LogDbContext>(options =>
            options. UseSqlite(logsConnectionString));
            
        // Registro do DbContext de Colaboradores
        services.AddDbContext<ColaboradoresDbContext>(options =>
            options. UseSqlite(colaboradoresConnectionString));

        // =====================================================================
        // 📦 REGISTRO DOS SERVIÇOS (DEPOIS das connection strings!)
        // =====================================================================
        
        services.AddScoped<ISolicitacoesCics2025Service, SolicitacoesCics2025Service>();
        services.AddScoped<IAmbienteCicsService, AmbienteCicsService>();
        services.AddScoped<IAmbienteTodosService, AmbienteTodosService>();
        services.AddScoped<ILogService, LogService>();

        return services;
    }
}