using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Button with the spec's scale-on-tap micro-interaction (1.0 -> 1.04 -> 1.0 in 0.08s).
    /// Replaces Unity Button's default color tint, which doesn't fit the warm theme.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class WhiskerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public Action onClick;
        private Coroutine pulseRoutine;
        private const float PulseDuration = 0.08f;
        private const float PulseScale = 1.04f;

        public void OnPointerDown(PointerEventData eventData) { Pulse(PulseScale); }
        public void OnPointerUp(PointerEventData eventData)   { Pulse(1f); }
        public void OnPointerClick(PointerEventData eventData) { onClick?.Invoke(); }

        private void Pulse(float target)
        {
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseTo(target));
        }

        private IEnumerator PulseTo(float target)
        {
            var rt = (RectTransform)transform;
            Vector3 start = rt.localScale;
            Vector3 end = new Vector3(target, target, 1f);
            float t = 0f;
            while (t < PulseDuration)
            {
                t += Time.unscaledDeltaTime;
                rt.localScale = Vector3.Lerp(start, end, Mathf.Clamp01(t / PulseDuration));
                yield return null;
            }
            rt.localScale = end;
            pulseRoutine = null;
        }

        public static WhiskerButton Attach(GameObject go, Action onClick)
        {
            var img = go.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
            var btn = go.AddComponent<WhiskerButton>();
            btn.onClick = onClick;
            return btn;
        }
    }
}
