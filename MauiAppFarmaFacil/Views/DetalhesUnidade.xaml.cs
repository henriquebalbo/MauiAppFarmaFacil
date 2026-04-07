using MauiAppFarmaFacil.Models;
using MauiAppFarmaFacil.Services;

namespace MauiAppFarmaFacil.Views;

/// <summary>
/// UC3 - Ver Detalhes da Unidade de Saúde
/// Exibe todas as informações da unidade selecionada junto com
/// a disponibilidade do medicamento buscado.
/// Estende UC1 e UC2 (relação &lt;&lt;extend&gt;&gt; no diagrama de casos de uso).
/// </summary>
public partial class DetalhesUnidade : ContentPage
{
    private readonly UnidadeDeSaude _unidade;
    private readonly Medicamento _medicamento;
    private readonly DatabaseService _dbService;

    public DetalhesUnidade(UnidadeDeSaude unidade, Medicamento medicamento, DatabaseService dbService)
    {
        InitializeComponent();
        _unidade = unidade;
        _medicamento = medicamento;
        _dbService = dbService;

        PreencherDados();
    }

    // ─── Preenchimento dos dados ───────────────────────────────────────────────

    private void PreencherDados()
    {
        // Informações da unidade
        lblNomeUnidade.Text = _unidade.Nome;
        lblEndereco.Text = _unidade.Endereco;
        lblTelefone.Text = string.IsNullOrWhiteSpace(_unidade.Telefone)
            ? "Não informado" : _unidade.Telefone;
        lblHorario.Text = string.IsNullOrWhiteSpace(_unidade.HorarioFuncionamento)
            ? "Não informado" : _unidade.HorarioFuncionamento;

        // Informações do medicamento
        lblNomeMedicamento.Text = _medicamento.Nome;
        lblPrincipioAtivo.Text = $"{_medicamento.PrincipioAtivo}  •  {_medicamento.Dosagem}  •  {_medicamento.Fabricante}";
        lblQuantidade.Text = _unidade.QuantidadeDisponivel > 0
            ? $"✅ {_unidade.QuantidadeDisponivel} unidades disponíveis"
            : "⚠️ Estoque não informado";

        // Distância (se calculada)
        if (_unidade.DistanciaKm > 0)
        {
            gridDistancia.IsVisible = true;
            lblDistancia.Text = _unidade.DistanciaKm < 1
                ? $"{_unidade.DistanciaKm * 1000:F0} metros"
                : $"{_unidade.DistanciaKm:F1} km";
        }

        // Habilita botão de ligar apenas se houver telefone
        btnLigar.IsEnabled = !string.IsNullOrWhiteSpace(_unidade.Telefone);
    }

    // ─── Eventos ───────────────────────────────────────────────────────────────

    private async void OnLigarClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_unidade.Telefone)) return;

        // Remove caracteres não numéricos para discagem
        var numero = new string(_unidade.Telefone.Where(char.IsDigit).ToArray());

        try
        {
            if (PhoneDialer.IsSupported)
                PhoneDialer.Open(numero);
            else
                await DisplayAlert("Indisponível", "Discagem telefônica não suportada neste dispositivo.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível realizar a ligação: {ex.Message}", "OK");
        }
    }

    private async void OnVoltarBuscaClicked(object sender, EventArgs e)
    {
        // Volta até a tela raiz de busca (remove UC2 e UC3 da pilha)
        await Navigation.PopToRootAsync();
    }
}