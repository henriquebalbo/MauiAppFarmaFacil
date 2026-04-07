using SQLite;

namespace MauiAppFarmaFacil.Models;

/// <summary>
/// Superclasse que representa o usuário do sistema (Cidadão).
/// Estratégia de herança: superclasse + subclasses com FK.
/// </summary>
public class Cidadao
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100), NotNull]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(150), Unique]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255), NotNull]
    public string Senha { get; set; } = string.Empty;

    [MaxLength(200)]
    public string EnderecoPadrao { get; set; } = string.Empty;

    [MaxLength(150)]
    public string LocalizacaoAtual { get; set; } = string.Empty;

    // Campos de subtipo armazenados na superclasse (TPH - Table Per Hierarchy)
    // para simplificar o SQLite local (sem suporte nativo a herança)
    [MaxLength(30)]
    public string CartaoSUS { get; set; } = string.Empty;

    public bool AceitaNotif { get; set; }

    /// <summary>Paciente | UsuarioApp | Cuidador</summary>
    [MaxLength(20)]
    public string TipoUsuario { get; set; } = "UsuarioApp";
}