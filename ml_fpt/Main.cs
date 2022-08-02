using ABI_RC.Core.Player;
using ABI_RC.Core.UI;
using UnityEngine;

namespace ml_fpt
{
    public class FourPointTracking : MelonLoader.MelonMod
    {
        static readonly Vector4 ms_pointVector4 = new Vector4(0f, 0f, 0f, 1f);

        public override void OnUpdate()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
                CalibrateHipsTracker(); // Separated, awaiting for release of UI mod
        }

        static void CalibrateHipsTracker()
        {
            bool l_result = false;

            if((PlayerSetup.Instance != null) && PlayerSetup.Instance._inVr && (PlayerSetup.Instance._animator != null) && PlayerSetup.Instance._animator.isHuman)
            {
                for(int i = 0; i < PlayerSetup.Instance._trackerManager.trackerNames.Length; i++)
                {
                    if(PlayerSetup.Instance._trackerManager.trackerNames[i] == "vive_tracker_waist")
                    {
                        Transform l_target = PlayerSetup.Instance._trackerManager.trackers[i].target;
                        Matrix4x4 l_offset = PlayerSetup.Instance._trackerManager.trackers[i].transform.GetMatrix().inverse * PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Hips).GetMatrix();
                        l_target.localPosition = l_offset * ms_pointVector4;
                        l_target.localRotation = l_offset.rotation;

                        var l_vrIK = PlayerSetup.Instance._avatar.GetComponent<RootMotion.FinalIK.VRIK>();
                        if((l_vrIK != null) && (l_vrIK.solver?.spine != null))
                        {
                            l_vrIK.solver.spine.pelvisTarget = l_target;
                            l_vrIK.solver.spine.pelvisPositionWeight = 1f;
                            l_vrIK.solver.spine.pelvisRotationWeight = 1f;
                        }

                        l_result = true;
                        break;
                    }
                }
            }

            if(CohtmlHud.Instance != null)
                CohtmlHud.Instance.ViewDropText("4-Point Tracking", (l_result ? "Calibration successful" : "Calibration failed"));
        }
    }
}
