//TODO add mimic emote feature
//TODO organize main window with proper tabs/settings
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
using System;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using Dalamud.Game.DutyState;
using Dalamud.Bindings.ImGui;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/kirbo";
    private const string CommandTest = "/kirbotest";
    private const string CommandHighlight = "/kirbohl";
    public string Version => $"v{GetType().Assembly.GetName().Version}";
    public string Name => $"Kirbo's {GetType().Assembly.GetName().Name}";

    internal static Plugin? P;
    public Configuration Configuration { get; init; }

    private readonly WindowSystem WindowSystem;
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    internal TestWindow TestWindow { get; init; }
    internal TargetHighlight TargetHighlightWindow { get; init; }

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.All);
        Service.Init(pluginInterface);
        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var assetsPath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "Assets");
        var goatImagePath = Path.Combine(assetsPath, "goat.png");
        var kirboImagePath = Path.Combine(assetsPath, "kirbo.png");
        var logoImagePath = Path.Combine(assetsPath, "logo.png");

        MainWindow = new MainWindow(this, kirboImagePath);
        ConfigWindow = new ConfigWindow(this);
        TargetHighlightWindow = new TargetHighlight(this);
        TestWindow = new TestWindow();
        WindowSystem = new();

        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(TargetHighlightWindow);
        WindowSystem.AddWindow(TestWindow);

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

        Svc.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        // Adds another button that is doing the same but for the main ui of the plugin
        Svc.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        Svc.DutyState.DutyStarted += DutyState_DutyStarted;
        Svc.DutyState.DutyWiped += DutyState_DutyWiped;
        Svc.DutyState.DutyRecommenced += DutyState_DutyRecommenced;
        Svc.DutyState.DutyCompleted += DutyState_DutyCompleted;
        Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;

        //if (Svc.PluginInterface.Reason == PluginLoadReason.Reload && !MainWindow.IsOpen)
        //{
        //    MainWindow.IsOpen = true;
        //}

        NamePlateUpdater.Enable();
        MainUpdater.Enable();
    }

    public void Dispose()
    {
        Svc.Log.Debug($"Plugin: Dispose started.");
        MainWindow.Dispose();
        TargetHighlightWindow.Dispose();
        TestWindow.Dispose();
        ConfigWindow.Dispose();
        NamePlateUpdater.Disable();
        MainUpdater.Disable();
        WindowSystem.RemoveAllWindows();

        Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUI;
        Svc.PluginInterface.UiBuilder.Draw -= DrawUI;

        Svc.DutyState.DutyStarted -= DutyState_DutyStarted;
        Svc.DutyState.DutyWiped -= DutyState_DutyWiped;
        Svc.DutyState.DutyRecommenced -= DutyState_DutyRecommenced;
        Svc.DutyState.DutyCompleted -= DutyState_DutyCompleted;


        Svc.Commands.RemoveHandler(CommandName);
        Svc.Commands.RemoveHandler(CommandTest);
        Svc.Commands.RemoveHandler(CommandHighlight);

        ECommonsMain.Dispose();
        P = null;
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

    //private void DrawUI() => WindowSystem.Draw();
    private void DrawUI()
    {
        if (Configuration.ShowInDevMenu && Svc.PluginInterface.IsDevMenuOpen && ImGui.BeginMainMenuBar())
        {
            if (ImGui.MenuItem("Kirbo"))
            {
                ToggleMainUI();
            }

            ImGui.EndMainMenuBar();
        }
    }

    public void ToggleHighlightUI() => TargetHighlightWindow.Toggle();
    public void ToggleTestUI() => TestWindow.Toggle();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    private static void DutyState_DutyCompleted(IDutyStateEventArgs e)
    {
        NamePlateUpdater.ClearList();
        Notify.Success("Duty Completed!");
    }

    private static void DutyState_DutyRecommenced(IDutyStateEventArgs e)
    {
        NamePlateUpdater.ClearList();
        Notify.Success("Duty Recommenced!");
    }

    private static void DutyState_DutyWiped(IDutyStateEventArgs e)
    {
        NamePlateUpdater.ClearList();
        Notify.Success("Duty Wiped!");
    }

    private static void DutyState_DutyStarted(IDutyStateEventArgs e)
    {
        NamePlateUpdater.ClearList();
        Notify.Success("Duty Started!");
    }

    private static void ClientState_TerritoryChanged(uint id)
    {
        NamePlateUpdater.ClearList();
        Notify.Success("Territory Changed!");
    }
}
