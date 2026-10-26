using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KarmaWarnings
{
    // Warning text right above the crosshair: an "Increases Karma" headline, a smaller line under it saying what the
    // action is, and with MithrixPreview set to Banner, a dimmer line per Karma level saying what it does to Mithrix.
    // Fades in and out; it never pulses or flashes.
    internal sealed class WarningBanner : MonoBehaviour
    {
        // Appears quickly so the warning is up before the player acts; leaves at a calmer pace.
        // The sidebar uses the same fades.
        internal const float FadeInDuration = 0.075f;
        internal const float FadeOutDuration = 0.15f;
        private const float Width = 1000f;
        // The main container is centered on the crosshair. This clears the crosshair's own graphics, in UI pixels.
        private const float CrosshairClearance = 60f;
        private const float HeadlineFontSize = 34f;
        private const float SublineFontSize = 22f;
        private const float MithrixFontSize = 18f;
        // Space between the subline and the Mithrix lines, in UI pixels.
        private const float MithrixGap = 6f;
        private const string HeadlineText = "Increases Karma";

        // The game's own colour for Karma text: the cLunarObjective style its Karma chat message uses.
        internal static readonly Color TextColor = new Color32(0xb8, 0xc4, 0xff, 0xff);
        // Dimmer, so the Mithrix lines read as extra detail under the warning.
        private static readonly Color MithrixColor = new Color32(0x92, 0x9b, 0xc9, 0xff);

        private CanvasGroup canvasGroup;
        private TextMeshProUGUI headline;
        private TextMeshProUGUI subline;
        private TextMeshProUGUI mithrix;
        private bool visible;

        public static WarningBanner Create(HUD hud)
        {
            Transform container = hud.mainContainer.transform;
            GameObject root = new GameObject("KarmaWarningsBanner", typeof(RectTransform));
            root.layer = container.gameObject.layer;
            RectTransform rect = (RectTransform)root.transform;
            rect.SetParent(container, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            // The bottom edge is the pivot, so the text grows upward and never reaches the crosshair.
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(Width, 0f);
            rect.anchoredPosition = new Vector2(0f, CrosshairClearance);

            root.AddComponent<LayoutElement>().ignoreLayout = true;

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            WarningBanner banner = root.AddComponent<WarningBanner>();
            banner.canvasGroup = group;
            banner.headline = CreateText(rect, "Headline", HeadlineFontSize);
            banner.subline = CreateText(rect, "Subline", SublineFontSize);
            banner.mithrix = CreateText(rect, "Mithrix", MithrixFontSize);
            banner.mithrix.color = MithrixColor;
            banner.mithrix.margin = new Vector4(0f, MithrixGap, 0f, 0f);
            // RiskUI keeps the crosshair centered, so only the font changes there.
            if (RiskUICompat.IsRiskUIHud(hud))
            {
                RiskUICompat.ApplyFont(hud, banner.headline);
                RiskUICompat.ApplyFont(hud, banner.subline);
                RiskUICompat.ApplyFont(hud, banner.mithrix);
            }
            banner.subline.gameObject.SetActive(false);
            banner.mithrix.gameObject.SetActive(false);
            return banner;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string name, float fontSize)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);
            HGTextMeshProUGUI text = textObject.AddComponent<HGTextMeshProUGUI>();
            text.richText = true;
            text.enableWordWrapping = true;
            text.alignment = TextAlignmentOptions.Bottom;
            text.fontSize = fontSize;
            text.color = TextColor;
            text.raycastTarget = false;
            // No outline or bold: the game font's material already draws a drop shadow, and either one on top of it
            // pushes each glyph past the font atlas's padding, so a sliver of the glyph above it in the atlas shows as
            // a thin line over the text.
            return text;
        }

        // mithrixText is one line per Karma level gained, or null to leave the Mithrix lines out.
        public void Show(KarmaWarning warning, string mithrixText)
        {
            visible = true;
            bool showHeadline = !warning.HideHeadline;
            if (headline.gameObject.activeSelf != showHeadline)
            {
                headline.gameObject.SetActive(showHeadline);
            }
            if (showHeadline && headline.text != HeadlineText)
            {
                headline.text = HeadlineText;
            }
            SetLine(subline, warning.Text);
            SetLine(mithrix, mithrixText);
        }

        // Hides the line when the text is empty. Only touches the text when it changes, so TMP doesn't rebuild every frame.
        private static void SetLine(TextMeshProUGUI line, string text)
        {
            bool hasText = !string.IsNullOrEmpty(text);
            if (line.gameObject.activeSelf != hasText)
            {
                line.gameObject.SetActive(hasText);
            }
            if (hasText && line.text != text)
            {
                line.text = text;
            }
        }

        public void Hide()
        {
            visible = false;
        }

        private void LateUpdate()
        {
            float duration = visible ? FadeInDuration : FadeOutDuration;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, visible ? 1f : 0f, Time.unscaledDeltaTime / duration);
        }
    }
}
