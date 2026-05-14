using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Blockblast.View
{
    public static class UIBuilder
    {
        public const float DesignWidth = 1080f;
        public const float DesignHeight = 1920f;

        public static void BuildEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;

            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemGo.AddComponent<StandaloneInputModule>();
#endif
        }

        public static Canvas BuildCanvas(Transform parent)
        {
            var canvasGo = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            canvasGo.transform.SetParent(parent, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = mainCamera;
                canvas.planeDistance = 1f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(DesignWidth, DesignHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            return canvas;
        }

        public static Image BuildBackground(Transform parent)
        {
            var backgroundGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundGo.transform.SetParent(parent, false);
            var backgroundRect = (RectTransform)backgroundGo.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;
            backgroundRect.anchoredPosition = Vector2.zero;

            var image = backgroundGo.GetComponent<Image>();
            image.color = new Color(0.05f, 0.07f, 0.12f, 1f);
            image.raycastTarget = false;
            return image;
        }

        public static (Text text, RectTransform rect) BuildScoreText(Transform parent)
        {
            var scoreGo = new GameObject("ScoreText", typeof(RectTransform), typeof(Text));
            scoreGo.transform.SetParent(parent, false);

            var scoreRect = (RectTransform)scoreGo.transform;
            scoreRect.anchorMin = new Vector2(0f, 1f);
            scoreRect.anchorMax = new Vector2(1f, 1f);
            scoreRect.pivot = new Vector2(0.5f, 1f);
            scoreRect.sizeDelta = new Vector2(0f, 200f);
            scoreRect.anchoredPosition = new Vector2(0f, -80f);

            var text = scoreGo.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 96;
            text.color = new Color(0.95f, 0.95f, 1f, 1f);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            text.text = "Score: 0";

            return (text, scoreRect);
        }

        public static Text BuildBestScoreText(Transform parent)
        {
            var bestGo = new GameObject("BestScoreText", typeof(RectTransform), typeof(Text));
            bestGo.transform.SetParent(parent, false);

            var bestRect = (RectTransform)bestGo.transform;
            bestRect.anchorMin = new Vector2(0f, 1f);
            bestRect.anchorMax = new Vector2(1f, 1f);
            bestRect.pivot = new Vector2(0.5f, 1f);
            bestRect.sizeDelta = new Vector2(0f, 60f);
            bestRect.anchoredPosition = new Vector2(0f, -240f);

            var text = bestGo.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 40;
            text.color = new Color(0.65f, 0.70f, 0.85f, 1f);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            text.text = "Meilleur : 0";

            return text;
        }

        public static RectTransform BuildCelebrationOverlay(Transform parent)
        {
            var overlayGo = new GameObject("CelebrationOverlay", typeof(RectTransform));
            overlayGo.transform.SetParent(parent, false);
            var rect = (RectTransform)overlayGo.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        public static GridView BuildGridView(Transform parent)
        {
            var gridContainerGo = new GameObject("GridContainer", typeof(RectTransform));
            gridContainerGo.transform.SetParent(parent, false);
            var containerRect = (RectTransform)gridContainerGo.transform;
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = new Vector2(0f, 80f);

            var gridView = gridContainerGo.AddComponent<GridView>();
            gridView.Build(containerRect);
            return gridView;
        }

        public static PieceTrayView BuildTrayView(Transform parent, Canvas rootCanvas, float cellSize)
        {
            var trayHostGo = new GameObject("TrayHost", typeof(RectTransform));
            trayHostGo.transform.SetParent(parent, false);

            var trayHostRect = (RectTransform)trayHostGo.transform;
            trayHostRect.anchorMin = Vector2.zero;
            trayHostRect.anchorMax = Vector2.one;
            trayHostRect.pivot = new Vector2(0.5f, 0.5f);
            trayHostRect.anchoredPosition = Vector2.zero;
            trayHostRect.sizeDelta = Vector2.zero;

            var trayView = trayHostGo.AddComponent<PieceTrayView>();
            trayView.Build(trayHostGo.transform, rootCanvas, cellSize);
            return trayView;
        }

        public static GameOverPanel BuildGameOverPanel(Transform parent)
        {
            var hostGo = new GameObject("GameOverHost", typeof(RectTransform));
            hostGo.transform.SetParent(parent, false);

            var hostRect = (RectTransform)hostGo.transform;
            hostRect.anchorMin = Vector2.zero;
            hostRect.anchorMax = Vector2.one;
            hostRect.pivot = new Vector2(0.5f, 0.5f);
            hostRect.sizeDelta = Vector2.zero;
            hostRect.anchoredPosition = Vector2.zero;

            var panel = hostGo.AddComponent<GameOverPanel>();
            panel.Build(hostGo.transform);
            return panel;
        }

        public static void BuildNewGameButton(Transform parent, UnityAction onClick)
        {
            var buttonGo = new GameObject("NewGameButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);

            var buttonRect = (RectTransform)buttonGo.transform;
            buttonRect.anchorMin = new Vector2(1f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.sizeDelta = new Vector2(78f, 78f);
            buttonRect.anchoredPosition = new Vector2(-30f, -30f);

            var buttonImage = buttonGo.GetComponent<Image>();
            buttonImage.sprite = CellSpriteFactory.GetRoundedSquare();
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = new Color(1f, 1f, 1f, 0.08f);

            var button = buttonGo.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            var buttonColors = button.colors;
            buttonColors.normalColor = new Color(1f, 1f, 1f, 1f);
            buttonColors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            buttonColors.pressedColor = new Color(0.8f, 0.8f, 0.85f, 1f);
            buttonColors.selectedColor = new Color(1f, 1f, 1f, 1f);
            button.colors = buttonColors;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(buttonGo.transform, false);
            var iconRect = (RectTransform)iconGo.transform;
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(46f, 46f);
            iconRect.anchoredPosition = Vector2.zero;

            var iconImage = iconGo.GetComponent<Image>();
            iconImage.sprite = RefreshIconSpriteFactory.GetRefreshIcon();
            iconImage.color = new Color(0.95f, 0.95f, 1f, 0.9f);
            iconImage.raycastTarget = false;
        }
    }
}
