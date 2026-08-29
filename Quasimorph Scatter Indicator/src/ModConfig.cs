using MGSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Quasimorph_Scatter_Indicator
{
    public class ModConfig
    {
        public const int DotSizePercentMin = 10;
        public const int DotSizePercentMax = 200;
        public const int DotSizePercentStep = 5;

        public int SideLinesLengthTiles { get; set; } = 2;
        public bool ShowOneTileWidthPair { get; set; } = true;
        public bool ShowCursorTilePair { get; set; } = true;
        public bool SmartActivation { get; set; } = true;
        public int DotSizePercent { get; set; } = 100;

        public static int ClampDotSizePercent(int value)
        {
            return Math.Max(DotSizePercentMin, Math.Min(DotSizePercentMax, value));
        }

        public static int SnapDotSizePercent(int value)
        {
            value = ClampDotSizePercent(value);
            return (int)Math.Round((double)value / DotSizePercentStep) * DotSizePercentStep;
        }

        public const int DotSizeSliderStepMin = 0;
        public const int DotSizeSliderStepMax = (DotSizePercentMax - DotSizePercentMin) / DotSizePercentStep;

        public static int PercentToSliderStep(int percent)
        {
            return (SnapDotSizePercent(percent) - DotSizePercentMin) / DotSizePercentStep;
        }

        public static int SliderStepToPercent(int step)
        {
            step = Math.Max(DotSizeSliderStepMin, Math.Min(DotSizeSliderStepMax, step));
            return DotSizePercentMin + step * DotSizePercentStep;
        }

        public void Save(string configPath)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            string json = JsonConvert.SerializeObject(this, serializerSettings);
            File.WriteAllText(configPath, json);
        }

        public static ModConfig LoadConfig(string configPath)
        {
            ModConfig config;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            if (File.Exists(configPath))
            {
                try
                {
                    string sourceJson = File.ReadAllText(configPath);

                    config = JsonConvert.DeserializeObject<ModConfig>(sourceJson, serializerSettings);
                    config.DotSizePercent = SnapDotSizePercent(config.DotSizePercent);

                    //Add any new elements that have been added since the last mod version the user had.
                    string upgradeConfig = JsonConvert.SerializeObject(config, serializerSettings);

                    if (upgradeConfig != sourceJson)
                    {
                        Plugin.Logger.Log("Updating config with missing elements");
                        //re-write
                        File.WriteAllText(configPath, upgradeConfig);
                    }


                    return config;
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError("Error parsing configuration.  Ignoring config file and using defaults");
                    Plugin.Logger.LogException(ex);

                    //Not overwriting in case the user just made a typo.
                    config = new ModConfig();
                    return config;
                }
            }
            else
            {
                config = new ModConfig();

                string json = JsonConvert.SerializeObject(config, serializerSettings);
                File.WriteAllText(configPath, json);

                return config;
            }


        }
    }
}
