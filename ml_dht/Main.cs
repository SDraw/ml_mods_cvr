using ABI.CCK.Components;
using ABI_RC.Core.Player;
using System.Reflection;

namespace ml_dht
{
    public class DesktopHeadTracking : MelonLoader.MelonMod
    {
        static DesktopHeadTracking ms_instance = null;

        MemoryMapReader m_mapReader = null;
        byte[] m_buffer = null;
        TrackingData m_trackingData;

        HeadTracked m_localTracked = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();

            m_mapReader = new MemoryMapReader();
            m_buffer = new byte[1024];

            m_mapReader.Open("head/data");

            // Patches
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.CalibrateAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnCalibrateAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(CVREyeController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnEyeControllerUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(CVRFaceTracking).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnFaceTrackingUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(WaitForPlayer());
        }

        System.Collections.IEnumerator WaitForPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localTracked = PlayerSetup.Instance.gameObject.AddComponent<HeadTracked>();
            m_localTracked.SetEnabled(Settings.Enabled);
            m_localTracked.SetMirrored(Settings.Mirrored);
            m_localTracked.SetSmoothing(Settings.Smoothing);
            m_localTracked.SetFaceOverride(Settings.FaceOverride);
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_mapReader?.Close();
            m_mapReader = null;
            m_buffer = null;
            m_localTracked = null;
        }

        public override void OnUpdate()
        {
            if(Settings.Enabled && m_mapReader.Read(ref m_buffer))
            {
                m_trackingData = TrackingData.ToObject(m_buffer);
                if(m_localTracked != null)
                    m_localTracked.UpdateTrackingData(ref m_trackingData);
            }
        }

        static void OnCalibrateAvatar_Postfix() => ms_instance?.OnCalibrateAvatar();
        void OnCalibrateAvatar()
        {
            try
            {
                if(m_localTracked != null)
                    m_localTracked.OnCalibrateAvatar();
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

        static void OnEyeControllerUpdate_Postfix(ref CVREyeController __instance) => ms_instance?.OnEyeControllerUpdate(__instance);
        void OnEyeControllerUpdate(CVREyeController p_component)
        {
            try
            {
                if(p_component.isLocal && (m_localTracked != null))
                    m_localTracked.OnEyeControllerUpdate(p_component);
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnFaceTrackingUpdate_Postfix(ref CVRFaceTracking __instance) => ms_instance?.OnFaceTrackingUpdate(__instance);
        void OnFaceTrackingUpdate(CVRFaceTracking p_component)
        {
            try
            {
                if(p_component.isLocal && (m_localTracked != null))
                    m_localTracked.OnFaceTrackingUpdate(p_component);
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}