using Dalamud.Configuration;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;

namespace SamplePlugin;


public class Configuration : IPluginConfiguration
{
    public enum ImageAlignment
    {
        TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight
    }
    public int Version { get; set; } = 1;
    
 // public SourceConfig Sources = new();
    /// <summary>
    /// Value between 0 and 100 in percent
    /// </summary>
    public float GuiMainOpacity;
    public bool GuiMainShowResize;
    public bool GuiMainShowTitleBar = true;
    public bool GuiMainAllowResize = true;
    public bool GuiMainVisible;
    public bool GuiMainLocked;

    /// <summary>
    /// Value between 0 and 300 in percent
    /// </summary>
    public float GIFSpeed = 100f;

    public bool ShowHeaders = true;

    public ImageAlignment Alignment = ImageAlignment.Center;
    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
    
    /*  
    public CombinedSource LoadSources()
    {
        combined.AddSource(Mock.LoadSources());
        return combined;
    }
    */

    public static Configuration Load()
    {
        try
        {
            return Plugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        }
        catch (System.Exception ex)
        {
            PluginLog.LogWarning(ex, "Could not load Neko Fans config");
            return new Configuration();
        }
    }

    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

}
