using SQLite;

namespace MauiAppFarmaFacil.Models;

public class Cidadao
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
    public string EnderecoPadrao { get; set; }
    public string LocalizacaoAtual { get; set; }

    // Campos adicionais
    public string CartaoSUS { get; set; }
    public bool AceitaNotif { get; set; }
    public string TipoUsuario { get; set; }
}
