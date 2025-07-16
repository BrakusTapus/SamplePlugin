using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ImGuiNET;
using Lumina.Excel.Sheets;
using SamplePlugin.Configs;
using SamplePlugin.DalamudServices;
using SamplePlugin.Helpers.UI;
using SamplePlugin.Updaters;

namespace SamplePlugin.UI;

public class MainWindow : Window, IDisposable
{
    private string GoatImagePath;
    private Plugin Plugin;
    private Configuration Configuration;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar)
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

        GoatImagePath = goatImagePath;
        Plugin = plugin;
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using (var tabheader = ImRaii.TabBar("ATabBar"))
        {
            if (tabheader.Success)
            {
                using (var mainMenuTab = ImRaii.TabItem("Main Menu"))
                {
                    if (mainMenuTab.Success)
                    {
                        DrawImage();
                        DrawPluginInfo();
                    }
                }
                var playerInfoValue = Configuration.DisplayPlayerInfoTab;
                if (playerInfoValue)
                {
                    using (var playerInfoTab = ImRaii.TabItem("Player Info"))
                    {
                        if (playerInfoTab.Success)
                        {
                            DrawPlayerInfo();
                        }
                    }
                }

                var targetInfoValue = Configuration.DisplayTargetInfoTab;
                if (targetInfoValue)
                {
                    using (var targetInfoTab = ImRaii.TabItem("Target Info"))
                    {
                        if (targetInfoTab.Success)
                        {
                            DrawTargetInfo();
                        }
                    }
                }

                var highlightValue = Configuration.DisplayHighLightInfoTab;
                if (highlightValue)
                {
                    using (var highlightInfoTab = ImRaii.TabItem("Highlight Info"))
                    {
                        if (highlightInfoTab.Success)
                        {
                            DrawHighlightTab();
                        }
                    }
                }
                if (!Configuration.DisplayHighLightInfoTab)
                {
                    if (Configuration.EnableHighLightOverlay)
                    {
                        Configuration.EnableHighLightOverlay = false;
                    }

                    if (Configuration.HighlightPlayer)
                    {
                        Configuration.HighlightPlayer = false;
                    }

                    if (Configuration.HighlightAllGameObjects)
                    {
                        Configuration.HighlightAllGameObjects = false;
                    }
                }
            }
        }
    }

    public void DrawImage()
    {
        float availableWidth = ImGui.GetContentRegionAvail().X;
        var goatImage = Svc.Texture.GetFromFile(GoatImagePath).GetWrapOrDefault();
        using (var child = ImRaii.Child("SomeChildWithAScrollbar2", new Vector2(availableWidth, goatImage.Height + 15), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                if (goatImage != null)
                {
                    ImGuiHelpers.CenterCursorFor(goatImage.Width);
                    ImGui.Image(goatImage.ImGuiHandle, new Vector2(goatImage.Width, goatImage.Height));
                }
                else
                {
                    ImGui.TextUnformatted("Image not found.");
                }

                ImGuiHelpers.ScaledDummy(20.0f);
            }
        }
    }

    public unsafe static void DrawPlayerInfo()
    {
        using (var child = ImRaii.Child("SomeChildWithAScrollbar3", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                // Example for other services that Dalamud provides.
                // ClientState provides a wrapper filled with information about the local player object and client.

                var localPlayer = Svc.ClientState.LocalPlayer;
                if (localPlayer == null)
                {
                    ImGui.TextUnformatted("Our local player is currently not loaded.");
                    return;
                }

                if (!localPlayer.ClassJob.IsValid)
                {
                    ImGui.TextUnformatted("Our current job is currently not valid.");
                    return;
                }

                // ExtractText() should be the preferred method to read Lumina SeStrings,
                // as ToString does not provide the actual text values, instead gives an encoded macro string.
                ImGui.TextUnformatted($"Our current job is ({localPlayer.ClassJob.RowId}) \"{localPlayer.ClassJob.Value.Abbreviation.ExtractText()}\"");
                ImGui.TextUnformatted($"Hitbox Radius: ({localPlayer.HitboxRadius})");
                ImGui.SliderFloat($"Hitbox Radius###{localPlayer.Name}{localPlayer.EntityId}", ref ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)localPlayer.Address)->HitboxRadius, 0f, 100f);

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

            }
        }
    }

    public void DrawPluginInfo()
    {
        using (var child = ImRaii.Child("SomeChildWithAScrollbar4", Vector2.Zero, true))
        {
            if (child.Success)
            {
                var playerInfoValue = Configuration.DisplayPlayerInfoTab;
                if (ImGui.Checkbox("Enable player info tab?", ref playerInfoValue))
                {
                    Configuration.DisplayPlayerInfoTab = playerInfoValue;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    Configuration.Save();
                }
                var targetInfoValue = Configuration.DisplayTargetInfoTab;
                if (ImGui.Checkbox("Enable target info tab?", ref targetInfoValue))
                {
                    Configuration.DisplayTargetInfoTab = targetInfoValue;
                    Configuration.Save();
                }
                var highlightValue = Configuration.DisplayHighLightInfoTab;
                if (ImGui.Checkbox("Enable highlight tab?", ref highlightValue))
                {
                    Configuration.DisplayHighLightInfoTab = highlightValue;
                    Configuration.Save();
                }

                // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
                // These expect formatting parameter if any part of the text contains a "%", which we can't
                // provide through our bindings, leading to a Crash to Desktop.
                // Replacements can be found in the ImGuiHelpers Class
                //ImGui.TextUnformatted($"The random config bool is {Plugin.Configuration.DisplayPlayerInfoTab}");

                if (ImGui.Button("Show Test window"))
                {
                    Plugin.ToggleTestUI();
                }

                ImGui.Spacing();
            }
        }
    }

    public unsafe static void DrawTargetInfo()
    {
        if (ImGui.Button("Request Redraw"))
        {
            Service.NamePlateGui.RequestRedraw();
        }
        ImGui.SameLine();
        if (ImGui.Button("Clear List"))
        {
            NamePlateUpdater.ClearList();
        }
        ImGui.Separator();

        foreach (Dalamud.Game.ClientState.Objects.Types.IBattleChara item in MainUpdater.AllTargets)
        {
            ImGui.TextUnformatted(item.Name.ToString());
            ImGui.TextUnformatted(item.GameObjectId.ToString());
            ImGui.SliderFloat($"Hitbox Radius###{item.Name}{item.EntityId}", ref ((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)item.Address)->HitboxRadius, 0f, 100f);
        }
        ImGui.Separator();
        foreach (NamePlateEntry entry in NamePlateUpdater.AllNamePlates)
        {
            if (ImGui.BeginTable($"NamePlateTable_{entry.GameObjectId}", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInner | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                // Define fixed width for both columns
                ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 100f);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed, 200f);

                Dalamud.Interface.Textures.TextureWraps.IDalamudTextureWrap? icon2 = ImGuiExt.GetGameIconTexture((uint)entry.NameIconId).GetWrapOrDefault();
                if (icon2 != null)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextUnformatted("Name");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextUnformatted(entry.Name);
                    ImGui.SameLine();
                    ImGui.Image(icon2.ImGuiHandle, new Vector2(22, 22));

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.TextUnformatted($"NameIconId: {entry.NameIconId}");
                        ImGui.EndTooltip();
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.TextUnformatted("GameObjectId");
                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted(entry.GameObjectId.ToString());

                ImGui.EndTable();
            }

            // Add vertical spacing between entries
            ImGui.Dummy(new Vector2(0, 10));
        }

        //foreach (NamePlateEntry entry in NamePlateUpdater.AllNamePlates)
        //{
        //    Dalamud.Interface.Textures.TextureWraps.IDalamudTextureWrap? icon2 = ImGuiExt.GetGameIconTexture((uint)entry.NameIconId).GetWrapOrDefault();
        //    ImGui.TextUnformatted($"Name: {entry.Name}");
        //    ImGui.TextUnformatted($"NameIconId: {entry.NameIconId}");
        //    if (icon2 != null)
        //    {
        //        ImGui.SameLine();
        //        ImGui.Image(icon2.ImGuiHandle, new Vector2(22, 22));
        //    }
        //    ImGui.TextUnformatted($"GameObjectId: {entry.GameObjectId.ToString()}");
        //}
    }

    public void DrawHighlightTab()
    {
        var highlightOverlayValue = Configuration.EnableHighLightOverlay;
        if (ImGui.Checkbox("Enable highlight overlay?", ref highlightOverlayValue))
        {
            Configuration.EnableHighLightOverlay = highlightOverlayValue;
            Plugin.TargetHighlightWindow.IsOpen = true;
            Configuration.Save();
        }
        var highlightPlayer = Configuration.HighlightPlayer;
        var highlightGameObjects = Configuration.HighlightAllGameObjects;
        if (highlightOverlayValue)
        {
            ImGui.Indent();
            if (ImGui.Checkbox("Highlight Player?", ref highlightPlayer))
            {
                Configuration.HighlightPlayer = highlightPlayer;
                Configuration.Save();
            }
            if (ImGui.Checkbox("Highlight All GameObjects?", ref highlightGameObjects))
            {
                Configuration.HighlightAllGameObjects = highlightGameObjects;
                Configuration.Save();
            }
        }

    }

    //public void DrawNamePlates()
    //{
    //    using (ImRaii.IEndObject child = ImRaii.Child("NamePlatesChild", Vector2.Zero, true))
    //    {
    //        if (child.Success)
    //        {
    //            if (ImGui.Button("Request Redraw"))
    //            {
    //                Feature.namePlateEntries.Clear();
    //                Service.NamePlateGui.RequestRedraw();
    //            }
    //            if (Feature.namePlateEntries.Count == 0)
    //            {
    //                ImGui.TextUnformatted("No relevant nameplates detected.");
    //            }
    //            else
    //            {
    //                foreach (NamePlateEntry entry in Feature.namePlateEntries)
    //                {
    //                    Dalamud.Interface.Textures.TextureWraps.IDalamudTextureWrap? icon2 = ImGuiExt.GetGameIconTexture((uint)entry.NameIconId).GetWrapOrDefault();
    //                    ImGui.TextUnformatted($"Name: {entry.Name}");
    //                    ImGui.TextUnformatted($"ObjectId: {entry.ObjectId:X8}");
    //                    ImGui.TextUnformatted($"NameIconId: {entry.NameIconId}");
    //                    if (icon2 != null)
    //                    {
    //                        ImGui.SameLine();
    //                        ImGui.Image(icon2.ImGuiHandle, new Vector2(22, 22));
    //                    }
    //                    ImGui.TextUnformatted($"IsBoss: {entry.IsBoss}");
    //                    ImGui.Separator();
    //                }
    //            }
    //        }

    //    }
    //}

}

