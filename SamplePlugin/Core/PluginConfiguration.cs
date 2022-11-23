using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Utility;
using Newtonsoft.Json;
using SamplePlugin.Services;


namespace SamplePlugin.Core
{
    /// <summary> Plugin configuration. </summary>
    [Serializable]
    public class PluginConfiguration : IPluginConfiguration
    {
        static PluginConfiguration()
        {

        }

        #region Version

        /// <summary> Gets or sets the configuration version. </summary>
        public int Version { get; set; } = 5;

        #endregion

        #region Settings Options
        
        /// <summary> Gets or sets a value indicating whether to output combat log to the chatbox. </summary>
        public bool EnabledOutputLog { get; set; } = false;

        /// <summary> Gets or sets a value indicating whether to hide combos which conflict with enabled presets. </summary>
        public bool HideConflictedCombos { get; set; } = false;

        /// <summary> Gets or sets a value indicating whether to hide the children of a feature if it is disabled. </summary>
        public bool HideChildren { get; set; } = false;

        /// <summary> Gets or sets the offset of the melee range check. Default is 0. </summary>
        public double MeleeOffset { get; set; } = 0;

        #region Tab Options 

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

        #region Other (SpecialEvent, MotD, Save)

        /// <summary> Hides the message of the day. </summary>
        public bool HideMessageOfTheDay { get; set; } = false;

        /// <summary> Save the configuration to disk. </summary>
        public void Save() => Service.Interface.SavePluginConfig(this);

        #endregion
    }
}
