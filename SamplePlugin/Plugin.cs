using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.UI;
using SamplePlugin.DalamudServices;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Serilog;
using System.Collections.Generic;
using SamplePlugin.Configs;
using SamplePlugin.Updaters;
using ECommons;
using ECommons.DalamudServices;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/kirbo";
    private const string CommandTest = "/kirbotest";
    private const string CommandHighlight = "/kirbohl";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private TestWindow TestWindow { get; init; }
    private TargetHighlight TargetHighlightWindow { get; init; }

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this, Module.All);
        Service.Init(pluginInterface);
        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
        var kirboImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "kirbo.png");

        TargetHighlightWindow = new TargetHighlight(this);
        TestWindow = new TestWindow(this);
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(TargetHighlightWindow);
        WindowSystem.AddWindow(TestWindow);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Svc.Commands.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens Menu"
        });

        Svc.Commands.AddHandler(CommandTest, new CommandInfo(OnTestCommand)
        {
            HelpMessage = "Opens Test window"
        });

        Svc.Commands.AddHandler(CommandHighlight, new CommandInfo(OnHighlightCommand)
        {
            HelpMessage = "Opens Highlight window"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        pluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;

        if (Svc.PluginInterface.Reason == PluginLoadReason.Reload && !MainWindow.IsOpen)
        {
            MainWindow.IsOpen = true;
        }

        NamePlateUpdater.Enable();
        MainUpdater.Enable();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        TargetHighlightWindow.Dispose();
        TestWindow.Dispose();
        ConfigWindow.Dispose();
        MainWindow.Dispose();
        NamePlateUpdater.Dispose();
        MainUpdater.Dispose();
        Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
        Svc.Commands.RemoveHandler(CommandName);
        Svc.Commands.RemoveHandler(CommandTest);
        Svc.Commands.RemoveHandler(CommandHighlight);
        ECommonsMain.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void OnTestCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleTestUI();
    }

    private void OnHighlightCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleHighlightUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleHighlightUI() => TargetHighlightWindow.Toggle();
    public void ToggleTestUI() => TestWindow.Toggle();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    private static void ClientState_TerritoryChanged(ushort obj)
    {
        NamePlateUpdater.ClearList();
    }
}
