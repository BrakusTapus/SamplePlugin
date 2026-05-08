using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;
using SamplePlugin.Helpers.UI;

namespace SamplePlugin.UI;
public class TestWindow : Window, IDisposable
{
    private Plugin Plugin;
    private bool disposedValue;
    public TestWindow(Plugin plugin) : base("Test - Window##TestWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(150, 100),
            MaximumSize = new Vector2(3440, 1440)
        };
        Plugin = plugin;
    }

    //protected virtual void Dispose(bool disposing)
    //{
    //    if (!disposedValue)
    //    {
    //        if (disposing)
    //        {
    //            // Dispose managed resources here if needed.
    //        }

    //        disposedValue = true;
    //    }
    //}

    //public void Dispose()
    //{
    //    Dispose(disposing: true);
    //    GC.SuppressFinalize(this);
    //}

    public void Dispose() { }

    public override void Draw()
    {
        DrawBody();
    }

    private void DrawHeader()
    {
        // Use logo.png as the logo from the Assets folder in output directory
        try
        { 
            // Get available width in the current ImGui window
            float availableWidth = ImGui.GetContentRegionAvail().X;
            ImGui.SetCursorPos(new Vector2(5, 0));
            using (var child = ImRaii.Child("SomeChildWithAScrollbar4", new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (child.Success)
                {
                    var assemblyDir = Svc.PluginInterface.AssemblyLocation.Directory?.FullName!;
                    var logoPath = System.IO.Path.Combine(assemblyDir, "Assets", "logo.png");

                    if (!System.IO.File.Exists(logoPath))
                    {
                        // Try looking in another location
                        logoPath = System.IO.Path.Combine(assemblyDir, "logo.png");
                    }

                    var logoImage = Svc.Texture.GetFromFile(logoPath).GetWrapOrDefault();
                    if (logoImage != null)
                    {
                        // Maintain aspect ratio
                        float imageAspectRatio = (float)logoImage.Width / logoImage.Height;

                        // Set max width or scale image based on available space
                        float desiredWidth = Math.Min(availableWidth, logoImage.Width);
                        float desiredHeight = desiredWidth / imageAspectRatio;
                        ImGuiHelpers.CenterCursorFor(desiredWidth);
                        // Render the image at scaled size
                        ImGui.Image(logoImage.Handle, new Vector2(desiredWidth, desiredHeight));
                    }
                    ImGui.Text("Kirbo - Test Window");
                    ImGui.Separator();
                    ImGui.Text("placeholder.");
                    ImGui.Spacing();
                }
            }
        }
        catch (Exception ex)
        {
            // If we can't load the image, just don't display it
            Svc.Log.Error($"Failed to load logo: {ex.Message}");
        }
        // Reference class member to ensure non-static
        _ = Plugin;
    }

    private void DrawBody()
    {
        var windowHeight = ImGui.GetWindowHeight();
        var windowWidth = ImGui.GetWindowWidth();
        Vector2 windowSize = new Vector2(windowWidth, windowHeight);
        Vector2 initialCursorPos = ImGui.GetCursorPos(); // Get the current cursor position to account for the title bar and menu bar

        DrawBackground();

        // Calculate the height for the Icons window
        float iconSize = 48;
        float iconPadding = 5;
        float iconWindowHeight = iconSize + (2 * iconPadding);
        DrawHeader();
        //DrawIcons(windowSize, initialCursorPos, iconWindowHeight, iconPadding, iconSize);

        // Reserve space for bottom buttons
        float bottomButtonHeight = 70;
        float remainingHeight = windowSize.Y - initialCursorPos.Y - iconWindowHeight - bottomButtonHeight - 20; // Adjusted for padding and separators

        //DrawEasyCombatBody(windowSize, initialCursorPos, iconWindowHeight, remainingHeight);

        //DrawBottomButtons(windowSize, bottomButtonHeight);
    }

    public static void DrawBackground()
    {
        var windowHeight = ImGui.GetWindowHeight();
        var windowWidth = ImGui.GetWindowWidth();
        ImGui.SetCursorPos(new Vector2(0, 0));
        using (var background = ImRaii.Child("background", new Vector2(windowWidth, windowHeight), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground))
        {
            if (background)
            {
                ImGuiExt.DrawBackgroundFill(193902);
            }
        }
    }
}
