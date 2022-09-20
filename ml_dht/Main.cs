using ABI_RC.Core.Player;

namespace ml_dht
{
    public class DesktopHeadTracking : MelonLoader.MelonMod
    {
        static DesktopHeadTracking ms_instance = null;
        
        MemoryMapReader m_mapReader = null;
        byte[] m_buffer = null;
        TrackingData m_trackingData;
        
        FaceTracked m_localTracked = null;
        
        public override void OnApplicationStart()
        {
            if(ms_instance == null)
                ms_instance = this;
                
            Settings.Init();
            Settings.EnabledChange += this.OnEnabledChanged;
            Settings.MirroredChange += this.OnMirroredChanged;
            Settings.SmoothingChange += this.OnSmoothingChanged;
            
            m_mapReader = new MemoryMapReader();
            m_buffer = new byte[1024];
            
            m_mapReader.Open("head/data");

            // Patches
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnAvatarClear_Postfix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod("SetupAvatarGeneral", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnSetupAvatarGeneral_Postfix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            );
            HarmonyInstance.Patch(
                typeof(CVREyeController).GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic),
                null,
                new HarmonyLib.HarmonyMethod(typeof(DesktopHeadTracking).GetMethod(nameof(OnEyeControllerUpdate_Postfix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(WaitForPlayer());    
        }

        System.Collections.IEnumerator WaitForPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;
                
            m_localTracked = PlayerSetup.Instance.gameObject.AddComponent<FaceTracked>();
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
        
        static void OnSetupAvatarGeneral_Postfix() => ms_instance?.OnSetupAvatarGeneral();
        void OnSetupAvatarGeneral()
        {
            try
            {
                if(m_localTracked != null)
                    m_localTracked.OnSetupAvatarGeneral();
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

        static void OnEyeControllerUpdate_Postfix(ref CVREyeController __instance)
        {
            try
            {
                if(__instance == PlayerSetup.Instance.eyeMovement)
                    ms_instance?.OnEyeControllerUpdate();
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
        void OnEyeControllerUpdate()
        {
            if(m_localTracked != null)
                m_localTracked.OnEyeControllerUpdate();
        }
    }
}