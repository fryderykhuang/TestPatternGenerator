using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WinFormsComInterop;

namespace TestPatternGenerator;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ComWrappers.RegisterForMarshalling(WinFormsComWrappers.Instance);
        var settingsManager = new SettingsManager();
        settingsManager.RegisterType<UserSettings, ResolutionPageSettings>(x => x.Patterns.ResolutionPageSettings);
        settingsManager.RegisterType<UserSettings, WhiteBalancePageSettings>(x => x.Patterns.WhiteBalancePageSettings);
        settingsManager.RegisterType<UserSettings, ConvergencePageSettings>(x => x.Patterns.ConvergencePageSettings);
        settingsManager.RegisterType<UserSettings, SpectrumPageSettings>(x => x.Patterns.SpectrumPageSettings);
        settingsManager.RegisterType<UserSettings, GammaPageSettings>(x => x.Patterns.GammaPageSettings);
        settingsManager.RegisterType<UserSettings, PatternViewState>(x => x.PatternViewState);
        settingsManager.WriteDefaultsIfNotExists();

        var builder = new HostBuilder()
            .ConfigureAppConfiguration(builder => settingsManager.AddFileLocations(builder))
            .ConfigureServices((hostContext, services) =>
            {
                services.AddScoped<MainForm>();
                services.AddTransient<DrawingSurface>();
                services.AddSingleton<IEventBus, UiThreadEventBus>();
                services.AddSingleton(settingsManager);
                services.AddLogging(configure => configure.AddConsole());
                settingsManager.Configure(hostContext.Configuration, services);
            });


        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        var host = builder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var mf = sp.GetRequiredService<MainForm>();
            Application.Run(mf);
        }

        settingsManager.SaveSettings();
    }
}