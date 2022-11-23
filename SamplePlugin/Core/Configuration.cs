using Dalamud.Configuration;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Utility;
using Newtonsoft.Json;
using SamplePlugin.Services;

namespace SamplePlugin.Core;

/// <summary> This class stores configurations settings for the plugin. </summary>
public class Configuration : IPluginConfiguration
{
    #region Plugin Configuration Versions

    /// <summary> Gets or sets the configuration version. </summary>
    public int Version { get; set; } = 0;

    #endregion

    #region Save Plugin Configuration

    /// <summary> xx </summary>
    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);

    #endregion

    #region Load Plugin Configuration

    /// <summary> xx </summary>
    public static Configuration Load()
    {
        try
        {
            return Plugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        }
        catch (Exception ex)
        {
            PluginLog.LogWarning(ex, "Could not load config");
            return new Configuration();
        }
    }

    #endregion

    #region Gui Options/Settings

    #region Gui Tab Options 

    /// <summary> Enables PvE Tab </summary>
    public bool HidePvEFeatures { get; set; } = false;

    /// <summary> Enables PvP Tab </summary>
    public bool HidePvPFeatures { get; set; } = false;

    /// <summary> Enables Settings Tab </summary>
    public bool HideSettings { get; set; } = false;

    /// <summary> Enables About Tab </summary>
    public bool HideAboutSloth = true;

    /// <summary> Enables Debug Tab </summary>
    public bool HideDebug { get; set; } = false;

    #endregion

    #region Gui Main Window settings/options

    /// <summary> Value between 0 and 100 in percent. </summary>
    public float GuiMainOpacity;

    /// <summary> Wether or not to show the resize button on the corner of the Main window. </summary>
    public bool GuiMainShowResize;

    /// <summary> Wether or not to show the Title bar of the Main window. </summary>
    public bool GuiMainShowTitleBar = true;

    /// <summary> Wether or not to allow the resizing of the Main window. </summary>
    public bool GuiMainAllowResize = true;

    /// <summary> Determines wether or not the Main window is visible. </summary>
    public bool GuiMainVisible;

    /// <summary> Determines wether or not the Main window can be moved around. </summary>
    public bool GuiMainLocked;

    /// <summary> Value between 0 and 300 in percent </summary>
    public float GIFSpeed = 100f;

    #endregion Gui Main settings

    #region Gui Config Window settings/options

    /// <summary> Show or hide the image at the top of the window. </summary>
    public bool ShowHeaders = true;

    #endregion


    #endregion

    #region Plugin Settings/Options

    #region ImageAlignment
    /// <summary> Enum for Image alignment options. </summary>
    public enum ImageAlignment
    {
        TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight
    }

    /// <summary> I think this just makes 'Center' the default position. </summary>
    public ImageAlignment Alignment = ImageAlignment.Center;
    #endregion

    #region Custom Bool Values

    [JsonProperty]
    private static Dictionary<string, bool> CustomBoolValues { get; set; } = new Dictionary<string, bool>();

    /// <summary> Gets a custom boolean value. </summary>
    public static bool GetCustomBoolValue(string config)
    {
        if (!CustomBoolValues.TryGetValue(config, out bool configValue))
        {
            SetCustomBoolValue(config, false);
            return false;
        }

        return configValue;
    }

    /// <summary> Sets a custom boolean value. </summary>
    public static void SetCustomBoolValue(string config, bool value) => CustomBoolValues[config] = value;

    #endregion

    #region Custom Bool Array Values

    [JsonProperty]
    private static Dictionary<string, bool[]> CustomBoolArrayValues { get; set; } = new Dictionary<string, bool[]>();

    /// <summary> Gets a custom boolean array value. </summary>
    public static bool[] GetCustomBoolArrayValue(string config)
    {
        if (!CustomBoolArrayValues.TryGetValue(config, out bool[]? configValue))
        {
            SetCustomBoolArrayValue(config, Array.Empty<bool>());
            return Array.Empty<bool>();
        }

        return configValue;
    }

    /// <summary> Sets a custom boolean array value. </summary>
    public static void SetCustomBoolArrayValue(string config, bool[] value) => CustomBoolArrayValues[config] = value;

    #endregion

    #region Custom Int Values

    [JsonProperty]
    private static Dictionary<string, int> CustomIntValues { get; set; } = new Dictionary<string, int>();

    /// <summary> Gets a custom integer value. </summary>
    public static int GetCustomIntValue(string config, int defaultMinVal = 0)
    {
        if (!CustomIntValues.TryGetValue(config, out int configValue))
        {
            SetCustomIntValue(config, defaultMinVal);
            return defaultMinVal;
        }

        return configValue;
    }

    /// <summary> Sets a custom integer value. </summary>
    public static void SetCustomIntValue(string config, int value) => CustomIntValues[config] = value;

    #endregion

    #region Custom Float Values

    [JsonProperty]
    private static Dictionary<string, float> CustomFloatValues { get; set; } = new Dictionary<string, float>();

    /// <summary> Gets a custom float value. </summary>
    public static float GetCustomFloatValue(string config, float defaultMinValue = 0)
    {
        if (!CustomFloatValues.TryGetValue(config, out float configValue))
        {
            SetCustomFloatValue(config, defaultMinValue);
            return defaultMinValue;
        }

        return configValue;
    }

    /// <summary> Sets a custom float value. </summary>
    public static void SetCustomFloatValue(string config, float value) => CustomFloatValues[config] = value;

    #endregion

    #region SomePropertyToBeSavedAndWithADefault

    /// <summary> Property that by default retrusn 'true' and has to be saved </summary>
    /// <summary> A bool is a Property </summary>
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true; // 

    #endregion

    #endregion

}
