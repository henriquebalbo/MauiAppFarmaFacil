using MauiAppFarmaFacil.Models;
using MauiAppFarmaFacil.Services;

namespace MauiAppFarmaFacil.Views;

/// <summary>
/// UC2 - Consultar Unidades de Saúde Próximas
/// Exibe unidades que possuem o medicamento selecionado em estoque.
/// Inclui geolocalização para ordenação por proximidade.
/// </summary>
public partial class UnidadesDeSaude : ContentPage
{
    private readonly Medicamento _medicamento;
    private readonly DatabaseService _dbService;
    private double _latUsuario;
    private double _lngUsuario;

    public UnidadesDeSaude(Medicamento medicamento, DatabaseService dbService)
    {
        InitializeComponent();
        _medicamento = medicamento;
        _dbService = dbService;

        lblTitulo.Text = medicamento.Nome;
        lblSubtitulo.Text = $"Princípio ativo: {medicamento.PrincipioAtivo}  •  {medicamento.Dosagem}";
        Title = medicamento.Nome;
    }

    // ─── Ciclo de vida ─────────────────────────────────────────────────────────

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarUnidades();
    }

    // ─── Eventos ───────────────────────────────────────────────────────────────

    private async void OnUsarLocalizacaoClicked(object sender, EventArgs e)
    {
        await ObterLocalizacaoUsuario();
        await CarregarUnidades();
    }

    /// <summary>UC3 - Navega para detalhes da unidade selecionada.</summary>
    private async void OnVerDetalhesClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is UnidadeDeSaude unidade)
        {
            await Navigation.PushAsync(new DetalhesUnidade(unidade, _medicamento, _dbService));
        }
    }

    // ─── Lógica ────────────────────────────────────────────────────────────────

    private async Task CarregarUnidades()
    {
        SetLoading(true);
        try
        {
            var unidades = await _dbService.ConsultarUnidadesComMedicamento(
                _medicamento.CodMedicamento, _latUsuario, _lngUsuario);

            listaUnidades.ItemsSource = unidades;
            lblQtdUnidades.Text = unidades.Count == 0
                ? string.Empty
                : $"{unidades.Count} unidade(s) com estoque disponível";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar as unidades: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task ObterLocalizacaoUsuario()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permissão negada",
                    "Ative a permissão de localização nas configurações do dispositivo.", "OK");
                return;
            }

            var location = await Geolocation.GetLastKnownLocationAsync()
                           ?? await Geolocation.GetLocationAsync(new GeolocationRequest
                           {
                               DesiredAccuracy = GeolocationAccuracy.Medium,
                               Timeout = TimeSpan.FromSeconds(10)
                           });

            if (location != null)
            {
                _latUsuario = location.Latitude;
                _lngUsuario = location.Longitude;
                lblLocalizacao.Text = $"📍 Lat: {_latUsuario:F4}, Lng: {_lngUsuario:F4} (ordenado por proximidade)";
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Recurso indisponível", "Geolocalização não suportada neste dispositivo.", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permissão negada", "Permissão de localização não concedida.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível obter localização: {ex.Message}", "OK");
        }
    }

    private void SetLoading(bool carregando)
    {
        loadingIndicator.IsRunning = carregando;
        loadingIndicator.IsVisible = carregando;
        listaUnidades.IsVisible = !carregando;
    }
}