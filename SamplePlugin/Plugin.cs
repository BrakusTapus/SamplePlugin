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

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this, Module.All);
        Service.Init(pluginInterface);
        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Svc.Commands.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens Menu"
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
        ConfigWindow.Dispose();
        MainWindow.Dispose();
        Svc.Commands.RemoveHandler(CommandName);
        NamePlateUpdater.Dispose();
        MainUpdater.Dispose();
        Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
        ECommonsMain.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    private static void ClientState_TerritoryChanged(ushort obj)
    {
        NamePlateUpdater.ClearList();
    }
}
