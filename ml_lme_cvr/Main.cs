using ABI_RC.Core.Player;
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
        GameObject m_leapControllerModel = null;
        LeapTracked m_leapTracked = null;

        public override void OnApplicationStart()
        {
            ms_instance = this;

            DependenciesHandler.ExtractDependencies();

            Settings.Init(HarmonyInstance);
            Settings.EnabledChange += this.OnSettingsEnableChange;
            Settings.DesktopOffsetChange += this.OnSettingsDesktopOffsetChange;
            Settings.FingersOnlyChange += this.OnSettingsFingersOptionChange;
            Settings.ModelVisibilityChange += this.OnSettingsModelVisibilityChange;
            Settings.HmdModeChange += this.OnSettingsHmdModeChange;
            Settings.RootAngleChange += this.OnSettingsRootAngleChange;
            Settings.HeadAttachChange += this.OnSettingsHeadAttachChange;
            Settings.HeadOffsetChange += this.OnSettingsHeadOffsetChange;

            m_leapController = new Leap.Controller();
            m_gesturesData = new GestureMatcher.GesturesData();

            m_leapHands = new GameObject[GestureMatcher.GesturesData.ms_handsCount];

            // Patches
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(LeapMotionExtension).GetMethod(nameof(OnAvatarSetup), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(CreateTrackingObjects());
        }

        System.Collections.IEnumerator CreateTrackingObjects()
        {
            AssetsHandler.Load();

            while(PlayerSetup.Instance == null)
                yield return null;
            while(PlayerSetup.Instance.desktopCameraRig == null)
                yield return null;
            while(PlayerSetup.Instance.desktopCamera == null)
                yield return null;
            while(PlayerSetup.Instance.vrCameraRig == null)
                yield return null;
            while(PlayerSetup.Instance.vrCamera == null)
                yield return null;

            m_leapTrackingRoot = new GameObject("[LeapRoot]");

            for(int i = 0; i < GestureMatcher.GesturesData.ms_handsCount; i++)
            {
                m_leapHands[i] = new GameObject("LeapHand" + i);
                m_leapHands[i].transform.parent = m_leapTrackingRoot.transform;
                m_leapHands[i].transform.localPosition = Vector3.zero;
                m_leapHands[i].transform.localRotation = Quaternion.identity;
            }

            m_leapControllerModel = AssetsHandler.GetAsset("assets/models/leapmotion/leap_motion_1_0.obj");
            if(m_leapControllerModel != null)
            {
                m_leapControllerModel.name = "LeapModel";
                m_leapControllerModel.transform.parent = m_leapTrackingRoot.transform;
                m_leapControllerModel.transform.localPosition = Vector3.zero;
                m_leapControllerModel.transform.localRotation = Quaternion.identity;
            }

            Settings.Reload();

            OnSettingsEnableChange();
            OnSettingsFingersOptionChange();
            OnSettingsModelVisibilityChange();
            OnSettingsHmdModeChange();
            OnSettingsHeadAttachChange(); // Includes offsets and parenting
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
                        if((m_leapHands[i] != null) && m_gesturesData.m_handsPresenses[i])
                        {
                            Vector3 l_pos = m_gesturesData.m_handsPositons[i];
                            Quaternion l_rot = m_gesturesData.m_handsRotations[i];
                            ReorientateLeapToUnity(ref l_pos, ref l_rot, Settings.HmdMode);
                            m_leapHands[i].transform.localPosition = l_pos;
                            m_leapHands[i].transform.localRotation = l_rot;
                        }
                    }
                }

                if(m_leapTracked != null)
                    m_leapTracked.UpdateTracking(m_gesturesData);
            }
        }

        // Settings changes
        void OnSettingsEnableChange()
        {
            if(Settings.Enabled)
            {
                m_leapController.StartConnection();
                m_leapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                if(Settings.HmdMode)
                    m_leapController.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                else
                    m_leapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            }
            else
                m_leapController.StopConnection();

            if(m_leapTracked != null)
                m_leapTracked.SetEnabled(Settings.Enabled);
        }

        void OnSettingsDesktopOffsetChange()
        {
            if((m_leapTrackingRoot != null) && !Settings.HeadAttach)
            {
                if(!PlayerSetup.Instance._inVr)
                    m_leapTrackingRoot.transform.localPosition = Settings.DesktopOffset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                else
                    m_leapTrackingRoot.transform.localPosition = Settings.DesktopOffset;
            }
        }

        void OnSettingsFingersOptionChange()
        {
            if(m_leapTracked != null)
                m_leapTracked.SetFingersOnly(Settings.FingersOnly);
        }

        void OnSettingsModelVisibilityChange()
        {
            if(m_leapControllerModel != null)
                m_leapControllerModel.SetActive(Settings.ModelVisibility);
        }

        void OnSettingsHmdModeChange()
        {
            if(m_leapController != null)
            {
                m_leapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_SCREENTOP);
                if(Settings.HmdMode)
                    m_leapController.SetPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
                else
                    m_leapController.ClearPolicy(Leap.Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            }

            if(m_leapControllerModel != null)
                m_leapControllerModel.transform.localRotation = (Settings.HmdMode ? Quaternion.Euler(270f, 180f, 0f) : Quaternion.identity);
        }

        void OnSettingsRootAngleChange()
        {
            if(m_leapTrackingRoot != null)
                m_leapTrackingRoot.transform.localRotation = Quaternion.Euler(Settings.RootAngle, 0f, 0f);
        }

        void OnSettingsHeadAttachChange()
        {
            if(m_leapTrackingRoot != null)
            {
                if(Settings.HeadAttach)
                {
                    if(!PlayerSetup.Instance._inVr)
                    {
                        m_leapTrackingRoot.transform.parent = PlayerSetup.Instance.desktopCamera.transform;
                        m_leapTrackingRoot.transform.localPosition = Settings.HeadOffset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                        m_leapTrackingRoot.transform.localScale = PlayerSetup.Instance.vrCameraRig.transform.localScale;
                    }
                    else
                    {
                        m_leapTrackingRoot.transform.parent = PlayerSetup.Instance.vrCamera.transform;
                        m_leapTrackingRoot.transform.localPosition = Settings.HeadOffset;
                        m_leapTrackingRoot.transform.localScale = Vector3.one;
                    }
                }
                else
                {
                    if(!PlayerSetup.Instance._inVr)
                    {
                        m_leapTrackingRoot.transform.parent = PlayerSetup.Instance.desktopCameraRig.transform;
                        m_leapTrackingRoot.transform.localPosition = Settings.DesktopOffset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                        m_leapTrackingRoot.transform.localScale = PlayerSetup.Instance.vrCameraRig.transform.localScale;
                    }
                    else
                    {
                        m_leapTrackingRoot.transform.parent = PlayerSetup.Instance.vrCameraRig.transform;
                        m_leapTrackingRoot.transform.localPosition = Settings.DesktopOffset;
                        m_leapTrackingRoot.transform.localScale = Vector3.one;
                    }
                }

                m_leapTrackingRoot.transform.localRotation = Quaternion.Euler(Settings.RootAngle, 0f, 0f);
            }
        }

        void OnSettingsHeadOffsetChange()
        {
            if((m_leapTrackingRoot != null) && Settings.HeadAttach)
            {
                if(!PlayerSetup.Instance._inVr)
                    m_leapTrackingRoot.transform.localPosition = Settings.HeadOffset * PlayerSetup.Instance.vrCameraRig.transform.localScale.x;
                else
                    m_leapTrackingRoot.transform.localPosition = Settings.HeadOffset;
            }
        }

        // Patches
        static void OnAvatarSetup(ref PlayerSetup __instance)
        {
            if(__instance != null && __instance == PlayerSetup.Instance)
            {
                ms_instance?.OnLocalPlayerAvatarSetup(__instance._animator, __instance.GetComponent<IndexIK>());
            }
        }
        void OnLocalPlayerAvatarSetup(Animator p_animator, IndexIK p_indexIK)
        {
            if(m_leapTracked != null)
                Object.DestroyImmediate(m_leapTracked);

            m_leapTracked = p_indexIK.gameObject.AddComponent<LeapTracked>();
            m_leapTracked.SetEnabled(Settings.Enabled);
            m_leapTracked.SetAnimator(p_animator);
            m_leapTracked.SetHands(m_leapHands[0].transform, m_leapHands[1].transform);
            m_leapTracked.SetFingersOnly(Settings.FingersOnly);

            OnSettingsHeadAttachChange();
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
