using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.ImGuiMethods;
using Lumina.Excel.Sheets;
using KirboToolbox.Configs;
using KirboToolbox.Helpers.Extensions;
using KirboToolbox.Helpers.UI;
using KirboToolbox.Updaters;

namespace KirboToolbox.UI;

public class MainWindow : Window, IDisposable
{
    private readonly string kirboImagePath;
    private readonly Plugin plugin;
    private readonly Configuration configuration;
    private readonly HighlightTab highlightTab;

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
                using (var playerInfoTab = ImRaii.TabItem("Player Info"))
                {
                    if (playerInfoTab.Success)
                    {
                        DrawPlayerInfo();
                    }
                }
            }

            if (configuration.DisplayTargetInfoTab)
            {
                using (var targetInfoTab = ImRaii.TabItem("Target Info"))
                {
                    if (targetInfoTab.Success)
                    {
                        DrawTargetInfo();
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

    public unsafe void DrawPlayerInfo()
    {
        using (var child = ImRaii.Child("SomeChildWithAScrollbar3", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                // Example for other services that Dalamud provides.
                // ClientState provides a wrapper filled with information about the local player object and client.

                var playerState = Svc.PlayerState;
                if (playerState == null || !playerState.IsLoaded)
                {
                    ImGui.Text("IPlayerState is either null or not loaded!");
                    return;
                }

                if (!playerState.ClassJob.IsValid)
                {
                    ImGui.Text("Our current job is currently not valid.");
                    return;
                }

                var localPlayer = Svc.Objects.LocalPlayer;
                if (localPlayer == null)
                {
                    ImGui.Text("Local player is unavailable.");
                    return;
                }

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Current job:");
                ImGui.SameLine(120 * ImGuiHelpers.GlobalScale);
                // ExtractText() should be the preferred method to read Lumina SeStrings,
                // as ToString does not provide the actual text values, instead gives an encoded macro string.
                ImGuiExt.DrawJobIcon(playerState);
                ImGui.SameLine();
                ImGui.Text(playerState.ClassJob.Value.Abbreviation.ToString());
                ImGui.NewLine();

                var speed = configuration.SpeedMultiplier;
                var step = configuration.SpeedStep;

                ImGui.Text("Current movement speed: " + localPlayer.CurrentSpeed.ToString("F3"));
                ImGui.Text("Movement speed multiplier: " + localPlayer.Speed.ToString("F3"));
                ImGui.NewLine();

                ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
                if (ImGui.SliderFloat("##SpeedSlider", ref speed, 0.5f, 5f, "%.2f"))
                {
                    speed = MathF.Round(speed, 2);
                    configuration.SpeedMultiplier = speed;
                    localPlayer.Speed = speed;
                    configuration.Save();
                }

                ImGui.SameLine();
                if (ImGui.Button("-##SpeedDown"))
                {
                    speed = MathF.Max(0.1f, MathF.Round(speed - step, 2));
                    configuration.SpeedMultiplier = speed;
                    localPlayer.Speed = speed;
                    configuration.Save();
                }

                ImGui.SameLine();
                if (ImGui.Button("+##SpeedUp"))
                {
                    speed = MathF.Round(speed + step, 2);
                    configuration.SpeedMultiplier = speed;
                    localPlayer.Speed = speed;
                    configuration.Save();
                }

                ImGui.SameLine();
                if (ImGui.Button("Reset##SpeedReset"))
                {
                    configuration.SpeedMultiplier = 1f;
                    localPlayer.Speed = 1f;
                    configuration.Save();
                }

                ImGui.SetNextItemWidth(80 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputFloat("Step size", ref step, 0f, 0f, "%.2f"))
                {
                    configuration.SpeedStep = Math.Clamp(step, 0.01f, 1f);
                    configuration.Save();
                }
                ImGui.NewLine();

                ImGui.TextUnformatted($"Hitbox Radius: ({localPlayer.HitboxRadius})");
                ImGui.SliderFloat($"Hitbox Radius###{playerState.CharacterName}{playerState.EntityId}", ref ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)localPlayer.Address)->HitboxRadius, 0f, 100f);
                ImGui.SameLine();
                if (ImGui.Button("Reset##HitboxRadius"))
                {
                    ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)localPlayer.Address)->HitboxRadius = 0.5f;
                }
                ImGui.NewLine();

                // Example for quarrying Lumina directly, getting the name of our current area.
                var territoryId = Svc.ClientState.TerritoryType;
                if (Svc.Data.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
                {
                    ImGui.TextUnformatted($"We are currently in ({territoryId}) \"{territoryRow.PlaceName.Value.Name.ExtractText()}\"");
                }
                else
                {
                    ImGui.TextUnformatted("Invalid territory.");
                }


                var target = Svc.Targets.Target as IBattleChara;
                var actionmanagerCheck = target != null && FFXIVClientStructs.FFXIV.Client.Game.ActionManager.CanUseActionOnTarget(142, (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Struct());
                if (target != null)
                {
                    ImGui.Text("can use on target " + actionmanagerCheck);
                }
                else
                {
                    ImGui.Text("target is NULL");
                }

            }
        }
    }

    public unsafe static void DrawTargetInfo()
    {
        if (ImGui.Button("Redraw Nameplate"))
        {
            Svc.NamePlates.RequestRedraw();
        }
        ImGuiEx.Tooltip("Send a redraw request for nameplates");
        ImGui.SameLine();
        if (ImGui.Button("Clear List"))
        {
            NamePlateUpdater.ClearList();
        }
        ImGuiEx.Tooltip("Clears the listed nameplates");
        ImGui.SameLine();
        ImGui.TextUnformatted("AllNamePlates: " + NamePlateUpdater.AllNamePlates.Count());
        ImGui.Separator();

        float availableHeight = ImGui.GetContentRegionAvail().Y;
        float availableWidth = ImGui.GetContentRegionAvail().X;
        using (var child1 = ImRaii.Child("targetinfochild1", new Vector2(availableWidth / 2, availableHeight - 10), true, ImGuiWindowFlags.NoScrollbar))
        {
            if (child1.Success)
            {

                var targetId = Svc.Targets.Target?.GameObjectId ?? 0;
                var nameplate2 = NamePlateUpdater.AllNamePlates.FirstOrDefault(n => n.GameObjectId == targetId);
                if (nameplate2 != null)
                {
                    ImGui.TextUnformatted($"Direct: IconID = {nameplate2.NameIconId}");
                    ImGui.TextUnformatted($"1 = {nameplate2.Name}");
                    ImGui.TextUnformatted($"2 = {nameplate2.IsBoss}");
                    ImGui.TextUnformatted($"3 = {nameplate2.GameObjectId}");
                }
                else
                {
                    ImGui.TextUnformatted("Direct: NOT FOUND");
                }
            }
        }
        ImGui.SameLine();
        using (var child2 = ImRaii.Child("targetinfochild2", new Vector2((availableWidth / 2) - 10, availableHeight - 10), true, ImGuiWindowFlags.NoScrollbar))
        {
            if (child2.Success)
            {
                foreach (NamePlateEntry entry in NamePlateUpdater.AllNamePlates)
                {
                    if (!entry.IsTargetable)
                    {
                        continue;
                    }
                    if (ImGui.BeginTable($"NamePlateTable_{entry.GameObjectId}", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInner | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                    {
                        // Define fixed width for both columns
                        ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 150f);
                        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed, 200f);

                        IDalamudTextureWrap? icon2 = ImGuiExt.GetGameIconTexture((uint)entry.NameIconId)?.GetWrapOrDefault();

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted("Name");
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(entry.Name);

                        if (icon2 != null)
                        {
                            ImGui.SameLine();
                            ImGui.Image(icon2.Handle, new Vector2(22, 22));

                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.TextUnformatted($"NameIconId: {entry.NameIconId}");
                                ImGui.EndTooltip();
                            }
                        }

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted("GameObjectId:");
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(entry.GameObjectId.ToString());

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted("Boss from icon:");
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(entry.IsBoss.ToString());

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted("Targetable:");
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(entry.IsTargetable.ToString());

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted("Hitbox Radius:");
                        ImGui.TableSetColumnIndex(1);

                        if (entry.BattleChara != null && entry.BattleChara.Address != IntPtr.Zero)
                        {
                            ImGui.SetNextItemWidth(100);
                            var gameObject = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)entry.BattleChara.Address;
                            ImGui.SliderFloat(
                                $"##HitboxRadiusSlider{entry.BattleChara.Name}{entry.BattleChara.EntityId}",
                                ref gameObject->HitboxRadius,
                                0f, 100f
                            );
                        }
                        else
                        {
                            ImGui.TextUnformatted("N/A (No BattleChara)");
                        }

                        ImGui.EndTable();
                    }

                    // Add vertical spacing between entries
                    ImGui.Dummy(new Vector2(0, 10));
                }
            }
        }
    }

    public void DrawHighlightTab() => highlightTab.Draw();

}
