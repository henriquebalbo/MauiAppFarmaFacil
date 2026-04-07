using Microsoft.Extensions.Logging;
using MauiAppFarmaFacil.Services;
using MauiAppFarmaFacil.Views;

namespace MauiAppFarmaFacil;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ─── Injeção de Dependência ──────────────────────────────────────────
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddTransient<BuscaMedicamento>();
        builder.Services.AddTransient<UnidadesDeSaude>();
        builder.Services.AddTransient<DetalhesUnidade>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}