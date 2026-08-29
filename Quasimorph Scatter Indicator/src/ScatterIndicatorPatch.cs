using HarmonyLib;
using MGSC;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Quasimorph_Scatter_Indicator
{
    [HarmonyPatch(typeof(SelectTargetView), "DrawLinearEffectiveRangeLine")]
    internal static class ScatterIndicatorPatch
    {
        private const float DefaultScatterAngleDegrees = 5f;
        private const float DotSpacingFactor = 0.18f;
        private const int ScatterSortingOrder = 10000;

        internal static readonly HashSet<GameObject> ScatterDots = new HashSet<GameObject>();

        private static readonly MethodInfo TakeSelectionObjectMethod =
            AccessTools.Method(typeof(SelectTargetView), "TakeSelectionObject");

        private static void Postfix(SelectTargetView __instance, Vector3 startPos, Vector3 endPos, float effectiveDistanceWorld)
        {
            bool shiftHeld = IsPreciseShootMode();

            var traverse = Traverse.Create(__instance);
            var creatures = traverse.Field("_creatures").GetValue<Creatures>();
            var mapRenderer = traverse.Field("_mapRenderer").GetValue<MapRenderer>();
            if (creatures == null || mapRenderer == null)
            {
                return;
            }

            ScaleVanillaCenterLineDots(traverse);

            Player player = creatures.Player;
            BasePickupItem currentWeapon = player?.CreatureData?.Inventory?.CurrentWeapon;
            WeaponRecord weaponRecord = currentWeapon?.Record<WeaponRecord>();
            if (currentWeapon == null || weaponRecord == null || weaponRecord.IsMelee
                || weaponRecord.WeaponClass == WeaponClass.GrenadeLauncher)
            {
                return;
            }

            bool coneVisible = ModeAllows(Plugin.Config?.ConeDisplayMode, shiftHeld);
            bool cursorPairVisible = ModeAllows(Plugin.Config?.CursorTileMode, shiftHeld);
            bool oneTileVisible = ModeAllows(Plugin.Config?.OneTileWidthMode, shiftHeld);

            if (!coneVisible && !cursorPairVisible && !oneTileVisible)
            {
                return;
            }

            Vector3 aimDelta = endPos - startPos;
            if (aimDelta.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float scatterAngle = GetWeaponScatterAngle(player, currentWeapon);
            int sideLinesLengthTiles = Mathf.Max(0, Plugin.Config?.SideLinesLengthTiles ?? 1);
            bool hideConeLines = sideLinesLengthTiles <= 0;
            float maxLength = sideLinesLengthTiles * mapRenderer.WorldTileSize.x;
            float dotSpacing = mapRenderer.WorldTileSize.x * DotSpacingFactor;
            Vector3 aimDirection = aimDelta.normalized;

            float tileSize = mapRenderer.WorldTileSize.x;
            float targetDistance = aimDelta.magnitude;
            Vector3 perpendicular = new Vector3(-aimDirection.y, aimDirection.x, 0f);

            float coneDrawLength = maxLength;
            if (cursorPairVisible && !hideConeLines && targetDistance < maxLength)
            {
                float cosAngle = Mathf.Cos(Mathf.Abs(scatterAngle) * Mathf.Deg2Rad);
                if (cosAngle > 0.0001f)
                {
                    coneDrawLength = Mathf.Min(maxLength, targetDistance / cosAngle);
                }
            }

            if (coneVisible && !hideConeLines)
            {
                DrawScatterLine(__instance, traverse, startPos, aimDirection, scatterAngle, coneDrawLength, dotSpacing);
                DrawScatterLine(__instance, traverse, startPos, aimDirection, -scatterAngle, coneDrawLength, dotSpacing);
            }

            if (cursorPairVisible)
            {
                float spreadOffsetAtTarget = targetDistance * Mathf.Tan(scatterAngle * Mathf.Deg2Rad);
                DrawScatterDot(__instance, traverse, startPos, endPos + perpendicular * spreadOffsetAtTarget, effectiveDistanceWorld);
                DrawScatterDot(__instance, traverse, startPos, endPos - perpendicular * spreadOffsetAtTarget, effectiveDistanceWorld);
            }

            float oneTileSpreadDistance = tileSize / (2f * Mathf.Tan(scatterAngle * Mathf.Deg2Rad));
            if (oneTileVisible
                && oneTileSpreadDistance > maxLength
                && oneTileSpreadDistance <= targetDistance)
            {
                Vector3 oneTilePoint = startPos + aimDirection * oneTileSpreadDistance;
                DrawScatterDot(__instance, traverse, startPos, oneTilePoint + perpendicular * (tileSize * 0.5f), effectiveDistanceWorld);
                DrawScatterDot(__instance, traverse, startPos, oneTilePoint - perpendicular * (tileSize * 0.5f), effectiveDistanceWorld);
            }
        }

        private static float DotScale
        {
            get
            {
                int percent = Plugin.Config?.DotSizePercent ?? 100;
                return Mathf.Clamp(percent, 10, 200) / 100f;
            }
        }

        private static void ApplyDotScale(GameObject dot)
        {
            float scale = DotScale;
            dot.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static void ScaleVanillaCenterLineDots(Traverse viewTraverse)
        {
            List<GameObject> borders = viewTraverse.Field("_borders").GetValue<List<GameObject>>();
            if (borders == null)
            {
                return;
            }

            foreach (GameObject dot in borders)
            {
                if (dot == null || ScatterDots.Contains(dot))
                {
                    continue;
                }

                ApplyDotScale(dot);
                ScatterDots.Add(dot);
            }
        }

        private static bool IsPreciseShootMode()
        {
            InputController input = SingletonMonoBehaviour<InputController>.Instance;
            return input != null && input.IsKey("HighlightPreciseShoot");
        }

        private static bool ModeAllows(string mode, bool shiftHeld)
        {
            switch (mode ?? ModConfig.DisplayModeAlways)
            {
                case ModConfig.DisplayModeNever:
                    return false;
                case ModConfig.DisplayModeOnlyWithShift:
                    return shiftHeld;
                case ModConfig.DisplayModeWithoutShift:
                    return !shiftHeld;
                default:
                    return true;
            }
        }

        private static float GetWeaponScatterAngle(Player player, BasePickupItem weapon)
        {
            try
            {
                float scatterAngle = player.CreatureData.GetScatterAngle(weapon);
                if (scatterAngle > 0f)
                {
                    return scatterAngle;
                }
            }
            catch
            {
                // Fall back to the conventional default scatter angle.
            }

            return DefaultScatterAngleDegrees;
        }

        private static void DrawScatterLine(
            SelectTargetView view,
            Traverse viewTraverse,
            Vector3 origin,
            Vector3 aimDirection,
            float angleOffsetDegrees,
            float maxLength,
            float dotSpacing)
        {
            Vector3 direction = RotateDirection(aimDirection, angleOffsetDegrees);
            Pool bordersPool = viewTraverse.Field("_bordersPool").GetValue<Pool>();
            List<GameObject> borders = viewTraverse.Field("_borders").GetValue<List<GameObject>>();
            Sprite greenDot = viewTraverse.Field("_shootGreenDotSp").GetValue<Sprite>();
            if (bordersPool == null || borders == null || greenDot == null || TakeSelectionObjectMethod == null)
            {
                return;
            }

            for (float distance = 0f; distance <= maxLength; distance += dotSpacing)
            {
                GameObject dot = TakeSelectionObjectMethod.Invoke(view, new object[] { bordersPool }) as GameObject;
                if (dot == null)
                {
                    break;
                }

                dot.transform.position = origin + direction * distance;
                SpriteRenderer renderer = dot.GetComponent<SpriteRenderer>();
                renderer.sprite = greenDot;
                renderer.sortingOrder = ScatterSortingOrder;
                ApplyDotScale(dot);
                ScatterDots.Add(dot);
                borders.Add(dot);
            }
        }

        private static void DrawScatterDot(
            SelectTargetView view,
            Traverse viewTraverse,
            Vector3 startPos,
            Vector3 position,
            float effectiveDistanceWorld)
        {
            Pool bordersPool = viewTraverse.Field("_bordersPool").GetValue<Pool>();
            List<GameObject> borders = viewTraverse.Field("_borders").GetValue<List<GameObject>>();
            Sprite greenDot = viewTraverse.Field("_shootGreenDotSp").GetValue<Sprite>();
            Sprite redDot = viewTraverse.Field("_shootRedDotSp").GetValue<Sprite>();
            if (bordersPool == null || borders == null || greenDot == null || redDot == null
                || TakeSelectionObjectMethod == null)
            {
                return;
            }

            GameObject dot = TakeSelectionObjectMethod.Invoke(view, new object[] { bordersPool }) as GameObject;
            if (dot == null)
            {
                return;
            }

            dot.transform.position = position;
            SpriteRenderer renderer = dot.GetComponent<SpriteRenderer>();
            renderer.sprite = Vector3.Distance(startPos, position) <= effectiveDistanceWorld ? greenDot : redDot;
            renderer.sortingOrder = ScatterSortingOrder;
            ApplyDotScale(dot);
            ScatterDots.Add(dot);
            borders.Add(dot);
        }

        private static Vector3 RotateDirection(Vector3 direction, float angleDeltaDegrees)
        {
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float angle = (baseAngle + angleDeltaDegrees) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        }
    }

    [HarmonyPatch(typeof(SelectTargetView), "FreeBorders")]
    internal static class ScatterBorderResetPatch
    {
        private static void Postfix()
        {
            foreach (GameObject dot in ScatterIndicatorPatch.ScatterDots)
            {
                if (dot != null && dot.TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer))
                {
                    renderer.sortingOrder = 0;
                    dot.transform.localScale = Vector3.one;
                }
            }

            ScatterIndicatorPatch.ScatterDots.Clear();
        }
    }
}
