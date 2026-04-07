using SQLite;

namespace MauiAppFarmaFacil.Models;

public class UnidadeDeSaude
{
    [PrimaryKey, AutoIncrement]
    public int CodUnidade { get; set; }

    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Endereco { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [MaxLength(120)]
    public string HorarioFuncionamento { get; set; } = string.Empty;

    // Armazenado como "latitude,longitude"
    [MaxLength(100)]
    public string Coordenadas { get; set; } = string.Empty;

    // Propriedades auxiliares (não mapeadas no banco)
    [Ignore]
    public double Latitude
    {
        get
        {
            if (string.IsNullOrEmpty(Coordenadas)) return 0;
            var parts = Coordenadas.Split(',');
            return parts.Length > 0 && double.TryParse(parts[0], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : 0;
        }
    }

    [Ignore]
    public double Longitude
    {
        get
        {
            if (string.IsNullOrEmpty(Coordenadas)) return 0;
            var parts = Coordenadas.Split(',');
            return parts.Length > 1 && double.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var lng) ? lng : 0;
        }
    }

    [Ignore]
    public double DistanciaKm { get; set; }

    [Ignore]
    public int QuantidadeDisponivel { get; set; }
}