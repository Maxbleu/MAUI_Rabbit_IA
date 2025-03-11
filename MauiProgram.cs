using MauiApp_rabbit_mq_cliente_1.ViewModels;
using MauiApp_rabbit_mq_cliente_1.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace MauiApp_rabbit_mq_cliente_1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit();

            builder.Services.AddSingleton<SettingsRabbitMQViewModel>();
            builder.Services.AddSingleton<SettingsModeloViewModel>();
            builder.Services.AddSingleton<ChatViewModel>();

            builder.Services.AddSingleton<SettingsRabbitMQPage>();
            builder.Services.AddSingleton<SettingsModeloPage>();
            builder.Services.AddSingleton<ChatPage>();
            
            builder.Services.AddSingleton<AppShell>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}