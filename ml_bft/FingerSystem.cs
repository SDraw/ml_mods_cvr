using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.InputManagement;
using System.Collections.Generic;
using UnityEngine;

namespace ml_bft
{
    class FingerSystem
    {
        enum PlaneType
        {
            OXZ,
            OYX
        }

        struct RotationOffset
        {
            public Transform m_target;
            public Transform m_source;
            public Quaternion m_offset;

            public void Reset()
            {
                m_source = null;
                m_target = null;
                m_offset = Quaternion.identity;
            }
        }

        static readonly HumanBodyBones[] ms_leftFingerBones =
        {
            HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal,
            HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,
            HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,
            HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal,
            HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal
        };
        static readonly HumanBodyBones[] ms_rightFingerBones =
        {
            HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal,
            HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,
            HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal,
            HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal,
            HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal
        };
        static readonly (HumanBodyBones, HumanBodyBones, bool)[] ms_rotationFixChains =
        {
            (HumanBodyBones.LeftThumbProximal,HumanBodyBones.LeftThumbIntermediate,true), (HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal,true),
            (HumanBodyBones.LeftIndexProximal,HumanBodyBones.LeftIndexIntermediate,true), (HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,true),
            (HumanBodyBones.LeftMiddleProximal,HumanBodyBones.LeftMiddleIntermediate,true), (HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,true),
            (HumanBodyBones.LeftRingProximal,HumanBodyBones.LeftRingIntermediate,true), (HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal,true),
            (HumanBodyBones.LeftLittleProximal,HumanBodyBones.LeftLittleIntermediate,true), (HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal,true),
            (HumanBodyBones.RightThumbProximal,HumanBodyBones.RightThumbIntermediate,false), (HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal,false),
            (HumanBodyBones.RightIndexProximal,HumanBodyBones.RightIndexIntermediate,false), (HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,false),
            (HumanBodyBones.RightMiddleProximal,HumanBodyBones.RightMiddleIntermediate,false), (HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal,false),
            (HumanBodyBones.RightRingProximal,HumanBodyBones.RightRingIntermediate,false), (HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal,false),
            (HumanBodyBones.RightLittleProximal,HumanBodyBones.RightLittleIntermediate,false), (HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal,false)
        };

        public static FingerSystem Instance { get; private set; } = null;

        RotationOffset m_leftHandOffset; // From avatar hand to controller wrist
        RotationOffset m_rightHandOffset;
        readonly List<RotationOffset> m_leftFingerOffsets = null; // From controller finger bone to avatar finger bone
        readonly List<RotationOffset> m_rightFingerOffsets = null;

        public readonly float[] m_lastValues;

        bool m_ready = false;
        HumanPose m_pose;

        internal FingerSystem()
        {
            if(Instance == null)
                Instance = this;

            m_leftFingerOffsets = new List<RotationOffset>();
            m_rightFingerOffsets = new List<RotationOffset>();

            m_pose = new HumanPose();
            m_lastValues = new float[40];

            GameEvents.OnAvatarSetup.AddListener(this.OnAvatarSetup);
            GameEvents.OnAvatarClear.AddListener(this.OnAvatarClear);
            GameEvents.OnAvatarReuse.AddListener(this.OnAvatarReuse);
            GameEvents.OnIKSystemLateUpdate.AddListener(this.OnIKSystemLateUpdate);
        }
        internal void Cleanup()
        {
            if(Instance == this)
                Instance = null;

            m_leftFingerOffsets.Clear();
            m_rightFingerOffsets.Clear();
            m_ready = false;

            GameEvents.OnAvatarSetup.RemoveListener(this.OnAvatarSetup);
            GameEvents.OnAvatarClear.RemoveListener(this.OnAvatarClear);
            GameEvents.OnAvatarReuse.RemoveListener(this.OnAvatarReuse);
            GameEvents.OnIKSystemLateUpdate.RemoveListener(this.OnIKSystemLateUpdate);
        }

