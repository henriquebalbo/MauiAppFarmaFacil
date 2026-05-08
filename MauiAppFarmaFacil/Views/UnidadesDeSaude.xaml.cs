using MauiAppFarmaFacil.Models;
using MauiAppFarmaFacil.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MauiAppFarmaFacil.Views;

public partial class UnidadesDeSaude : ContentPage
{
    private readonly Medicamento _medicamento;
    private readonly DatabaseService _dbService;
    private double _latUsuario;
    private double _lngUsuario;
    private bool _mapaExpandido = false;
    private List<UnidadeDeSaude> _ultimasUnidades = new();

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

        // Após obter localização, exibe e expande o mapa automaticamente
        if (_latUsuario != 0 || _lngUsuario != 0)
        {
            secaoMapa.IsVisible = true;
            _mapaExpandido = true;
            mapaUnidades.IsVisible = true;
            lblSetaMapa.Text = "▲";
            AtualizarPinsMapa(_ultimasUnidades);
        }
    }

    /// <summary>Abre ou fecha a seção do mapa ao tocar no cabeçalho.</summary>
    private void OnToggleMapaClicked(object sender, EventArgs e)
    {
        _mapaExpandido = !_mapaExpandido;
        mapaUnidades.IsVisible = _mapaExpandido;
        lblSetaMapa.Text = _mapaExpandido ? "▲" : "▼";

        if (_mapaExpandido)
            CentralizarMapa();
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

            _ultimasUnidades = unidades;
            listaUnidades.ItemsSource = unidades;
            lblQtdUnidades.Text = unidades.Count == 0
                ? string.Empty
                : $"{unidades.Count} unidade(s) com estoque disponível";

            // Atualiza pins se o mapa já estiver visível
            if (secaoMapa.IsVisible && _mapaExpandido)
                AtualizarPinsMapa(unidades);
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

    // ─── Mapa ──────────────────────────────────────────────────────────────────

    private void AtualizarPinsMapa(List<UnidadeDeSaude> unidades)
    {
        mapaUnidades.Pins.Clear();

        // Pin do usuário
        if (_latUsuario != 0 || _lngUsuario != 0)
        {
            mapaUnidades.Pins.Add(new Pin
            {
                Label = "Minha localização",
                Address = $"Lat: {_latUsuario:F4}, Lng: {_lngUsuario:F4}",
                Location = new Location(_latUsuario, _lngUsuario),
                Type = PinType.SavedPin
            });
        }

        // Pins das unidades de saúde
        foreach (var u in unidades)
        {
            if (u.Latitude == 0 && u.Longitude == 0) continue;

            mapaUnidades.Pins.Add(new Pin
            {
                Label = u.Nome,
                Address = $"{u.Endereco}  •  ✅ {u.QuantidadeDisponivel} unidades",
                Location = new Location(u.Latitude, u.Longitude),
                Type = PinType.Place
            });
        }

        CentralizarMapa();
    }

    private void CentralizarMapa()
    {
        if (mapaUnidades.Pins.Count == 0 && _ultimasUnidades.Count > 0)
            AtualizarPinsMapa(_ultimasUnidades);

        var pontos = mapaUnidades.Pins.Select(p => p.Location).ToList();

        if (pontos.Count == 0)
        {
            if (_latUsuario != 0 || _lngUsuario != 0)
                mapaUnidades.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(_latUsuario, _lngUsuario),
                    Distance.FromKilometers(5)));
            return;
        }

        var minLat = pontos.Min(p => p.Latitude);
        var maxLat = pontos.Max(p => p.Latitude);
        var minLng = pontos.Min(p => p.Longitude);
        var maxLng = pontos.Max(p => p.Longitude);

        var spanLat = Math.Max((maxLat - minLat) * 1.3, 0.01);
        var spanLng = Math.Max((maxLng - minLng) * 1.3, 0.01);

        mapaUnidades.MoveToRegion(new MapSpan(
            new Location((minLat + maxLat) / 2, (minLng + maxLng) / 2),
            spanLat, spanLng));
    }

    private void SetLoading(bool carregando)
    {
        loadingIndicator.IsRunning = carregando;
        loadingIndicator.IsVisible = carregando;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!carregando)
                await Task.Delay(50);
            listaUnidades.IsVisible = !carregando;
        });
    }
}