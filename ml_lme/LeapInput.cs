using ABI.CCK.Components;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.InputManagement;
using ABI_RC.Systems.VRModeSwitch;
using System.Collections;
using UnityEngine;

namespace ml_lme
{
    class LeapInput : CVRInputModule
    {
        bool m_inVR = false;
        bool m_gripToGrab = true;

        bool m_handVisibleLeft = false;
        bool m_handVisibleRight = false;
        ControllerRay m_handRayLeft = null;
        ControllerRay m_handRayRight = null;
        LineRenderer m_lineLeft = null;
        LineRenderer m_lineRight = null;
        bool m_interactLeft = false;
        bool m_interactRight = false;
        bool m_gripLeft = false;
        bool m_gripRight = false;

        public override void ModuleAdded()
        {
            base.ModuleAdded();
            base.InputEnabled = Settings.Enabled;
            base.HapticFeedback = false;

            m_inVR = Utils.IsInVR();

            m_handRayLeft = LeapTracking.Instance.GetLeftHand().gameObject.AddComponent<ControllerRay>();
            m_handRayLeft.hand = true;
            m_handRayLeft.generalMask = -269;
            m_handRayLeft.isInteractionRay = true;
            m_handRayLeft.triggerGazeEvents = false;
            m_handRayLeft.holderRoot = m_handRayLeft.gameObject;
            m_handRayLeft.attachmentDistance = 0f;
            m_handRayLeft.uiMask = 32;
            m_handRayLeft.isDesktopRay = !m_inVR;

            m_lineLeft = m_handRayLeft.gameObject.AddComponent<LineRenderer>();
            m_lineLeft.endWidth = 1f;
            m_lineLeft.startWidth = 1f;
            m_lineLeft.textureMode = LineTextureMode.Tile;
            m_lineLeft.useWorldSpace = false;
            m_lineLeft.widthMultiplier = 1f;
            m_lineLeft.allowOcclusionWhenDynamic = false;
            m_lineLeft.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_lineLeft.enabled = false;
            m_lineLeft.receiveShadows = false;
            m_handRayLeft.lineRenderer = m_lineLeft;

            m_handRayRight = LeapTracking.Instance.GetRightHand().gameObject.AddComponent<ControllerRay>();
            m_handRayRight.hand = false;
            m_handRayRight.generalMask = -269;
            m_handRayRight.isInteractionRay = true;
            m_handRayRight.triggerGazeEvents = false;
            m_handRayRight.holderRoot = m_handRayRight.gameObject;
            m_handRayRight.attachmentDistance = 0f;
            m_handRayRight.uiMask = 32;
            m_handRayRight.isDesktopRay = !m_inVR;

            m_lineRight = m_handRayRight.gameObject.AddComponent<LineRenderer>();
            m_lineRight.endWidth = 1f;
            m_lineRight.startWidth = 1f;
            m_lineRight.textureMode = LineTextureMode.Tile;
            m_lineRight.useWorldSpace = false;
            m_lineRight.widthMultiplier = 1f;
            m_lineRight.allowOcclusionWhenDynamic = false;
            m_lineRight.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_lineRight.enabled = false;
            m_lineRight.receiveShadows = false;
            m_handRayRight.lineRenderer = m_lineRight;

            m_handRayLeft.otherRay = m_handRayRight;
            m_handRayRight.otherRay = m_handRayLeft;

            Settings.EnabledChange += this.OnEnableChange;
            Settings.InteractionChange += this.OnInteractionChange;
            Settings.GesturesChange += this.OnGesturesChange;
            Settings.FingersOnlyChange += this.OnFingersOnlyChange;

            OnEnableChange(Settings.Enabled);
            OnInteractionChange(Settings.Interaction);
            OnGesturesChange(Settings.Gestures);
            OnFingersOnlyChange(Settings.FingersOnly);

            MelonLoader.MelonCoroutines.Start(WaitForSettings());
            MelonLoader.MelonCoroutines.Start(WaitForMaterial());

            VRModeSwitchEvents.OnInitializeXR.AddListener(OnModeSwitch);
            VRModeSwitchEvents.OnDeinitializeXR.AddListener(OnModeSwitch);
        }

