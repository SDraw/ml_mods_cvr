using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Player.EyeMovement;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using System.Reflection;

namespace ml_dht
{
    public class DesktopHeadTracking : MelonLoader.MelonMod
    {
        static DesktopHeadTracking ms_instance = null;

        DataParser m_dataParser = null;
        HeadTracked m_localTracked = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();

            // Patches
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(IKSystem).GetMethod(nameof(IKSystem.ReinitializeAvatar), BindingFlags.Instance | BindingFlags.Public),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnAvatarReinitialize_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(WaitForInstances());
        }

        System.Collections.IEnumerator WaitForInstances()
        {
            while(MetaPort.Instance == null)
                yield return null;

            while(PlayerSetup.Instance == null)
                yield return null;

            m_dataParser = new DataParser();
            m_localTracked = PlayerSetup.Instance.gameObject.AddComponent<HeadTracked>();

            // If you think it's a joke to put patch here, go on, try to put it in OnInitializeMelon, you melon :>
            HarmonyInstance.Patch(
                typeof(EyeMovementController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnEyeControllerUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(CVRFaceTracking).GetMethod("UpdateLocalData", BindingFlags.Instance | BindingFlags.NonPublic),
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnFaceTrackingLocalUpdate_Prefix), BindingFlags.Static | BindingFlags.NonPublic))
            );
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_dataParser = null;
            m_localTracked = null;
        }

        public override void OnUpdate()
        {
            if(Settings.Enabled && (m_dataParser != null))
            {
                m_dataParser.Update();
                if(m_localTracked != null)
                    m_localTracked.UpdateTrackingData(ref m_dataParser.GetLatestTrackingData());
            }
        }

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                if(m_localTracked != null)
                    m_localTracked.OnSetupAvatar();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                if(m_localTracked != null)
                    m_localTracked.OnAvatarClear();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnAvatarReinitialize_Postfix() => ms_instance?.OnAvatarReinitialize();
        void OnAvatarReinitialize()
        {
            try
            {
                if(m_localTracked != null)
                    m_localTracked.OnAvatarReinitialize();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnEyeControllerUpdate_Postfix(ref EyeMovementController __instance) => ms_instance?.OnEyeControllerUpdate(__instance);
        void OnEyeControllerUpdate(EyeMovementController p_component)
        {
            try
            {
                if(p_component.IsLocal && (m_localTracked != null))
                    m_localTracked.OnEyeControllerUpdate(p_component);
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static bool OnFaceTrackingLocalUpdate_Prefix(ref CVRFaceTracking __instance)
        {
            bool? l_result = ms_instance?.OnFaceTrackingLocalUpdate(__instance);
            return l_result.GetValueOrDefault(true);
        }
        bool OnFaceTrackingLocalUpdate(CVRFaceTracking p_component)
        {
            bool l_result = true;
            if(p_component.UseFacialTracking && (m_localTracked != null))
                l_result = !m_localTracked.UpdateFaceTracking(p_component);
            return l_result;
        }
    }
}