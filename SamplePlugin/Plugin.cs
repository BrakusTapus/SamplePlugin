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

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Sample Plugin";
    private const string CommandName = "/sample";

    private DalamudPluginInterface PluginInterface { get; init; }
    private CommandManager CommandManager { get; init; }
    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem = new("SamplePlugin");


    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager)
    {
        this.PluginInterface = pluginInterface;
        this.CommandManager = commandManager;

        this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.Configuration.Initialize(this.PluginInterface);

        var AssemblyLocation = Service.Interface.AssemblyLocation;
        var manifest = Path.Join(AssemblyLocation.DirectoryName, "SamplePlugin.json");

        // you might normally want to embed resources and load them from the manifest stream
        var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "banner.png");
        var imagepath2 = Path.Combine();
        var bannerImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

        WindowSystem.AddWindow(new ConfigWindow(this));
        WindowSystem.AddWindow(new MainWindow(this, bannerImage));

        this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        this.PluginInterface.UiBuilder.Draw += DrawUI;
        this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }



    public void Dispose()
    {
        this.WindowSystem.RemoveAllWindows();
        this.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string arguments)
    {
        string[]? argumentsParts = arguments.Split();

        switch (argumentsParts[0].ToLower())
        {
        }
    }

    private void DrawUI()
    {
        this.WindowSystem.Draw();
    }
    public void DrawConfigUI()
    {
        WindowSystem.GetWindow("A Wonderful Configuration Window").IsOpen = true;
    }

}