        IEnumerator WaitForSettings()
        {
            while(MetaPort.Instance == null)
                yield return null;
            while(MetaPort.Instance.settings == null)
                yield return null;

            m_gripToGrab = MetaPort.Instance.settings.GetSettingsBool("ControlUseGripToGrab", true);
            MetaPort.Instance.settings.settingBoolChanged.AddListener(this.OnGameSettingBoolChange);
        }

        IEnumerator WaitForMaterial()
        {
            while(PlayerSetup.Instance == null)
                yield return null;
            while(PlayerSetup.Instance.vrRayLeft == null)
                yield return null;
            while(PlayerSetup.Instance.vrRayLeft.lineRenderer == null)
                yield return null;

            m_lineLeft.material = PlayerSetup.Instance.vrRayLeft.lineRenderer.material;
            m_lineLeft.gameObject.layer = PlayerSetup.Instance.vrRayLeft.gameObject.layer;
            m_handRayLeft.highlightMaterial = PlayerSetup.Instance.vrRayLeft.highlightMaterial;
            m_handRayLeft.SetVRActive(m_inVR);

            m_lineRight.material = PlayerSetup.Instance.vrRayLeft.lineRenderer.material;
            m_lineRight.gameObject.layer = PlayerSetup.Instance.vrRayLeft.gameObject.layer;
            m_handRayRight.highlightMaterial = PlayerSetup.Instance.vrRayLeft.highlightMaterial;
            m_handRayRight.SetVRActive(m_inVR);
        }

        public override void ModuleDestroyed()
        {
            base.ModuleDestroyed();

            if(m_handRayLeft != null)
                Object.Destroy(m_handRayLeft);
            m_handRayLeft = null;

            if(m_handRayRight != null)
                Object.Destroy(m_handRayRight);
            m_handRayRight = null;

            if(m_lineLeft != null)
                Object.Destroy(m_lineLeft);
            m_lineLeft = null;

            if(m_lineRight != null)
                Object.Destroy(m_lineRight);
            m_lineRight = null;

            Settings.EnabledChange -= this.OnEnableChange;
            Settings.InteractionChange -= this.OnInteractionChange;
            Settings.GesturesChange -= this.OnGesturesChange;
            Settings.FingersOnlyChange -= this.OnFingersOnlyChange;

            MetaPort.Instance.settings.settingBoolChanged.RemoveListener(this.OnGameSettingBoolChange);
            VRModeSwitchEvents.OnInitializeXR.RemoveListener(OnModeSwitch);
            VRModeSwitchEvents.OnDeinitializeXR.RemoveListener(OnModeSwitch);
        }

