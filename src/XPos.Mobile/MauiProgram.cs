using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Blazored.LocalStorage;
using XPos.Mobile.Services;

namespace XPos.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        try
        {
		    var builder = MauiApp.CreateBuilder();
		    builder
			    .UseMauiApp<App>()
			    .ConfigureFonts(fonts =>
			    {
				    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			    });

		    builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();

            // STORAGE
            builder.Services.AddBlazoredLocalStorage();

#if DEBUG
		    builder.Services.AddBlazorWebViewDeveloperTools();
		    builder.Logging.AddDebug();
#endif
            
            // HttpClient Configuration
            // Emülatör: 10.0.2.2, Gerçek cihaz: bilgisayar IP
            string baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5029" : "http://127.0.0.1:5029";
            
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });
            builder.Services.AddScoped<Services.AuthStateService>();
            builder.Services.AddSingleton<Services.OrderStateService>();

            // Global Exception Handling
            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "XPos_Crash_Log.txt");
                File.AppendAllText(logPath, $"[CRASH] {DateTime.Now}: {error.ExceptionObject}\n\n");
            };

            TaskScheduler.UnobservedTaskException += (sender, error) =>
            {
                 var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "XPos_Task_Log.txt");
                 File.AppendAllText(logPath, $"[TASK ERROR] {DateTime.Now}: {error.Exception}\n\n");
            };

		    return builder.Build();
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "maui_startup_error.txt");
            File.WriteAllText(logPath, ex.ToString());
            throw;
        }
	}
}
