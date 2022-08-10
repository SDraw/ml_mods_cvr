using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using UnityEngine;

namespace ml_fpt
{
    public class FourPointTracking : MelonLoader.MelonMod
    {
        static FourPointTracking ms_instance = null;

        IndexIK m_indexIk = null;
        CVR_IK_Calibrator m_ikCalibrator = null;

        bool m_inCalibration = false;
        int m_hipsTrackerIndex = -1;

        RuntimeAnimatorController m_oldRuntimeAnimator = null;
        RootMotion.FinalIK.VRIK m_origVrIk = null;

        bool m_playerReady = false;

        public override void OnApplicationStart()
        {
            ms_instance = this;

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(FourPointTracking).GetMethod(nameof(OnAvatarClear_Postfix), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            );

            MelonLoader.MelonCoroutines.Start(WaitForMainMenuView());
            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        public override void OnUpdate()
        {
            if(m_playerReady && m_inCalibration && (m_hipsTrackerIndex != -1))
            {
                if(m_origVrIk != null)
                    m_origVrIk.enabled = false;

                m_indexIk.calibrated = false;
                m_indexIk.enabled = false;

                Transform l_hips = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips);
                PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowLine(true, l_hips);

                if((CVRInputManager.Instance.interactLeftValue > 0.9f) && (CVRInputManager.Instance.interactRightValue > 0.9f))
                {
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].target.transform.position = l_hips.position;
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].target.transform.rotation = l_hips.rotation;

                    if((m_origVrIk != null) && (m_origVrIk.solver?.spine != null))
                    {
                        m_origVrIk.solver.spine.pelvisTarget = PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].target;
                        m_origVrIk.solver.spine.pelvisPositionWeight = 1f;
                        m_origVrIk.solver.spine.pelvisRotationWeight = 1f;
                    }

                    m_indexIk.calibrated = true;
                    m_indexIk.enabled = true;

                    PlayerSetup.Instance._animator.runtimeAnimatorController = m_oldRuntimeAnimator;

                    m_ikCalibrator.leftHandModel.SetActive(false);
                    m_ikCalibrator.rightHandModel.SetActive(false);
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowTracker(false);
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowLine(false);
                    CVR_InteractableManager.enableInteractions = true;

                    Reset();

                    ShowHudNotification("Calibration completed");
                }
            }
        }

        System.Collections.IEnumerator WaitForMainMenuView()
        {
            while(ViewManager.Instance == null)
                yield return null;
            while(ViewManager.Instance.gameMenuView == null)
                yield return null;
            while(ViewManager.Instance.gameMenuView.Listener == null)
                yield return null;

            ViewManager.Instance.gameMenuView.Listener.ReadyForBindings += () =>
            {
                ViewManager.Instance.gameMenuView.View.RegisterForEvent("MelonMod_FPT_Action_Calibrate", new System.Action(this.StartCalibration));
            };

            ViewManager.Instance.gameMenuView.Listener.FinishLoad += (_) =>
            {
                ViewManager.Instance.gameMenuView.View.ExecuteScript(Scripts.GetEmbeddedScript("menu.js"));
            };
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_indexIk = PlayerSetup.Instance.gameObject.GetComponent<IndexIK>();
            m_ikCalibrator = PlayerSetup.Instance.gameObject.GetComponent<CVR_IK_Calibrator>();

            m_playerReady = true;
        }

        void StartCalibration()
        {
            if(m_playerReady && !m_inCalibration && PlayerSetup.Instance._inVr && !PlayerSetup.Instance.fullBodyActive && PlayerSetup.Instance._animator.isHuman && !m_ikCalibrator.inFullbodyCalibration && m_indexIk.calibrated)
            {
                for(int i = 0; i < PlayerSetup.Instance._trackerManager.trackerNames.Length; i++)
                {
                    if(PlayerSetup.Instance._trackerManager.trackerNames[i] == "vive_tracker_waist")
                    {
                        m_hipsTrackerIndex = i;
                        break;
                    }
                }

                if(m_hipsTrackerIndex != -1)
                {
                    m_oldRuntimeAnimator = PlayerSetup.Instance._animator.runtimeAnimatorController;
                    PlayerSetup.Instance._animator.runtimeAnimatorController = PlayerSetup.Instance.tPoseAnimatorController;

                    m_origVrIk = PlayerSetup.Instance._animator.GetComponent<RootMotion.FinalIK.VRIK>();

                    m_ikCalibrator.leftHandModel.SetActive(true);
                    m_ikCalibrator.rightHandModel.SetActive(true);
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowTracker(true);
                    CVR_InteractableManager.enableInteractions = false;

                    m_inCalibration = true;

                    ViewManager.Instance.ForceUiStatus(false);
                    ShowHudNotification("Calibration started");
                }
                else
                    ShowMenuAlert("No hips tracker detected. Check if tracker has waist role in SteamVR settings.");
            }
            else
                ShowMenuAlert("Calibraton requirements aren't met: be in VR, be not in FBT or avatar calibration, humanoid avatar");
        }

        void Reset()
        {
            m_inCalibration = false;
            m_hipsTrackerIndex = -1;
            m_oldRuntimeAnimator = null;
            m_origVrIk = null;
        }

        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            if(m_inCalibration)
            {
                m_indexIk.calibrated = true;
                m_indexIk.enabled = true;

                m_ikCalibrator.leftHandModel.SetActive(false);
                m_ikCalibrator.rightHandModel.SetActive(false);

                if(m_hipsTrackerIndex != -1)
                {
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowTracker(false);
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowLine(false);
                }
                CVR_InteractableManager.enableInteractions = true;

                Reset();

                ShowHudNotification("Calibration canceled");
            }
        }

        static void ShowHudNotification(string p_message)
        {
            if(CohtmlHud.Instance != null)
                CohtmlHud.Instance.ViewDropText("4-Point Tracking", p_message);
        }

        static void ShowMenuAlert(string p_message)
        {
            if(ViewManager.Instance != null)
                ViewManager.Instance.TriggerAlert("4-Point Tracking", p_message, 0, false);
        }
    }
}
