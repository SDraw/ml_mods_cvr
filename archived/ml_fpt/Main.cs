using ABI.CCK.Scripts;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.IK.SubSystems;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ml_fpt
{
    public class FourPointTracking : MelonLoader.MelonMod
    {
        static FourPointTracking ms_instance = null;

        bool m_ready = false;

        IndexIK m_indexIK = null;
        RootMotion.FinalIK.VRIK m_vrIK = null;
        RuntimeAnimatorController m_runtimeAnimator = null;
        List<CVRAdvancedSettingsFileProfileValue> m_aasParameters = null;

        bool m_calibrationActive = false;
        object m_calibrationTask = null;

        int m_hipsTrackerIndex = -1;
        Transform m_hips = null;

        Dictionary<string, Tuple<Vector3, Quaternion>> m_avatarCalibrations = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            m_avatarCalibrations = new Dictionary<string, Tuple<Vector3, Quaternion>>();

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(FourPointTracking).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(FourPointTracking).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(WaitForMainMenuView());
            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
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
                ViewManager.Instance.gameMenuView.View.RegisterForEvent("MelonMod_FPT_Action_Calibrate", new Action(this.StartCalibration));
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

            m_indexIK = PlayerSetup.Instance.gameObject.GetComponent<IndexIK>();

            m_ready = true;
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_ready = false;
            m_aasParameters?.Clear();
            m_aasParameters = null;
            m_avatarCalibrations?.Clear();
            m_avatarCalibrations = null;
            m_hipsTrackerIndex = -1;

            if(m_calibrationTask != null)
                MelonLoader.MelonCoroutines.Stop(m_calibrationTask);
            m_calibrationTask = null;
        }

        void StartCalibration()
        {
            if(m_ready && !m_calibrationActive && PlayerSetup.Instance._inVr && !PlayerSetup.Instance.avatarIsLoading && PlayerSetup.Instance._animator.isHuman && !BodySystem.isCalibrating && !BodySystem.isCalibratedAsFullBody)
            {
                m_hipsTrackerIndex = GetHipsTracker();
                if(m_hipsTrackerIndex != -1)
                {
                    m_avatarCalibrations.Remove(MetaPort.Instance.currentAvatarGuid);

                    m_runtimeAnimator = PlayerSetup.Instance._animator.runtimeAnimatorController;
                    m_aasParameters = PlayerSetup.Instance.animatorManager.GetAdditionalSettingsCurrent();
                    PlayerSetup.Instance._animator.runtimeAnimatorController = PlayerSetup.Instance.tPoseAnimatorController;
                    PlayerSetup.Instance.animatorManager.SetAnimator(PlayerSetup.Instance._animator, PlayerSetup.Instance.tPoseAnimatorController);

                    m_hips = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips);
                    m_vrIK = PlayerSetup.Instance._animator.GetComponent<RootMotion.FinalIK.VRIK>();

                    if(m_vrIK != null)
                        m_vrIK.solver.OnPreUpdate += this.OverrideIKWeight;

                    IKSystem.Instance.leftHandModel.SetActive(true);
                    IKSystem.Instance.rightHandModel.SetActive(true);
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowTracker(true);
                    CVR_InteractableManager.enableInteractions = false;

                    m_calibrationActive = true;
                    m_calibrationTask = MelonLoader.MelonCoroutines.Start(CalibrationTask());

                    ViewManager.Instance.ForceUiStatus(false);
                    ShowHudNotification("Calibration started");
                }
                else
                    ShowMenuAlert("No hips tracker detected. Check if tracker has waist role in SteamVR settings.");
            }
            else
                ShowMenuAlert("Calibraton requirements aren't met: be in VR, be not in FBT or avatar calibration, humanoid avatar");
        }

        System.Collections.IEnumerator CalibrationTask()
        {
            while(m_calibrationActive)
            {
                if(m_vrIK != null)
                    m_vrIK.enabled = false;

                m_indexIK.enabled = false;

                PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowLine(true, m_hips);

                if((CVRInputManager.Instance.interactLeftValue > 0.9f) && (CVRInputManager.Instance.interactRightValue > 0.9f))
                {
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].target.transform.position = m_hips.position;
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].target.transform.rotation = m_hips.rotation;

                    m_avatarCalibrations.Add(
                        MetaPort.Instance.currentAvatarGuid,
                        new Tuple<Vector3, Quaternion>(
                            PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].target.transform.localPosition,
                            PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].target.transform.localRotation
                       )
                    );

                    if(m_vrIK != null)
                    {
                        m_vrIK.solver.spine.pelvisTarget = PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].target;
                        m_vrIK.solver.spine.pelvisPositionWeight = 1f;
                        m_vrIK.solver.spine.pelvisRotationWeight = 1f;
                        m_vrIK.solver.OnPreUpdate -= this.OverrideIKWeight;
                        m_vrIK.solver.IKPositionWeight = 1f;
                        m_vrIK.enabled = true;
                    }

                    m_indexIK.enabled = true;

                    PlayerSetup.Instance._animator.runtimeAnimatorController = m_runtimeAnimator;
                    PlayerSetup.Instance.animatorManager.SetAnimator(PlayerSetup.Instance._animator, m_runtimeAnimator);
                    if(m_aasParameters != null)
                    {
                        foreach(var l_param in m_aasParameters)
                        {
                            PlayerSetup.Instance.animatorManager.SetAnimatorParameter(l_param.name, l_param.value);
                        }
                    }

                    IKSystem.Instance.leftHandModel.SetActive(false);
                    IKSystem.Instance.rightHandModel.SetActive(false);
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowTracker(false);
                    PlayerSetup.Instance._trackerManager.trackers[m_hipsTrackerIndex].ShowLine(false);
                    CVR_InteractableManager.enableInteractions = true;

                    Reset();

                    ShowHudNotification("Calibration completed");
                }

                yield return null;
            }

            m_calibrationTask = null; // Idk if it's safe or not
        }

        void OverrideIKWeight()
        {
            if(m_calibrationActive)
            {
                m_vrIK.solver.IKPositionWeight = 0f;
            }
        }

        void Reset()
        {
            m_vrIK = null;
            m_runtimeAnimator = null;
            m_aasParameters = null;
            m_calibrationActive = false;
            m_calibrationTask = null;
            m_hipsTrackerIndex = -1;
            m_hips = null;
        }

        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                if(m_calibrationActive)
                {
                    if(m_calibrationTask != null)
                        MelonLoader.MelonCoroutines.Stop(m_calibrationTask);

                    m_indexIK.enabled = true;

                    IKSystem.Instance.leftHandModel.SetActive(false);
                    IKSystem.Instance.rightHandModel.SetActive(false);

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
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                if(m_ready && PlayerSetup.Instance._inVr && PlayerSetup.Instance._animator.isHuman && !VRTrackerManager.Instance.CheckFullBody())
                {
                    int l_hipsTracker = GetHipsTracker();
                    if((l_hipsTracker != -1) && m_avatarCalibrations.TryGetValue(MetaPort.Instance.currentAvatarGuid, out var l_stored))
                    {
                        var l_vrIK = PlayerSetup.Instance._animator.GetComponent<RootMotion.FinalIK.VRIK>();
                        if(l_vrIK != null)
                        {
                            l_vrIK.solver.spine.pelvisTarget = PlayerSetup.Instance._trackerManager.trackers[l_hipsTracker].target;
                            l_vrIK.solver.spine.pelvisPositionWeight = 1f;
                            l_vrIK.solver.spine.pelvisRotationWeight = 1f;

                            l_vrIK.solver.spine.pelvisTarget.localPosition = l_stored.Item1;
                            l_vrIK.solver.spine.pelvisTarget.localRotation = l_stored.Item2;

                            ShowHudNotification("Applied saved calibration");
                        }
                    }
                }
            }
            catch(System.Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
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

        static int GetHipsTracker()
        {
            int l_result = -1;
            for(int i = 0; i < PlayerSetup.Instance._trackerManager.trackerNames.Length; i++)
            {
                if((PlayerSetup.Instance._trackerManager.trackerNames[i] == "vive_tracker_waist") && PlayerSetup.Instance._trackerManager.trackers[i].active)
                {
                    l_result = i;
                    break;
                }
            }
            return l_result;
        }
    }
}