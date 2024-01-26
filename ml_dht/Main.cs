using ABI_RC.Core.Player;
using ABI_RC.Core.Player.EyeMovement;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.FaceTracking;
using System.Reflection;

namespace ml_dht
{
    public class DesktopHeadTracking : MelonLoader.MelonMod
    {
        static DesktopHeadTracking ms_instance = null;

        TrackingModule m_trackingModule = null;
        HeadTracked m_localTracked = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();

            m_trackingModule = new TrackingModule();

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

            MelonLoader.MelonCoroutines.Start(WaitForInstances());
        }

        System.Collections.IEnumerator WaitForInstances()
        {
            while(MetaPort.Instance == null)
                yield return null;

            while(PlayerSetup.Instance == null)
                yield return null;

            m_localTracked = PlayerSetup.Instance.gameObject.AddComponent<HeadTracked>();
            FaceTrackingManager.Instance.RegisterModule(m_trackingModule);

            // If you think it's a joke to put patch here, go on, try to put it in OnInitializeMelon, you melon :>
            HarmonyInstance.Patch(
                typeof(EyeMovementController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnEyeControllerUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_trackingModule = null;
            m_localTracked = null;
        }

        public override void OnUpdate()
        {
            if(Settings.Enabled && (m_trackingModule != null))
            {
                m_trackingModule.Update();
                if(m_localTracked != null)
                    m_localTracked.UpdateTrackingData(ref m_trackingModule.GetLatestTrackingData());
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
    }
}