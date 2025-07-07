using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ImGuiNET;
using SamplePlugin.DalamudServices;

namespace SamplePlugin.Helpers.UI;
internal static class ImGuiExt
{
    /// <summary>
    /// Obtain an icon texture in the game using its ID.
    /// </summary>
    /// <param name="iconId"></param>
    /// <returns></returns>
    internal static ISharedImmediateTexture GetGameIconTexture(uint iconId)
    {
        var path = Svc.Texture.GetIconPath(new GameIconLookup(iconId));
        return Svc.Texture.GetFromGame(path);
    }

    /// <summary>
    /// Round (i) Image with a tooltip
    /// </summary>
    /// <param name="desc">What gets shown on hover</param>
    internal static void HelpMarker(string desc)
    {
        if (desc.Length == 0)
            return;

        ImGuiComponents.HelpMarker(desc);
    }

    internal static void DrawBackgroundFill(uint textureId)
    {
        // Load the texture based on the given ID
        if (!GetTexture(textureId, out var texture))
            return;

        // Get the current window size
        Vector2 windowSize = ImGui.GetWindowSize();

        // Draw the background image, stretching it to fill the window size
        ImGui.SetCursorPos(Vector2.Zero);
        ImGui.Image(texture.ImGuiHandle, windowSize);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="texture"></param>
    /// <param name="default"></param>
    /// <returns></returns>
    public static bool GetTexture(uint id, out IDalamudTextureWrap texture, uint @default = 0)
        => ThreadLoadImageHandler.TryGetIconTextureWrap(id, true, out texture)
        || ThreadLoadImageHandler.TryGetIconTextureWrap(@default, true, out texture)
        || ThreadLoadImageHandler.TryGetIconTextureWrap(0, true, out texture);

}
