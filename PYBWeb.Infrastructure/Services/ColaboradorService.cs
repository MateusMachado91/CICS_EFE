using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PYBWeb.Infrastructure.Data;
using PYBWeb.Domain.Interfaces; // para ILogService
using PYBWeb.Domain.Entities;

public class ColaboradorService
{
    private readonly ColaboradoresDbContext _context;
    private readonly ILogService _logService;

    public ColaboradorService(ColaboradoresDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<List<Colaborador>> ObterTodosAsync()
    {
        // Não rastrear a lista de resultados evita conflitos de tracking em operações subsequentes
        return await _context.Colaboradores
                             .AsNoTracking()
                             .OrderBy(c => c.Nome)
                             .ToListAsync();
    }

    public async Task<Colaborador?> ObterPorMatriculaAsync(string matricula)
    {
        return await _context.Colaboradores.FindAsync(matricula);
    }

    public async Task AdicionarAsync(Colaborador colaborador)
    {
        if (colaborador == null) throw new ArgumentNullException(nameof(colaborador));

        _context.Colaboradores.Add(colaborador);
        await _context.SaveChangesAsync();

        await _logService.RegistrarColaboradorAsync("CRIAR", colaborador);
    }

    public async Task AtualizarAsync(Colaborador colaborador)
    {
        if (colaborador == null) throw new ArgumentNullException(nameof(colaborador));

        // Carrega a entidade já rastreada (se existir) e aplica os valores,
        // evitando anexar uma segunda instância com a mesma chave.
        var existente = await _context.Colaboradores.FindAsync(colaborador.Matricula);

        if (existente == null)
        {
            // Se não existir, tratar como criação (alternativa: lançar erro)
            await AdicionarAsync(colaborador);
            return;
        }

        // Atualiza propriedades escalares do objeto rastreado.
        // Atenção: navegações/coleções devem ser tratadas separadamente, se necessário.
        _context.Entry(existente).CurrentValues.SetValues(colaborador);

        await _context.SaveChangesAsync();

        await _logService.RegistrarColaboradorAsync("EDITAR", colaborador);

    }

    public async Task ExcluirAsync(string matricula)
    {
        var colaborador = await _context.Colaboradores.FindAsync(matricula);
        if (colaborador != null)
        {
            // guarda dados para o log antes de remover
            var detalhes = $"Nome={colaborador.Nome};Email={colaborador.Email};Setor={colaborador.Setor}";

            _context.Colaboradores.Remove(colaborador);
            await _context.SaveChangesAsync();
            await _logService.RegistrarColaboradorAsync("EXCLUIR", colaborador);
        }
    }

    public string ObterNomePorMatricula(string matricula)
    {
        var colaborador = _context.Colaboradores.FirstOrDefault(c => c.Matricula == matricula);
        return colaborador?.Nome ?? matricula;
    }
}