// Normally a BeginChild() would have to be followed by an unconditional EndChild(),
// ImRaii takes care of this after the scope ends.
// This works for all ImGui functions that require specific handling, examples are BeginTable() or Indent().
//using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
//{
//    // Check if this child is drawing
//    if (child.Success)
//    {
//        ImGui.TextUnformatted("Have a goat:");
//        var goatImage = Service.TextureProvider.GetFromFile(GoatImagePath).GetWrapOrDefault();
//        if (goatImage != null)
//        {
//            using (ImRaii.PushIndent(55f))
//            {
//                ImGui.Image(goatImage.ImGuiHandle, new Vector2(goatImage.Width, goatImage.Height));
//            }
//        }
//        else
//        {
//            ImGui.TextUnformatted("Image not found.");
//        }

//        ImGuiHelpers.ScaledDummy(20.0f);

//        // Example for other services that Dalamud provides.
//        // ClientState provides a wrapper filled with information about the local player object and client.

//        var localPlayer = Service.ClientState.LocalPlayer;
//        if (localPlayer == null)
//        {
//            ImGui.TextUnformatted("Our local player is currently not loaded.");
//            return;
//        }

//        if (!localPlayer.ClassJob.IsValid)
//        {
//            ImGui.TextUnformatted("Our current job is currently not valid.");
//            return;
//        }

