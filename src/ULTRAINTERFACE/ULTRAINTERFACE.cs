using BepInEx.Logging;
using BepInEx.Configuration;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;

namespace ULTRAINTERFACE {
	public static class UI {
		public static List<Action<Scene>> OnSceneLoadActions { get; private set; } = new List<Action<Scene>>();
		public static ManualLogSource Log { get; private set; }

		public static GameObject ScrollRectPrefab { get; internal set; }
		public static GameObject ScrollbarPrefab { get; internal set; }
		public static GameObject ButtonPrefab { get; internal set; }
		public static GameObject TextPrefab { get; internal set; }

		static bool IsUISetup = false;
		static bool HasInitalisedBefore = false;

		public static void RegisterOnSceneLoad(Action<Scene> action, bool executeNow = true) {
			OnSceneLoadActions.Add(action);
			
			if (executeNow) {
				if (!Init()) {
					Log.LogWarning("Failed to intialise UI, cannot execute newly registered OnSceneLoad Actions");
					return;
				}

				action(SceneManager.GetActiveScene());
			}
		}

		public static CustomScrollView CreateScrollView(Transform parent, int width = 620, int height = 520, TextAnchor childAlignment = TextAnchor.UpperCenter, string name = "Custom Scroll View") {
			if (!Init()) return null;

			RectTransform scrollViewRect = new GameObject(name, new Type[]{typeof(RectTransform)}).GetComponent<RectTransform>();
			scrollViewRect.gameObject.layer = 5;
			scrollViewRect.sizeDelta = new Vector2(width, height);
			scrollViewRect.localPosition = Vector3.zero;
			scrollViewRect.SetParent(parent, false);

			HorizontalLayoutGroup scrollViewLayoutGroup = scrollViewRect.gameObject.AddComponent<HorizontalLayoutGroup>();
			scrollViewLayoutGroup.childControlWidth = false;
			scrollViewLayoutGroup.childControlHeight = false;
			scrollViewLayoutGroup.spacing = 5;

			ScrollRect scrollRect = GameObject.Instantiate(ScrollRectPrefab, scrollViewRect).GetComponent<ScrollRect>();
			Scrollbar scrollbar = GameObject.Instantiate(ScrollbarPrefab, scrollViewRect).GetComponent<Scrollbar>();

			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
			scrollRect.verticalScrollbar = scrollbar;
			scrollRect.gameObject.name = "Scroll Rect";

			RectTransform scrollbarRect = scrollbar.GetComponent<RectTransform>();
			scrollbarRect.sizeDelta = new Vector2(30, height);
			scrollbarRect.localPosition = Vector3.zero;
			scrollbarRect.gameObject.name = "Scrollbar";

			RectTransform scrollRectTrans = scrollRect.GetComponent<RectTransform>();
			scrollRectTrans.sizeDelta = new Vector2(width - 35, height);
			scrollRectTrans.localPosition = Vector3.zero;

			RectTransform scrollRectContent = scrollRect.transform.GetChild(0).GetComponent<RectTransform>();
			scrollRectContent.sizeDelta = new Vector2(width, height + 160);
			scrollRectContent.localPosition = Vector3.zero;
			scrollRectContent.gameObject.name = "Content";

			VerticalLayoutGroup scrollRectContentLayout = scrollRectContent.gameObject.AddComponent<VerticalLayoutGroup>();
			scrollRectContentLayout.childAlignment = childAlignment;
			scrollRectContentLayout.childForceExpandHeight = false;
			scrollRectContentLayout.childForceExpandWidth = false;
			scrollRectContentLayout.childControlHeight = false;
			scrollRectContentLayout.childControlWidth = false;
			scrollRectContentLayout.spacing = 10;

			ContentSizeFitter scrollRectContentFitter = scrollRectContent.gameObject.AddComponent<ContentSizeFitter>();
			scrollRectContentFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

			for (; scrollRectContent.childCount > 0;) {
				GameObject.DestroyImmediate(scrollRectContent.GetChild(0).gameObject);
			}

			CustomScrollView scrollView = scrollViewRect.gameObject.AddComponent<CustomScrollView>();
			scrollView.Init(scrollRectContent, scrollRect, scrollbar);

			return scrollView;
		}

		public static Button CreateButton(Transform parent, string text = "New Button", int width = 160, int height = 50, bool forceCaps = true) {
			if (!Init()) return null;
			if (forceCaps) text = text.ToUpper();

			GameObject buttonGO = GameObject.Instantiate(ButtonPrefab, parent);
			buttonGO.name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLower());
			buttonGO.AddComponent<UIComponent>();

			RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
			buttonRect.sizeDelta = new Vector2(width, height);
			buttonRect.anchoredPosition = Vector2.zero;

			Button button = buttonGO.GetComponent<Button>();
			button.onClick.RemoveAllListeners();

			// Disable all the persisten listeners
			for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++) {
				button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
			}

			Text buttonText = buttonGO.GetComponentInChildren<Text>();
			buttonText.horizontalOverflow = HorizontalWrapMode.Overflow;
			buttonText.verticalOverflow = VerticalWrapMode.Overflow;
			buttonText.gameObject.name = "Text";
			buttonText.text = text;

			return button;
		}

		public static Text CreateText(Transform parent, string displayText = "New Text", int fontSize = 24, int width = 240, int height = 30, TextAnchor anchor = TextAnchor.MiddleCenter, bool forceCaps = true) {
			if (!Init()) return null;
			if (forceCaps) displayText = displayText.ToUpper();

			GameObject textGO = GameObject.Instantiate(TextPrefab, parent);
			textGO.AddComponent<UIComponent>();
			textGO.name = "Text";

			RectTransform textRect = textGO.GetComponent<RectTransform>();
			textRect.sizeDelta = new Vector2(width, height);
			textRect.anchoredPosition = Vector2.zero;

			Text text = textGO.GetComponent<Text>();
			text.fontSize = fontSize;
			text.text = displayText;
			text.alignment = anchor;

			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			text.verticalOverflow = VerticalWrapMode.Overflow;

			return text;
		}

		public static void Unload() {
			SceneManager.sceneLoaded -= OnSceneLoad;
			OnSceneLoadActions.Clear();

			Options.Unload();

			foreach (UIComponent ui in Resources.FindObjectsOfTypeAll<UIComponent>()) {
				GameObject.Destroy(ui.gameObject);
			}
		}

		internal static bool Init() {
			if (!HasInitalisedBefore) {
				Log = new ManualLogSource("ULTRAINTERFACE");
				BepInEx.Logging.Logger.Sources.Add(Log);

				SceneManager.sceneLoaded += OnSceneLoad;

				HasInitalisedBefore = true;
			}

			IsUISetup = SetupUI();
			return IsUISetup;
		}

		static void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode) { 
			IsUISetup = false;
			IsUISetup = SetupUI();

			if (OnSceneLoadActions.Count <= 0) return;
			if (!IsUISetup) {
				Log.LogWarning("UI failed to initalised, not calling OnSceneLoad Actions");
				return;
			}

			Log.LogInfo("Calling OnSceneLoad Actions");

			foreach (Action<Scene> action in OnSceneLoadActions) {
				action(scene);
			}

			Log.LogInfo("Finished Calling OnSceneLoad Actions");
		}

		static bool SetupUI() {
			if (IsUISetup) return true;

			OptionsMenuToManager optionsMenuToManager = GameObject.FindObjectOfType<OptionsMenuToManager>();
			if (optionsMenuToManager == null) {
				Log.LogError("Failed to find the OptionsMenu, will attempt to setup UI on next scene load");
				return false;
			}

			Options.OptionsMenu = optionsMenuToManager.transform.Find("OptionsMenu").GetComponent<RectTransform>();
			
			ScrollRectPrefab = Options.OptionsMenu.Find("Gameplay Options").Find("Scroll Rect (1)").gameObject;
			ScrollbarPrefab = Options.OptionsMenu.Find("Gameplay Options").Find("Scrollbar (1)").gameObject;

			TextPrefab = Options.OptionsMenu.Find("Gameplay Options").Find("Text").gameObject;

			Transform possibleButtonPrefab = Options.OptionsMenu.Find("Gameplay");
			
			if (possibleButtonPrefab != null) ButtonPrefab = possibleButtonPrefab.gameObject;
			else ButtonPrefab = Options.OptionsMenu.Find("Options Scroll View").GetChild(0).GetChild(0).Find("Gameplay").gameObject;

			Log.LogInfo($"Initalised UI");
			return true;
		}
	}

	public class CustomScrollView : UIComponent {
		public RectTransform Content { get; private set; }
		public ScrollRect ScrollRect { get; private set; }
		public Scrollbar Scrollbar { get; private set; }

		internal void Init(RectTransform content, ScrollRect scrollRect, Scrollbar scrollbar) {
			if (Content != null) {
				UI.Log.LogError($"Scroll View \"{gameObject.name}\" already initalised, returning...");
				return;
			}

			this.Content = content;
			this.ScrollRect = scrollRect;
			this.Scrollbar = scrollbar;
		}
	}

	// This is purely used to tag UI created with this library
	public class UIComponent : MonoBehaviour {}
}