        internal void OnAvatarSetup()
        {
            if(PlayerSetup.Instance._animator.isHuman)
            {
                Utils.SetAvatarTPose();
                InputHandler.Instance.Rebind(PlayerSetup.Instance.transform.rotation);

                if(Settings.FixFingers)
                {
                    foreach(var l_tuple in ms_rotationFixChains)
                    {
                        ReorientateTowards(
                            PlayerSetup.Instance._animator.GetBoneTransform(l_tuple.Item1),
                            PlayerSetup.Instance._animator.GetBoneTransform(l_tuple.Item2),
                            InputHandler.Instance.GetSourceForBone(l_tuple.Item1, l_tuple.Item3),
                            InputHandler.Instance.GetSourceForBone(l_tuple.Item2, l_tuple.Item3),
                            PlaneType.OXZ
                        );
                        ReorientateTowards(
                            PlayerSetup.Instance._animator.GetBoneTransform(l_tuple.Item1),
                            PlayerSetup.Instance._animator.GetBoneTransform(l_tuple.Item2),
                            InputHandler.Instance.GetSourceForBone(l_tuple.Item1, l_tuple.Item3),
                            InputHandler.Instance.GetSourceForBone(l_tuple.Item2, l_tuple.Item3),
                            PlaneType.OYX
                        );
                    }
                }

                // Bind hands
                m_leftHandOffset.m_source = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.LeftHand);
                m_leftHandOffset.m_target = InputHandler.Instance.GetSourceForBone(HumanBodyBones.LeftHand, true);
                if((m_leftHandOffset.m_source != null) && (m_leftHandOffset.m_target != null))
                    m_leftHandOffset.m_offset = Quaternion.Inverse(m_leftHandOffset.m_source.rotation) * m_leftHandOffset.m_target.rotation;

                m_rightHandOffset.m_source = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                m_rightHandOffset.m_target = InputHandler.Instance.GetSourceForBone(HumanBodyBones.RightHand, false);
                if((m_rightHandOffset.m_source != null) && (m_rightHandOffset.m_target != null))
                    m_rightHandOffset.m_offset = Quaternion.Inverse(m_rightHandOffset.m_source.rotation) * m_rightHandOffset.m_target.rotation;

                // Bind fingers
                foreach(HumanBodyBones p_bone in ms_leftFingerBones)
                {
                    Transform l_avatarBone = PlayerSetup.Instance._animator.GetBoneTransform(p_bone);
                    Transform l_controllerBone = InputHandler.Instance.GetSourceForBone(p_bone, true);
                    if((l_avatarBone != null) && (l_controllerBone != null))
                    {
                        RotationOffset l_offset = new RotationOffset();
                        l_offset.m_source = l_controllerBone;
                        l_offset.m_target = l_avatarBone;
                        l_offset.m_offset = Quaternion.Inverse(l_controllerBone.rotation) * l_avatarBone.rotation;
                        m_leftFingerOffsets.Add(l_offset);
                    }
                }
                foreach(HumanBodyBones p_bone in ms_rightFingerBones)
                {
                    Transform l_avatarBone = PlayerSetup.Instance._animator.GetBoneTransform(p_bone);
                    Transform l_controllerBone = InputHandler.Instance.GetSourceForBone(p_bone, false);
                    if((l_avatarBone != null) && (l_controllerBone != null))
                    {
                        RotationOffset l_offset = new RotationOffset();
                        l_offset.m_source = l_controllerBone;
                        l_offset.m_target = l_avatarBone;
                        l_offset.m_offset = Quaternion.Inverse(l_controllerBone.rotation) * l_avatarBone.rotation;
                        m_rightFingerOffsets.Add(l_offset);
                    }
                }

                m_ready = ((m_leftFingerOffsets.Count > 0) || (m_rightFingerOffsets.Count > 0));
            }
        }

        internal void OnAvatarClear()
        {
            m_ready = false;
            m_pose = new HumanPose();

            m_leftHandOffset.Reset();
            m_rightHandOffset.Reset();

            m_leftFingerOffsets.Clear();
            m_rightFingerOffsets.Clear();
        }

        internal void OnAvatarReuse()
        {
            OnAvatarClear();
            OnAvatarSetup();
        }

