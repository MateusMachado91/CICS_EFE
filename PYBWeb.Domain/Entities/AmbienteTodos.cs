namespace PYBWeb.Domain.Entities;

/// <summary>
/// Entidade para controle de ambientes remotos FCT
/// Tabela: ambientetodos no banco ambiente.db
/// </summary>
public class AmbienteTodos
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Em_Todos { get; set; } // 1 = Sim (ativo), 0 = Não (inativo)
    
    /// <summary>
    /// Propriedade helper para verificar se está ativo
    /// </summary>
    public bool Ativo => Em_Todos == 1;
    
    /// <summary>
    /// Texto para exibição no dropdown
    /// </summary>
    public string DisplayText => $"{Nome} ({(Ativo ? "Sim" : "Não")})";
}