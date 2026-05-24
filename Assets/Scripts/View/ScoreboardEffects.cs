using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Blockblast.View
{
    public class ScoreboardEffects
    {
        private readonly MonoBehaviour coroutineHost;
        private readonly Text scoreText;
        private readonly RectTransform scoreRect;
        private readonly Image backgroundImage;
        private readonly RectTransform celebrationOverlayRect;

        private Coroutine bumpRoutine;

        public ScoreboardEffects(MonoBehaviour coroutineHost, Text scoreText, RectTransform scoreRect,
                                 Image backgroundImage, RectTransform celebrationOverlayRect)
        {
            this.coroutineHost = coroutineHost;
            this.scoreText = scoreText;
            this.scoreRect = scoreRect;
            this.backgroundImage = backgroundImage;
            this.celebrationOverlayRect = celebrationOverlayRect;
        }

        public void BumpScore(bool big, Color accentColor)
        {
            if (bumpRoutine != null) coroutineHost.StopCoroutine(bumpRoutine);
            bumpRoutine = coroutineHost.StartCoroutine(BumpScoreRoutine(big, accentColor));
        }

        public void PulseBackground(int comboCount)
        {
            coroutineHost.StartCoroutine(PulseBackgroundRoutine(comboCount));
        }

        public void ShowLevelCelebration(string themeName, Color accentColor)
        {
            coroutineHost.StartCoroutine(SpawnFloatingTextRoutine(
                text: $"{themeName} débloqué !",
                color: accentColor,
                fontSize: 110,
                startAnchoredY: 0f,
                riseDistance: 200f,
                holdDuration: 1.4f
            ));
        }

        public void ShowFloatingBonus(int bonusPoints, int comboMultiplier, Color accentColor)
        {
            string bonusText = comboMultiplier > 1 ? $"×{comboMultiplier}   +{bonusPoints}" : $"+{bonusPoints}";
            int bonusFontSize = comboMultiplier >= 4 ? 180 : comboMultiplier >= 3 ? 160 : comboMultiplier >= 2 ? 145 : 130;
            float bonusHoldDuration = comboMultiplier >= 3 ? 1.2f : 0.9f;
            float bonusRiseDistance = comboMultiplier >= 3 ? 260f : 220f;
            coroutineHost.StartCoroutine(SpawnFloatingTextRoutine(
                text: bonusText,
                color: accentColor,
                fontSize: bonusFontSize,
                startAnchoredY: 80f,
                riseDistance: bonusRiseDistance,
                holdDuration: bonusHoldDuration
            ));
        }

        private IEnumerator BumpScoreRoutine(bool big, Color accentColor)
        {
            float duration = big ? 0.5f : 0.25f;
            float peakScale = big ? 1.6f : 1.15f;
            Color baseColor = scoreText.color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float curve = 4f * t * (1f - t);
                float scale = Mathf.Lerp(1f, peakScale, curve);
                scoreRect.localScale = Vector3.one * scale;
                if (big) scoreText.color = Color.Lerp(baseColor, accentColor, curve);
                elapsed += Time.deltaTime;
                yield return null;
            }
            scoreRect.localScale = Vector3.one;
            if (big) scoreText.color = baseColor;
            bumpRoutine = null;
        }

        private IEnumerator PulseBackgroundRoutine(int comboCount)
        {
            if (backgroundImage == null) yield break;
            float pulseIntensity = Mathf.Clamp01((comboCount - 1) / 3f);
            float pulseStrength = Mathf.Lerp(0.08f, 0.22f, pulseIntensity);
            float pulseDuration = Mathf.Lerp(0.20f, 0.40f, pulseIntensity);
            Color baseColor = backgroundImage.color;
            Color pulseColor = Color.Lerp(baseColor, Color.white, pulseStrength);
            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                float t = elapsed / pulseDuration;
                float curve = 4f * t * (1f - t);
                backgroundImage.color = Color.Lerp(baseColor, pulseColor, curve);
                elapsed += Time.deltaTime;
                yield return null;
            }
            backgroundImage.color = baseColor;
        }

        private IEnumerator SpawnFloatingTextRoutine(string text, Color color, int fontSize, float startAnchoredY, float riseDistance, float holdDuration)
        {
            var textGo = new GameObject("FloatingText", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(celebrationOverlayRect, false);

            var rect = (RectTransform)textGo.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, 200f);
            rect.anchoredPosition = new Vector2(0f, startAnchoredY);

            var label = textGo.GetComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = fontSize;
            label.color = color;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.raycastTarget = false;

            float elapsed = 0f;
            Vector2 startPos = rect.anchoredPosition;
            while (elapsed < holdDuration)
            {
                float t = elapsed / holdDuration;
                rect.anchoredPosition = startPos + new Vector2(0f, riseDistance * t);
                Color c = label.color;
                c.a = t < 0.7f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);
                label.color = c;
                rect.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.05f, Mathf.Clamp01(t * 3f));
                elapsed += Time.deltaTime;
                yield return null;
            }
            Object.Destroy(textGo);
        }
    }
}
