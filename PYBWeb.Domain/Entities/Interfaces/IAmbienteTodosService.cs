using PYBWeb.Domain.Entities;

namespace PYBWeb.Domain.Interfaces;

/// <summary>
/// Interface para serviços de ambientes remotos FCT
/// </summary>
public interface IAmbienteTodosService
{

    
    /// <summary>
    /// Obtém todos os ambientes da tabela ambientetodos
    /// </summary>
    /// <returns>Lista de ambientes</returns>
    Task<List<AmbienteTodos>> ObterTodosAmbientesAsync();
    
    /// <summary>
    /// Obtém apenas os ambientes ativos (EmTodos = 1)
    /// </summary>
    /// <returns>Lista de ambientes ativos</returns>
    Task<List<AmbienteTodos>> ObterAmbientesAtivosAsync();
    
    /// <summary>
    /// Obtém um ambiente por ID
    /// </summary>
    /// <param name="id">ID do ambiente</param>
    /// <returns>Ambiente encontrado ou null</returns>
    Task<AmbienteTodos?> ObterAmbientePorIdAsync(int id);
    
    /// <summary>
    /// Obtém um ambiente por nome
    /// </summary>
    /// <param name="nome">Nome do ambiente</param>
    /// <returns>Ambiente encontrado ou null</returns>
    Task<AmbienteTodos?> ObterAmbientePorNomeAsync(string nome);

    /// <summary>
    /// Atualiza um ambiente
    /// </summary>
    /// <param name="ambiente">Ambiente a ser atualizado</param>
    /// <returns>True se atualizado com sucesso</returns>
    Task<bool> AtualizarAmbienteAsync(AmbienteTodos ambiente);

    /// <summary>
    /// Altera o status Em_Todos de um ambiente
    /// </summary>
    /// <param name="id">ID do ambiente</param>
    /// <param name="emTodos">Novo valor (1 = Sim, 0 = Não)</param>
    /// <returns>True se alterado com sucesso</returns>
    Task<bool> AlterarStatusAsync(int id, int emTodos);

    Task<bool> CriarAmbienteTodosAsync(AmbienteTodos novoAmbiente);

    Task<bool> ExcluirAmbienteTodosAsync(int id);
}