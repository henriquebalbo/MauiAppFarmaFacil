using MauiAppFarmaFacil.Services;
using MauiAppFarmaFacil.Views;

namespace MauiAppFarmaFacil;

public partial class App : Application
{
    public App(DatabaseService dbService)
    {
        InitializeComponent();

        // Inicializa a navegação com a tela de busca (UC1)
        MainPage = new NavigationPage(new BuscaMedicamento(dbService))
        {
            BarBackgroundColor = Color.FromArgb("#1565C0"),
            BarTextColor = Colors.White
        };
    }
}