using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PYBWeb.Domain.Entities;
using PYBWeb.Domain.Interfaces;
using PYBWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PYBWeb.Infrastructure.Services;

/// <summary>
/// Serviço para gerenciar ambientes CICS
/// </summary>
public class AmbienteCicsService : IAmbienteCicsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AmbienteCicsService> _logger;

    public AmbienteCicsService(IConfiguration configuration, ILogger<AmbienteCicsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<AmbienteCics>> ObterAmbientesAtivosAsync()
    {
        try
        {
            var connectionString = ObterConnectionString();
            using var context = new AmbienteDbContext(connectionString);
            
            // Verificar se há dados, se não há, inicializar com dados de exemplo
            var count = await context.Ambientes.CountAsync();
            if (count == 0)
            {
                await InicializarDadosExemplo(context);
            }
            
            return await context.Ambientes
                .Where(a => a.Ativo)
                .OrderBy(a => a.Nome)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter ambientes ativos");
            return new List<AmbienteCics>();
        }
    }

    public async Task<IEnumerable<AmbienteCics>> ObterTodosAmbientesAsync()
    {
        try
        {
            var connectionString = ObterConnectionString();
            using var context = new AmbienteDbContext(connectionString);
            
            return await context.Ambientes
                .OrderBy(a => a.Nome)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todos os ambientes");
            return new List<AmbienteCics>();
        }
    }

    private async Task InicializarDadosExemplo(AmbienteDbContext context)
    {
        _logger.LogInformation("Inicializando dados de ambiente CICS no ambiente.db");
        
        var ambientesReais = new List<AmbienteCics>
        {
            // Produção - Máquina A
            new AmbienteCics { IdChave = "ACICSE", Nome = "ACICSE", Descricao = "Arquivos de MQS com DB2", Ambiente = "ESA", Maquina = "A", Sufixo = "E", Isc = "ISCE", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICSF", Nome = "ACICSF", Descricao = "Arquivos de Automação", Ambiente = "ESA", Maquina = "A", Sufixo = "F", Isc = "ISCF", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICSG", Nome = "ACICSG", Descricao = "BRM", Ambiente = "ESA", Maquina = "A", Sufixo = "G", Isc = "ISCG", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICSH", Nome = "ACICSH", Descricao = "BPH - BJR", Ambiente = "ESA", Maquina = "A", Sufixo = "H", Isc = "ISCH", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICSI", Nome = "ACICSI", Descricao = "BQR - CORRESPONDENTE BANCÁRIO", Ambiente = "ESA", Maquina = "A", Sufixo = "I", Isc = "ISCI", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICSJ", Nome = "ACICSJ", Descricao = "Projeto Master Card", Ambiente = "ESA", Maquina = "A", Sufixo = "J", Isc = "ISCJ", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICSM", Nome = "ACICSM", Descricao = "Arquivos de SPB", Ambiente = "ESA", Maquina = "A", Sufixo = "M", Isc = "ISCM", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICSN", Nome = "ACICSN", Descricao = "BMQ", Ambiente = "ESA", Maquina = "A", Sufixo = "N", Isc = "ISCN", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICSO", Nome = "ACICSO", Descricao = "BRI", Ambiente = "ESA", Maquina = "A", Sufixo = "O", Isc = "ISCO", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICS", Nome = "ACICS", Descricao = "Automação e Tele-Serviços", Ambiente = "ESA", Maquina = "A", Sufixo = "1", Isc = "ISC1", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICS2", Nome = "ACICS2", Descricao = "Automação e Tele-Serviços", Ambiente = "ESA", Maquina = "A", Sufixo = "2", Isc = "ISC2", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICS6", Nome = "ACICS6", Descricao = "PHA", Ambiente = "ESA", Maquina = "A", Sufixo = "6", Isc = "ISC6", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICS7", Nome = "ACICS7", Descricao = "BCC", Ambiente = "ESA", Maquina = "A", Sufixo = "7", Isc = "AIS7", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICS8", Nome = "ACICS8", Descricao = "BFQ, BVJ, RVA, ...", Ambiente = "ESA", Maquina = "A", Sufixo = "8", Isc = "ISC8", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "ACICS9", Nome = "ACICS9", Descricao = "BAL, BIC, BNB, BPL, ...", Ambiente = "ESA", Maquina = "A", Sufixo = "9", Isc = "ISC9", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            
            // Produção - Máquina B
            new AmbienteCics { IdChave = "BCICS5", Nome = "BCICS5", Descricao = "BGW", Ambiente = "ESA", Maquina = "B", Sufixo = "5", Isc = "ISC5", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            
            // Produção - Máquina C
            new AmbienteCics { IdChave = "CCICS3", Nome = "CCICS3", Descricao = "Conversões IDMS/CICS", Ambiente = "MVS", Maquina = "B", Sufixo = "3", Isc = "ISC3", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            new AmbienteCics { IdChave = "CCICS4", Nome = "CCICS4", Descricao = "DB2,BLU, PZY, ...", Ambiente = "ESA", Maquina = "A", Sufixo = "4", Isc = "ISC4", SteplibCsd = "SYS3.PYB.SYSS.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSS.V74.DFHCSD.RLSP", Ativo = true },
            
            // Desenvolvimento - Máquina D
            new AmbienteCics { IdChave = "DCICS", Nome = "DCICS", Descricao = "Desenvolvimento", Ambiente = "ESA", Maquina = "D", Sufixo = "D", Isc = "ISCD", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICS7", Nome = "DCICS7", Descricao = "Dsenvolvimento - Arquivos", Ambiente = "ESA", Maquina = "D", Sufixo = "7", Isc = "ISC7", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICSE", Nome = "DCICSE", Descricao = "Arquivos de MQS com DB2", Ambiente = "ESA", Maquina = "D", Sufixo = "E", Isc = "DISE", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICSF", Nome = "DCICSF", Descricao = "Arquivos de Automação", Ambiente = "ESA", Maquina = "D", Sufixo = "F", Isc = "DISF", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICSG", Nome = "DCICSG", Descricao = "BRM", Ambiente = "ESA", Maquina = "D", Sufixo = "G", Isc = "DISG", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICSH", Nome = "DCICSH", Descricao = "Desenvolvimento (BPH - BJR)", Ambiente = "ESA", Maquina = "D", Sufixo = "H", Isc = "DISH", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICSI", Nome = "DCICSI", Descricao = "Desenvolvimento(BQR-CORRESPONDENTE BANCÁRIO)", Ambiente = "ESA", Maquina = "D", Sufixo = "I", Isc = "DISI", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICSJ", Nome = "DCICSJ", Descricao = "Projeto Master Card", Ambiente = "ESA", Maquina = "D", Sufixo = "J", Isc = "DISJ", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICSM", Nome = "DCICSM", Descricao = "Arquivos de SPB", Ambiente = "ESA", Maquina = "D", Sufixo = "M", Isc = "DISM", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICSN", Nome = "DCICSN", Descricao = "BMQ", Ambiente = "ESA", Maquina = "D", Sufixo = "N", Isc = "DISN", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICSO", Nome = "DCICSO", Descricao = "BRI", Ambiente = "ESA", Maquina = "D", Sufixo = "O", Isc = "DISO", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICS2", Nome = "DCICS2", Descricao = "Automação e Tele-Serviços", Ambiente = "ESA", Maquina = "D", Sufixo = "2", Isc = "DIS2", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICS8", Nome = "DCICS8", Descricao = "BFQ, BVJ, RVA, ...", Ambiente = "ESA", Maquina = "D", Sufixo = "8", Isc = "DIS8", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICS9", Nome = "DCICS9", Descricao = "BAL, BIC, BNB, BPL, ...", Ambiente = "ESA", Maquina = "D", Sufixo = "9", Isc = "DIS9", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICS5", Nome = "DCICS5", Descricao = "BGW", Ambiente = "ESA", Maquina = "D", Sufixo = "5", Isc = "DIS5", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICS3", Nome = "DCICS3", Descricao = "Conversões IDMS/CICS", Ambiente = "MVS", Maquina = "D", Sufixo = "3", Isc = "DIS3", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICS4", Nome = "DCICS4", Descricao = "DB2,BLU, PZY, ...", Ambiente = "ESA", Maquina = "D", Sufixo = "4", Isc = "DIS4", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true },
            new AmbienteCics { IdChave = "DCICS6", Nome = "DCICS6", Descricao = "PHA", Ambiente = "ESA", Maquina = "D", Sufixo = "6", Isc = "DIS6", SteplibCsd = "SYS3.PYB.SYSD.V74.SDFHLOAD", DsnameDfhcsd = "SYS3.PYB.SYSD.V74DFHCSD.RLSD", Ativo = true }
        };
        
        context.Ambientes.AddRange(ambientesReais);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Dados reais de ambiente CICS inicializados com sucesso - {Count} ambientes inseridos", ambientesReais.Count);
    }

    public async Task<AmbienteCics?> ObterAmbientePorIdAsync(int id)
    {
        try
        {
            var connectionString = ObterConnectionString();
            using var context = new AmbienteDbContext(connectionString);
            
            return await context.Ambientes
                .FirstOrDefaultAsync(a => a.Id == id && a.Ativo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter ambiente por ID: {Id}", id);
            return null;
        }
    }

    public async Task<AmbienteCics?> ObterAmbientePorNomeAsync(string nome)
    {
        try
        {
            var connectionString = ObterConnectionString();
            using var context = new AmbienteDbContext(connectionString);
            
            return await context.Ambientes
                .FirstOrDefaultAsync(a => a.Nome.ToLower() == nome.ToLower() && a.Ativo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter ambiente por nome: {Nome}", nome);
            return null;
        }
    }

    public async Task<AmbienteCics? > ObterPorNomeAsync(string idChave)
{
    try
    {
        var connectionString = ObterConnectionString();
        using var context = new AmbienteDbContext(connectionString);
        
        // Busca por IdChave (case-insensitive)
        return await context.Ambientes
            .FirstOrDefaultAsync(a => a.IdChave.ToLower() == idChave.ToLower());
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao obter ambiente por IdChave: {IdChave}", idChave);
        return null;
    }
}

    public async Task<List<AmbienteCics>> ObterAmbientesPxuPxsAsync()
    {
        try
        {
            using var context = new AmbienteDbContext(ObterConnectionString());
            
            // Lista dos ambientes que fazem parte do "TODOS" para PXU/PXS
            var ambientesPxuPxs = new[]
            {
                "ACICSE", "ACICSF", "ACICSG", "ACICSH", "ACICSI", "ACICSJ",
                "ACICSM", "ACICSN", "ACICSO", "ACICS", "ACICS2", "ACICS8",
                "ACICS9", "BCICS5", "CCICS3", "CCICS4", "ACICS6", "ACICS7"
            };
            
            return await context.Ambientes
                .Where(a => a.Ativo && ambientesPxuPxs.Contains(a.IdChave))
                .OrderBy(a => a.IdChave)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter ambientes PXU/PXS");
            return new List<AmbienteCics>();
        }
    }

    public async Task<bool> CriarAmbienteAsync(AmbienteCics novoAmbiente)
    {
        try
        {
            using var context = new AmbienteDbContext(ObterConnectionString());
            context.Ambientes.Add(novoAmbiente);
            await context.SaveChangesAsync();
            _logger.LogInformation("Novo ambiente criado com sucesso: {Nome}", novoAmbiente.Nome);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar ambiente: {Nome}", novoAmbiente.Nome);
            return false;
        }
    }

    public async Task<bool> ExcluirAmbienteAsync(int id)
    {
        try
        {
            using var context = new AmbienteDbContext(ObterConnectionString());
            var ambiente = await context.Ambientes.FindAsync(id);
            if (ambiente == null)
                return false;

            context.Ambientes.Remove(ambiente);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            // logar erro se quiser
            return false;
        }
    }


    public async Task<bool> AtualizarAmbienteAsync(AmbienteCics ambiente)
    {
        try
        {
            using var context = new AmbienteDbContext(ObterConnectionString());
            
            var ambienteExistente = await context.Ambientes.FindAsync(ambiente.Id);
            if (ambienteExistente == null)
            {
                _logger.LogWarning("Ambiente não encontrado para atualização: {Id}", ambiente.Id);
                return false;
            }

            // Atualizar campos
            ambienteExistente.Nome = ambiente.Nome;
            ambienteExistente.Descricao = ambiente.Descricao;
            ambienteExistente.Ambiente = ambiente.Ambiente;
            ambienteExistente.Maquina = ambiente.Maquina;
            ambienteExistente.Sufixo = ambiente.Sufixo;
            ambienteExistente.Isc = ambiente.Isc;
            ambienteExistente.SteplibCsd = ambiente.SteplibCsd;
            ambienteExistente.DsnameDfhcsd = ambiente.DsnameDfhcsd;
            ambienteExistente.Servidor = ambiente.Servidor;
            ambienteExistente.Porta = ambiente.Porta;
            ambienteExistente.Ativo = ambiente.Ativo;

            await context.SaveChangesAsync();
            _logger.LogInformation("Ambiente atualizado com sucesso: {Id} - {Nome}", ambiente.Id, ambiente.Nome);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar ambiente: {Id}", ambiente.Id);
            return false;
        }
    }

    public async Task<bool> AlterarStatusAmbienteAsync(int id, bool ativo)
    {
        try
        {
            using var context = new AmbienteDbContext(ObterConnectionString());
            
            var ambiente = await context.Ambientes.FindAsync(id);
            if (ambiente == null)
            {
                _logger.LogWarning("Ambiente não encontrado para alteração de status: {Id}", id);
                return false;
            }

            ambiente.Ativo = ativo;
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Status do ambiente alterado: {Id} - {Nome} -> {Ativo}", id, ambiente.Nome, ativo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status do ambiente: {Id}", id);
            return false;
        }
    }

    private string ObterConnectionString()
    {
        var pastaData = _configuration.GetValue<string>("PastaData") ?? ".\\DATA_PYB\\";
        var caminhoCompleto = Path.GetFullPath(Path.Combine(pastaData, "ambiente.db"));
        return $"Data Source={caminhoCompleto}";
    }
}