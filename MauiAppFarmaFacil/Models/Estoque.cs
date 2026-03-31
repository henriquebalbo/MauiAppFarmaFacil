using SQLite;

namespace MauiAppFarmaFacil.Models;

public class Estoque
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public int CodUnidade { get; set; } // FK para UnidadeDeSaude [cite: 22, 30]
    [Indexed]
    public int CodMedicamento { get; set; } // FK para Medicamento [cite: 22, 32]
    public int QuantidadeDisponivel { get; set; }
   
}