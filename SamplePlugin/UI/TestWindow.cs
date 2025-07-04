using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace SamplePlugin.UI;
public class TestWindow : Window, IDisposable
{
    private Plugin Plugin;
    private bool disposedValue;
    public TestWindow(Plugin plugin) : base("Test - Window##TestWindow", ImGuiWindowFlags.None)
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
        DrawHeader();
    }

    private void DrawHeader()
    {
        // Use kirbo.png as the logo from the Assets folder in output directory
        try
        {
            // Get available width in the current ImGui window
            float availableWidth = ImGui.GetContentRegionAvail().X;
            using (var child = ImRaii.Child("SomeChildWithAScrollbar4", new Vector2(availableWidth, 300), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (child.Success)
                {
                    var assemblyDir = Svc.PluginInterface.AssemblyLocation.Directory?.FullName!;
                    var logoPath = System.IO.Path.Combine(assemblyDir, "Assets", "kirbo.png");

                    if (!System.IO.File.Exists(logoPath))
                    {
                        // Try looking in another location
                        logoPath = System.IO.Path.Combine(assemblyDir, "kirbo.png");
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
                        ImGui.Image(logoImage.ImGuiHandle, new Vector2(desiredWidth, desiredHeight));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // If we can't load the image, just don't display it
            Svc.Log.Error($"Failed to load logo: {ex.Message}");
        }

        ImGui.TextUnformatted("Kirbo - Test Window");
        ImGui.Separator();
        ImGui.TextUnformatted("placeholder.");
        ImGui.Spacing();

        // Reference class member to ensure non-static
        _ = Plugin;
    }
}
