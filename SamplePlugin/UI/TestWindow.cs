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
            MinimumSize = new Vector2(375, 300),
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
        // Use Belias.png as the logo from the Assets folder in output directory
        try
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
                float scale = 0.8f; // Scale down the logo if needed
                ImGui.Image(logoImage.ImGuiHandle, new Vector2(logoImage.Width * scale, logoImage.Height * scale));
            }
        }
        catch (Exception ex)
        {
            // If we can't load the image, just don't display it
            Svc.Log.Error($"Failed to load logo: {ex.Message}");
        }

        ImGui.TextUnformatted("Kirbo - Test Window");
        ImGui.Separator();
        ImGui.TextUnformatted("Create and manage rotations for Rotation Solver Reborn with ease.");
        ImGui.Spacing();

        // Reference class member to ensure non-static
        _ = Plugin;
    }
}
