using ABI_RC.Core.Savior;
using ABI_RC.Systems.InputManagement;
using ABI_RC.Systems.VRModeSwitch;
using UnityEngine;

namespace ml_bft
{
    // Not an actual module, but can be used as one
    class InputHandler
    {
        public static InputHandler Instance { get; private set; } = null;

        bool m_active = false;

        HandHandler m_leftHandHandler = null;
        HandHandler m_rightHandHandler = null;

        internal InputHandler()
        {
            if(Instance == null)
                Instance = this;

            m_active = false;

            if(MetaPort.Instance.isUsingVr)
                SetupHandlers();

            VRModeSwitchEvents.OnInitializeXR.AddListener(this.OnSwitchToVR);
            VRModeSwitchEvents.OnDeinitializeXR.AddListener(this.OnSwitchToDesktop);

            Settings.OnSkeletalInputChanged.AddListener(this.OnSkeletalInputChanged);

            GameEvents.OnInputUpdate.AddListener(this.OnInputUpdate);
        }
        internal void Cleanup()
        {
            if(Instance == this)
                Instance = null;

            RemoveHandlers();

            Settings.OnSkeletalInputChanged.RemoveListener(this.OnSkeletalInputChanged);

            GameEvents.OnInputUpdate.RemoveListener(this.OnInputUpdate);
        }

        void SetupHandlers()
        {
            if(!CheckVR.Instance.forceOpenXr)
            {
                m_leftHandHandler = new HandHandlerVR(CVRInputManager.Instance.leftHandTransform, true);
                m_rightHandHandler = new HandHandlerVR(CVRInputManager.Instance.rightHandTransform, false);
                m_active = true;
            }
        }
        void RemoveHandlers()
        {
            m_leftHandHandler?.Cleanup();
            m_leftHandHandler = null;
            m_rightHandHandler?.Cleanup();
            m_rightHandHandler = null;
            m_active = false;
        }

        public void Rebind(Quaternion p_base)
        {
            if(m_active)
            {
                m_leftHandHandler?.Rebind(p_base);
                m_rightHandHandler?.Rebind(p_base);
            }
        }

        public Transform GetSourceForBone(HumanBodyBones p_bone, bool p_left)
        {
            Transform l_result;
            if(p_left)
                l_result = m_leftHandHandler?.GetSourceForBone(p_bone);
            else
                l_result = m_rightHandHandler?.GetSourceForBone(p_bone);
            return l_result;
        }

