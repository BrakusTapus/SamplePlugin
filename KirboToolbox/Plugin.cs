using System;
using System.IO;

using Dalamud.Game.Command;
using Dalamud.Game.DutyState;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

using ECommons;
using ECommons.DalamudServices;

using KirboToolbox.Configs;
using KirboToolbox.Core.Trackers;
using KirboToolbox.Features.Highlight;
using KirboToolbox.Core.Extensions;
using KirboToolbox.UI.Windows;

namespace KirboToolbox;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/kirbo";
    private const string CommandTest = "/kirbotest";
    private const string CommandHighlight = "/kirbohl";
    public string Version => $"v{GetType().Assembly.GetName().Version}";
    public string Name => $"Kirbo's {GetType().Assembly.GetName().Name}";

    public Configuration Configuration { get; init; }

    private readonly WindowSystem windowSystem;
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    internal TestWindow TestWindow { get; init; }
    internal TargetHighlight TargetHighlightWindow { get; init; }

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector, Module.ObjectFunctions);
        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.MigrateIfNeeded();

        // you might normally want to embed resources and load them from the manifest stream
        var assetsPath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "Assets");
        var kirboImagePath = Path.Combine(assetsPath, "kirbo.png");

        MainWindow = new MainWindow(this, kirboImagePath);
        ConfigWindow = new ConfigWindow(this);
        TargetHighlightWindow = new TargetHighlight(this);
        TargetHighlightWindow.IsOpen = Configuration.EnableHighLightOverlay;
        TestWindow = new TestWindow();
        windowSystem = new();

        windowSystem.AddWindow(MainWindow);
        windowSystem.AddWindow(ConfigWindow);
        windowSystem.AddWindow(TargetHighlightWindow);
        windowSystem.AddWindow(TestWindow);

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

        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI; // Adds another button that is doing the same but for the main ui of the plugin
        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI; // This adds a button to the plugin installer entry of this plugin which allows to toggle the display status of the configuration ui

        Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;

        Svc.DutyState.DutyStarted += DutyState_DutyStarted;
        Svc.DutyState.DutyCompleted += DutyState_DutyCompleted;
        Svc.DutyState.DutyWiped += DutyState_DutyWiped;
        Svc.DutyState.DutyRecommenced += DutyState_DutyRecommenced;

        NamePlateTracker.Enable();
        ObjectTracker.Enable();
    }

    public void Dispose()
    {
        Svc.Log.Debug($"Plugin: Dispose started.");

        ResetPlayerSpeed();
        NamePlateTracker.Disable();
        ObjectTracker.Disable();

        Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUI;
        Svc.PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUI;

        windowSystem.RemoveAllWindows();

        MainWindow.Dispose();
        TargetHighlightWindow.Dispose();
        TestWindow.Dispose();
        ConfigWindow.Dispose();

        Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;

        Svc.DutyState.DutyStarted -= DutyState_DutyStarted;
        Svc.DutyState.DutyCompleted -= DutyState_DutyCompleted;
        Svc.DutyState.DutyWiped -= DutyState_DutyWiped;
        Svc.DutyState.DutyRecommenced -= DutyState_DutyRecommenced;

        Svc.Commands.RemoveHandler(CommandName);
        Svc.Commands.RemoveHandler(CommandTest);
        Svc.Commands.RemoveHandler(CommandHighlight);

        ECommonsMain.Dispose();
    }

    /// <summary>
    /// Puts the player's movement speed back to normal so we don't leave modified game memory behind.
    /// </summary>
    private void ResetPlayerSpeed()
    {
        try
        {
            var player = Svc.Objects.LocalPlayer;
            if (player == null)
            {
                return; // Not logged in, nothing to reset.
            }

            player.Speed = 1f;

            // Keep the saved slider value in sync with the real game speed.
            Configuration.SpeedMultiplier = 1f;
            Svc.Log.Debug($"Player speed was reset back to 1.");
            Configuration.Save();
        }
        catch (Exception ex)
        {
            // Never let a failure here stop the rest of the cleanup.
            Svc.Log.Error(ex, "Failed to reset movement speed on unload.");
        }
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
        ToggleHighlightUI();
    }

    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleTestUI() => TestWindow.Toggle();
    public void ToggleHighlightUI()
    {
        // Flip the setting, keep the window in sync with it, and save.
        Configuration.EnableHighLightOverlay = !Configuration.EnableHighLightOverlay;
        TargetHighlightWindow.IsOpen = Configuration.EnableHighLightOverlay;
        Configuration.Save();

        Svc.Log.Debug($"Highlight overlay is now {(Configuration.EnableHighLightOverlay ? "enabled" : "disabled")}.");
    }

    private void ClientState_TerritoryChanged(uint id)
    {
        Svc.Log.Debug($"Territory changed to {id}.");
        NamePlateTracker.ClearList();
    }

    private static void DutyState_DutyStarted(IDutyStateEventArgs e)
    {
        Svc.Log.Debug("Duty started.");
        NamePlateTracker.ClearList();
    }

    private static void DutyState_DutyCompleted(IDutyStateEventArgs e)
    {
        Svc.Log.Debug("Duty completed.");
        NamePlateTracker.ClearList();
    }

    private static void DutyState_DutyWiped(IDutyStateEventArgs e)
    {
        Svc.Log.Debug("Duty wiped.");
        NamePlateTracker.ClearList();
    }

    private static void DutyState_DutyRecommenced(IDutyStateEventArgs e)
    {
        Svc.Log.Debug("Duty recommenced.");
        NamePlateTracker.ClearList();
    }

}