        public override void UpdateInput()
        {
            if(base.InputEnabled)
            {
                LeapParser.LeapData l_data = LeapManager.Instance.GetLatestData();

                if(l_data.m_leftHand.m_present)
                {
                    m_handVisibleLeft = true;

                    SetFingersInput(l_data.m_leftHand, true);

                    if(Settings.Gestures)
                    {
                        base._inputManager.gestureLeftRaw = 0f;

                        // Finger Point & Finger Gun
                        if((base._inputManager.fingerCurlLeftIndex < 0.2f) && (base._inputManager.fingerCurlLeftMiddle > 0.75f) &&
                            (base._inputManager.fingerCurlLeftRing > 0.75f) && (base._inputManager.fingerCurlLeftPinky > 0.75f))
                        {
                            base._inputManager.gestureLeftRaw = (base._inputManager.fingerCurlLeftThumb >= 0.5f) ? 4f : 3f;
                        }

                        // Peace Sign
                        if((base._inputManager.fingerCurlLeftIndex < 0.2f) && (base._inputManager.fingerCurlLeftMiddle < 0.2f) &&
                            (base._inputManager.fingerCurlLeftRing > 0.75f) && (base._inputManager.fingerCurlLeftPinky > 0.75f))
                        {
                            base._inputManager.gestureLeftRaw = 5f;
                        }

                        // Rock and Roll
                        if((base._inputManager.fingerCurlLeftIndex < 0.2f) && (base._inputManager.fingerCurlLeftMiddle > 0.75f) &&
                            (base._inputManager.fingerCurlLeftRing > 0.75f) && (base._inputManager.fingerCurlLeftPinky < 0.5f))
                        {
                            base._inputManager.gestureLeftRaw = 6f;
                        }

                        // Fist & Thumbs Up
                        if((base._inputManager.fingerCurlLeftIndex > 0.5f) && (base._inputManager.fingerCurlLeftMiddle > 0.5f) &&
                            (base._inputManager.fingerCurlLeftRing > 0.5f) && (base._inputManager.fingerCurlLeftPinky > 0.5f))
                        {
                            base._inputManager.gestureLeftRaw = (base._inputManager.fingerCurlLeftThumb >= 0.5f) ? ((l_data.m_leftHand.m_grabStrength - 0.5f) * 2f) : 2f;
                        }

                        // Open Hand
                        if((base._inputManager.fingerCurlLeftIndex < 0.2f) && (base._inputManager.fingerCurlLeftMiddle < 0.2f) &&
                            (base._inputManager.fingerCurlLeftRing < 0.2f) && (base._inputManager.fingerCurlLeftPinky < 0.2f))
                        {
                            base._inputManager.gestureLeftRaw = -1f;
                        }

                        base._inputManager.gestureLeft = base._inputManager.gestureLeftRaw;
                    }
                }
                else
                {
                    if(m_handVisibleLeft)
                    {
                        ResetFingers(true);
                        if(Settings.Gestures)
                            ResetGestures(true);
                    }

                    m_handVisibleLeft = false;
                }

                if(l_data.m_rightHand.m_present)
                {
                    m_handVisibleRight = true;

                    SetFingersInput(l_data.m_rightHand, false);

                    if(Settings.Gestures)
                    {
                        base._inputManager.gestureRightRaw = 0f;

                        // Finger Point & Finger Gun
                        if((base._inputManager.fingerCurlRightIndex < 0.2f) && (base._inputManager.fingerCurlRightMiddle > 0.75f) &&
                            (base._inputManager.fingerCurlRightRing > 0.75f) && (base._inputManager.fingerCurlRightPinky > 0.75f))
                        {
                            base._inputManager.gestureRightRaw = (base._inputManager.fingerCurlRightThumb >= 0.5f) ? 4f : 3f;
                        }

                        // Peace Sign
                        if((base._inputManager.fingerCurlRightIndex < 0.2f) && (base._inputManager.fingerCurlRightMiddle < 0.2f) &&
                            (base._inputManager.fingerCurlRightRing > 0.75f) && (base._inputManager.fingerCurlRightPinky > 0.75f))
                        {
                            base._inputManager.gestureRightRaw = 5f;
                        }

                        // Rock and Roll
                        if((base._inputManager.fingerCurlRightIndex < 0.2f) && (base._inputManager.fingerCurlRightMiddle > 0.75f) &&
                            (base._inputManager.fingerCurlRightRing > 0.75f) && (base._inputManager.fingerCurlRightPinky < 0.5f))
                        {
                            base._inputManager.gestureRightRaw = 6f;
                        }

                        // Fist & Thumbs Up
                        if((base._inputManager.fingerCurlRightIndex > 0.5f) && (base._inputManager.fingerCurlRightMiddle > 0.5f) &&
                            (base._inputManager.fingerCurlRightRing > 0.5f) && (base._inputManager.fingerCurlRightPinky > 0.5f))
                        {
                            base._inputManager.gestureRightRaw = (base._inputManager.fingerCurlRightThumb >= 0.5f) ? ((l_data.m_rightHand.m_grabStrength - 0.5f) * 2f) : 2f;
                        }

                        // Open Hand
                        if((base._inputManager.fingerCurlRightIndex < 0.2f) && (base._inputManager.fingerCurlRightMiddle < 0.2f) &&
                            (base._inputManager.fingerCurlRightRing < 0.2f) && (base._inputManager.fingerCurlRightPinky < 0.2f))
                        {
                            base._inputManager.gestureRightRaw = -1f;
                        }

                        base._inputManager.gestureRight = base._inputManager.gestureRightRaw;
                    }
                }
                else
                {
                    if(m_handVisibleRight)
                    {
                        ResetFingers(false);
                        if(Settings.Gestures)
                            ResetGestures(false);
                    }

                    m_handVisibleRight = false;
                }

                if(!ModSupporter.SkipFingersOverride() && (!m_inVR || !Utils.AreKnucklesInUse()))
                    SetGameFingersTracking(m_handVisibleRight || m_handVisibleLeft);

                base.UpdateInput();
            }
        }

