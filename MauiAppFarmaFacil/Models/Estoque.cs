using SQLite;

namespace MauiAppFarmaFacil.Models;

/// <summary>
/// Classe associativa que resolve a relação N:N entre
/// UnidadeDeSaude e Medicamento.
/// </summary>
public class Estoque
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CodUnidade { get; set; }       // FK → UnidadeDeSaude

    [Indexed]
    public int CodMedicamento { get; set; }   // FK → Medicamento

    public int QuantidadeDisponivel { get; set; }
}