        internal void OnIKSystemLateUpdate(HumanPoseHandler p_handler, Transform p_hips)
        {
            if(m_ready && MetaPort.Instance.isUsingVr && (p_handler != null) && Settings.SkeletalInput)
            {
                if(CVRInputManager.Instance._leftController != ABI_RC.Systems.InputManagement.XR.eXRControllerType.None)
                {
                    Quaternion l_turnBack = (m_leftHandOffset.m_source.rotation * m_leftHandOffset.m_offset) * Quaternion.Inverse(m_leftHandOffset.m_target.rotation);
                    foreach(var l_offset in m_leftFingerOffsets)
                        l_offset.m_target.rotation = l_turnBack * (l_offset.m_source.rotation * l_offset.m_offset);
                }

                if(CVRInputManager.Instance._rightController != ABI_RC.Systems.InputManagement.XR.eXRControllerType.None)
                {
                    Quaternion l_turnBack = (m_rightHandOffset.m_source.rotation * m_rightHandOffset.m_offset) * Quaternion.Inverse(m_rightHandOffset.m_target.rotation);
                    foreach(var l_offset in m_rightFingerOffsets)
                        l_offset.m_target.rotation = l_turnBack * (l_offset.m_source.rotation * l_offset.m_offset);
                }

                p_handler.GetHumanPose(ref m_pose);
                m_lastValues[0] = m_pose.muscles[(int)MuscleIndex.LeftThumb1Stretched];
                m_lastValues[1] = m_pose.muscles[(int)MuscleIndex.LeftThumb2Stretched];
                m_lastValues[2] = m_pose.muscles[(int)MuscleIndex.LeftThumb3Stretched];
                m_lastValues[3] = m_pose.muscles[(int)MuscleIndex.LeftThumbSpread];
                m_lastValues[4] = m_pose.muscles[(int)MuscleIndex.LeftIndex1Stretched];
                m_lastValues[5] = m_pose.muscles[(int)MuscleIndex.LeftIndex2Stretched];
                m_lastValues[6] = m_pose.muscles[(int)MuscleIndex.LeftIndex3Stretched];
                m_lastValues[7] = m_pose.muscles[(int)MuscleIndex.LeftIndexSpread];
                m_lastValues[8] = m_pose.muscles[(int)MuscleIndex.LeftMiddle1Stretched];
                m_lastValues[9] = m_pose.muscles[(int)MuscleIndex.LeftMiddle2Stretched];
                m_lastValues[10] = m_pose.muscles[(int)MuscleIndex.LeftMiddle3Stretched];
                m_lastValues[11] = m_pose.muscles[(int)MuscleIndex.LeftMiddleSpread];
                m_lastValues[12] = m_pose.muscles[(int)MuscleIndex.LeftRing1Stretched];
                m_lastValues[13] = m_pose.muscles[(int)MuscleIndex.LeftRing2Stretched];
                m_lastValues[14] = m_pose.muscles[(int)MuscleIndex.LeftRing3Stretched];
                m_lastValues[15] = m_pose.muscles[(int)MuscleIndex.LeftRingSpread];
                m_lastValues[16] = m_pose.muscles[(int)MuscleIndex.LeftLittle1Stretched];
                m_lastValues[17] = m_pose.muscles[(int)MuscleIndex.LeftLittle2Stretched];
                m_lastValues[18] = m_pose.muscles[(int)MuscleIndex.LeftLittle3Stretched];
                m_lastValues[19] = m_pose.muscles[(int)MuscleIndex.LeftLittleSpread];
                m_lastValues[20] = m_pose.muscles[(int)MuscleIndex.RightThumb1Stretched];
                m_lastValues[21] = m_pose.muscles[(int)MuscleIndex.RightThumb2Stretched];
                m_lastValues[22] = m_pose.muscles[(int)MuscleIndex.RightThumb3Stretched];
                m_lastValues[23] = m_pose.muscles[(int)MuscleIndex.RightThumbSpread];
                m_lastValues[24] = m_pose.muscles[(int)MuscleIndex.RightIndex1Stretched];
                m_lastValues[25] = m_pose.muscles[(int)MuscleIndex.RightIndex2Stretched];
                m_lastValues[26] = m_pose.muscles[(int)MuscleIndex.RightIndex3Stretched];
                m_lastValues[27] = m_pose.muscles[(int)MuscleIndex.RightIndexSpread];
                m_lastValues[28] = m_pose.muscles[(int)MuscleIndex.RightMiddle1Stretched];
                m_lastValues[29] = m_pose.muscles[(int)MuscleIndex.RightMiddle2Stretched];
                m_lastValues[30] = m_pose.muscles[(int)MuscleIndex.RightMiddle3Stretched];
                m_lastValues[31] = m_pose.muscles[(int)MuscleIndex.RightMiddleSpread];
                m_lastValues[32] = m_pose.muscles[(int)MuscleIndex.RightRing1Stretched];
                m_lastValues[33] = m_pose.muscles[(int)MuscleIndex.RightRing2Stretched];
                m_lastValues[34] = m_pose.muscles[(int)MuscleIndex.RightRing3Stretched];
                m_lastValues[35] = m_pose.muscles[(int)MuscleIndex.RightRingSpread];
                m_lastValues[36] = m_pose.muscles[(int)MuscleIndex.RightLittle1Stretched];
                m_lastValues[37] = m_pose.muscles[(int)MuscleIndex.RightLittle2Stretched];
                m_lastValues[38] = m_pose.muscles[(int)MuscleIndex.RightLittle3Stretched];
                m_lastValues[39] = m_pose.muscles[(int)MuscleIndex.RightLittleSpread];

                if(Settings.MechanimFilter && (p_hips != null))
                {
                    // Yoinked from IKSystem.OnPostSolverUpdateGeneral
                    Vector3 l_pos = p_hips.position;
                    Quaternion l_rot = p_hips.rotation;
                    p_handler.SetHumanPose(ref m_pose);
                    p_hips.SetPositionAndRotation(l_pos, l_rot);
                }
            }
        }

