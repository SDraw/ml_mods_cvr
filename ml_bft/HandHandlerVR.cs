using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace ml_bft
{
    class HandHandlerVR
    {
        // 31 bones in each hand, get index at Valve.VR.SteamVR_Skeleton_JointIndexes or SteamVR_Skeleton_JointIndexEnum
        const int c_fingerBonesCount = (int)SteamVR_Skeleton_JointIndexEnum.pinkyAux + 1;

        bool m_left = false;
        readonly List<Transform> m_bones = null;
        readonly List<Quaternion> m_localRotations = null;
        Transform m_prefabRoot = null;
        readonly List<Renderer> m_renderers = null;

        SteamVR_Action_Skeleton m_skeletonAction;

        public HandHandlerVR(Transform p_root, bool p_left)
        {
            m_left = p_left;
            m_bones = new List<Transform>();
            m_localRotations = new List<Quaternion>();
            m_renderers = new List<Renderer>();

            for(int i = 0; i < c_fingerBonesCount; i++)
            {
                m_bones.Add(null);
                m_localRotations.Add(Quaternion.identity);
            }

            // Fill finger transforms
            m_prefabRoot = AssetsHandler.GetAsset(string.Format("assets/steamvr/models/[openvr] {0}.prefab", m_left ? "left" : "right")).transform;
            m_prefabRoot.name = "[FingersTracking_VR]";
            m_prefabRoot.parent = p_root;
            m_prefabRoot.localPosition = Vector3.zero;
            m_prefabRoot.localRotation = Quaternion.identity;

            m_prefabRoot.GetComponentsInChildren(true, m_renderers);

            // Ah yes, the stupid code
            char l_side = (m_left ? 'l' : 'r');
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.root] = m_prefabRoot.Find("Root");
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.wrist] = m_prefabRoot.Find(string.Format("Root/wrist_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_thumb_0_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_thumb_0_{0}/finger_thumb_1_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_thumb_0_{0}/finger_thumb_1_{0}/finger_thumb_2_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_thumb_0_{0}/finger_thumb_1_{0}/finger_thumb_2_{0}/finger_thumb_{0}_end", l_side));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexMetacarpal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}/finger_index_0_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}/finger_index_0_{0}/finger_index_1_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}/finger_index_0_{0}/finger_index_1_{0}/finger_index_2_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_index_meta_{0}/finger_index_0_{0}/finger_index_1_{0}/finger_index_2_{0}/finger_index_{0}_end", l_side));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleMetacarpal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}/finger_middle_0_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}/finger_middle_0_{0}/finger_middle_1_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}/finger_middle_0_{0}/finger_middle_1_{0}/finger_middle_2_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_middle_meta_{0}/finger_middle_0_{0}/finger_middle_1_{0}/finger_middle_2_{0}/finger_middle_{0}_end", l_side));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringMetacarpal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}/finger_ring_0_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}/finger_ring_0_{0}/finger_ring_1_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}/finger_ring_0_{0}/finger_ring_1_{0}/finger_ring_2_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_ring_meta_{0}/finger_ring_0_{0}/finger_ring_1_{0}/finger_ring_2_{0}/finger_ring_{0}_end", l_side));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyMetacarpal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyProximal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}/finger_pinky_0_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyMiddle] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}/finger_pinky_0_{0}/finger_pinky_1_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyDistal] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}/finger_pinky_0_{0}/finger_pinky_1_{0}/finger_pinky_2_{0}", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyTip] = m_prefabRoot.Find(string.Format("Root/wrist_{0}/finger_pinky_meta_{0}/finger_pinky_0_{0}/finger_pinky_1_{0}/finger_pinky_2_{0}/finger_pinky_{0}_end", l_side));

            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbAux] = m_prefabRoot.Find(string.Format("Root/finger_thumb_{0}_aux", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexAux] = m_prefabRoot.Find(string.Format("Root/finger_index_{0}_aux", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleAux] = m_prefabRoot.Find(string.Format("Root/finger_middle_{0}_aux", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringAux] = m_prefabRoot.Find(string.Format("Root/finger_ring_{0}_aux", l_side));
            m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyAux] = m_prefabRoot.Find(string.Format("Root/finger_pinky_{0}_aux", l_side));

            // Remember local rotations
            for(int i = 0; i < c_fingerBonesCount; i++)
            {
                if(m_bones[i] != null)
                    m_localRotations[i] = m_bones[i].localRotation;
            }

            m_skeletonAction = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>(p_left ? "SkeletonLeftHand" : "SkeletonRightHand");

            OnShowHandsChanged(Settings.ShowHands);
            OnMotionRangeChanged(Settings.MotionRange);

            Settings.OnShowHandsChanged.AddListener(this.OnShowHandsChanged);
            Settings.OnMotionRangeChanged.AddListener(this.OnMotionRangeChanged);
        }

        public void Cleanup()
        {
            if(m_prefabRoot != null)
                Object.Destroy(m_prefabRoot.gameObject);
            m_prefabRoot = null;

            m_bones.Clear();
            m_localRotations.Clear();
            m_renderers.Clear();

            m_skeletonAction = null;

            Settings.OnShowHandsChanged.RemoveListener(this.OnShowHandsChanged);
            Settings.OnMotionRangeChanged.RemoveListener(this.OnMotionRangeChanged);
        }

        public void Update()
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

        public Transform GetSourceForBone(HumanBodyBones p_bone)
        {
            Transform l_result = null;
            switch(p_bone)
            {
                case HumanBodyBones.LeftHand:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.wrist] : null);
                    break;
                case HumanBodyBones.LeftThumbProximal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbProximal] : null);
                    break;
                case HumanBodyBones.LeftThumbIntermediate:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbMiddle] : null);
                    break;
                case HumanBodyBones.LeftThumbDistal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbDistal] : null);
                    break;

                case HumanBodyBones.LeftIndexProximal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexProximal] : null);
                    break;
                case HumanBodyBones.LeftIndexIntermediate:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexMiddle] : null);
                    break;
                case HumanBodyBones.LeftIndexDistal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexDistal] : null);
                    break;

                case HumanBodyBones.LeftMiddleProximal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleProximal] : null);
                    break;
                case HumanBodyBones.LeftMiddleIntermediate:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleMiddle] : null);
                    break;
                case HumanBodyBones.LeftMiddleDistal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleDistal] : null);
                    break;

                case HumanBodyBones.LeftRingProximal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringProximal] : null);
                    break;
                case HumanBodyBones.LeftRingIntermediate:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringMiddle] : null);
                    break;
                case HumanBodyBones.LeftRingDistal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringDistal] : null);
                    break;

                case HumanBodyBones.LeftLittleProximal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyProximal] : null);
                    break;
                case HumanBodyBones.LeftLittleIntermediate:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyMiddle] : null);
                    break;
                case HumanBodyBones.LeftLittleDistal:
                    l_result = (m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyDistal] : null);
                    break;

                case HumanBodyBones.RightHand:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.wrist] : null);
                    break;
                case HumanBodyBones.RightThumbProximal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbProximal] : null);
                    break;
                case HumanBodyBones.RightThumbIntermediate:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbMiddle] : null);
                    break;
                case HumanBodyBones.RightThumbDistal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.thumbDistal] : null);
                    break;

                case HumanBodyBones.RightIndexProximal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexProximal] : null);
                    break;
                case HumanBodyBones.RightIndexIntermediate:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexMiddle] : null);
                    break;
                case HumanBodyBones.RightIndexDistal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.indexDistal] : null);
                    break;

                case HumanBodyBones.RightMiddleProximal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleProximal] : null);
                    break;
                case HumanBodyBones.RightMiddleIntermediate:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleMiddle] : null);
                    break;
                case HumanBodyBones.RightMiddleDistal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.middleDistal] : null);
                    break;

                case HumanBodyBones.RightRingProximal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringProximal] : null);
                    break;
                case HumanBodyBones.RightRingIntermediate:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringMiddle] : null);
                    break;
                case HumanBodyBones.RightRingDistal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.ringDistal] : null);
                    break;

                case HumanBodyBones.RightLittleProximal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyProximal] : null);
                    break;
                case HumanBodyBones.RightLittleIntermediate:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyMiddle] : null);
                    break;
                case HumanBodyBones.RightLittleDistal:
                    l_result = (!m_left ? m_bones[(int)SteamVR_Skeleton_JointIndexEnum.pinkyDistal] : null);
                    break;
            }
            return l_result;
        }

        public void Rebind(Quaternion p_base)
        {
            for(int i = 0; i < c_fingerBonesCount; i++)
            {
                if(m_bones[i] != null)
                    m_bones[i].localRotation = m_localRotations[i];
            }

            if(m_bones[(int)SteamVR_Skeleton_JointIndexEnum.root] != null)
                m_bones[(int)SteamVR_Skeleton_JointIndexEnum.root].rotation = p_base * (m_left ? Quaternion.Euler(0f, -90f, -90f) : Quaternion.Euler(0f, 90f, 90f));
        }

        // Settings
        void OnShowHandsChanged(bool p_state)
        {
            foreach(var l_render in m_renderers)
            {
                if(l_render != null)
                    l_render.enabled = p_state;
            }
        }

        void OnMotionRangeChanged(Settings.MotionRangeType p_mode)
        {
            switch(p_mode)
            {
                case Settings.MotionRangeType.WithController:
                    m_skeletonAction?.SetRangeOfMotion(EVRSkeletalMotionRange.WithController);
                    break;
                case Settings.MotionRangeType.WithoutController:
                    m_skeletonAction?.SetRangeOfMotion(EVRSkeletalMotionRange.WithoutController);
                    break;
            }
        }
    }
}
