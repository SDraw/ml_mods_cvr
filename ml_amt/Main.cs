using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using System;
using System.Collections;
using System.Reflection;

namespace ml_amt
{
    public class AvatarMotionTweaker : MelonLoader.MelonMod
    {
        static AvatarMotionTweaker ms_instance = null;

        MotionTweaker m_localTweaker = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(IKSystem).GetMethod(nameof(IKSystem.ReinitializeAvatar), BindingFlags.Instance | BindingFlags.Public),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnAvatarReinitialize_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod("SetPlaySpaceScale", BindingFlags.NonPublic | BindingFlags.Instance),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AvatarMotionTweaker).GetMethod(nameof(OnPlayspaceScale_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localTweaker = PlayerSetup.Instance.gameObject.AddComponent<MotionTweaker>();
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            if(m_localTweaker != null)
                UnityEngine.Object.Destroy(m_localTweaker);
            m_localTweaker = null;
        }

        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnAvatarClear();
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnSetupAvatar();
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnAvatarReinitialize_Postfix() => ms_instance?.OnAvatarReinitialize();
        void OnAvatarReinitialize()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnAvatarReinitialize();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnPlayspaceScale_Postfix() => ms_instance?.OnPlayspaceScale();
        void OnPlayspaceScale()
        {
            try
            {
                if(m_localTweaker != null)
                    m_localTweaker.OnPlayspaceScale();
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }
    }
}