        public override void Update_Interaction()
        {
            if(Settings.Interaction)
            {
                LeapParser.LeapData l_data = LeapManager.Instance.GetLatestData();

                if(m_handVisibleLeft && !Settings.FingersOnly)
                {
                    float l_strength = l_data.m_leftHand.m_grabStrength;

                    float l_interactValue;
                    if(m_gripToGrab)
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(Settings.GripThreadhold, Settings.InteractThreadhold), Mathf.Max(Settings.GripThreadhold, Settings.InteractThreadhold), l_strength));
                    else
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.InteractThreadhold, l_strength));
                    base._inputManager.interactLeftValue = Mathf.Max(l_interactValue, base._inputManager.interactLeftValue);

                    if(m_interactLeft != (l_strength > Settings.InteractThreadhold))
                    {
                        m_interactLeft = (l_strength > Settings.InteractThreadhold);
                        base._inputManager.interactLeftDown |= m_interactLeft;
                        base._inputManager.interactLeftUp |= !m_interactLeft;
                    }

                    float l_gripValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.GripThreadhold, l_strength));
                    base._inputManager.gripLeftValue = Mathf.Max(l_gripValue, base._inputManager.gripLeftValue);
                    if(m_gripLeft != (l_strength > Settings.GripThreadhold))
                    {
                        m_gripLeft = (l_strength > Settings.GripThreadhold);
                        base._inputManager.gripLeftDown |= m_gripLeft;
                        base._inputManager.gripLeftUp |= !m_gripLeft;
                    }
                }

                if(m_handVisibleRight && !Settings.FingersOnly)
                {
                    float l_strength = l_data.m_rightHand.m_grabStrength;

                    float l_interactValue;
                    if(m_gripToGrab)
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(Settings.GripThreadhold, Settings.InteractThreadhold), Mathf.Max(Settings.GripThreadhold, Settings.InteractThreadhold), l_strength));
                    else
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.InteractThreadhold, l_strength));
                    base._inputManager.interactRightValue = Mathf.Max(l_interactValue, base._inputManager.interactRightValue);

                    if(m_interactRight != (l_strength > Settings.InteractThreadhold))
                    {
                        m_interactRight = (l_strength > Settings.InteractThreadhold);
                        base._inputManager.interactRightDown |= m_interactRight;
                        base._inputManager.interactRightUp |= !m_interactRight;
                    }

                    float l_gripValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.GripThreadhold, l_strength));
                    base._inputManager.gripRightValue = Mathf.Max(l_gripValue, base._inputManager.gripRightValue);
                    if(m_gripRight != (l_strength > Settings.GripThreadhold))
                    {
                        m_gripRight = (l_strength > Settings.GripThreadhold);
                        base._inputManager.gripRightDown |= m_gripRight;
                        base._inputManager.gripRightUp |= !m_gripRight;
                    }
                }

                ToggleHandRay(m_handVisibleLeft && !Settings.FingersOnly, true);
                ToggleHandRay(m_handVisibleRight && !Settings.FingersOnly, false);
            }
        }

        // Settings changes
        void OnEnableChange(bool p_state)
        {
            base.InputEnabled = p_state;

            m_handVisibleLeft &= p_state;
            m_handVisibleRight &= p_state;

            if(!p_state)
            {
                ResetFingers(true);
                ResetFingers(false);

                if(Settings.Gestures)
                {
                    ResetGestures(true);
                    ResetGestures(false);
                }

                // Reset to default, FreedomFingers can go brrr, player should press funny controller button two times
                SetGameFingersTracking(m_inVR && Utils.AreKnucklesInUse() && !CVRInputManager._moduleXR.GestureToggleValue);
            }

            OnInteractionChange(Settings.Interaction);
        }

        void OnInteractionChange(bool p_state)
        {
            bool l_state = (p_state && Settings.Enabled && !Settings.FingersOnly);

            ToggleHandRay(l_state, true);
            ToggleHandRay(l_state, false);

            if(!l_state)
            {
                m_handRayLeft.DropObject(true);
                m_handRayLeft.ClearGrabbedObject();

                m_handRayRight.DropObject(true);
                m_handRayRight.ClearGrabbedObject();

                m_interactLeft = false;
                m_interactRight = false;
                m_gripLeft = false;
                m_gripRight = false;
            }
        }

        void OnGesturesChange(bool p_state)
        {
            base._inputManager.gestureLeft = 0f;
            base._inputManager.gestureLeftRaw = 0f;
            base._inputManager.gestureRight = 0f;
            base._inputManager.gestureRightRaw = 0f;
        }

        void OnFingersOnlyChange(bool p_state)
        {
            OnInteractionChange(Settings.Interaction);
        }

        // Game events
        internal void OnRayScale(float p_scale)
        {
            m_handRayLeft.SetRayScale(p_scale);
            m_handRayRight.SetRayScale(p_scale);
        }

        internal void OnPickupGrab(CVRPickupObject p_pickup)
        {
            if(p_pickup.gripType == CVRPickupObject.GripType.Origin)
            {
                if(p_pickup._controllerRay == m_handRayLeft)
                {
                    m_handRayLeft.attachmentPoint.localPosition = Vector3.zero;
                    m_handRayLeft.attachmentPoint.localRotation = Quaternion.Euler(0f, 0f, 270f);
                }
                if(p_pickup._controllerRay == m_handRayRight)
                {
                    m_handRayRight.attachmentPoint.localPosition = Vector3.zero;
                    m_handRayRight.attachmentPoint.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
            }
        }

        void OnModeSwitch()
        {
            m_inVR = Utils.IsInVR();
            base._inputManager.SetModuleAsLast(this);

            if(m_handRayLeft != null)
            {
                m_handRayLeft.isDesktopRay = !m_inVR;
                m_handRayLeft.SetVRActive(m_inVR);
            }
            if(m_handRayRight != null)
            {
                m_handRayRight.isDesktopRay = !m_inVR;
                m_handRayRight.SetVRActive(m_inVR);
            }

            OnEnableChange(Settings.Enabled);
        }

        // Arbitrary
        void SetFingersInput(LeapParser.HandData p_hand, bool p_left)
        {
            // Game has spreads in range of [0;1], but mod now operates in range of [-1;1]
            // So spreads will be normalized towards game's range
            if(p_left)
            {
                base._inputManager.fingerCurlLeftThumb = p_hand.m_bends[0];
                base._inputManager.fingerCurlLeftIndex = p_hand.m_bends[1];
                base._inputManager.fingerCurlLeftMiddle = p_hand.m_bends[2];
                base._inputManager.fingerCurlLeftRing = p_hand.m_bends[3];
                base._inputManager.fingerCurlLeftPinky = p_hand.m_bends[4];
                base._inputManager.fingerSpreadLeftThumb = 1f - (p_hand.m_spreads[0] * 0.5f + 0.5f);
                base._inputManager.fingerSpreadLeftIndex = 1f - (p_hand.m_spreads[1] * 0.5f + 0.5f);
                base._inputManager.fingerSpreadLeftMiddle = 1f - (p_hand.m_spreads[2] * 0.5f + 0.5f);
                base._inputManager.fingerSpreadLeftRing = 1f - (p_hand.m_spreads[3] * 0.5f + 0.5f);
                base._inputManager.fingerSpreadLeftPinky = 1f - (p_hand.m_spreads[4] * 0.5f + 0.5f);
            }
            else
            {
                base._inputManager.fingerCurlRightThumb = p_hand.m_bends[0];
                base._inputManager.fingerCurlRightIndex = p_hand.m_bends[1];
                base._inputManager.fingerCurlRightMiddle = p_hand.m_bends[2];
                base._inputManager.fingerCurlRightRing = p_hand.m_bends[3];
                base._inputManager.fingerCurlRightPinky = p_hand.m_bends[4];
                base._inputManager.fingerSpreadRightThumb = 1f - (p_hand.m_spreads[0] * 0.5f + 0.5f);
                base._inputManager.fingerSpreadRightIndex = 1f - (p_hand.m_spreads[1] * 0.5f + 0.5f);
                base._inputManager.fingerSpreadRightMiddle = 1f - (p_hand.m_spreads[2] * 0.5f + 0.5f);
                base._inputManager.fingerSpreadRightRing = 1f - (p_hand.m_spreads[3] * 0.5f + 0.5f);
                base._inputManager.fingerSpreadRightPinky = 1f - (p_hand.m_spreads[4] * 0.5f + 0.5f);
            }
        }

        void ResetFingers(bool p_left)
        {
            if(p_left)
            {
                base._inputManager.fingerCurlLeftThumb = 0f;
                base._inputManager.fingerCurlLeftIndex = 0f;
                base._inputManager.fingerCurlLeftMiddle = 0f;
                base._inputManager.fingerCurlLeftRing = 0f;
                base._inputManager.fingerCurlLeftPinky = 0f;
                base._inputManager.fingerSpreadLeftThumb = 0.5f;
                base._inputManager.fingerSpreadLeftIndex = 0.5f;
                base._inputManager.fingerSpreadLeftMiddle = 0.5f;
                base._inputManager.fingerSpreadLeftRing = 0.5f;
                base._inputManager.fingerSpreadLeftPinky = 0.5f;
            }
            else
            {
                base._inputManager.fingerCurlRightThumb = 0f;
                base._inputManager.fingerCurlRightIndex = 0f;
                base._inputManager.fingerCurlRightMiddle = 0f;
                base._inputManager.fingerCurlRightRing = 0f;
                base._inputManager.fingerCurlRightPinky = 0f;
                base._inputManager.fingerSpreadRightThumb = 0.5f;
                base._inputManager.fingerSpreadRightIndex = 0.5f;
                base._inputManager.fingerSpreadRightMiddle = 0.5f;
                base._inputManager.fingerSpreadRightRing = 0.5f;
                base._inputManager.fingerSpreadRightPinky = 0.5f;
            }
        }

        void ResetGestures(bool p_left)
        {
            if(p_left)
            {
                base._inputManager.gestureLeft = 0f;
                base._inputManager.gestureLeftRaw = 0f;
            }
            else
            {
                base._inputManager.gestureRight = 0f;
                base._inputManager.gestureRightRaw = 0f;
            }
        }

        void ToggleHandRay(bool p_state, bool p_left)
        {
            if(p_left)
            {
                m_handRayLeft.enabled = p_state;
                ((MonoBehaviour)m_handRayLeft).enabled = p_state;
                m_lineLeft.enabled = p_state;
                m_lineLeft.forceRenderingOff = !p_state;
            }
            else
            {
                m_handRayRight.enabled = p_state;
                ((MonoBehaviour)m_handRayRight).enabled = p_state;
                m_lineRight.enabled = p_state;
                m_lineRight.forceRenderingOff = !p_state;
            }
        }

        // Game settings
        void OnGameSettingBoolChange(string p_name, bool p_state)
        {
            if(p_name == "ControlUseGripToGrab")
                m_gripToGrab = p_state;
        }

        void SetGameFingersTracking(bool p_state)
        {
            base._inputManager.individualFingerTracking = p_state;
            IKSystem.Instance.FingerSystem.controlActive = base._inputManager.individualFingerTracking;
        }
    }
}
