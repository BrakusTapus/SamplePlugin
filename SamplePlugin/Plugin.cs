using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using SamplePlugin.DalamudServices;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    //[PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    //[PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    //[PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    //[PluginService] internal static IClientState ClientState { get; private set; } = null!;
    //[PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    //[PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/kirbo";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Service.Init(pluginInterface);
        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens Menu"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        pluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        if (Service.PluginInterface.Reason == PluginLoadReason.Reload && !MainWindow.IsOpen)
        {
            MainWindow.IsOpen = true;
        }

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        //Service.Log.Information($"Finished loading: {pluginInterface.Manifest.Name}.");
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        Service.CommandManager.RemoveHandler(CommandName);
        //Service.Log.Information($"Finished unloading: {Service.PluginInterface.Manifest.Name}."); 
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
