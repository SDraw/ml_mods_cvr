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

            m_handRayLeft = LeapTracking.Instance.GetLeftHand().GetRoot().gameObject.AddComponent<ControllerRay>();
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

            m_handRayRight = LeapTracking.Instance.GetRightHand().GetRoot().gameObject.AddComponent<ControllerRay>();
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
                        if((base._inputManager.fingerFullCurlNormalizedLeftIndex < 0.2f) && (base._inputManager.fingerFullCurlNormalizedLeftMiddle > 0.75f) &&
                            (base._inputManager.fingerFullCurlNormalizedLeftRing > 0.75f) && (base._inputManager.fingerFullCurlNormalizedLeftPinky > 0.75f))
                        {
                            base._inputManager.gestureLeftRaw = (base._inputManager.fingerFullCurlNormalizedLeftThumb >= 0.5f) ? 4f : 3f;
                        }

                        // Peace Sign
                        if((base._inputManager.fingerFullCurlNormalizedLeftIndex < 0.2f) && (base._inputManager.fingerFullCurlNormalizedLeftMiddle < 0.2f) &&
                            (base._inputManager.fingerFullCurlNormalizedLeftRing > 0.75f) && (base._inputManager.fingerFullCurlNormalizedLeftPinky > 0.75f))
                        {
                            base._inputManager.gestureLeftRaw = 5f;
                        }

                        // Rock and Roll
                        if((base._inputManager.fingerFullCurlNormalizedLeftIndex < 0.2f) && (base._inputManager.fingerFullCurlNormalizedLeftMiddle > 0.75f) &&
                            (base._inputManager.fingerFullCurlNormalizedLeftRing > 0.75f) && (base._inputManager.fingerFullCurlNormalizedLeftPinky < 0.5f))
                        {
                            base._inputManager.gestureLeftRaw = 6f;
                        }

                        // Fist & Thumbs Up
                        if((base._inputManager.fingerFullCurlNormalizedLeftIndex > 0.5f) && (base._inputManager.fingerFullCurlNormalizedLeftMiddle > 0.5f) &&
                            (base._inputManager.fingerFullCurlNormalizedLeftRing > 0.5f) && (base._inputManager.fingerFullCurlNormalizedLeftPinky > 0.5f))
                        {
                            base._inputManager.gestureLeftRaw = (base._inputManager.fingerFullCurlNormalizedLeftThumb >= 0.5f) ? ((l_data.m_leftHand.m_grabStrength - 0.5f) * 2f) : 2f;
                        }

                        // Open Hand
                        if((base._inputManager.fingerFullCurlNormalizedLeftIndex < 0.2f) && (base._inputManager.fingerFullCurlNormalizedLeftMiddle < 0.2f) &&
                            (base._inputManager.fingerFullCurlNormalizedLeftRing < 0.2f) && (base._inputManager.fingerFullCurlNormalizedLeftPinky < 0.2f))
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
                        if((base._inputManager.fingerFullCurlNormalizedRightIndex < 0.2f) && (base._inputManager.fingerFullCurlNormalizedRightMiddle > 0.75f) &&
                            (base._inputManager.fingerFullCurlNormalizedRightRing > 0.75f) && (base._inputManager.fingerFullCurlNormalizedRightPinky > 0.75f))
                        {
                            base._inputManager.gestureRightRaw = (base._inputManager.fingerFullCurlNormalizedRightThumb >= 0.5f) ? 4f : 3f;
                        }

                        // Peace Sign
                        if((base._inputManager.fingerFullCurlNormalizedRightIndex < 0.2f) && (base._inputManager.fingerFullCurlNormalizedRightMiddle < 0.2f) &&
                            (base._inputManager.fingerFullCurlNormalizedRightRing > 0.75f) && (base._inputManager.fingerFullCurlNormalizedRightPinky > 0.75f))
                        {
                            base._inputManager.gestureRightRaw = 5f;
                        }

                        // Rock and Roll
                        if((base._inputManager.fingerFullCurlNormalizedRightIndex < 0.2f) && (base._inputManager.fingerFullCurlNormalizedRightMiddle > 0.75f) &&
                            (base._inputManager.fingerFullCurlNormalizedRightRing > 0.75f) && (base._inputManager.fingerFullCurlNormalizedRightPinky < 0.5f))
                        {
                            base._inputManager.gestureRightRaw = 6f;
                        }

                        // Fist & Thumbs Up
                        if((base._inputManager.fingerFullCurlNormalizedRightIndex > 0.5f) && (base._inputManager.fingerFullCurlNormalizedRightMiddle > 0.5f) &&
                            (base._inputManager.fingerFullCurlNormalizedRightRing > 0.5f) && (base._inputManager.fingerFullCurlNormalizedRightPinky > 0.5f))
                        {
                            base._inputManager.gestureRightRaw = (base._inputManager.fingerFullCurlNormalizedRightThumb >= 0.5f) ? ((l_data.m_rightHand.m_grabStrength - 0.5f) * 2f) : 2f;
                        }

                        // Open Hand
                        if((base._inputManager.fingerFullCurlNormalizedRightIndex < 0.2f) && (base._inputManager.fingerFullCurlNormalizedRightMiddle < 0.2f) &&
                            (base._inputManager.fingerFullCurlNormalizedRightRing < 0.2f) && (base._inputManager.fingerFullCurlNormalizedRightPinky < 0.2f))
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
            if(p_left)
            {
                base._inputManager.finger1StretchedLeftThumb = LeapTracked.ms_lastLeftFingerBones[0];
                base._inputManager.finger2StretchedLeftThumb = LeapTracked.ms_lastLeftFingerBones[1];
                base._inputManager.finger3StretchedLeftThumb = LeapTracked.ms_lastLeftFingerBones[2];
                base._inputManager.fingerSpreadLeftThumb = LeapTracked.ms_lastLeftFingerBones[3];

                base._inputManager.finger1StretchedLeftIndex = LeapTracked.ms_lastLeftFingerBones[4];
                base._inputManager.finger2StretchedLeftIndex = LeapTracked.ms_lastLeftFingerBones[5];
                base._inputManager.finger3StretchedLeftIndex = LeapTracked.ms_lastLeftFingerBones[6];
                base._inputManager.fingerSpreadLeftIndex = LeapTracked.ms_lastLeftFingerBones[7];

                base._inputManager.finger1StretchedLeftMiddle = LeapTracked.ms_lastLeftFingerBones[8];
                base._inputManager.finger2StretchedLeftMiddle = LeapTracked.ms_lastLeftFingerBones[9];
                base._inputManager.finger3StretchedLeftMiddle = LeapTracked.ms_lastLeftFingerBones[10];
                base._inputManager.fingerSpreadLeftMiddle = LeapTracked.ms_lastLeftFingerBones[11];

                base._inputManager.finger1StretchedLeftRing = LeapTracked.ms_lastLeftFingerBones[12];
                base._inputManager.finger2StretchedLeftRing = LeapTracked.ms_lastLeftFingerBones[13];
                base._inputManager.finger3StretchedLeftRing = LeapTracked.ms_lastLeftFingerBones[14];
                base._inputManager.fingerSpreadLeftRing = LeapTracked.ms_lastLeftFingerBones[15];

                base._inputManager.finger1StretchedLeftPinky = LeapTracked.ms_lastLeftFingerBones[16];
                base._inputManager.finger2StretchedLeftPinky = LeapTracked.ms_lastLeftFingerBones[17];
                base._inputManager.finger3StretchedLeftPinky = LeapTracked.ms_lastLeftFingerBones[18];
                base._inputManager.fingerSpreadLeftPinky = LeapTracked.ms_lastLeftFingerBones[19];

                base._inputManager.fingerFullCurlNormalizedLeftThumb = p_hand.m_bends[0];
                base._inputManager.fingerFullCurlNormalizedLeftIndex = p_hand.m_bends[1];
                base._inputManager.fingerFullCurlNormalizedLeftMiddle = p_hand.m_bends[2];
                base._inputManager.fingerFullCurlNormalizedLeftRing = p_hand.m_bends[3];
                base._inputManager.fingerFullCurlNormalizedLeftPinky = p_hand.m_bends[4];
            }
            else
            {
                base._inputManager.finger1StretchedRightThumb = LeapTracked.ms_lastRightFingerBones[0];
                base._inputManager.finger2StretchedRightThumb = LeapTracked.ms_lastRightFingerBones[1];
                base._inputManager.finger3StretchedRightThumb = LeapTracked.ms_lastRightFingerBones[2];
                base._inputManager.fingerSpreadRightThumb = LeapTracked.ms_lastRightFingerBones[3];

                base._inputManager.finger1StretchedRightIndex = LeapTracked.ms_lastRightFingerBones[4];
                base._inputManager.finger2StretchedRightIndex = LeapTracked.ms_lastRightFingerBones[5];
                base._inputManager.finger3StretchedRightIndex = LeapTracked.ms_lastRightFingerBones[6];
                base._inputManager.fingerSpreadRightIndex = LeapTracked.ms_lastRightFingerBones[7];

                base._inputManager.finger1StretchedRightMiddle = LeapTracked.ms_lastRightFingerBones[8];
                base._inputManager.finger2StretchedRightMiddle = LeapTracked.ms_lastRightFingerBones[9];
                base._inputManager.finger3StretchedRightMiddle = LeapTracked.ms_lastRightFingerBones[10];
                base._inputManager.fingerSpreadRightMiddle = LeapTracked.ms_lastRightFingerBones[11];

                base._inputManager.finger1StretchedRightRing = LeapTracked.ms_lastRightFingerBones[12];
                base._inputManager.finger2StretchedRightRing = LeapTracked.ms_lastRightFingerBones[13];
                base._inputManager.finger3StretchedRightRing = LeapTracked.ms_lastRightFingerBones[14];
                base._inputManager.fingerSpreadRightRing = LeapTracked.ms_lastRightFingerBones[15];

                base._inputManager.finger1StretchedRightPinky = LeapTracked.ms_lastRightFingerBones[16];
                base._inputManager.finger2StretchedRightPinky = LeapTracked.ms_lastRightFingerBones[17];
                base._inputManager.finger3StretchedRightPinky = LeapTracked.ms_lastRightFingerBones[18];
                base._inputManager.fingerSpreadRightPinky = LeapTracked.ms_lastRightFingerBones[19];

                base._inputManager.fingerFullCurlNormalizedRightThumb = p_hand.m_bends[0];
                base._inputManager.fingerFullCurlNormalizedRightIndex = p_hand.m_bends[1];
                base._inputManager.fingerFullCurlNormalizedRightMiddle = p_hand.m_bends[2];
                base._inputManager.fingerFullCurlNormalizedRightRing = p_hand.m_bends[3];
                base._inputManager.fingerFullCurlNormalizedRightPinky = p_hand.m_bends[4];
            }
        }

        void ResetFingers(bool p_left)
        {
            if(p_left)
            {
                base._inputManager.finger1StretchedLeftThumb = -0.5f;
                base._inputManager.finger2StretchedLeftThumb = 0.7f;
                base._inputManager.finger3StretchedLeftThumb = 0.7f;
                base._inputManager.fingerSpreadLeftThumb = 0f;

                base._inputManager.finger1StretchedLeftIndex = 0.5f;
                base._inputManager.finger2StretchedLeftIndex = 0.7f;
                base._inputManager.finger3StretchedLeftIndex = 0.7f;
                base._inputManager.fingerSpreadLeftIndex = 0f;

                base._inputManager.finger1StretchedLeftMiddle = 0.5f;
                base._inputManager.finger2StretchedLeftMiddle = 0.7f;
                base._inputManager.finger3StretchedLeftMiddle = 0.7f;
                base._inputManager.fingerSpreadLeftMiddle = 0f;

                base._inputManager.finger1StretchedLeftRing = 0.5f;
                base._inputManager.finger2StretchedLeftRing = 0.7f;
                base._inputManager.finger3StretchedLeftRing = 0.7f;
                base._inputManager.fingerSpreadLeftRing = 0f;

                base._inputManager.finger1StretchedLeftPinky = 0.5f;
                base._inputManager.finger2StretchedLeftPinky = 0.7f;
                base._inputManager.finger3StretchedLeftPinky = 0.7f;
                base._inputManager.fingerSpreadLeftPinky = 0f;

                base._inputManager.fingerFullCurlNormalizedLeftThumb = 0f;
                base._inputManager.fingerFullCurlNormalizedLeftIndex = 0f;
                base._inputManager.fingerFullCurlNormalizedLeftMiddle = 0f;
                base._inputManager.fingerFullCurlNormalizedLeftRing = 0f;
                base._inputManager.fingerFullCurlNormalizedLeftPinky = 0f;
            }
            else
            {
                base._inputManager.finger1StretchedRightThumb = -0.5f;
                base._inputManager.finger2StretchedRightThumb = 0.7f;
                base._inputManager.finger3StretchedRightThumb = 0.7f;
                base._inputManager.fingerSpreadRightThumb = 0f;

                base._inputManager.finger1StretchedRightIndex = 0.5f;
                base._inputManager.finger2StretchedRightIndex = 0.7f;
                base._inputManager.finger3StretchedRightIndex = 0.7f;
                base._inputManager.fingerSpreadRightIndex = 0f;

                base._inputManager.finger1StretchedRightMiddle = 0.5f;
                base._inputManager.finger2StretchedRightMiddle = 0.7f;
                base._inputManager.finger3StretchedRightMiddle = 0.7f;
                base._inputManager.fingerSpreadRightMiddle = 0f;

                base._inputManager.finger1StretchedRightRing = 0.5f;
                base._inputManager.finger2StretchedRightRing = 0.7f;
                base._inputManager.finger3StretchedRightRing = 0.7f;
                base._inputManager.fingerSpreadRightRing = 0f;

                base._inputManager.finger1StretchedRightPinky = 0.5f;
                base._inputManager.finger2StretchedRightPinky = 0.7f;
                base._inputManager.finger3StretchedRightPinky = 0.7f;
                base._inputManager.fingerSpreadRightPinky = 0f;

                base._inputManager.fingerFullCurlNormalizedRightThumb = 0f;
                base._inputManager.fingerFullCurlNormalizedRightIndex = 0f;
                base._inputManager.fingerFullCurlNormalizedRightMiddle = 0f;
                base._inputManager.fingerFullCurlNormalizedRightRing = 0f;
                base._inputManager.fingerFullCurlNormalizedRightPinky = 0f;
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
