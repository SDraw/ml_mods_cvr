﻿using UnityEngine;
using Valve.VR;

namespace ml_bft
{
    class HandHandlerOVR : HandHandler
    {
        // 31 bones in each hand, get index at Valve.VR.SteamVR_Skeleton_JointIndexes or SteamVR_Skeleton_JointIndexEnum
        const int c_fingerBonesCount = (int)SteamVR_Skeleton_JointIndexEnum.pinkyAux + 1;

        SteamVR_Action_Skeleton m_skeletonAction;

        public HandHandlerOVR(Transform p_root, bool p_left) : base(p_left)
        {
            for(int i = 0; i < c_fingerBonesCount; i++)
            {
                m_bones.Add(null);
                m_localRotations.Add(Quaternion.identity);
            }

            // Fill finger transforms
            m_prefabRoot = AssetsHandler.GetAsset(string.Format("assets/steamvr/models/[openvr] {0}.prefab", m_left ? "left" : "right")).transform;
            m_prefabRoot.name = "[FingersTracking_OVR]";
            m_prefabRoot.parent = p_root;
            m_prefabRoot.localPosition = Vector3.zero;
            m_prefabRoot.localRotation = Quaternion.identity;

            m_prefabRoot.GetComponentsInChildren(true, m_renderers);

            // Ah yes, the stupid code
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.root] = m_prefabRoot.Find("Root");
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.wrist] = m_prefabRoot.Find(string.Format("Root/wrist_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_thumb_0_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_thumb_0_{0}/finger_thumb_1_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_thumb_0_{0}/finger_thumb_1_{0}/finger_thumb_2_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_thumb_0_{0}/finger_thumb_1_{0}/finger_thumb_2_{0}/finger_thumb_{0}_end", m_left ? 'l' : 'r'));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexMetacarpal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}/finger_index_0_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}/finger_index_0_{0}/finger_index_1_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}/finger_index_0_{0}/finger_index_1_{0}/finger_index_2_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}/finger_index_0_{0}/finger_index_1_{0}/finger_index_2_{0}/finger_index_{0}_end", m_left ? 'l' : 'r'));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleMetacarpal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}/finger_middle_0_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}/finger_middle_0_{0}/finger_middle_1_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}/finger_middle_0_{0}/finger_middle_1_{0}/finger_middle_2_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}/finger_middle_0_{0}/finger_middle_1_{0}/finger_middle_2_{0}/finger_middle_{0}_end", m_left ? 'l' : 'r'));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringMetacarpal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}/finger_ring_0_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}/finger_ring_0_{0}/finger_ring_1_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}/finger_ring_0_{0}/finger_ring_1_{0}/finger_ring_2_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}/finger_ring_0_{0}/finger_ring_1_{0}/finger_ring_2_{0}/finger_ring_{0}_end", m_left ? 'l' : 'r'));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyMetacarpal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}/finger_pinky_0_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}/finger_pinky_0_{0}/finger_pinky_1_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}/finger_pinky_0_{0}/finger_pinky_1_{0}/finger_pinky_2_{0}", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}/finger_pinky_0_{0}/finger_pinky_1_{0}/finger_pinky_2_{0}/finger_pinky_{0}_end", m_left ? 'l' : 'r'));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbAux] = m_prefabRoot.Find(string.Format("Root/finger_thumb_{0}_aux", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexAux] = m_prefabRoot.Find(string.Format("Root/finger_index_{0}_aux", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleAux] = m_prefabRoot.Find(string.Format("Root/finger_middle_{0}_aux", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringAux] = m_prefabRoot.Find(string.Format("Root/finger_ring_{0}_aux", m_left ? 'l' : 'r'));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyAux] = m_prefabRoot.Find(string.Format("Root/finger_pinky_{0}_aux", m_left ? 'l' : 'r'));

            // Remember local rotations
            for(int i = 0; i < c_fingerBonesCount; i++)
            {
                if(m_bones[i] != null)
                    m_localRotations[i] = m_bones[i].localRotation;
            }

            m_skeletonAction = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>(p_left ? "SkeletonLeftHand" : "SkeletonRightHand");

            base.OnShowHandsChange(Settings.ShowHands);
        }

        public override void Cleanup()
        {
            base.Cleanup();

            m_skeletonAction = null;
        }

        public override void Update()
        {
            if(m_skeletonAction != null)
            {
                var l_rotations = m_skeletonAction.GetBoneRotations();
                var l_positions = m_skeletonAction.GetBonePositions();
                for(int i = 0; i < c_fingerBonesCount; i++)
                {
                    if(m_bones[i] != null)
                    {
                        m_bones[i].localRotation = l_rotations[i];
                        m_bones[i].localPosition = l_positions[i];
                    }
                }
            }
        }

        public override Transform GetSourceForBone(HumanBodyBones p_bone)
        {
            Transform l_result = null;
            if(m_left)
            {
                switch(p_bone)
                {
                    case HumanBodyBones.LeftHand:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.wrist];
                        break;
                    case HumanBodyBones.LeftThumbProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbProximal];
                        break;
                    case HumanBodyBones.LeftThumbIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbMiddle];
                        break;
                    case HumanBodyBones.LeftThumbDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbDistal];
                        break;

                    case HumanBodyBones.LeftIndexProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexProximal];
                        break;
                    case HumanBodyBones.LeftIndexIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexMiddle];
                        break;
                    case HumanBodyBones.LeftIndexDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexDistal];
                        break;

                    case HumanBodyBones.LeftMiddleProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleProximal];
                        break;
                    case HumanBodyBones.LeftMiddleIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleMiddle];
                        break;
                    case HumanBodyBones.LeftMiddleDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleDistal];
                        break;

                    case HumanBodyBones.LeftRingProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringProximal];
                        break;
                    case HumanBodyBones.LeftRingIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringMiddle];
                        break;
                    case HumanBodyBones.LeftRingDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringDistal];
                        break;

                    case HumanBodyBones.LeftLittleProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyProximal];
                        break;
                    case HumanBodyBones.LeftLittleIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyMiddle];
                        break;
                    case HumanBodyBones.LeftLittleDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyDistal];
                        break;
                }
            }
            else
            {
                switch(p_bone)
                {
                    case HumanBodyBones.RightHand:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.wrist];
                        break;
                    case HumanBodyBones.RightThumbProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbProximal];
                        break;
                    case HumanBodyBones.RightThumbIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbMiddle];
                        break;
                    case HumanBodyBones.RightThumbDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbDistal];
                        break;

                    case HumanBodyBones.RightIndexProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexProximal];
                        break;
                    case HumanBodyBones.RightIndexIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexMiddle];
                        break;
                    case HumanBodyBones.RightIndexDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexDistal];
                        break;

                    case HumanBodyBones.RightMiddleProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleProximal];
                        break;
                    case HumanBodyBones.RightMiddleIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleMiddle];
                        break;
                    case HumanBodyBones.RightMiddleDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleDistal];
                        break;

                    case HumanBodyBones.RightRingProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringProximal];
                        break;
                    case HumanBodyBones.RightRingIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringMiddle];
                        break;
                    case HumanBodyBones.RightRingDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringDistal];
                        break;

                    case HumanBodyBones.RightLittleProximal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyProximal];
                        break;
                    case HumanBodyBones.RightLittleIntermediate:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyMiddle];
                        break;
                    case HumanBodyBones.RightLittleDistal:
                        l_result = m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyDistal];
                        break;
                }
            }
            return l_result;
        }

        public override void Rebind(Quaternion p_base)
        {
            for(int i = 0; i < c_fingerBonesCount; i++)
            {
                if(m_bones[i] != null)
                    m_bones[i].localRotation = m_localRotations[i];
            }

            if(m_bones[(int)SteamVR_Skeleton_JointIndexEnum.root] != null)
                m_bones[(int)SteamVR_Skeleton_JointIndexEnum.root].rotation = p_base * (m_left ? Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, 90f, 0f));
        }
    }
}
