using System;
using System.Collections;
using UnityEngine;

namespace Blockblast.View
{
    public static class TweenUtils
    {
        // Drives a SmoothStep-eased animation over `duration` seconds.
        // `onStep` receives the eased t in [0, 1] each frame, and is invoked
        // one final time with t=1f after the loop so callers can rely on it
        // for the snap-to-target step.
        public static IEnumerator LerpEased(float duration, Action<float> onStep)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                onStep(Mathf.SmoothStep(0f, 1f, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            onStep(1f);
        }
    }
}
