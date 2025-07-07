using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ImGuiNET;
using SamplePlugin.DalamudServices;

namespace SamplePlugin.Helpers.UI;
internal unsafe static class ImGuiExt
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

    /// <summary>
    /// Converts RGB color to <see cref="Vector4"/> for ImGui
    /// </summary>
    /// <param name="col">Color in format 0xRRGGBB</param>
    /// <param name="alpha">Optional transparency value between 0 and 1</param>
    /// <returns>Color in <see cref="Vector4"/> format ready to be used with <see cref="ImGui"/> functions</returns>
    public static Vector4 Vector4FromRGB(uint col, float alpha = 1.0f)
    {
        byte* bytes = (byte*)&col;
        return new Vector4((float)bytes[2] / 255f, (float)bytes[1] / 255f, (float)bytes[0] / 255f, alpha);
    }

    /// <summary>
    /// Converts RGBA color to <see cref="Vector4"/> for ImGui
    /// </summary>
    /// <param name="col">Color in format 0xRRGGBBAA</param>
    /// <returns>Color in <see cref="Vector4"/> format ready to be used with <see cref="ImGui"/> functions</returns>
    public static Vector4 Vector4FromRGBA(uint col)
    {
        byte* bytes = (byte*)&col;
        return new Vector4((float)bytes[3] / 255f, (float)bytes[2] / 255f, (float)bytes[1] / 255f, (float)bytes[0] / 255f);
    }

    /// <summary>
    /// Returns a color based on the provided percentage value.
    /// </summary>
    public static Vector4 GetParsedColor(int percent)
    {
        if (percent < 25)
        {
            return ImGuiColors.ParsedGrey;
        }
        else if (percent < 50)
        {
            return ImGuiColors.ParsedGreen;
        }
        else if (percent < 75)
        {
            return ImGuiColors.ParsedBlue;
        }
        else if (percent < 95)
        {
            return ImGuiColors.ParsedPurple;
        }
        else if (percent < 99)
        {
            return ImGuiColors.ParsedOrange;
        }
        else if (percent == 99)
        {
            return ImGuiColors.ParsedPink;
        }
        else if (percent == 100)
        {
            return ImGuiColors.ParsedGold;
        }
        else
        {
            return ImGuiColors.DalamudRed;
        }
    }
}