        void ReorientateTowards(Transform p_target, Transform p_targetEnd, Transform p_source, Transform p_sourceEnd, PlaneType p_plane)
        {
            if((p_target != null) && (p_targetEnd != null) && (p_source != null) && (p_sourceEnd != null))
            {
                Quaternion l_playerInv = Quaternion.Inverse(PlayerSetup.Instance.transform.rotation);
                Vector3 l_targetDir = l_playerInv * (p_targetEnd.position - p_target.position);
                Vector3 l_sourceDir = l_playerInv * (p_sourceEnd.position - p_source.position);
                switch(p_plane)
                {
                    case PlaneType.OXZ:
                        l_targetDir.y = 0f;
                        l_sourceDir.y = 0f;
                        break;
                    case PlaneType.OYX:
                        l_targetDir.z = 0f;
                        l_sourceDir.z = 0f;
                        break;
                }
                l_targetDir = Vector3.Normalize(l_targetDir);
                l_sourceDir = Vector3.Normalize(l_sourceDir);

                Quaternion l_targetRot = Quaternion.identity;
                Quaternion l_sourceRot = Quaternion.identity;
                switch(p_plane)
                {
                    case PlaneType.OXZ:
                        l_targetRot = Quaternion.LookRotation(l_targetDir, Vector3.up);
                        l_sourceRot = Quaternion.LookRotation(l_sourceDir, Vector3.up);
                        break;
                    case PlaneType.OYX:
                        l_targetRot = Quaternion.LookRotation(l_targetDir, Vector3.forward);
                        l_sourceRot = Quaternion.LookRotation(l_sourceDir, Vector3.forward);
                        break;
                }

                Quaternion l_diff = Quaternion.Inverse(l_targetRot) * l_sourceRot;
                if(p_plane == PlaneType.OYX)
                    l_diff = Quaternion.Euler(0f, 0f, l_diff.eulerAngles.y);

                Quaternion l_adjusted = l_diff * (l_playerInv * p_target.rotation);
                p_target.rotation = PlayerSetup.Instance.transform.rotation * l_adjusted;
            }
        }
    }
}
