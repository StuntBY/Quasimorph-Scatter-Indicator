using MGSC;
using System.Collections.Generic;
using System.Reflection;
using static MGSC.Localization;

namespace Quasimorph_Scatter_Indicator
{
    internal static class ModLocalization
    {
        private const string Prefix = "scatter_indicator.";

        public const string ModName = Prefix + "mod.name";
        public const string SideLinesLabel = Prefix + "sidelines.label";
        public const string SideLinesTooltip = Prefix + "sidelines.tooltip";
        public const string OneTileLabel = Prefix + "onetile.label";
        public const string OneTileTooltip = Prefix + "onetile.tooltip";
        public const string CursorTileLabel = Prefix + "cursortile.label";
        public const string CursorTileTooltip = Prefix + "cursortile.tooltip";
        public const string SmartActivationLabel = Prefix + "smart.label";
        public const string SmartActivationTooltip = Prefix + "smart.tooltip";

        private static readonly FieldInfo DbField = typeof(Localization).GetField(
            "db",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly Dictionary<string, Dictionary<Lang, string>> Entries = BuildEntries();

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

        private static Dictionary<string, Dictionary<Lang, string>> BuildEntries()
        {
            return new Dictionary<string, Dictionary<Lang, string>>
            {
                [ModName] = Row(
                    "Scatter Indicator",
                    "Индикатор разброса",
                    "Streuungsanzeige",
                    "Indicateur de dispersion",
                    "Indicador de dispersión",
                    "Wskaźnik rozrzutu",
                    "散布指示器"),
                [SideLinesLabel] = Row(
                    "Spread Cone Length",
                    "Длина конуса разброса",
                    "Länge des Streukegels",
                    "Longueur du cône de dispersion",
                    "Longitud del cono de dispersión",
                    "Długość stożka rozrzutu",
                    "散布锥长度"),
                [SideLinesTooltip] = Row(
                    "Length of the side scatter lines in tiles. 0 hides the lines.",
                    "Длина боковых линий разброса в клетках. 0 скрывает линии.",
                    "Länge der seitlichen Streulinien in Feldern. 0 blendet die Linien aus.",
                    "Longueur des lignes latérales de dispersion en cases. 0 masque les lignes.",
                    "Longitud de las líneas laterales de dispersión en casillas. 0 oculta las líneas.",
                    "Długość bocznych linii rozrzutu w polach. 0 ukrywa linie.",
                    "侧边散布线的格子长度。0 会隐藏线条。"),
                [OneTileLabel] = Row(
                    "Show One-Tile Width Dots",
                    "Точки ширины одной клетки",
                    "Punkte für eine Feld-Breite",
                    "Points pour la largeur d'une case",
                    "Puntos de ancho de una casilla",
                    "Punkty szerokości jednego pola",
                    "单格宽度点"),
                [OneTileTooltip] = Row(
                    "Show the pair of dots indicating the spread width of one tile.",
                    "Показывать пару точек, обозначающих ширину разброса на одну клетку.",
                    "Zeigt das Punktpaar für die Streubreite eines Feldes.",
                    "Affiche la paire de points indiquant la dispersion sur une case.",
                    "Muestra el par de puntos que indica la dispersión de una casilla.",
                    "Pokazuje parę punktów oznaczających rozrzut na jedno pole.",
                    "显示表示单格散布宽度的两个点。"),
                [CursorTileLabel] = Row(
                    "Show Cursor Tile Dots",
                    "Точки на целевой клетке",
                    "Punkte auf dem Zielfeld",
                    "Points sur la case ciblée",
                    "Puntos en la casilla objetivo",
                    "Punkty na docelowym polu",
                    "目标格点"),
                [CursorTileTooltip] = Row(
                    "Show the pair of dots at the target tile.",
                    "Показывать пару точек на целевой клетке.",
                    "Zeigt das Punktpaar auf dem Zielfeld.",
                    "Affiche la paire de points sur la case ciblée.",
                    "Muestra el par de puntos en la casilla objetivo.",
                    "Pokazuje parę punktów na docelowym polu.",
                    "在目标格子上显示两个点。"),
                [SmartActivationLabel] = Row(
                    "Smart Activation",
                    "Умная активация",
                    "Intelligente Aktivierung",
                    "Activation intelligente",
                    "Activación inteligente",
                    "Inteligentna aktywacja",
                    "智能激活"),
                [SmartActivationTooltip] = Row(
                    "When enabled, the indicator also appears when aiming at an enemy without holding Shift. Shift always works.",
                    "Если включено, индикатор также появляется при наведении на врага без Shift. Shift по-прежнему работает.",
                    "Wenn aktiv, erscheint die Anzeige auch beim Zielen auf einen Feind ohne Shift. Shift funktioniert weiterhin.",
                    "Si activé, l'indicateur apparaît aussi sur un ennemi sans maintenir Shift. Shift fonctionne toujours.",
                    "Si está activado, el indicador también aparece al apuntar a un enemigo sin Shift. Shift sigue funcionando.",
                    "Po włączeniu wskaźnik pojawia się też na wrogu bez Shift. Shift nadal działa.",
                    "启用后，瞄准敌人时无需按住 Shift 也会显示指示器。Shift 仍然有效。"),
            };
        }

        private static Dictionary<Lang, string> Row(
            string en,
            string ru,
            string de,
            string fr,
            string es,
            string pl,
            string zh)
        {
            return new Dictionary<Lang, string>
            {
                [Lang.EnglishUS] = en,
                [Lang.Russian] = ru,
                [Lang.German] = de,
                [Lang.French] = fr,
                [Lang.Spanish] = es,
                [Lang.Polish] = pl,
                [Lang.ChineseSimp] = zh,
            };
        }
    }
}
