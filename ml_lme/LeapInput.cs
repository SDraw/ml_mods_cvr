using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.InputManagement;
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

        ~LeapInput()
        {
            Settings.EnabledChange -= this.OnEnableChange;
            Settings.InputChange -= this.OnInputChange;

            MetaPort.Instance.settings.settingBoolChanged.RemoveListener(this.OnGameSettingBoolChange);
        }

        public override void ModuleAdded()
        {
            base.ModuleAdded();

            InputEnabled = Settings.Enabled;
            HapticFeedback = false;

            m_inVR = Utils.IsInVR();

            m_handRayLeft = LeapTracking.Instance.GetLeftHand().gameObject.AddComponent<ControllerRay>();
            m_handRayLeft.hand = true;
            m_handRayLeft.generalMask = -1485;
            m_handRayLeft.isInteractionRay = true;
            m_handRayLeft.triggerGazeEvents = false;
            m_handRayLeft.holderRoot = m_handRayLeft.gameObject;

            m_handRayRight = LeapTracking.Instance.GetRightHand().gameObject.AddComponent<ControllerRay>();
            m_handRayRight.hand = false;
            m_handRayRight.generalMask = -1485;
            m_handRayRight.isInteractionRay = true;
            m_handRayRight.triggerGazeEvents = false;
            m_handRayRight.holderRoot = m_handRayRight.gameObject;

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

            Settings.EnabledChange += this.OnEnableChange;
            Settings.InputChange += this.OnInputChange;

            OnEnableChange(Settings.Enabled);
            OnInputChange(Settings.Input);

            MelonLoader.MelonCoroutines.Start(WaitForSettings());
            MelonLoader.MelonCoroutines.Start(WaitForMaterial());
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
            while(PlayerSetup.Instance.leftRay == null)
                yield return null;
            while(PlayerSetup.Instance.leftRay.lineRenderer == null)
                yield return null;

            m_lineLeft.material = PlayerSetup.Instance.leftRay.lineRenderer.material;
            m_lineLeft.gameObject.layer = PlayerSetup.Instance.leftRay.gameObject.layer;
            m_lineRight.material = PlayerSetup.Instance.leftRay.lineRenderer.material;
            m_lineRight.gameObject.layer = PlayerSetup.Instance.leftRay.gameObject.layer;
        }

        public override void UpdateInput()
        {
            if(InputEnabled)
            {
                GestureMatcher.LeapData l_data = LeapManager.Instance.GetLatestData();

                if(l_data.m_leftHand.m_present)
                {
                    SetFingersInput(l_data.m_leftHand, true);
                    m_handVisibleLeft = true;
                }
                else
                {
                    if(m_handVisibleLeft)
                    {
                        ResetFingers(true);
                        m_handVisibleLeft = false;
                    }
                }

                if(l_data.m_rightHand.m_present)
                {
                    SetFingersInput(l_data.m_rightHand, false);
                    m_handVisibleRight = true;
                }
                else
                {
                    if(m_handVisibleRight)
                    {
                        ResetFingers(false);
                        m_handVisibleRight = false;
                    }
                }

                if(!ModSupporter.SkipFingersOverride())
                {
                    if(m_inVR)
                    {
                        _inputManager.individualFingerTracking = !CVRInputManager._moduleXR.GestureToggleValue;
                        _inputManager.individualFingerTracking |= (l_data.m_leftHand.m_present || l_data.m_rightHand.m_present);
                    }
                    else
                        _inputManager.individualFingerTracking = (l_data.m_leftHand.m_present || l_data.m_rightHand.m_present);
                    IKSystem.Instance.FingerSystem.controlActive = _inputManager.individualFingerTracking;
                }

                m_handRayLeft.enabled = (l_data.m_leftHand.m_present && (!m_inVR || !Utils.IsLeftHandTracked() || !Settings.FingersOnly));
                m_handRayRight.enabled = (l_data.m_rightHand.m_present && (!m_inVR || !Utils.IsRightHandTracked() || !Settings.FingersOnly));

                base.UpdateInput();
            }
        }

        public override void Update_Interaction()
        {
            if(Settings.Input)
            {
                GestureMatcher.LeapData l_data = LeapManager.Instance.GetLatestData();

                if(l_data.m_leftHand.m_present && (!m_inVR || !Utils.IsLeftHandTracked() || !Settings.FingersOnly))
                {
                    float l_strength = l_data.m_leftHand.m_grabStrength;

                    float l_interactValue = 0f;
                    if(m_gripToGrab)
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(Settings.GripThreadhold, Settings.InteractThreadhold), Mathf.Max(Settings.GripThreadhold, Settings.InteractThreadhold), l_strength));
                    else
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.InteractThreadhold, l_strength));
                    _inputManager.interactLeftValue = Mathf.Max(l_interactValue, _inputManager.interactLeftValue);

                    if(m_interactLeft != (l_strength > Settings.InteractThreadhold))
                    {
                        m_interactLeft = (l_strength > Settings.InteractThreadhold);
                        _inputManager.interactLeftDown |= m_interactLeft;
                        _inputManager.interactLeftUp |= !m_interactLeft;
                    }

                    float l_gripValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.GripThreadhold, l_strength));
                    _inputManager.gripLeftValue = Mathf.Max(l_gripValue, _inputManager.gripLeftValue);
                    if(m_gripLeft != (l_strength > Settings.GripThreadhold))
                    {
                        m_gripLeft = (l_strength > Settings.GripThreadhold);
                        _inputManager.gripLeftDown |= m_gripLeft;
                        _inputManager.gripLeftUp |= !m_gripLeft;
                    }
                }

                if(l_data.m_rightHand.m_present && (!m_inVR || !Utils.IsRightHandTracked() || !Settings.FingersOnly))
                {
                    float l_strength = l_data.m_rightHand.m_grabStrength;

                    float l_interactValue = 0f;
                    if(m_gripToGrab)
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(Mathf.Min(Settings.GripThreadhold, Settings.InteractThreadhold), Mathf.Max(Settings.GripThreadhold, Settings.InteractThreadhold), l_strength));
                    else
                        l_interactValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.InteractThreadhold, l_strength));
                    _inputManager.interactRightValue = Mathf.Max(l_interactValue, _inputManager.interactRightValue);

                    if(m_interactRight != (l_strength > Settings.InteractThreadhold))
                    {
                        m_interactRight = (l_strength > Settings.InteractThreadhold);
                        _inputManager.interactRightDown |= m_interactRight;
                        _inputManager.interactRightUp |= !m_interactRight;
                    }

                    float l_gripValue = Mathf.Clamp01(Mathf.InverseLerp(0f, Settings.GripThreadhold, l_strength));
                    _inputManager.gripRightValue = Mathf.Max(l_gripValue, _inputManager.gripRightValue);
                    if(m_gripRight != (l_strength > Settings.GripThreadhold))
                    {
                        m_gripRight = (l_strength > Settings.GripThreadhold);
                        _inputManager.gripRightDown |= m_gripRight;
                        _inputManager.gripRightUp |= !m_gripRight;
                    }
                }
            }
        }

        // Settings changes
        void OnEnableChange(bool p_state)
        {
            InputEnabled = p_state;

            OnInputChange(p_state && Settings.Input);
            UpdateFingerTracking();
            m_handVisibleLeft &= p_state;
            m_handVisibleRight &= p_state;
        }

        void OnInputChange(bool p_state)
        {
            ((MonoBehaviour)m_handRayLeft).enabled = (p_state && Settings.Enabled);
            ((MonoBehaviour)m_handRayRight).enabled = (p_state && Settings.Enabled);
            m_lineLeft.enabled = (p_state && Settings.Enabled);
            m_lineRight.enabled = (p_state && Settings.Enabled);

            if(!p_state)
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

        // Game events
        internal void OnAvatarSetup()
        {
            m_inVR = Utils.IsInVR();
            UpdateFingerTracking();
        }

        internal void OnRayScale(float p_scale)
        {
            m_handRayLeft.SetRayScale(p_scale);
            m_handRayRight.SetRayScale(p_scale);
        }

        // Arbitrary
        void UpdateFingerTracking()
        {
            _inputManager.individualFingerTracking = (Settings.Enabled || (m_inVR && Utils.AreKnucklesInUse() && !CVRInputManager._moduleXR.GestureToggleValue));
            IKSystem.Instance.FingerSystem.controlActive = _inputManager.individualFingerTracking;

            if(!Settings.Enabled)
            {
                ResetFingers(true);
                ResetFingers(false);
            }
        }

        void SetFingersInput(GestureMatcher.HandData p_hand, bool p_left)
        {
            if(p_left)
            {
                _inputManager.fingerCurlLeftThumb = p_hand.m_bends[0];
                _inputManager.fingerCurlLeftIndex = p_hand.m_bends[1];
                _inputManager.fingerCurlLeftMiddle = p_hand.m_bends[2];
                _inputManager.fingerCurlLeftRing = p_hand.m_bends[3];
                _inputManager.fingerCurlLeftPinky = p_hand.m_bends[4];
                _inputManager.fingerSpreadLeftThumb = p_hand.m_spreads[0];
                _inputManager.fingerSpreadLeftIndex = p_hand.m_spreads[1];
                _inputManager.fingerSpreadLeftMiddle = p_hand.m_spreads[2];
                _inputManager.fingerSpreadLeftRing = p_hand.m_spreads[3];
                _inputManager.fingerSpreadLeftPinky = p_hand.m_spreads[4];
            }
            else
            {
                _inputManager.fingerCurlRightThumb = p_hand.m_bends[0];
                _inputManager.fingerCurlRightIndex = p_hand.m_bends[1];
                _inputManager.fingerCurlRightMiddle = p_hand.m_bends[2];
                _inputManager.fingerCurlRightRing = p_hand.m_bends[3];
                _inputManager.fingerCurlRightPinky = p_hand.m_bends[4];
                _inputManager.fingerSpreadRightThumb = p_hand.m_spreads[0];
                _inputManager.fingerSpreadRightIndex = p_hand.m_spreads[1];
                _inputManager.fingerSpreadRightMiddle = p_hand.m_spreads[2];
                _inputManager.fingerSpreadRightRing = p_hand.m_spreads[3];
                _inputManager.fingerSpreadRightPinky = p_hand.m_spreads[4];
            }
        }

        void ResetFingers(bool p_left)
        {
            if(p_left)
            {
                _inputManager.fingerCurlLeftThumb = 0f;
                _inputManager.fingerCurlLeftIndex = 0f;
                _inputManager.fingerCurlLeftMiddle = 0f;
                _inputManager.fingerCurlLeftRing = 0f;
                _inputManager.fingerCurlLeftPinky = 0f;
                _inputManager.fingerSpreadLeftThumb = 0f;
                _inputManager.fingerSpreadLeftIndex = 0f;
                _inputManager.fingerSpreadLeftMiddle = 0f;
                _inputManager.fingerSpreadLeftRing = 0f;
                _inputManager.fingerSpreadLeftPinky = 0f;
            }
            else
            {
                _inputManager.fingerCurlRightThumb = 0f;
                _inputManager.fingerCurlRightIndex = 0f;
                _inputManager.fingerCurlRightMiddle = 0f;
                _inputManager.fingerCurlRightRing = 0f;
                _inputManager.fingerCurlRightPinky = 0f;
                _inputManager.fingerSpreadRightThumb = 0f;
                _inputManager.fingerSpreadRightIndex = 0f;
                _inputManager.fingerSpreadRightMiddle = 0f;
                _inputManager.fingerSpreadRightRing = 0f;
                _inputManager.fingerSpreadRightPinky = 0f;
            }
        }

        // Game settings
        void OnGameSettingBoolChange(string p_name, bool p_state)
        {
            if(p_name == "ControlUseGripToGrab")
                m_gripToGrab = p_state;
        }
    }
}
