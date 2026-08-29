using MGSC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static MGSC.Localization;

namespace Quasimorph_Scatter_Indicator
{
    internal static class ModLocalization
    {
        private const string Prefix = "scatter_indicator.";
        private const string LocalizationFileName = "localization.json";

        public const string ModName = Prefix + "mod.name";
        public const string SideLinesLabel = Prefix + "sidelines.label";
        public const string SideLinesTooltip = Prefix + "sidelines.tooltip";
        public const string OneTileLabel = Prefix + "onetile.label";
        public const string OneTileTooltip = Prefix + "onetile.tooltip";
        public const string CursorTileLabel = Prefix + "cursortile.label";
        public const string CursorTileTooltip = Prefix + "cursortile.tooltip";
        public const string ConeLabel = Prefix + "cone.label";
        public const string ConeTooltip = Prefix + "cone.tooltip";
        public const string ModeNever = Prefix + "mode.never";
        public const string ModeOnlyWithShift = Prefix + "mode.shift";
        public const string ModeWithoutShift = Prefix + "mode.noshift";
        public const string ModeAlways = Prefix + "mode.always";
        public const string DotSizeLabel = Prefix + "dotsize.label";
        public const string DotSizeTooltip = Prefix + "dotsize.tooltip";

        private static readonly FieldInfo DbField = typeof(Localization).GetField(
            "db",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static Dictionary<string, Dictionary<Lang, string>> entries;

        /// <summary>Имя мода в боковой панели MCM (не через Localization.Get).</summary>
        public static string ModDisplayName => Resolve(ModName, Lang.EnglishUS);

        public static void Register()
        {
            object dbObject = DbField?.GetValue(Singleton<Localization>.Instance);
            if (!(dbObject is Dictionary<Lang, Dictionary<string, string>> db))
            {
                return;
            }

            foreach (KeyValuePair<string, Dictionary<Lang, string>> entry in Entries)
            {
                foreach (KeyValuePair<Lang, string> translation in entry.Value)
                {
                    if (!db.TryGetValue(translation.Key, out Dictionary<string, string> dict))
                    {
                        continue;
                    }

                    if (!dict.ContainsKey(entry.Key))
                    {
                        dict.Add(entry.Key, translation.Value);
                    }
                }
            }
        }

        private static Dictionary<string, Dictionary<Lang, string>> Entries
        {
            get
            {
                if (entries == null)
                {
                    entries = LoadFromFile();
                }

                return entries;
            }
        }

        private static string Resolve(string key, Lang lang)
        {
            if (!Entries.TryGetValue(key, out Dictionary<Lang, string> byLang))
            {
                return key;
            }

            if (byLang.TryGetValue(lang, out string value))
            {
                return value;
            }

            if (byLang.TryGetValue(Lang.EnglishUS, out value))
            {
                return value;
            }

            return key;
        }

        private static Dictionary<string, Dictionary<Lang, string>> LoadFromFile()
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(assemblyDir))
            {
                return new Dictionary<string, Dictionary<Lang, string>>();
            }

            string path = Path.Combine(assemblyDir, LocalizationFileName);
            if (!File.Exists(path))
            {
                return new Dictionary<string, Dictionary<Lang, string>>();
            }

            try
            {
                string json = File.ReadAllText(path);
                Dictionary<string, Dictionary<string, string>> raw = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
                if (raw == null)
                {
                    return new Dictionary<string, Dictionary<Lang, string>>();
                }

                Dictionary<string, Dictionary<Lang, string>> result = new Dictionary<string, Dictionary<Lang, string>>();
                foreach (KeyValuePair<string, Dictionary<string, string>> entry in raw)
                {
                    Dictionary<Lang, string> byLang = new Dictionary<Lang, string>();
                    foreach (KeyValuePair<string, string> translation in entry.Value)
                    {
                        if (Enum.TryParse(translation.Key, out Lang lang))
                        {
                            byLang[lang] = translation.Value;
                        }
                    }

                    result[entry.Key] = byLang;
                }

                return result;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[Scatter Indicator] Не удалось загрузить {LocalizationFileName}: {ex.Message}");
                return new Dictionary<string, Dictionary<Lang, string>>();
            }
        }

        public static string GetText(string key)
        {
            string value = MGSC.Localization.Get(key);
            if (!string.IsNullOrEmpty(value) && value != key)
            {
                return value;
            }

            return Resolve(key, Lang.EnglishUS);
        }

        public static string LocalizeMode(string mode)
        {
            switch (mode ?? ModConfig.DisplayModeAlways)
            {
                case ModConfig.DisplayModeNever:
                    return GetText(ModeNever);
                case ModConfig.DisplayModeOnlyWithShift:
                    return GetText(ModeOnlyWithShift);
                case ModConfig.DisplayModeWithoutShift:
                    return GetText(ModeWithoutShift);
                default:
                    return GetText(ModeAlways);
            }
        }

        public static string ResolveModeKey(string localizedValue)
        {
            string[] modeKeys = { ModeNever, ModeOnlyWithShift, ModeWithoutShift, ModeAlways };
            foreach (string key in modeKeys)
            {
                if (Entries.TryGetValue(key, out Dictionary<Lang, string> byLang)
                    && byLang.Values.Contains(localizedValue))
                {
                    return ModeKeyFromLocalizationKey(key);
                }
            }

            return ModConfig.DisplayModeAlways;
        }

        private static string ModeKeyFromLocalizationKey(string key)
        {
            if (key == ModeNever) return ModConfig.DisplayModeNever;
            if (key == ModeOnlyWithShift) return ModConfig.DisplayModeOnlyWithShift;
            if (key == ModeWithoutShift) return ModConfig.DisplayModeWithoutShift;
            return ModConfig.DisplayModeAlways;
        }
    }
}
