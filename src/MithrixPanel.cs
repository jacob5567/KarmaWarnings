using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaWarnings
{
    // With MithrixPreview set to Sidebar: a panel on the right, under the objectives, saying what each Karma level the
    // warning would add does to Mithrix. Shows and fades with the banner.
    //
    // The objective panel sits in a vertical layout (RightInfoBar, in the game's HUD and in RiskUI's). The panel joins
    // that layout as its last child, so it also sits below LookingGlass's stats panel, which LookingGlass adds there and
    // moves to the end once, a second after creating it.
    internal sealed class MithrixPanel : MonoBehaviour
    {
        // RiskUI's font draws smaller, so these suit it. On the game's HUD the panel takes the objective panel's sizes, or
        // the smaller fallbacks when they can't be read.
        private const float RiskUIHeaderFontSize = 16f;
        private const float RiskUIBodyFontSize = 13.5f;
        private const float FallbackHeaderFontSize = 13f;
        private const float FallbackBodyFontSize = 11f;
        private const int Padding = 10;
        private const float Spacing = 6f;
        // Used only when there's no objective layout to join.
        private const float FallbackWidth = 300f;
        private static readonly Vector2 FallbackPosition = new Vector2(-24f, -220f);

        private static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color BodyColor = new Color32(0xc3, 0xc7, 0xd3, 0xff);
        private static readonly string AccentHex = "#" + ColorUtility.ToHtmlStringRGB(WarningBanner.TextColor);

        private CanvasGroup canvasGroup;
        private LayoutElement layoutElement;
        private bool inLayout;
        private TextMeshProUGUI header;
        private TextMeshProUGUI body;
        private bool visible;
        private int shownFrom = -1;
        private int shownTo = -1;

        public static MithrixPanel Create(HUD hud)
        {
            ObjectivePanelController objectives = hud.GetComponentInChildren<ObjectivePanelController>(true);
            Transform column = objectives ? objectives.transform.parent : null;
            VerticalLayoutGroup columnLayout = column ? column.GetComponent<VerticalLayoutGroup>() : null;
            Transform parent = columnLayout ? column : hud.mainContainer.transform;

            GameObject root = new GameObject("KarmaWarningsMithrixPanel", typeof(RectTransform));
            root.layer = parent.gameObject.layer;
            RectTransform rect = (RectTransform)root.transform;
            rect.SetParent(parent, false);

            MithrixPanel panel = root.AddComponent<MithrixPanel>();
            panel.inLayout = columnLayout;
            panel.layoutElement = root.AddComponent<LayoutElement>();
            if (panel.inLayout)
            {
                rect.SetAsLastSibling();
                // The column sizes the panel when it controls its children; otherwise the panel matches the objectives.
                if (!columnLayout.childControlWidth)
                {
                    rect.sizeDelta = new Vector2(((RectTransform)objectives.transform).rect.width, 0f);
                }
            }
            else
            {
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = FallbackPosition;
                rect.sizeDelta = new Vector2(FallbackWidth, 0f);
                panel.layoutElement.ignoreLayout = true;
            }
            if (!panel.inLayout || !columnLayout.childControlHeight)
            {
                ContentSizeFitter fitter = root.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            root.AddComponent<Image>().color = BackgroundColor;

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(Padding, Padding, Padding, Padding);
            layout.spacing = Spacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            panel.canvasGroup = group;

            bool riskUI = RiskUICompat.IsRiskUIHud(hud);
            float headerSize = riskUI ? RiskUIHeaderFontSize : FontSize(objectives ? objectives.transform : null, FallbackHeaderFontSize);
            // The objective strip prefab, so it works before any objective is listed.
            float bodySize = riskUI ? RiskUIBodyFontSize : FontSize(objectives && objectives.objectiveTrackerPrefab ? objectives.objectiveTrackerPrefab.transform : null, FallbackBodyFontSize);

            panel.header = CreateText(rect, "Header", headerSize, WarningBanner.TextColor, TextAlignmentOptions.BottomLeft);
            panel.body = CreateText(rect, "Body", bodySize, BodyColor, TextAlignmentOptions.TopLeft);

            if (riskUI)
            {
                RiskUICompat.ApplyFont(hud, panel.header);
                RiskUICompat.ApplyFont(hud, panel.body);
            }
            panel.SetInLayout(false);
            if (KarmaWarningsPlugin.Settings.DebugLogging.Value)
            {
                KarmaWarningsPlugin.Log.LogInfo($"Mithrix panel: under {(panel.inLayout ? column.name : "the fallback position")}, font sizes {headerSize}/{bodySize}, RiskUI={riskUI}");
            }
            return panel;
        }

        // The size of the text in the object's "Label" child, which both the objective panel and its strips have.
        private static float FontSize(Transform owner, float fallback)
        {
            Transform label = owner ? owner.Find("Label") : null;
            TMP_Text text = label ? label.GetComponent<TMP_Text>() : null;
            return text && text.fontSize > 0f ? text.fontSize : fallback;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string name, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);
            HGTextMeshProUGUI text = textObject.AddComponent<HGTextMeshProUGUI>();
            text.richText = true;
            text.enableWordWrapping = true;
            text.alignment = alignment;
            text.fontSize = fontSize;
            text.color = color;
            text.raycastTarget = false;
            // No outline or bold, for the reason in WarningBanner.CreateText.
            return text;
        }

        // The levels after fromKarma, up to toKarma.
        public void Show(int fromKarma, int toKarma)
        {
            if (toKarma <= fromKarma)
            {
                Hide();
                return;
            }
            visible = true;
            SetInLayout(true);
            if (fromKarma == shownFrom && toKarma == shownTo) return;

            shownFrom = fromKarma;
            shownTo = toKarma;
            header.text = $"Mithrix at Karma {toKarma}";
            body.text = MithrixEffects.SidebarText(fromKarma, toKarma, AccentHex);
        }

        public void Hide()
        {
            visible = false;
        }

        private void LateUpdate()
        {
            float duration = visible ? WarningBanner.FadeInDuration : WarningBanner.FadeOutDuration;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, visible ? 1f : 0f, Time.unscaledDeltaTime / duration);
            if (!inLayout) return;

            // Once faded out it leaves the column, so it doesn't hold an empty gap under the objectives.
            if (!visible && canvasGroup.alpha <= 0f)
            {
                SetInLayout(false);
            }
            // Something added to the column after the panel, such as LookingGlass's stats, goes back above it.
            Transform column = transform.parent;
            if (transform.GetSiblingIndex() != column.childCount - 1)
            {
                transform.SetAsLastSibling();
            }
        }

        private void SetInLayout(bool value)
        {
            if (inLayout && layoutElement.ignoreLayout == value)
            {
                layoutElement.ignoreLayout = !value;
            }
        }
    }
}
