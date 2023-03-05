using Opsive.Shared.Events;
using Opsive.UltimateCharacterController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Opsive.UltimateCharacterController.UI.HealthFlashMonitor;

namespace MBS.Lightfall
{
    public class ScreenFlashMonitor : CharacterMonitor
    {

        //holds temporal data for an active overlay, such as the remaining duration
        private class OverlayData
        {
            public Overlay Overlay;
            public Image Image;
            public float Duration;
            public bool WaitingForCleanup;

            private float m_fadeOutStartTime;
            private float m_fadeInStartTime;
            private float m_endKeyframeTime;
            private bool fadingOut;
            private bool fadingIn;

            public OverlayData(Overlay overlay, Image image)
            {
                Initalize(overlay, image);
            }

            public void Initalize(Overlay overlay, Image image)
            {
                Overlay = overlay;
                Image = image;
                Duration = overlay.VisiblityDuration;
                WaitingForCleanup = false;
                fadingOut = false;

                Image.color = overlay.Color;
                Image.sprite = overlay.Sprite;
                Image.name = "Overlay " + overlay.name;

                Image.gameObject.SetActive(true);
            }

            public void UpdateOverlayData()
            {
                if (!fadingOut)
                    HandleFadeIn();

                if (Duration != -1)
                    Duration -= Time.deltaTime;

                if (Duration <= 0 && Duration != -1)
                    HandleFadeOut();
            }

            private void HandleFadeIn()
            {
                if (!fadingIn)
                {
                    fadingIn = true;
                    m_fadeInStartTime = Time.time;
                    if (Overlay.FadeIn.length > 0)
                        m_endKeyframeTime = Overlay.FadeIn.keys[Overlay.FadeIn.length - 1].time;
                    else
                        m_endKeyframeTime = -1;
                }
                float currentTime = Time.time - m_fadeInStartTime;

                if (currentTime > m_endKeyframeTime)
                    return;

                HandleFade(currentTime, m_endKeyframeTime, Overlay.FadeIn);
            }

            private void HandleFadeOut()
            {
                if (!fadingOut)
                {
                    fadingOut = true;
                    m_fadeOutStartTime = Time.time;
                    if (Overlay.FadeIn.length > 0)
                        m_endKeyframeTime = Overlay.FadeIn.keys[Overlay.FadeIn.length - 1].time;
                    else
                        m_endKeyframeTime = -1;
                }
                float currentTime = Time.time - m_fadeOutStartTime;


                if (!WaitingForCleanup)
                    HandleFade(currentTime, m_endKeyframeTime, Overlay.FadeOut, true);

                if (currentTime > m_endKeyframeTime && !WaitingForCleanup)
                {
                    WaitingForCleanup = true;
                    var color = Image.color;
                    color.a = 0;
                    Image.color = color;
                }
            }

            private void HandleFade(float evaluationTime, float endFrameTime, AnimationCurve curve, bool isFadingOut = false)
            {
                float alpha;
                if (endFrameTime == -1)
                    alpha = isFadingOut ? 0 : Overlay.Color.a;
                else
                    alpha = Mathf.Min(curve.Evaluate(evaluationTime), 1) * Overlay.Color.a;

                var color = Image.color;
                color.a = alpha;
                Image.color = color;
            }
        }

        [SerializeField] private Image OverlayImagePrefab;
        private List<OverlayData> activeOverlays;
        private List<OverlayData> cleanupList;

        private bool IsOverlayAlreadyActive(Overlay overlay)
        {
            for (int i = 0; i < activeOverlays.Count; i++)
            {
                if (activeOverlays[i].Overlay == overlay)
                    return true;
            }
            return false;
        }

