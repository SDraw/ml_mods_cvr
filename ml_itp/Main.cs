using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using System;
using System.Collections;
using Valve.VR;
using VRBinding;

namespace ml_itp
{
    public class IndexTrackpadParams : MelonLoader.MelonMod
    {
        AvatarParameter m_leftTouchParameter = null;
        AvatarParameter m_leftForceParameter = null;
        AvatarParameter m_leftAxisParameterX = null;
        AvatarParameter m_leftAxisParameterY = null;

        AvatarParameter m_rightTouchParameter = null;
        AvatarParameter m_rightForceParameter = null;
        AvatarParameter m_rightAxisParameterX = null;
        AvatarParameter m_rightAxisParameterY = null;

        public override void OnInitializeMelon()
        {
            VRBindingMod.RegisterBinding("IndexLeftTrackpadTouch", "Linked with boolean avatar parameter", VRBindingMod.Requirement.optional, OnLeftTouchUpdate);
            VRBindingMod.RegisterBinding("IndexLeftTrackpadForce", "Linked with float avatar parameter", VRBindingMod.Requirement.optional, OnLeftForceUpdate);
            VRBindingMod.RegisterBinding("IndexLeftTrackpadAxis", "Linked with vector2 avatar parameter", VRBindingMod.Requirement.optional, OnLeftAxisUpdate);

            VRBindingMod.RegisterBinding("IndexRightTrackpadTouch", "Linked with boolean avatar parameter", VRBindingMod.Requirement.optional, OnRightTouchUpdate);
            VRBindingMod.RegisterBinding("IndexRightTrackpadForce", "Linked with float avatar parameter", VRBindingMod.Requirement.optional, OnRightForceUpdate);
            VRBindingMod.RegisterBinding("IndexRightTrackpadAxis", "Linked with vector2 avatar parameter", VRBindingMod.Requirement.optional, OnRightAxisUpdate);

            MelonLoader.MelonCoroutines.Start(WaitForInstance());
        }

        IEnumerator WaitForInstance()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(OnAvatarSetup);
            CVRGameEventSystem.Avatar.OnLocalAvatarClear.AddListener(OnAvatarClear);
        }

        void OnLeftTouchUpdate(SteamVR_Action_Boolean p_state)
        {
            m_leftTouchParameter?.SetValue(p_state.GetState(SteamVR_Input_Sources.Any));
        }
        void OnRightTouchUpdate(SteamVR_Action_Boolean p_state)
        {
            m_rightTouchParameter?.SetValue(p_state.GetState(SteamVR_Input_Sources.Any));
        }

        void OnLeftForceUpdate(SteamVR_Action_Single p_value)
        {
            m_leftForceParameter?.SetValue(p_value.GetAxis(SteamVR_Input_Sources.Any));
        }
        void OnRightForceUpdate(SteamVR_Action_Single p_value)
        {
            m_rightForceParameter?.SetValue(p_value.GetAxis(SteamVR_Input_Sources.Any));
        }

        void OnLeftAxisUpdate(SteamVR_Action_Vector2 p_axis)
        {
            var l_value = p_axis.GetAxis(SteamVR_Input_Sources.Any);
            m_leftAxisParameterX?.SetValue(l_value.x);
            m_leftAxisParameterY?.SetValue(l_value.y);
        }
        void OnRightAxisUpdate(SteamVR_Action_Vector2 p_axis)
        {
            var l_value = p_axis.GetAxis(SteamVR_Input_Sources.Any);
            m_rightAxisParameterX?.SetValue(l_value.x);
            m_rightAxisParameterY?.SetValue(l_value.y);
        }

        void OnAvatarSetup(CVRAvatar p_avatar)
        {
            try
            {
                m_leftTouchParameter = new AvatarParameter("IndexLeftTrackpadTouch", PlayerSetup.Instance.AnimatorManager);
                m_leftForceParameter = new AvatarParameter("IndexLeftTrackpadForce", PlayerSetup.Instance.AnimatorManager);
                m_leftAxisParameterX = new AvatarParameter("IndexLeftTrackpadAxis-x", PlayerSetup.Instance.AnimatorManager);
                m_leftAxisParameterY = new AvatarParameter("IndexLeftTrackpadAxis-y", PlayerSetup.Instance.AnimatorManager);

                m_rightTouchParameter = new AvatarParameter("IndexRightTrackpadTouch", PlayerSetup.Instance.AnimatorManager);
                m_rightForceParameter = new AvatarParameter("IndexRightTrackpadForce", PlayerSetup.Instance.AnimatorManager);
                m_rightAxisParameterX = new AvatarParameter("IndexRightTrackpadAxis-x", PlayerSetup.Instance.AnimatorManager);
                m_rightAxisParameterY = new AvatarParameter("IndexRightTrackpadAxis-y", PlayerSetup.Instance.AnimatorManager);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        void OnAvatarClear(CVRAvatar p_avatar)
        {
            try
            {
                m_leftTouchParameter = null;
                m_leftForceParameter = null;
                m_leftAxisParameterX = null;
                m_leftAxisParameterY = null;

                m_rightTouchParameter = null;
                m_rightForceParameter = null;
                m_rightAxisParameterX = null;
                m_rightAxisParameterY = null;
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}
