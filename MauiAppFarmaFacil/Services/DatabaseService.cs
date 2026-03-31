using SQLite;
using MauiAppFarmaFacil.Models;

namespace MauiAppFarmaFacil.Services;
public class DatabaseService
{
    private SQLiteAsyncConnection _db;

    async Task Init()
    {
        if (_db is not null) return;
        var path = Path.Combine(FileSystem.AppDataDirectory, "FarmaFacil.db3");
        _db = new SQLiteAsyncConnection(path);

        await _db.CreateTableAsync<Cidadao>();
        await _db.CreateTableAsync<Medicamento>();
        await _db.CreateTableAsync<Estoque>();
        await _db.CreateTableAsync<UnidadeDeSaude>();
    }

    // Caso de Uso: buscarMedicamentoPorNome 
    public async Task<List<Medicamento>> BuscarMedicamentos(string nome)
    {
        await Init();
        return await _db.Table<Medicamento>()
                        .Where(m => m.Nome.Contains(nome))
                        .ToListAsync();
    }
}