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
        private const string Header = "Scatter Indicator";

        public static void Register()
        {
            List<IConfigValue> configData = new List<IConfigValue>
            {
                new DropdownConfig(
                    "SideLinesLengthTiles",
                    Plugin.Config.SideLinesLengthTiles,
                    Header,
                    Plugin.Config.SideLinesLengthTiles,
                    "Length of the side scatter lines in tiles. Hide disables the lines.",
                    "Spread Cone Length",
                    new List<object> { "1", "2", "3", "Hide" }),
                new ConfigValue(
                    "ShowOneTileWidthPair",
                    Plugin.Config.ShowOneTileWidthPair,
                    Header,
                    Plugin.Config.ShowOneTileWidthPair,
                    "Show the pair of dots indicating the spread width of one tile.",
                    "Show One-Tile width Dots"),
                new ConfigValue(
                    "ShowCursorTilePair",
                    Plugin.Config.ShowCursorTilePair,
                    Header,
                    Plugin.Config.ShowCursorTilePair,
                    "Show the pair of dots at the target tile.",
                    "Show Cursor Tile Dots"),
            };

            ModConfigMenuAPI.RegisterModConfig("Scatter Indicator", configData, OnConfigSaved);
        }

        private static bool OnConfigSaved(Dictionary<string, object> currentConfig, out string feedbackMessage)
        {
            feedbackMessage = string.Empty;
            try
            {
                Plugin.Config.SideLinesLengthTiles = Convert.ToString(currentConfig["SideLinesLengthTiles"]);
                Plugin.Config.ShowOneTileWidthPair = Convert.ToBoolean(currentConfig["ShowOneTileWidthPair"]);
                Plugin.Config.ShowCursorTilePair = Convert.ToBoolean(currentConfig["ShowCursorTilePair"]);
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