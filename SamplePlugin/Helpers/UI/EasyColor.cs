using System.Numerics;
using Dalamud.Interface.Colors;

namespace SamplePlugin.Helpers.UI;

/// <summary>
/// A set of fancy color for use in plugins. You can redefine them to match necessary style!
/// </summary>
internal static class EasyColor
{
    public static uint RedBrightColor { get; set; }
    public static uint GreenBrightColor { get; set; }

    static EasyColor()
    {
        RedBrightColor = ((uint)(RedBright.W * 255) << 24) | ((uint)(RedBright.X * 255) << 16) | ((uint)(RedBright.Y * 255) << 8) | (uint)(RedBright.Z * 255);
        GreenBrightColor = ((uint)(GreenBright.W * 255) << 24) | ((uint)(GreenBright.X * 255) << 16) | ((uint)(GreenBright.Y * 255) << 8) | (uint)(GreenBright.Z * 255);
    }

    // Color Collection
    public readonly  static Vector4 Red = ImGuiExt.Vector4FromRGB(0xAA0000);
    public readonly  static Vector4 RedBright = ImGuiExt.Vector4FromRGB(0xFF0000);
    public readonly  static Vector4 Peach = ImGuiExt.Vector4FromRGB(0xFF6E59);
    public readonly  static Vector4 Maroon = ImGuiExt.Vector4FromRGB(0x800000);
    public readonly  static Vector4 Orange = ImGuiExt.Vector4FromRGB(0xAA5400);
    public readonly  static Vector4 Bronze = ImGuiExt.Vector4FromRGB(0xCD7F32);
    public readonly static Vector4 Indigo = ImGuiExt.Vector4FromRGB(0x4B0082);
    public readonly static Vector4 DarkType = ImGuiExt.Vector4FromRGB(0x5e1196);
    public readonly  static Vector4 GhostType = ImGuiExt.Vector4FromRGB(0xFF6E59FF);
    public readonly static Vector4 BrightGhostType = ImGuiExt.Vector4FromRGB(0xab57ff);
    public readonly static Vector4 Violet = ImGuiExt.Vector4FromRGB(0xAA00AA);
    public readonly static Vector4 Purple = ImGuiExt.Vector4FromRGB(0xAA0058);
    public readonly static Vector4 Fuchsia = ImGuiExt.Vector4FromRGB(0xAD0066);
    public readonly static Vector4 VioletBright = ImGuiExt.Vector4FromRGB(0xFF00FF);
    public readonly static Vector4 PastelPurple = ImGuiExt.Vector4FromRGB(0xd557ff);
    public readonly static Vector4 Pink = ImGuiExt.Vector4FromRGB(0xFF6FFF);
    public readonly static Vector4 PinkLight = ImGuiExt.Vector4FromRGB(0xFFABD6);
    public readonly static Vector4 Blue = ImGuiExt.Vector4FromRGB(0x0000AA);
    public readonly static Vector4 BlueBright = ImGuiExt.Vector4FromRGB(0x0000FF);
    public readonly static Vector4 BlueSea = ImGuiExt.Vector4FromRGB(0x0058AA);
    public readonly static Vector4 BlueSky = ImGuiExt.Vector4FromRGB(0x0085FF);
    public readonly static Vector4 Cyan = ImGuiExt.Vector4FromRGB(0x00AAAA);
    public readonly static Vector4 CyanBright = ImGuiExt.Vector4FromRGB(0x00FFFF);
    public readonly static Vector4 LightBlue = ImGuiExt.Vector4FromRGB(0xADD8E6);
    public readonly static Vector4 Lavender = ImGuiExt.Vector4FromRGB(0xE6E6FA);
    public readonly  static Vector4 Green = ImGuiExt.Vector4FromRGB(0x00AA00);
    public readonly  static Vector4 Olive = ImGuiExt.Vector4FromRGB(0x808000);
    public readonly  static Vector4 GreenBright = ImGuiExt.Vector4FromRGB(0x00FF00);
    public readonly  static Vector4 GreenLight = ImGuiExt.Vector4FromRGB(0xCCFF99);
    public readonly  static Vector4 Yellow = ImGuiExt.Vector4FromRGB(0xAAAA00);
    public readonly  static Vector4 Gold = ImGuiExt.Vector4FromRGB(0xFFD700);
    public readonly  static Vector4 YellowBright = ImGuiExt.Vector4FromRGB(0xFFFF00);
    public readonly  static Vector4 Lemon = ImGuiExt.Vector4FromRGB(0xFFFF00);
    public readonly  static Vector4 Black = ImGuiExt.Vector4FromRGB(0x000000);
    public readonly  static Vector4 Silver = ImGuiExt.Vector4FromRGB(0xC0C0C0);
    public readonly  static Vector4 White = ImGuiExt.Vector4FromRGB(0xFFFFFF);

    // Dalamud UI Colors
    public readonly  static Vector4 DalamudRed = ImGuiColors.DalamudRed;
    public readonly  static Vector4 DalamudGrey = ImGuiColors.DalamudGrey;
    public readonly  static Vector4 DalamudGrey2 = ImGuiColors.DalamudGrey2;
    public readonly  static Vector4 DalamudGrey3 = ImGuiColors.DalamudGrey3;
    public readonly  static Vector4 DalamudWhite = ImGuiColors.DalamudWhite;
    public readonly  static Vector4 DalamudWhite2 = ImGuiColors.DalamudWhite2;
    public readonly  static Vector4 DalamudOrange = ImGuiColors.DalamudOrange;
    public readonly  static Vector4 DalamudYellow = ImGuiColors.DalamudYellow;
    public readonly  static Vector4 DalamudViolet = ImGuiColors.DalamudViolet;

    // Job Role Colors
    public readonly  static Vector4 TankBlue = ImGuiColors.TankBlue;
    public readonly  static Vector4 HealerGreen = ImGuiColors.HealerGreen;
    public readonly  static Vector4 DPSRed = ImGuiColors.DPSRed;

    // FFLogs Parse Colors
    public readonly  static Vector4 ParsedGrey = ImGuiColors.ParsedGrey;
    public readonly  static Vector4 ParsedGreen = ImGuiColors.ParsedGreen;
    public readonly  static Vector4 ParsedBlue = ImGuiColors.ParsedBlue;
    public readonly  static Vector4 ParsedPurple = ImGuiColors.ParsedPurple;
    public readonly  static Vector4 ParsedOrange = ImGuiColors.ParsedOrange;
    public readonly  static Vector4 ParsedPink = ImGuiColors.ParsedPink;
    public readonly  static Vector4 ParsedGold = ImGuiColors.ParsedGold;
}
