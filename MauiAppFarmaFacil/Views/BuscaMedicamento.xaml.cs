using MauiAppFarmaFacil.Services;
using Microsoft.Maui.Controls;


namespace FarmaFacil.Views;

public partial class BuscaMedicamento : ContentPage
{
    private DatabaseService _dbService = new DatabaseService();

    public BuscaMedicamento()
    {
        InitializeComponent();
    }

    private async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        string termo = searchBar.Text ?? string.Empty;

        var resultado = await _dbService.BuscarMedicamentos(termo);

        listaMedicamentos.ItemsSource = resultado;
    }
}