using System;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

using ECommons.DalamudServices;

using KirboToolbox.Configs;
using KirboToolbox.Features.Highlight;
using KirboToolbox.Features.PlayerInfo;
using KirboToolbox.Features.TargetInfo;

namespace KirboToolbox.UI.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly string kirboImagePath;
    private readonly Plugin plugin;
    private readonly Configuration configuration;
    private readonly HighlightTab highlightTab;
    private readonly PlayerInfoTab playerInfoTab;
    private readonly TargetInfoTab targetInfoTab;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string kirboImagePath)
        : base($"Kirbo's Plugin v{plugin.GetType().Assembly.GetName().Version}###Plugin", ImGuiWindowFlags.NoScrollbar)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        TitleBarButtons.Add(new()
        {
            Click = (m) =>
            {
                if (m != ImGuiMouseButton.Left)
                {
                    return;
                }

                plugin.ToggleConfigUI();
            },
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new(2, 2),
            ShowTooltip = () => ImGui.SetTooltip(("Toggle settings window.")),
        });

        this.kirboImagePath = kirboImagePath;
        this.plugin = plugin;
        configuration = plugin.Configuration;
        highlightTab = new HighlightTab(plugin);
        playerInfoTab = new PlayerInfoTab(plugin);
        targetInfoTab = new TargetInfoTab();
    }

    public void Dispose() { }

    public override void Draw()
    {
        using (var tabheader = ImRaii.TabBar("ATabBar"))
        {
            if (!tabheader.Success)
            {
                return;
            }

            using (var mainMenuTab = ImRaii.TabItem("Main Menu"))
            {
                if (mainMenuTab.Success)
                {
                    DrawImage();
                    DrawPluginInfo();
                }
            }

            if (configuration.DisplayPlayerInfoTab)
            {
                using (var playerInfoTabItem = ImRaii.TabItem("Player Info"))
                {
                    if (playerInfoTabItem.Success)
                    {
                        playerInfoTab.Draw();
                    }
                }
            }

            if (configuration.DisplayTargetInfoTab)
            {
                using (var targetInfoTabItem = ImRaii.TabItem("Target Info"))
                {
                    if (targetInfoTabItem.Success)
                    {
                        targetInfoTab.Draw();
                    }
                }
            }

            if (configuration.DisplayHighLightInfoTab)
            {
                using (var highlightInfoTab = ImRaii.TabItem("Highlight Info"))
                {
                    if (highlightInfoTab.Success)
                    {
                        DrawHighlightTab();
                    }
                }
            }
        }
    }

    public void DrawImage()
    {
        float availableWidth = ImGui.GetContentRegionAvail().X;
        IDalamudTextureWrap? kirboImage = Svc.Texture.GetFromFile(kirboImagePath).GetWrapOrDefault();
        // Use a fallback height if the image isn't available
        float childHeight = kirboImage != null ? kirboImage.Height + 15 : 50;
        using (var child = ImRaii.Child("SomeChildWithAScrollbar2##Some Child With A Scrollbar nr2", new Vector2(availableWidth, childHeight), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                if (kirboImage != null)
                {
                    ImGuiHelpers.CenterCursorFor(kirboImage.Width);
                    ImGui.Image(kirboImage.Handle, new Vector2(kirboImage.Width, kirboImage.Height));
                }
                else
                {
                    ImGui.TextUnformatted("Image not found.");
                }

                ImGuiHelpers.ScaledDummy(20.0f);
            }
        }
    }

    public void DrawPluginInfo()
    {
        using (var child = ImRaii.Child("SomeChildWithAScrollbar4", Vector2.Zero, true))
        {
            if (child.Success)
            {
                var playerInfoValue = configuration.DisplayPlayerInfoTab;
                if (ImGui.Checkbox("Enable player info tab?", ref playerInfoValue))
                {
                    configuration.DisplayPlayerInfoTab = playerInfoValue;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    configuration.Save();
                }
                var targetInfoValue = configuration.DisplayTargetInfoTab;
                if (ImGui.Checkbox("Enable target info tab?", ref targetInfoValue))
                {
                    configuration.DisplayTargetInfoTab = targetInfoValue;
                    configuration.Save();
                }
                var highlightValue = configuration.DisplayHighLightInfoTab;
                if (ImGui.Checkbox("Enable highlight tab?", ref highlightValue))
                {
                    configuration.DisplayHighLightInfoTab = highlightValue;

                    if (!highlightValue)
                    {
                        // Hiding the tab also switches the overlay off.
                        configuration.EnableHighLightOverlay = false;
                        configuration.HighlightPlayer = false;
                        plugin.TargetHighlightWindow.IsOpen = false;
                    }

                    configuration.Save();
                }

                // ShowInDevMenu is currently unused.
                //var showInDevMenuValue = Configuration.ShowInDevMenu;
                //if (ImGui.Checkbox("Show in dev menu", ref showInDevMenuValue))
                //{
                //    Configuration.ShowInDevMenu = showInDevMenuValue;
                //    Configuration.Save();
                //}

                // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
                // These expect formatting parameter if any part of the text contains a "%", which we can't
                // provide through our bindings, leading to a Crash to Desktop.
                // Replacements can be found in the ImGuiHelpers Class
                //ImGui.TextUnformatted($"The random config bool is {Plugin.Configuration.DisplayPlayerInfoTab}");

                if (ImGui.Button("Show Test window"))
                {
                    plugin.ToggleTestUI();
                }

                ImGui.Spacing();
            }
        }
    }

    public void DrawHighlightTab() => highlightTab.Draw();

}
