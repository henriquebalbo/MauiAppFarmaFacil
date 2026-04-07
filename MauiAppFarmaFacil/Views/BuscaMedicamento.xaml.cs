using MauiAppFarmaFacil.Models;
using MauiAppFarmaFacil.Services;

namespace MauiAppFarmaFacil.Views;

/// <summary>
/// UC1 - Buscar Medicamento por Nome
/// Tela principal de busca. Navega para UC2 ao selecionar um medicamento.
/// </summary>
public partial class BuscaMedicamento : ContentPage
{
    private readonly DatabaseService _dbService;
    private CancellationTokenSource? _debounceToken;

    public BuscaMedicamento(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    // ─── Eventos ───────────────────────────────────────────────────────────────

    /// <summary>Acionado pelo botão de busca do teclado.</summary>
    private async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        await RealizarBusca(searchBar.Text);
    }

    /// <summary>Debounce de 400ms para busca enquanto o usuário digita.</summary>
    private async void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _debounceToken?.Cancel();
        _debounceToken = new CancellationTokenSource();
        var token = _debounceToken.Token;

        try
        {
            await Task.Delay(400, token);
            if (!token.IsCancellationRequested)
                await RealizarBusca(e.NewTextValue);
        }
        catch (TaskCanceledException) { /* debounce cancelado */ }
    }

    /// <summary>UC2 - navega para a tela de unidades com o medicamento selecionado.</summary>
    private async void OnVerUnidadesClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Medicamento med)
        {
            await Navigation.PushAsync(new UnidadesDeSaude(med, _dbService));
        }
    }

    // ─── Lógica de busca ───────────────────────────────────────────────────────

    private async Task RealizarBusca(string? termo)
    {
        SetLoading(true);

        try
        {
            var resultados = await _dbService.BuscarMedicamentos(termo ?? string.Empty);

            if (string.IsNullOrWhiteSpace(termo))
            {
                MostrarEstadoVazio();
                return;
            }

            listaMedicamentos.ItemsSource = resultados;
            listaMedicamentos.IsVisible = true;
            emptyState.IsVisible = false;

            lblResultados.Text = resultados.Count == 0
                ? string.Empty
                : $"{resultados.Count} medicamento(s) encontrado(s)";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível realizar a busca: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void MostrarEstadoVazio()
    {
        listaMedicamentos.IsVisible = false;
        emptyState.IsVisible = true;
    }

    private void SetLoading(bool carregando)
    {
        loadingIndicator.IsRunning = carregando;
        loadingIndicator.IsVisible = carregando;
        searchBar.IsEnabled = !carregando;
    }
}