//        // ExtractText() should be the preferred method to read Lumina SeStrings,
//        // as ToString does not provide the actual text values, instead gives an encoded macro string.
//        ImGui.TextUnformatted($"Our current job is ({localPlayer.ClassJob.RowId}) \"{localPlayer.ClassJob.Value.Abbreviation.ExtractText()}\"");

//        // Example for quarrying Lumina directly, getting the name of our current area.
//        var territoryId = Service.ClientState.TerritoryType;
//        if (Service.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
//        {
//            ImGui.TextUnformatted($"We are currently in ({territoryId}) \"{territoryRow.PlaceName.Value.Name.ExtractText()}\"");
//        }
//        else
//        {
//            ImGui.TextUnformatted("Invalid territory.");
//        }
//    }
//}

//public void DrawImage()
//{
//    using (var child = ImRaii.Child("SomeChildWithAScrollbar2", Vector2.Zero, true))
//    {
//        // Check if this child is drawing
//        if (child.Success)
//        {
//            ImGui.TextUnformatted("Have a goat:");
//            var goatImage = Service.TextureProvider.GetFromFile(GoatImagePath).GetWrapOrDefault();
//            if (goatImage != null)
//            {
//                using (ImRaii.PushIndent(55f))
//                {
//                    ImGui.Image(goatImage.ImGuiHandle, new Vector2(goatImage.Width, goatImage.Height));
//                }
//            }
//            else
//            {
//                ImGui.TextUnformatted("Image not found.");
//            }

//            ImGuiHelpers.ScaledDummy(20.0f);

//            // Example for other services that Dalamud provides.
//            // ClientState provides a wrapper filled with information about the local player object and client.

//            var localPlayer = Service.ClientState.LocalPlayer;
//            if (localPlayer == null)
//            {
//                ImGui.TextUnformatted("Our local player is currently not loaded.");
//                return;
//            }

//            if (!localPlayer.ClassJob.IsValid)
//            {
//                ImGui.TextUnformatted("Our current job is currently not valid.");
//                return;
//            }

//            // ExtractText() should be the preferred method to read Lumina SeStrings,
//            // as ToString does not provide the actual text values, instead gives an encoded macro string.
//            ImGui.TextUnformatted($"Our current job is ({localPlayer.ClassJob.RowId}) \"{localPlayer.ClassJob.Value.Abbreviation.ExtractText()}\"");

//            // Example for quarrying Lumina directly, getting the name of our current area.
//            var territoryId = Service.ClientState.TerritoryType;
//            if (Service.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
//            {
//                ImGui.TextUnformatted($"We are currently in ({territoryId}) \"{territoryRow.PlaceName.Value.Name.ExtractText()}\"");
//            }
//            else
//            {
//                ImGui.TextUnformatted("Invalid territory.");
//            }
//        }
//    }
//}
