using HarmonyLib;
using ModConfigMenu;
using ModConfigMenu.Contracts;
using ModConfigMenu.Objects;
using System;
using System.Collections.Generic;

namespace Quasimorph_Scatter_Indicator
{
    internal static class McmConfig
    {
        public static void Register(Harmony harmony)
        {
            ModLocalization.Register();
            McmDotSizeSliderPatch.TryApply(harmony);

            List<IConfigValue> configData = new List<IConfigValue>
            {
                new ConfigValue(
                    "SideLinesLengthTiles",
                    Plugin.Config.SideLinesLengthTiles,
                    ModLocalization.ModName,
                    2,
                    ModLocalization.SideLinesTooltip,
                    ModLocalization.SideLinesLabel,
                    0f,
                    15f),
                new ConfigValue(
                    "ShowOneTileWidthPair",
                    Plugin.Config.ShowOneTileWidthPair,
                    ModLocalization.ModName,
                    Plugin.Config.ShowOneTileWidthPair,
                    ModLocalization.OneTileTooltip,
                    ModLocalization.OneTileLabel),
                new ConfigValue(
                    "ShowCursorTilePair",
                    Plugin.Config.ShowCursorTilePair,
                    ModLocalization.ModName,
                    Plugin.Config.ShowCursorTilePair,
                    ModLocalization.CursorTileTooltip,
                    ModLocalization.CursorTileLabel),
                new ConfigValue(
                    "SmartActivation",
                    Plugin.Config.SmartActivation,
                    ModLocalization.ModName,
                    Plugin.Config.SmartActivation,
                    ModLocalization.SmartActivationTooltip,
                    ModLocalization.SmartActivationLabel),
                new ConfigValue(
                    "DotSizePercent",
                    ModConfig.PercentToSliderStep(Plugin.Config.DotSizePercent),
                    ModLocalization.ModName,
                    ModConfig.PercentToSliderStep(100),
                    ModLocalization.DotSizeTooltip,
                    ModLocalization.DotSizeLabel,
                    ModConfig.DotSizeSliderStepMin,
                    ModConfig.DotSizeSliderStepMax),
            };

            ModConfigMenuAPI.RegisterModConfig(ModLocalization.ModDisplayName, configData, OnConfigSaved);
        }

        private static bool OnConfigSaved(Dictionary<string, object> currentConfig, out string feedbackMessage)
        {
            feedbackMessage = string.Empty;
            try
            {
                Plugin.Config.SideLinesLengthTiles = Convert.ToInt32(currentConfig["SideLinesLengthTiles"]);
                Plugin.Config.ShowOneTileWidthPair = Convert.ToBoolean(currentConfig["ShowOneTileWidthPair"]);
                Plugin.Config.ShowCursorTilePair = Convert.ToBoolean(currentConfig["ShowCursorTilePair"]);
                Plugin.Config.SmartActivation = Convert.ToBoolean(currentConfig["SmartActivation"]);
                int dotSizeStep = Convert.ToInt32(currentConfig["DotSizePercent"]);
                Plugin.Config.DotSizePercent = ModConfig.SliderStepToPercent(dotSizeStep);
                Plugin.Config.Save(Plugin.ConfigDirectories.ConfigPath);
                return true;
            }
            catch (Exception ex)
            {
                feedbackMessage = ex.Message;
                return false;
            }
        }
    }
}