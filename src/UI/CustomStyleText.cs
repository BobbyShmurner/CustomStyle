using UnityEngine;
using UnityEngine.UI;

using ULTRAINTERFACE;

namespace CustomStyle {
	public struct StyleData {
		public string Prefix;
		public string Suffix;

		public Color PrefixColor;
		public Color SuffixColor;

		public bool HasDropShadow;

		public StyleData(string prefix, string suffix, Color prefixColor, Color suffixColor, bool hasDropShadow = false) {
			Prefix = prefix;
			Suffix = suffix;

			PrefixColor = prefixColor;
			SuffixColor = suffixColor;

			HasDropShadow = hasDropShadow;
		}
	}

	public class CustomStyleText : UIComponent {
		public static Font StyleFont;

		public Text StyleText { get; private set; }
		public Text DropShadowText { get; private set; }

		public LetterSpacing StyleTextSpacing { get; private set; }
		public LetterSpacing DropShadowTextSpacing { get; private set; }

		public StyleData StyleData { get; private set; }
		public bool IsVisible { get; private set; } = true;

		void Update() {
			if (Input.GetKeyDown(KeyCode.RightShift)) ToggleVisibility();
		}

		public void ToggleVisibility() {
			SetVisibility(!IsVisible);
		}

		public void SetVisibility(bool visible) {
			IsVisible = visible;

			if (!IsVisible) {
				StyleText.enabled = false;
				DropShadowText.enabled = false;
			} else {
				StyleText.enabled = true;
				DropShadowText.enabled = StyleData.HasDropShadow;
			}
		}

		public void SetStyle(string prefix, string suffix, Color prefixColor, Color suffixColor, bool hasDropShadow = false) {
			SetStyle(new StyleData(prefix, suffix, prefixColor, suffixColor, hasDropShadow));
		}
		
		public void SetStyle(StyleData style) {
			StyleData = style;

			if (!StyleData.HasDropShadow) {
				DropShadowText.enabled = false;

				StyleText.transform.localScale = Vector3.one;

				StyleTextSpacing.spacing = -10;
				StyleText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(StyleData.PrefixColor)}><size=150>{StyleData.Prefix}</size></color><color=#{ColorUtility.ToHtmlStringRGB(StyleData.SuffixColor)}><size=98>{StyleData.Suffix}</size></color>";
			} else {
				DropShadowText.enabled = true;
				Plugin.Log.LogInfo("Less go");

				StyleText.transform.localScale = new Vector3(0.9f, 1, 1);
				DropShadowText.transform.localScale = new Vector3(0.9f, 1, 1);

				StyleTextSpacing.spacing = -15;
				DropShadowTextSpacing.spacing = -15;

				StyleText.text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(StyleData.PrefixColor)}><size=140>{StyleData.Prefix}</size></color></b>";
				DropShadowText.text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(StyleData.SuffixColor)}><size=140>{StyleData.Prefix}</size></color></b>";
			}
		}

		public static CustomStyleText Create(Transform parent, string prefix, string suffix, Color prefixColor, Color suffixColor, bool hasDropShadow = false) {
			return Create(parent, new StyleData(prefix, suffix, prefixColor, suffixColor, hasDropShadow));
		}

		public static CustomStyleText Create(Transform parent, StyleData style) {
			GameObject customStyleTextGO = new GameObject("Custom Style Text");
			customStyleTextGO.transform.SetParent(parent, false);

			RectTransform customStyleTextRect = customStyleTextGO.AddComponent<RectTransform>();

			CustomStyleText customStyleText = customStyleTextGO.AddComponent<CustomStyleText>();

			// Style Text

			customStyleText.StyleText = UI.CreateText(customStyleTextGO.transform);
			customStyleText.StyleText.gameObject.name = "Style Text";

            customStyleText.StyleText.rectTransform.pivot = new Vector2(0, 0.5f);
            customStyleText.StyleText.rectTransform.anchorMin = new Vector2(0, 0.5f);
            customStyleText.StyleText.rectTransform.anchorMax = new Vector2(0, 0.5f);
            customStyleText.StyleText.rectTransform.anchoredPosition = new Vector2(-150, -5);

            customStyleText.StyleText.font = StyleFont;
            customStyleText.StyleText.alignment = TextAnchor.MiddleLeft;
            customStyleText.StyleText.horizontalOverflow = HorizontalWrapMode.Overflow;

            customStyleText.StyleTextSpacing = customStyleText.StyleText.gameObject.AddComponent<LetterSpacing>();
            customStyleText.StyleTextSpacing.useRichText = true;

			// Drop Shadow Text

			customStyleText.DropShadowText = UI.CreateText(customStyleTextGO.transform);
			customStyleText.DropShadowText.gameObject.name = "Drop Shadow Text";

            customStyleText.DropShadowText.rectTransform.SetAsFirstSibling();
            customStyleText.DropShadowText.rectTransform.pivot = new Vector2(0, 0.5f);
            customStyleText.DropShadowText.rectTransform.anchorMin = new Vector2(0, 0.5f);
            customStyleText.DropShadowText.rectTransform.anchorMax = new Vector2(0, 0.5f);
            customStyleText.DropShadowText.rectTransform.anchoredPosition = new Vector2(-147, -8);

            customStyleText.DropShadowText.font = StyleFont;
            customStyleText.DropShadowText.alignment = TextAnchor.MiddleLeft;
            customStyleText.DropShadowText.horizontalOverflow = HorizontalWrapMode.Overflow;

            customStyleText.DropShadowTextSpacing = customStyleText.DropShadowText.gameObject.AddComponent<LetterSpacing>();
            customStyleText.DropShadowTextSpacing.useRichText = true;

			customStyleText.SetStyle(style);

			return customStyleText;
		}
	}
}