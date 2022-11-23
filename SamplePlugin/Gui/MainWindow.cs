using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;

namespace SamplePlugin.Windows;

public class MainWindow
{
    
    private bool visible = Plugin.Config.GuiMainVisible;

    public bool Visible
    {
        get => visible;
        set
        {
            if (Plugin.Config.GuiMainVisible != value)
            {
                Plugin.Config.GuiMainVisible = value;
                Plugin.Config.Save();
            }
            visible = value;
        }
    }

    private TextureWrap bannerImage;
    private Plugin Plugin;

    public MainWindow(Plugin plugin, TextureWrap bannerImage) : base(
        "My Amazing Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.bannerImage = bannerImage;
        this.Plugin = plugin;
    }

    public void Dispose()
    {
        this.bannerImage.Dispose();
    }

    public override void Draw()
    {
        ImGui.Text($"The random config bool is {this.Plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

        if (ImGui.Button("Show Settings"))
        {
            this.Plugin.DrawConfigUI();
        }

        ImGui.Spacing();

        ImGui.Text("Have a banner:");
        ImGui.Indent(55);
        ImGui.Image(this.bannerImage.ImGuiHandle, new Vector2(this.bannerImage.Width, this.bannerImage.Height));
        ImGui.Unindent(55);
    }
}