        private OverlayData GetActiveOverlay(Overlay overlay)
        {
            for (int i = 0; i < activeOverlays.Count; i++)
            {
                if (activeOverlays[i].Overlay == overlay)
                    return activeOverlays[i];
            }
            return null;
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            activeOverlays = new List<OverlayData>();
            cleanupList = new List<OverlayData>();

            base.Awake();
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null)
            {
                EventHandler.UnregisterEvent<Overlay>(m_Character, "StartOverlay", StartOverlay);
                EventHandler.UnregisterEvent<Overlay, bool>(m_Character, "StopOverlay", StopOverlay);
            }

            base.OnAttachCharacter(character);

            if (m_Character == null)
            {
                return;
            }

            EventHandler.RegisterEvent<Overlay>(m_Character, "StartOverlay", StartOverlay);
            EventHandler.RegisterEvent<Overlay, bool>(m_Character, "StopOverlay", StopOverlay);
        }


        private void StartOverlay(Overlay overlay)
        {

            if (IsOverlayAlreadyActive(overlay))
            {
                Debug.Log("Overlay is already active! Perhaps define a re-apply behavior to execute here?");
                return;
            }

            // Show the overlay/ flash image.

            Image newImage = Instantiate(OverlayImagePrefab, transform);

            activeOverlays.Add(new OverlayData(overlay, newImage));
        }

        private void StopOverlay(Overlay overlay, bool skipFadeOut)
        {
            OverlayData activeOverlay = GetActiveOverlay(overlay);
            if (activeOverlay == null)
                return;

            if (skipFadeOut)
                activeOverlay.WaitingForCleanup = true;
            else
                activeOverlay.Duration = 0;

        }

        private void Update()
        {

            for (int i = 0; i < activeOverlays.Count; i++)
            {
                activeOverlays[i].UpdateOverlayData();
                if (activeOverlays[i].WaitingForCleanup)
                    cleanupList.Add(activeOverlays[i]);

            }

            for (int i = 0; i < cleanupList.Count; i++)
            {
                Destroy(cleanupList[i].Image.gameObject);
                activeOverlays.Remove(cleanupList[i]);
            }
            cleanupList.Clear();
        }

    }

    [System.Serializable]
    public class Overlay
    {
        public string name;
        [Tooltip("Can the overlay be activated?")]
        [SerializeField] private bool m_CanActivate;
        [Tooltip("The Fade in duration and alpha curve")]
        [SerializeField]
        private AnimationCurve m_FadeIn;
        [Tooltip("The amount of time the flash should be fully visible for.")]
        [SerializeField] private float m_VisiblityDuration;
        [Tooltip("The Fade out duration and alpha curve")]
        [SerializeField]
        private AnimationCurve m_FadeOut;
        [Tooltip("The color of the image flash.")]
        [SerializeField] private Color m_Color;
        [Tooltip("The image of the flash.")]
        [SerializeField] private Sprite m_Sprite;

        public bool CanActivate { get { return m_CanActivate; } }
        public AnimationCurve FadeIn { get { return m_FadeIn; } }
        public float VisiblityDuration { get { return m_VisiblityDuration; } }
        public AnimationCurve FadeOut { get { return m_FadeOut; } }
        public Color Color { get { return m_Color; } }
        public Sprite Sprite { get { return m_Sprite; } }

        /// <summary>
        /// Constructor for the flash struct.
        /// </summary>
        /// <param name="canActivate">Can the flash be activated?</param>
        /// <param name="color">The amount of time the flash should be fully visible for.</param>
        /// <param name="visibilityDuration">The amount of time it takes the flash UI to fade.</param>
        /// <param name="fadeDuration">The amount of time it takes the flash UI to fade.</param>
        public Overlay(bool canActivate, Color color, float visibilityDuration, AnimationCurve fadeIn, AnimationCurve fadeOut)
        {
            m_CanActivate = canActivate;
            m_Color = color;
            m_VisiblityDuration = visibilityDuration;
            m_FadeIn = fadeIn;
            m_FadeOut = fadeOut;
            m_Sprite = null;
        }
    }
}