using System.Net.Http;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using SamplePlugin.Windows;
using SamplePlugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using System;
using System.Linq;
using System.Net.Http;

namespace SamplePlugin;

#pragma warning disable CA1816 // Dispose warining
#pragma warning disable RCS1170 // Use read-only auto-implemented property.

public class Plugin : IDalamudPlugin
{     
    [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static Dalamud.Game.ClientState.Keys.KeyState KeyState { get; private set; } = null!;

    public string Name => "Sample Plugin";

    public static Configuration Config { get; private set; } = null!;
    public static MainWindow? GuiMain { get; private set; }
    public static ConfigWindow? GuiConfig { get; private set; }
    public object Configuration { get; internal set; }

    public WindowSystem WindowSystem = new("SamplePlugin");
    private const string CommandMain = "/sample";
    private const string CommandConfig = "/samplecfg";



    public Plugin()
    {
        // Setup commands
        CommandManager.AddHandler(CommandConfig, new CommandInfo(OnCommand)
        {
            HelpMessage = "Configuration window"
        });

        CommandManager.AddHandler(CommandMain, new CommandInfo(OnCommand)
        {
            HelpMessage = "Display the main window, containing the image."
        });

        Config = Configuration.Load(); // Load Configuration
     // ImageSource = Config.LoadSources(); // Load ImageSources from config

     // Embedded.LoadAll(); // Load all embedded resources

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigGui;
        PluginInterface.UiBuilder.Draw += DrawUI;

        // Open Main Window
        if (Config.GuiMainVisible)
            ShowMainGui();
        
    public void Dispose()
    {
        CommandManager.RemoveHandler(CommandConfig);
        CommandManager.RemoveHandler(CommandMain);
    }

        var AssemblyLocation = Service.Interface.AssemblyLocation;
        var manifest = Path.Join(AssemblyLocation.DirectoryName, "SamplePlugin.json");

        // you might normally want to embed resources and load them from the manifest stream
        var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "banner.png");
        var imagepath2 = Path.Combine();
        var bannerImage = PluginInterface.UiBuilder.LoadImage(imagePath);

        WindowSystem.AddWindow(new ConfigWindow(this));
        WindowSystem.AddWindow(new MainWindow(this, bannerImage));

        CommandManager.AddHandler(CommandMain, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }



    private void OnCommand(string command, string args)
    {
        var input = command + args;

        if (input.Contains("cfg", System.StringComparison.CurrentCultureIgnoreCase)
         || input.Contains("config", System.StringComparison.CurrentCultureIgnoreCase))
        {
            ToggleConfigGui();
        }
        else
        {
            ToggleMainGui();
        }
    }

    private void DrawUI()
    {
        // Allow open/close with middle mouse button
        if (GuiMain?.Visible != true && Config.Hotkeys.ToggleWindow.IsPressed())
            ToggleMainGui();

        GuiMain?.Draw();
        GuiConfig?.Draw();
    }

    public static void ToggleMainGui()
    {
        GuiMain ??= new();
        GuiMain.Visible = !GuiMain.Visible;
        Config.Save();
    }

    public static void ToggleConfigGui()
    {
        GuiConfig ??= new();
        GuiConfig.Visible = !GuiConfig.Visible;
        // Config.Save();
    }

    public static void ShowMainGui()
    {
        GuiMain ??= new();
        GuiMain.Visible = true;
    }
    public static void ShowConfigGui()
    {
        GuiConfig ??= new();
        GuiConfig.Visible = true;
    }

}
