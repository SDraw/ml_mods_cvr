using UnityEngine;

namespace ml_lme_cvr
{

    public class LeapMotionExtension : MelonLoader.MelonMod
    {
        static readonly Quaternion ms_hmdRotationFix = new Quaternion(0f, 0.7071068f, 0.7071068f, 0f);

        static LeapMotionExtension ms_instance = null;

        Leap.Controller m_leapController = null;
        GestureMatcher.GesturesData m_gesturesData = null;

        GameObject m_leapTrackingRoot = null;
        GameObject[] m_leapHands = null;
        LeapTracked m_leapTracked = null;

        Transform m_vrRig = null;
        Transform m_desktopRig = null;

        public override void OnApplicationStart()
        {
            ms_instance = this;

            DependenciesHandler.ExtractDependencies();

            Settings.Init(HarmonyInstance);
            Settings.EnabledChange += this.OnSettingsEnableChange;
            Settings.DesktopOffsetChange += this.OnSettingsDesktopOffsetChange;
            Settings.FingersOnlyChange += this.OnSettingsFingersOptionChange;

            m_leapController = new Leap.Controller();
            m_gesturesData = new GestureMatcher.GesturesData();

            m_leapHands = new GameObject[GestureMatcher.GesturesData.ms_handsCount];

            // Patches
            HarmonyInstance.Patch(
                typeof(ABI_RC.Core.Player.PlayerSetup).GetMethod(nameof(ABI_RC.Core.Player.PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnAvatarSetup), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(CreateTrackingObjects());
        }

        System.Collections.IEnumerator CreateTrackingObjects()
        {
            while(ABI_RC.Core.Player.PlayerSetup.Instance == null)
                yield return null;

            while(m_vrRig == null)
            {
                m_vrRig = ABI_RC.Core.Player.PlayerSetup.Instance.transform.Find("[CameraRigVR]");
                yield return null;
            }

            while(m_desktopRig == null)
            {
                m_desktopRig = ABI_RC.Core.Player.PlayerSetup.Instance.transform.Find("[CameraRigDesktop]");
                yield return null;
            }

            m_leapTrackingRoot = new GameObject("[LeapRoot]");
            m_leapTrackingRoot.transform.parent = m_desktopRig;
            m_leapTrackingRoot.transform.localPosition = new Vector3(0f, -0.45637f, 0.35f);
            m_leapTrackingRoot.transform.localRotation = Quaternion.identity;

            for(int i = 0; i < GestureMatcher.GesturesData.ms_handsCount; i++)
            {
                m_leapHands[i] = new GameObject("LeapHand" + i);
                m_leapHands[i].transform.parent = m_leapTrackingRoot.transform;
                m_leapHands[i].transform.localPosition = Vector3.zero;
                m_leapHands[i].transform.localRotation = Quaternion.identity;
            }

            Settings.Reload();
        }

        public override void OnUpdate()
        {
            if(Settings.Enabled && (m_leapController != null))
            {
                Leap.Frame l_frame = m_leapController.Frame();
                if(l_frame != null)
                {
                    GestureMatcher.GetGestures(l_frame, ref m_gesturesData);

                    for(int i = 0; i < GestureMatcher.GesturesData.ms_handsCount; i++)
                    {
                        if(m_gesturesData.m_handsPresenses[i] && (m_leapHands[i] != null))
                        {
                            Vector3 l_pos = m_gesturesData.m_handsPositons[i];
                            Quaternion l_rot = m_gesturesData.m_handsRotations[i];
                            ReorientateLeapToUnity(ref l_pos, ref l_rot, false);
                            m_leapHands[i].transform.localPosition = l_pos;
                            m_leapHands[i].transform.localRotation = l_rot;
                        }
                    }

                    if(m_leapTracked != null)
                        m_leapTracked.UpdateTracking(m_gesturesData);
                }
            }
        }

        void OnSettingsEnableChange()
        {
            if(Settings.Enabled)
            {
                m_leapController.StartConnection();
                m_leapController.SetAndClearPolicy(Leap.Controller.PolicyFlag.POLICY_DEFAULT, Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP | Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            }
            else
                m_leapController.StopConnection();

            if(m_leapTracked != null)
                m_leapTracked.SetEnabled(Settings.Enabled);
        }

        void OnSettingsDesktopOffsetChange()
        {
            if((m_leapTrackingRoot != null) && (m_vrRig != null))
            {
                m_leapTrackingRoot.transform.localPosition = new Vector3(Settings.DesktopOffsetX, Settings.DesktopOffsetY, Settings.DesktopOffsetZ) * m_vrRig.transform.localScale.x;
                m_leapTrackingRoot.transform.localScale = m_vrRig.transform.localScale;
            }
        }

        void OnSettingsFingersOptionChange()
        {
            if(m_leapTracked != null)
                m_leapTracked.SetFingersOnly(Settings.FingersOnly);
        }

        static void OnAvatarSetup(ref ABI_RC.Core.Player.PlayerSetup __instance)
        {
            if(__instance != null && __instance == ABI_RC.Core.Player.PlayerSetup.Instance)
            {
                ms_instance?.OnLocalPlayerAvatarSetup(__instance._animator, __instance.GetComponent<ABI_RC.Core.Player.IndexIK>());
            }
        }
        void OnLocalPlayerAvatarSetup(Animator p_animator, ABI_RC.Core.Player.IndexIK p_indexIK)
        {
            if(m_leapTracked != null)
                Object.DestroyImmediate(m_leapTracked);

            m_leapTracked = p_indexIK.gameObject.AddComponent<LeapTracked>();
            m_leapTracked.SetEnabled(Settings.Enabled);
            m_leapTracked.SetAnimator(p_animator);
            m_leapTracked.SetHands(m_leapHands[0].transform, m_leapHands[1].transform);
            m_leapTracked.SetFingersOnly(Settings.FingersOnly);

            if((m_leapTrackingRoot != null) && (m_vrRig != null))
            {
                m_leapTrackingRoot.transform.localPosition = new Vector3(Settings.DesktopOffsetX, Settings.DesktopOffsetY, Settings.DesktopOffsetZ) * m_vrRig.transform.localScale.x;
                m_leapTrackingRoot.transform.localScale = m_vrRig.transform.localScale;
            }
        }

        static void ReorientateLeapToUnity(ref Vector3 p_pos, ref Quaternion p_rot, bool p_hmd)
        {
            p_pos *= 0.001f;
            p_pos.z *= -1f;
            p_rot.x *= -1f;
            p_rot.y *= -1f;

            if(p_hmd)
            {
                p_pos.x *= -1f;
                Utils.Swap(ref p_pos.y, ref p_pos.z);
                p_rot = (ms_hmdRotationFix * p_rot);
            }
        }
    }
}
