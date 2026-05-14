using System;
using UnityEngine;
using UnityEngine.UI;

namespace Blockblast.View
{
    public class GameOverPanel : MonoBehaviour
    {
        public event Action RestartRequested;

        private GameObject panelRoot;
        private Text scoreLabel;
        private Text bestLabel;
        private Text newRecordLabel;

        public void Build(Transform parent)
        {
            var panelGo = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(parent, false);
            panelRoot = panelGo;

            var panelRect = (RectTransform)panelGo.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panelGo.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.78f);
            panelImage.raycastTarget = true;

            BuildTitleText(panelGo.transform);
            newRecordLabel = BuildNewRecordText(panelGo.transform);
            scoreLabel = BuildScoreText(panelGo.transform);
            bestLabel = BuildBestText(panelGo.transform);
            BuildRestartButton(panelGo.transform);

            panelRoot.SetActive(false);
        }

        public void Show(int finalScore, int bestScore, bool isNewRecord)
        {
            scoreLabel.text = $"Score : {finalScore}";
            bestLabel.text = $"Meilleur : {bestScore}";
            newRecordLabel.gameObject.SetActive(isNewRecord);
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }

        public void Hide()
        {
            panelRoot.SetActive(false);
        }

        private static void BuildTitleText(Transform parent)
        {
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(parent, false);

            var titleRect = (RectTransform)titleGo.transform;
            titleRect.anchorMin = new Vector2(0f, 0.5f);
            titleRect.anchorMax = new Vector2(1f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0f);
            titleRect.sizeDelta = new Vector2(0f, 240f);
            titleRect.anchoredPosition = new Vector2(0f, 120f);

            var titleText = titleGo.GetComponent<Text>();
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 140;
            titleText.color = Color.white;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.text = "Game Over";
            titleText.raycastTarget = false;
        }

        private static Text BuildScoreText(Transform parent)
        {
            var scoreGo = new GameObject("Score", typeof(RectTransform), typeof(Text));
            scoreGo.transform.SetParent(parent, false);

            var scoreRect = (RectTransform)scoreGo.transform;
            scoreRect.anchorMin = new Vector2(0f, 0.5f);
            scoreRect.anchorMax = new Vector2(1f, 0.5f);
            scoreRect.pivot = new Vector2(0.5f, 0.5f);
            scoreRect.sizeDelta = new Vector2(0f, 130f);
            scoreRect.anchoredPosition = new Vector2(0f, 20f);

            var scoreText = scoreGo.GetComponent<Text>();
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.fontSize = 80;
            scoreText.color = new Color(0.92f, 0.95f, 1f, 1f);
            scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreText.raycastTarget = false;

            return scoreText;
        }

        private static Text BuildBestText(Transform parent)
        {
            var bestGo = new GameObject("Best", typeof(RectTransform), typeof(Text));
            bestGo.transform.SetParent(parent, false);

            var bestRect = (RectTransform)bestGo.transform;
            bestRect.anchorMin = new Vector2(0f, 0.5f);
            bestRect.anchorMax = new Vector2(1f, 0.5f);
            bestRect.pivot = new Vector2(0.5f, 0.5f);
            bestRect.sizeDelta = new Vector2(0f, 80f);
            bestRect.anchoredPosition = new Vector2(0f, -70f);

            var bestText = bestGo.GetComponent<Text>();
            bestText.alignment = TextAnchor.MiddleCenter;
            bestText.fontSize = 50;
            bestText.color = new Color(0.75f, 0.78f, 0.90f, 1f);
            bestText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bestText.raycastTarget = false;
            return bestText;
        }

        private static Text BuildNewRecordText(Transform parent)
        {
            var recordGo = new GameObject("NewRecord", typeof(RectTransform), typeof(Text));
            recordGo.transform.SetParent(parent, false);

            var recordRect = (RectTransform)recordGo.transform;
            recordRect.anchorMin = new Vector2(0f, 0.5f);
            recordRect.anchorMax = new Vector2(1f, 0.5f);
            recordRect.pivot = new Vector2(0.5f, 0.5f);
            recordRect.sizeDelta = new Vector2(0f, 110f);
            recordRect.anchoredPosition = new Vector2(0f, 110f);

            var recordText = recordGo.GetComponent<Text>();
            recordText.alignment = TextAnchor.MiddleCenter;
            recordText.fontSize = 70;
            recordText.color = new Color(1f, 0.85f, 0.30f, 1f);
            recordText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            recordText.text = "★ Nouveau record ! ★";
            recordText.raycastTarget = false;
            return recordText;
        }

        private void BuildRestartButton(Transform parent)
        {
            var buttonGo = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);

            var buttonRect = (RectTransform)buttonGo.transform;
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(480f, 160f);
            buttonRect.anchoredPosition = new Vector2(0f, -220f);

            var buttonImage = buttonGo.GetComponent<Image>();
            buttonImage.sprite = CellSpriteFactory.GetRoundedSquare();
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = new Color(0.30f, 0.55f, 1.00f, 1f);

            var button = buttonGo.GetComponent<Button>();
            button.onClick.AddListener(InvokeRestart);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelRect = (RectTransform)labelGo.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;

            var labelText = labelGo.GetComponent<Text>();
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontSize = 70;
            labelText.color = Color.white;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.text = "Rejouer";
            labelText.raycastTarget = false;
        }

        private void InvokeRestart()
        {
            RestartRequested?.Invoke();
        }
    }
}
