using SQLite;

namespace MauiAppFarmaFacil.Models;

public class Medicamento
{
    [PrimaryKey, AutoIncrement]
    public int CodMedicamento { get; set; }

    [MaxLength(120), NotNull]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(120)]
    public string PrincipioAtivo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Dosagem { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Fabricante { get; set; } = string.Empty;
}