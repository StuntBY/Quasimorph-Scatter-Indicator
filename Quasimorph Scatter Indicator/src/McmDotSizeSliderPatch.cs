using HarmonyLib;
using System;
using System.Globalization;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quasimorph_Scatter_Indicator
{
    internal static class McmDotSizeSliderPatch
    {
        private const string ConfigKey = "DotSizePercent";
        private static bool applied;

        public static void TryApply(Harmony harmony)
        {
            if (applied || harmony == null)
            {
                return;
            }

            Type mcmUi = AccessTools.TypeByName("ModConfigMenu.ModConfigMenu, MCM")
                ?? AccessTools.TypeByName("ModConfigMenu.ModConfigMenu");
            MethodInfo method = mcmUi == null ? null : AccessTools.Method(mcmUi, "CreateRangeControl");
            if (method == null)
            {
                return;
            }

            harmony.Patch(method, postfix: new HarmonyMethod(typeof(McmDotSizeSliderPatch), nameof(Postfix)));
            applied = true;
        }

        private static void Postfix(
            object config,
            float initialValue,
            Action<float> setUnstoredFloat,
            GameObject __result)
        {
            if (config == null || __result == null || setUnstoredFloat == null)
            {
                return;
            }

            PropertyInfo keyProperty = config.GetType().GetProperty("Key");
            if (keyProperty?.GetValue(config) as string != ConfigKey)
            {
                return;
            }

            Slider slider = __result.GetComponentInChildren<Slider>(true);
            TMP_InputField manualText = __result.GetComponentInChildren<TMP_InputField>(true);
            if (slider == null || manualText == null)
            {
                return;
            }

            int initialStep = Mathf.RoundToInt(initialValue);
            ApplyStepDisplay(slider, manualText, initialStep);

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(newVal =>
            {
                ApplyStepUserChange(slider, manualText, setUnstoredFloat, Mathf.RoundToInt(newVal));
            });

            manualText.onEndEdit.RemoveAllListeners();
            manualText.onEndEdit.AddListener(text =>
            {
                if (string.IsNullOrEmpty(text))
                {
                    text = ModConfig.DotSizePercentMin.ToString(CultureInfo.InvariantCulture);
                }

                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedPercent))
                {
                    return;
                }

                ApplyStepUserChange(slider, manualText, setUnstoredFloat, ModConfig.PercentToSliderStep(parsedPercent));
            });
        }

        private static void ApplyStepDisplay(Slider slider, TMP_InputField manualText, int stepIndex)
        {
            int step = Mathf.Clamp(stepIndex, ModConfig.DotSizeSliderStepMin, ModConfig.DotSizeSliderStepMax);
            int percent = ModConfig.SliderStepToPercent(step);
            if (!Mathf.Approximately(slider.value, step))
            {
                slider.SetValueWithoutNotify(step);
            }

            manualText.SetTextWithoutNotify(percent.ToString(CultureInfo.InvariantCulture));
        }

        private static void ApplyStepUserChange(Slider slider, TMP_InputField manualText, Action<float> setUnstoredFloat, int stepIndex)
        {
            int step = Mathf.Clamp(stepIndex, ModConfig.DotSizeSliderStepMin, ModConfig.DotSizeSliderStepMax);
            ApplyStepDisplay(slider, manualText, step);
            setUnstoredFloat(step);
        }
    }
}
