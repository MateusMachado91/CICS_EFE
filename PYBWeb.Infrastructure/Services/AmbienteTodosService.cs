using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using PYBWeb.Domain.Entities;
using PYBWeb.Domain.Interfaces;


namespace PYBWeb.Infrastructure.Services;

/// <summary>
/// Serviço para gerenciar ambientes remotos FCT
/// 🗄️ Acessa diretamente a tabela ambientetodos no banco ambiente.db
/// </summary>
public class AmbienteTodosService : IAmbienteTodosService
{
    private readonly string _connectionString;

    public AmbienteTodosService(IConfiguration configuration)
    {
        // Tentar obter do ConnectionStrings primeiro
        _connectionString = configuration.GetConnectionString("Ambiente") ?? "";
        
        // Se não encontrar, montar o caminho manualmente
        if (string.IsNullOrEmpty(_connectionString))
        {
            var pastaData = configuration. GetValue<string>("PastaData") ?? "..\\DATA_PYB";
            var pastaDataCompleta = Path. GetFullPath(Path.Combine(AppContext.BaseDirectory, pastaData));
            _connectionString = $"Data Source={Path.Combine(pastaDataCompleta, "ambiente.db")}";
        }
    }

    public async Task<List<AmbienteTodos>> ObterTodosAmbientesAsync()
    {
        var ambientes = new List<AmbienteTodos>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT id, nome, em_todos 
            FROM ambientetodos 
            ORDER BY nome";

        using var command = new SqliteCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            ambientes.Add(new AmbienteTodos
            {
                Id = reader.GetInt32(0), // id
                Nome = reader.GetString(1), // nome
                Em_Todos = reader.GetInt32(2) // em_todos
            });
        }

        return ambientes;
    }


    public async Task<bool> ExcluirAmbienteTodosAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "DELETE FROM ambientetodos WHERE id = @id";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        var linhasAfetadas = await command.ExecuteNonQueryAsync();
        return linhasAfetadas > 0;
    }

    public async Task<bool> CriarAmbienteTodosAsync(AmbienteTodos novoAmbiente)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO ambientetodos (nome, em_todos)
            VALUES (@nome, @emTodos)
        ";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@nome", novoAmbiente.Nome);
        command.Parameters.AddWithValue("@emTodos", novoAmbiente.Em_Todos);

        var linhasAfetadas = await command.ExecuteNonQueryAsync();
        return linhasAfetadas > 0;
    }

    public async Task<List<AmbienteTodos>> ObterAmbientesAtivosAsync()
    {
        var ambientes = new List<AmbienteTodos>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT id, nome, em_todos 
            FROM ambientetodos 
            WHERE em_todos = 1
            ORDER BY nome";

        using var command = new SqliteCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            ambientes.Add(new AmbienteTodos
            {
                Id = reader.GetInt32(0), // id
                Nome = reader.GetString(1), // nome
                Em_Todos = reader.GetInt32(2) // em_todos
            });
        }

        return ambientes;
    }

    public async Task<AmbienteTodos?> ObterAmbientePorIdAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT id, nome, em_todos 
            FROM ambientetodos 
            WHERE id = @id";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new AmbienteTodos
            {
                Id = reader.GetInt32(0), // id
                Nome = reader.GetString(1), // nome
                Em_Todos = reader.GetInt32(2) // em_todos
            };
        }

        return null;
    }

    public async Task<AmbienteTodos?> ObterAmbientePorNomeAsync(string nome)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT id, nome, em_todos 
            FROM ambientetodos 
            WHERE nome = @nome COLLATE NOCASE";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@nome", nome);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new AmbienteTodos
            {
                Id = reader.GetInt32(0), // id
                Nome = reader.GetString(1), // nome
                Em_Todos = reader.GetInt32(2) // em_todos
            };
        }

        return null;
    }

    public async Task<bool> AtualizarAmbienteAsync(AmbienteTodos ambiente)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            UPDATE ambientetodos 
            SET nome = @nome, em_todos = @emTodos 
            WHERE id = @id";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@id", ambiente.Id);
        command.Parameters.AddWithValue("@nome", ambiente.Nome);
        command.Parameters.AddWithValue("@emTodos", ambiente.Em_Todos);

        var linhasAfetadas = await command.ExecuteNonQueryAsync();
        return linhasAfetadas > 0;
    }

    public async Task<bool> AlterarStatusAsync(int id, int emTodos)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            UPDATE ambientetodos 
            SET em_todos = @emTodos 
            WHERE id = @id";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@emTodos", emTodos);

        var linhasAfetadas = await command.ExecuteNonQueryAsync();
        return linhasAfetadas > 0;
    }

}