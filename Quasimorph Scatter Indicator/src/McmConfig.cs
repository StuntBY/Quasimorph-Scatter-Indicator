using HarmonyLib;
using ModConfigMenu;
using ModConfigMenu.Contracts;
using ModConfigMenu.Implementations;
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
                new DropdownConfig(
                    "ConeDisplayMode",
                    ModLocalization.LocalizeMode(Plugin.Config.ConeDisplayMode),
                    ModLocalization.ModName,
                    ModLocalization.LocalizeMode(Plugin.Config.ConeDisplayMode),
                    ModLocalization.ConeTooltip,
                    ModLocalization.ConeLabel,
                    BuildModeOptions()),
                new DropdownConfig(
                    "OneTileWidthMode",
                    ModLocalization.LocalizeMode(Plugin.Config.OneTileWidthMode),
                    ModLocalization.ModName,
                    ModLocalization.LocalizeMode(Plugin.Config.OneTileWidthMode),
                    ModLocalization.OneTileTooltip,
                    ModLocalization.OneTileLabel,
                    BuildModeOptions()),
                new DropdownConfig(
                    "CursorTileMode",
                    ModLocalization.LocalizeMode(Plugin.Config.CursorTileMode),
                    ModLocalization.ModName,
                    ModLocalization.LocalizeMode(Plugin.Config.CursorTileMode),
                    ModLocalization.CursorTileTooltip,
                    ModLocalization.CursorTileLabel,
                    BuildModeOptions()),
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

        private static List<object> BuildModeOptions()
        {
            return new List<object>
            {
                ModLocalization.GetText(ModLocalization.ModeNever),
                ModLocalization.GetText(ModLocalization.ModeOnlyWithShift),
                ModLocalization.GetText(ModLocalization.ModeWithoutShift),
                ModLocalization.GetText(ModLocalization.ModeAlways),
            };
        }

        private static bool OnConfigSaved(Dictionary<string, object> currentConfig, out string feedbackMessage)
        {
            feedbackMessage = string.Empty;
            try
            {
                Plugin.Config.SideLinesLengthTiles = Convert.ToInt32(currentConfig["SideLinesLengthTiles"]);
                Plugin.Config.ConeDisplayMode = ModLocalization.ResolveModeKey(Convert.ToString(currentConfig["ConeDisplayMode"]));
                Plugin.Config.OneTileWidthMode = ModLocalization.ResolveModeKey(Convert.ToString(currentConfig["OneTileWidthMode"]));
                Plugin.Config.CursorTileMode = ModLocalization.ResolveModeKey(Convert.ToString(currentConfig["CursorTileMode"]));
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