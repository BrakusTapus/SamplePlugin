using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ImGuiNET;
using Lumina.Excel.Sheets;
using SamplePlugin.DalamudServices;
using SamplePlugin.Helpers.UI;
using SamplePlugin.Updaters;

namespace SamplePlugin.UI;

public class MainWindow : Window, IDisposable
{
    private string GoatImagePath;
    private Plugin Plugin;

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

        GoatImagePath = goatImagePath;
        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using (var tabheader = ImRaii.TabBar("ATabBar"))
        {
            if (tabheader.Success)
            {
                using (var tab1 = ImRaii.TabItem("Main Menu"))
                {
                    if (tab1.Success)
                    {
                        DrawImage();
                    }
                }
                using (var tab2 = ImRaii.TabItem("Player Info"))
                {
                    if (tab2.Success)
                    {
                        DrawPlayerInfo();
                    }
                }

                using (var tab3 = ImRaii.TabItem("Plugin Info"))
                {
                    if (tab3.Success)
                    {
                        DrawPluginInfo();
                    }
                }

                using (var tab4 = ImRaii.TabItem("Target Info"))
                {
                    if (tab4.Success)
                    {
                        DrawTargetInfo();
                    }
                }
            }
        }
    }

    public void DrawImage()
    {
        using (var child = ImRaii.Child("SomeChildWithAScrollbar2", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                //ImGui.TextUnformatted("Have a goat:");
                var goatImage = Svc.Texture.GetFromFile(GoatImagePath).GetWrapOrDefault();
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

    public static void DrawPlayerInfo()
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
                // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
                // These expect formatting parameter if any part of the text contains a "%", which we can't
                // provide through our bindings, leading to a Crash to Desktop.
                // Replacements can be found in the ImGuiHelpers Class
                ImGui.TextUnformatted($"The random config bool is {Plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

                if (ImGui.Button("Show Settings"))
                {
                    Plugin.ToggleConfigUI();
                }
                if (ImGui.Button("Request Redraw"))
                {
                    //Feature.namePlateEntries.Clear();
                    Service.NamePlateGui.RequestRedraw();
                }

                ImGui.Spacing();
            }
        }
    }

    public static void DrawTargetInfo()
    {
        foreach (Dalamud.Game.ClientState.Objects.Types.IBattleChara item in MainUpdater.AllTargets)
        {
            ImGui.TextUnformatted(item.Name.ToString());
            ImGui.TextUnformatted(item.GameObjectId.ToString());
        }

        ImGui.Separator();

        foreach (NamePlateEntry entry in NamePlateUpdater.AllNamePlates)
        {
            ImGui.TextUnformatted($"Name: {entry.Name}");
            ImGui.TextUnformatted($"NameIconId: {entry.NameIconId}");
            ImGui.TextUnformatted($"GameObjectId: {entry.GameObjectId.ToString()}");
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
