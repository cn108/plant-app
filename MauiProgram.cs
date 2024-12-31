using Microsoft.Extensions.Logging;

namespace FinalYearProject
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()                
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("CustomFont-Bold.ttf", "CustomFont-Bold");
                    fonts.AddFont("CustomFont-Regular.ttf", "CustomFont-Regular");
                });

#if DEBUG
    		builder.Logging.AddDebug();
            builder.Services.AddSingleton<HttpClient>();
#endif


            return builder.Build();
        }
    }
}
