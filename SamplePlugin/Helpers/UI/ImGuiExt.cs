using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using SamplePlugin.DalamudServices;

namespace SamplePlugin.Helpers.UI;
internal static unsafe class ImGuiExt
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
        ImGui.Image(texture.Handle, windowSize);
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


    //private static readonly Dictionary<ImGuiCol, Vector4>   ImGuiColToVector4   = [];
    //private static readonly Dictionary<ImGuiCol, uint>      ImGuiColToUInt      = [];
    //private static readonly Dictionary<KnownColor, Vector4> KnownColorToVector4 = [];
    //private static readonly Dictionary<KnownColor, uint>    KnownColorToUInt    = [];
    //private static readonly Dictionary<uint, Vector4>       UIntToVector4       = [];

    ////extension(ImGuiCol imguiCol)
    ////{
    //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    //public Vector4 ToVector4() =>
    //    //    ImGuiColToVector4.GetOrAdd(imguiCol, _ => ImGui.GetColorU32(imguiCol).ToVector4());

    //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    //public uint ToUInt() =>
    //    //    ImGuiColToUInt.GetOrAdd(imguiCol, _ => ImGui.GetColorU32(imguiCol));
    ////}

    ////extension(KnownColor knownColor)
    ////{
    ////    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ////    public Vector4 ToVector4() =>
    ////        KnownColorToVector4.GetOrAdd(knownColor, _ => knownColor.Vector());

    ////    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ////    public uint ToUInt() =>
    ////        KnownColorToUInt.GetOrAdd(knownColor, _ => knownColor.ToVector4().ToUInt());
    ////}

    //extension(uint color)
    //{
    //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    //public Vector4 ToVector4() =>
    //    //    UIntToVector4.GetOrAdd(color, _ => ImGui.ColorConvertU32ToFloat4(color));

    //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    //public Vector4 GetVector4UIColor() =>
    //    //    AtkStage.Instance()->AtkUIColorHolder->GetColor(true, color).ToVector4();

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public uint GetUIntUIColor() =>
    //        AtkStage.Instance()->AtkUIColorHolder->GetColor(true, color);
    //}

    //extension(scoped in Vector4 color)
    //{
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public Vector3 ToVector3() =>
    //        new(color.X, color.Y, color.Z);

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public Vector4 Invert() =>
    //        color with { X = 1f - color.X, Y = 1f - color.Y, Z = 1f - color.Z };

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public ByteColor ToByteColor()
    //    {
    //        var r = (byte)Math.Round(Math.Clamp(color.X, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
    //        var g = (byte)Math.Round(Math.Clamp(color.Y, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
    //        var b = (byte)Math.Round(Math.Clamp(color.Z, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
    //        var a = (byte)Math.Round(Math.Clamp(color.W, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);

    //        return new ByteColor { R = r, G = g, B = b, A = a };
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public uint ToUInt() =>
    //        ImGui.ColorConvertFloat4ToU32(color);
    //}

    //extension(ByteColor color)
    //{
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public Vector4 ToVector4()
    //    {
    //        var r = color.R / 255f;
    //        var g = color.G / 255f;
    //        var b = color.B / 255f;
    //        var a = color.A / 255f;

    //        return new Vector4(r, g, b, a);
    //    }
    //}
}
