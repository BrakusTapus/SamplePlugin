using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using System.IO;
using System;


// Define the namespace for the plugin.
namespace SamplePlugin;

// Define the Plugin class which implements the IDalamudPlugin interface.
// This interface is required for all Dalamud plugins.
public class Plugin : IDalamudPlugin
{
    // Name of the plugin, used by Dalamud to identify it.
    public string Name => "Sample Plugin";

    // A constant string to define the command name.
    private const string CommandName = "/psp";

    // Properties for various services and configurations.
    // These are initialized via dependency injection in the constructor.
    private DalamudPluginInterface PluginInterface { get; init; }

    private ICommandManager CommandManager { get; init; }
    internal IPluginLog PluginLog { get; init; }
    public IDalamudTextureWrap? GoatImage { get; private set; }

    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem = new("SamplePlugin");

    // Windows for the plugin's UI.
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    // Constructor for the plugin.
    public Plugin(DalamudPluginInterface pluginInterface, ICommandManager commandManager, IPluginLog pluginLog)
    {
        // Initialize properties with the injected services.
        this.PluginLog = pluginLog;        
        PluginLog.Debug($"Starting [{Name}]"); // Log a debug message indicating the plugin is starting.

        this.PluginInterface = pluginInterface;
        PluginLog.Debug("Initialize: " + PluginInterface);

        this.CommandManager = commandManager;
        PluginLog.Debug("Initialize: " + CommandManager);

        // Load or create the plugin configuration.
        this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Initialize the configuration.
        this.Configuration.Initialize(this.PluginInterface);

        // you might normally want to embed resources and load them from the manifest stream

        // Path 1 points to the build output path (ex: C:\Users\AJvdM\AppData\Roaming\XIVPlugins\Projects\SamplePlugin\SamplePlugin\bin\x64\Release)
        // Path 2 points to Path 1 + "images\goat.png"
        var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "icon.png");
        IDalamudTextureWrap? goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImage);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommandToggleMainWindow)
        {
            HelpMessage = "Opens Main menu"
        });

        this.PluginInterface.UiBuilder.Draw += DrawUI;
        this.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigWindow;
        this.PluginInterface.UiBuilder.OpenMainUi += ToggleMainWindow;
    }

    public void Dispose()
    {
        this.WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        this.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommandToggleMainWindow(string command, string args)
    {
        // in response to the slash command, just display our main ui
        ToggleMainWindow();
    }

    private void DrawUI()
    {
        this.WindowSystem.Draw();
    }

    /// <summary>
    /// Method: Toggles the visibility of the main window of the plugin.
    /// <remarks>Changes the open state of the MainWindow, showing or hiding it.</remarks>
    /// </summary>
    private void ToggleMainWindow()
    {
        MainWindow.IsOpen = !MainWindow.IsOpen;
    }

    public void ToggleConfigWindow()
    {
        ConfigWindow.IsOpen = !ConfigWindow.IsOpen;
    }
}