        // Game events
        internal void OnInputUpdate()
        {
            if(m_active && Settings.SkeletalInput)
            {
                m_leftHandHandler?.Update();
                m_rightHandHandler?.Update();

                CVRInputManager.Instance.individualFingerTracking = true;
                CVRInputManager.Instance.finger1StretchedLeftThumb = FingerSystem.Instance.m_lastValues[0];
                CVRInputManager.Instance.finger2StretchedLeftThumb = FingerSystem.Instance.m_lastValues[1];
                CVRInputManager.Instance.finger3StretchedLeftThumb = FingerSystem.Instance.m_lastValues[2];
                CVRInputManager.Instance.fingerSpreadLeftThumb = FingerSystem.Instance.m_lastValues[3];
                CVRInputManager.Instance.finger1StretchedLeftIndex = FingerSystem.Instance.m_lastValues[4];
                CVRInputManager.Instance.finger2StretchedLeftIndex = FingerSystem.Instance.m_lastValues[5];
                CVRInputManager.Instance.finger3StretchedLeftIndex = FingerSystem.Instance.m_lastValues[6];
                CVRInputManager.Instance.fingerSpreadLeftIndex = FingerSystem.Instance.m_lastValues[7];
                CVRInputManager.Instance.finger1StretchedLeftMiddle = FingerSystem.Instance.m_lastValues[8];
                CVRInputManager.Instance.finger2StretchedLeftMiddle = FingerSystem.Instance.m_lastValues[9];
                CVRInputManager.Instance.finger3StretchedLeftMiddle = FingerSystem.Instance.m_lastValues[10];
                CVRInputManager.Instance.fingerSpreadLeftMiddle = FingerSystem.Instance.m_lastValues[11];
                CVRInputManager.Instance.finger1StretchedLeftRing = FingerSystem.Instance.m_lastValues[12];
                CVRInputManager.Instance.finger2StretchedLeftRing = FingerSystem.Instance.m_lastValues[13];
                CVRInputManager.Instance.finger3StretchedLeftRing = FingerSystem.Instance.m_lastValues[14];
                CVRInputManager.Instance.fingerSpreadLeftRing = FingerSystem.Instance.m_lastValues[15];
                CVRInputManager.Instance.finger1StretchedLeftPinky = FingerSystem.Instance.m_lastValues[16];
                CVRInputManager.Instance.finger2StretchedLeftPinky = FingerSystem.Instance.m_lastValues[17];
                CVRInputManager.Instance.finger3StretchedLeftPinky = FingerSystem.Instance.m_lastValues[18];
                CVRInputManager.Instance.fingerSpreadLeftPinky = FingerSystem.Instance.m_lastValues[19];
                CVRInputManager.Instance.finger1StretchedRightThumb = FingerSystem.Instance.m_lastValues[20];
                CVRInputManager.Instance.finger2StretchedRightThumb = FingerSystem.Instance.m_lastValues[21];
                CVRInputManager.Instance.finger3StretchedRightThumb = FingerSystem.Instance.m_lastValues[22];
                CVRInputManager.Instance.fingerSpreadRightThumb = FingerSystem.Instance.m_lastValues[23];
                CVRInputManager.Instance.finger1StretchedRightIndex = FingerSystem.Instance.m_lastValues[24];
                CVRInputManager.Instance.finger2StretchedRightIndex = FingerSystem.Instance.m_lastValues[25];
                CVRInputManager.Instance.finger3StretchedRightIndex = FingerSystem.Instance.m_lastValues[26];
                CVRInputManager.Instance.fingerSpreadRightIndex = FingerSystem.Instance.m_lastValues[27];
                CVRInputManager.Instance.finger1StretchedRightMiddle = FingerSystem.Instance.m_lastValues[28];
                CVRInputManager.Instance.finger2StretchedRightMiddle = FingerSystem.Instance.m_lastValues[29];
                CVRInputManager.Instance.finger3StretchedRightMiddle = FingerSystem.Instance.m_lastValues[30];
                CVRInputManager.Instance.fingerSpreadRightMiddle = FingerSystem.Instance.m_lastValues[31];
                CVRInputManager.Instance.finger1StretchedRightRing = FingerSystem.Instance.m_lastValues[32];
                CVRInputManager.Instance.finger2StretchedRightRing = FingerSystem.Instance.m_lastValues[33];
                CVRInputManager.Instance.finger3StretchedRightRing = FingerSystem.Instance.m_lastValues[34];
                CVRInputManager.Instance.fingerSpreadRightRing = FingerSystem.Instance.m_lastValues[35];
                CVRInputManager.Instance.finger1StretchedRightPinky = FingerSystem.Instance.m_lastValues[36];
                CVRInputManager.Instance.finger2StretchedRightPinky = FingerSystem.Instance.m_lastValues[37];
                CVRInputManager.Instance.finger3StretchedRightPinky = FingerSystem.Instance.m_lastValues[38];
                CVRInputManager.Instance.fingerSpreadRightPinky = FingerSystem.Instance.m_lastValues[39];
            }
        }

        void OnSwitchToVR()
        {
            try
            {
                SetupHandlers();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnSwitchToDesktop()
        {
            try
            {
                RemoveHandlers();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        // Settings
        void OnSkeletalInputChanged(bool p_value)
        {
            if(!p_value)
                CVRInputManager.Instance.individualFingerTracking = Utils.AreKnucklesInUse();
        }
    }
}
