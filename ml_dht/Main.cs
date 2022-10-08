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
            Settings.EnabledChange += this.OnEnabledChanged;
            Settings.MirroredChange += this.OnMirroredChanged;
            Settings.SmoothingChange += this.OnSmoothingChanged;
            Settings.FaceOverrideChange += this.OnFaceOverrideChange;

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

        void OnEnabledChanged(bool p_state)
        {
            if(m_localTracked != null)
                m_localTracked.SetEnabled(p_state);
        }
        void OnMirroredChanged(bool p_state)
        {
            if(m_localTracked != null)
                m_localTracked.SetMirrored(p_state);
        }
        void OnSmoothingChanged(float p_value)
        {
            if(m_localTracked != null)
                m_localTracked.SetSmoothing(p_value);
        }
        void OnFaceOverrideChange(bool p_state)
        {
            if(m_localTracked != null)
                m_localTracked.SetFaceOverride(p_state);
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