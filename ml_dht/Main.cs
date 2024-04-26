using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;

namespace ml_dht
{
    public class DesktopHeadTracking : MelonLoader.MelonMod
    {
        DataParser m_dataParser = null;
        HeadTracked m_localTracked = null;

        public override void OnInitializeMelon()
        {
            Settings.Init();
            GameEvents.InitA(HarmonyInstance);

            MelonLoader.MelonCoroutines.Start(WaitForInstances());
        }

        System.Collections.IEnumerator WaitForInstances()
        {
            while(MetaPort.Instance == null)
                yield return null;

            while(PlayerSetup.Instance == null)
                yield return null;

            GameEvents.InitB(HarmonyInstance);

            m_dataParser = new DataParser();
            m_localTracked = PlayerSetup.Instance.gameObject.AddComponent<HeadTracked>();
        }

        public override void OnDeinitializeMelon()
        {
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
    }
}