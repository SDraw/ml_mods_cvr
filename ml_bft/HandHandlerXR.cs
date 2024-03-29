using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.Hands;
using UnityEngine.XR;

namespace ml_bft
{
    class HandHandlerXR : HandHandler
    {
        // 26 bones, get in XRHandJointID enum
        const int c_fingerBonesCount = (int)XRHandJointID.EndMarker - 1;

        public HandHandlerXR(Transform p_root, bool p_left) : base(p_left)
        {
            for(int i = 0; i < c_fingerBonesCount; i++)
            {
                m_bones.Add(null);
                m_localRotations.Add(Quaternion.identity);
            }

            m_prefabRoot = AssetsHandler.GetAsset(string.Format("Assets/OpenXR/Models/{0}Hand_IK.prefab", m_left ? "Left" : "Right")).transform;
            m_prefabRoot.name = "[FingersTracking_XR]";
            m_prefabRoot.parent = p_root;
            m_prefabRoot.localPosition = Vector3.zero;
            m_prefabRoot.localRotation = Quaternion.identity;

            m_prefabRoot.GetComponentsInChildren(true, m_renderers);

            // Ah yes, the stupid code
            char l_side = (m_left ? 'L' : 'R');
            m_bones[(int)XRHandJointID.Wrist - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist", l_side));
            m_bones[(int)XRHandJointID.Palm - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_Palm", l_side));

            m_bones[(int)XRHandJointID.ThumbMetacarpal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_ThumbMetacarpal", l_side));
            m_bones[(int)XRHandJointID.ThumbProximal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_ThumbMetacarpal/{0}_Wrist/{0}_ThumbProximal", l_side));
            m_bones[(int)XRHandJointID.ThumbDistal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_ThumbMetacarpal/{0}_Wrist/{0}_ThumbProximal/{0}_ThumbDistal", l_side));
            m_bones[(int)XRHandJointID.ThumbTip - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_ThumbMetacarpal/{0}_Wrist/{0}_ThumbProximal/{0}_ThumbDistal/{0}_ThumbTip", l_side));
            m_bones[(int)XRHandJointID.IndexMetacarpal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_IndexMetacarpal", l_side));
            m_bones[(int)XRHandJointID.IndexProximal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_IndexMetacarpal/{0}_IndexProximal", l_side));
            m_bones[(int)XRHandJointID.IndexIntermediate - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_IndexMetacarpal/{0}_IndexProximal/{0}_IndexIntermediate", l_side));
            m_bones[(int)XRHandJointID.IndexDistal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_IndexMetacarpal/{0}_IndexProximal/{0}_IndexIntermediate/{0}_IndexDistal", l_side));
            m_bones[(int)XRHandJointID.IndexTip - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_IndexMetacarpal/{0}_IndexProximal/{0}_IndexIntermediate/{0}_IndexDistal/{0}_IndexTip", l_side));

            m_bones[(int)XRHandJointID.MiddleMetacarpal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_MiddleMetacarpal", l_side));
            m_bones[(int)XRHandJointID.MiddleProximal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_MiddleMetacarpal/{0}_MiddleProximal", l_side));
            m_bones[(int)XRHandJointID.MiddleIntermediate - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_MiddleMetacarpal/{0}_MiddleProximal/{0}_MiddleIntermediate", l_side));
            m_bones[(int)XRHandJointID.MiddleDistal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_MiddleMetacarpal/{0}_MiddleProximal/{0}_MiddleIntermediate/{0}_MiddleDistal", l_side));
            m_bones[(int)XRHandJointID.MiddleTip - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_MiddleMetacarpal/{0}_MiddleProximal/{0}_MiddleIntermediate/{0}_MiddleDistal/{0}_MiddleTip", l_side));

            m_bones[(int)XRHandJointID.RingMetacarpal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_RingMetacarpal", l_side));
            m_bones[(int)XRHandJointID.RingProximal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_RingMetacarpal/{0}_RingProximal", l_side));
            m_bones[(int)XRHandJointID.RingIntermediate - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_RingMetacarpal/{0}_RingProximal/{0}_RingIntermediate", l_side));
            m_bones[(int)XRHandJointID.RingDistal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_RingMetacarpal/{0}_RingProximal/{0}_RingIntermediate/{0}_RingDistal", l_side));
            m_bones[(int)XRHandJointID.RingTip - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_RingMetacarpal/{0}_RingProximal/{0}_RingIntermediate/{0}_RingDistal/{0}_RingTip", l_side));

            m_bones[(int)XRHandJointID.LittleMetacarpal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_LittleMetacarpal", l_side));
            m_bones[(int)XRHandJointID.LittleProximal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_LittleMetacarpal/{0}_LittleProximal", l_side));
            m_bones[(int)XRHandJointID.LittleIntermediate - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_LittleMetacarpal/{0}_LittleProximal/{0}_LittleIntermediate", l_side));
            m_bones[(int)XRHandJointID.LittleDistal - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_LittleMetacarpal/{0}_LittleProximal/{0}_LittleIntermediate/{0}_LittleDistal", l_side));
            m_bones[(int)XRHandJointID.LittleTip - 1] = m_prefabRoot.Find(string.Format("{0}_Wrist/{0}_LittleMetacarpal/{0}_LittleProximal/{0}_LittleIntermediate/{0}_LittleDistal/{0}_LittleTip", l_side));

            for(int i = 0; i < c_fingerBonesCount; i++)
            {
                if(m_bones[i] != null)
                    m_localRotations[i] = m_bones[i].localRotation;
            }

            base.OnShowHandsChange(Settings.ShowHands);
        }

        public override Transform GetSourceForBone(HumanBodyBones p_bone)
        {
            Transform l_result = null;
            if(m_left)
            {
                switch(p_bone)
                {
                    case HumanBodyBones.LeftHand:
                        l_result = m_bones[(int)XRHandJointID.Wrist - 1];
                        break;
                    case HumanBodyBones.LeftThumbProximal:
                        l_result = m_bones[(int)XRHandJointID.ThumbMetacarpal - 1];
                        break;
                    case HumanBodyBones.LeftThumbIntermediate:
                        l_result = m_bones[(int)XRHandJointID.ThumbProximal - 1];
                        break;
                    case HumanBodyBones.LeftThumbDistal:
                        l_result = m_bones[(int)XRHandJointID.ThumbDistal - 1];
                        break;

                    case HumanBodyBones.LeftIndexProximal:
                        l_result = m_bones[(int)XRHandJointID.IndexProximal - 1];
                        break;
                    case HumanBodyBones.LeftIndexIntermediate:
                        l_result = m_bones[(int)XRHandJointID.IndexIntermediate - 1];
                        break;
                    case HumanBodyBones.LeftIndexDistal:
                        l_result = m_bones[(int)XRHandJointID.IndexDistal - 1];
                        break;

                    case HumanBodyBones.LeftMiddleProximal:
                        l_result = m_bones[(int)XRHandJointID.MiddleProximal - 1];
                        break;
                    case HumanBodyBones.LeftMiddleIntermediate:
                        l_result = m_bones[(int)XRHandJointID.MiddleIntermediate - 1];
                        break;
                    case HumanBodyBones.LeftMiddleDistal:
                        l_result = m_bones[(int)XRHandJointID.MiddleDistal - 1];
                        break;

                    case HumanBodyBones.LeftRingProximal:
                        l_result = m_bones[(int)XRHandJointID.RingProximal - 1];
                        break;
                    case HumanBodyBones.LeftRingIntermediate:
                        l_result = m_bones[(int)XRHandJointID.RingIntermediate - 1];
                        break;
                    case HumanBodyBones.LeftRingDistal:
                        l_result = m_bones[(int)XRHandJointID.RingDistal - 1];
                        break;

                    case HumanBodyBones.LeftLittleProximal:
                        l_result = m_bones[(int)XRHandJointID.LittleProximal - 1];
                        break;
                    case HumanBodyBones.LeftLittleIntermediate:
                        l_result = m_bones[(int)XRHandJointID.LittleIntermediate - 1];
                        break;
                    case HumanBodyBones.LeftLittleDistal:
                        l_result = m_bones[(int)XRHandJointID.LittleDistal - 1];
                        break;
                }
            }
            else
            {
                switch(p_bone)
                {
                    case HumanBodyBones.RightHand:
                        l_result = m_bones[(int)XRHandJointID.Wrist - 1];
                        break;
                    case HumanBodyBones.RightThumbProximal:
                        l_result = m_bones[(int)XRHandJointID.ThumbMetacarpal - 1];
                        break;
                    case HumanBodyBones.RightThumbIntermediate:
                        l_result = m_bones[(int)XRHandJointID.ThumbProximal - 1];
                        break;
                    case HumanBodyBones.RightThumbDistal:
                        l_result = m_bones[(int)XRHandJointID.ThumbDistal - 1];
                        break;

                    case HumanBodyBones.RightIndexProximal:
                        l_result = m_bones[(int)XRHandJointID.IndexProximal - 1];
                        break;
                    case HumanBodyBones.RightIndexIntermediate:
                        l_result = m_bones[(int)XRHandJointID.IndexIntermediate - 1];
                        break;
                    case HumanBodyBones.RightIndexDistal:
                        l_result = m_bones[(int)XRHandJointID.IndexDistal - 1];
                        break;

                    case HumanBodyBones.RightMiddleProximal:
                        l_result = m_bones[(int)XRHandJointID.MiddleProximal - 1];
                        break;
                    case HumanBodyBones.RightMiddleIntermediate:
                        l_result = m_bones[(int)XRHandJointID.MiddleIntermediate - 1];
                        break;
                    case HumanBodyBones.RightMiddleDistal:
                        l_result = m_bones[(int)XRHandJointID.MiddleDistal - 1];
                        break;

                    case HumanBodyBones.RightRingProximal:
                        l_result = m_bones[(int)XRHandJointID.RingProximal - 1];
                        break;
                    case HumanBodyBones.RightRingIntermediate:
                        l_result = m_bones[(int)XRHandJointID.RingIntermediate - 1];
                        break;
                    case HumanBodyBones.RightRingDistal:
                        l_result = m_bones[(int)XRHandJointID.RingDistal - 1];
                        break;

                    case HumanBodyBones.RightLittleProximal:
                        l_result = m_bones[(int)XRHandJointID.LittleProximal - 1];
                        break;
                    case HumanBodyBones.RightLittleIntermediate:
                        l_result = m_bones[(int)XRHandJointID.LittleIntermediate - 1];
                        break;
                    case HumanBodyBones.RightLittleDistal:
                        l_result = m_bones[(int)XRHandJointID.LittleDistal - 1];
                        break;
                }
            }
            return l_result;
        }

        public override void Update()
        {
            var l_tracking = OpenXRSettings.Instance.GetFeature<HandTrackingFeature>();
            var l_device = InputDevices.GetDeviceAtXRNode(m_left ? XRNode.LeftHand : XRNode.RightHand);
            if((l_device != null) && l_device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion l_deviceRot) && (l_tracking != null))
            {
                Quaternion l_handInv = Quaternion.Inverse(l_deviceRot);
                l_tracking.GetHandJoints(m_left ? HandTrackingFeature.Hand_Index.L : HandTrackingFeature.Hand_Index.R, out var l_positions, out var l_rotations, out _);
                if(l_positions.Length >= c_fingerBonesCount)
                {
                    // Joints rotations are in global space, locations are in local space ... wth is wrong with OpenXR?
                    Quaternion l_prefabRot = m_prefabRoot.rotation;
                    for(int i = 0; i < c_fingerBonesCount; i++)
                    {
                        if(m_bones[i] != null)
                        {
                            m_bones[i].localPosition = l_positions[i];
                            m_bones[i].rotation = l_prefabRot * (l_handInv * l_rotations[i]);
                        }
                    }
                }
            }
        }

        public override void Rebind(Quaternion p_base)
        {
            for(int i = 0; i < c_fingerBonesCount; i++)
            {
                if(m_bones[i] != null)
                    m_bones[i].localRotation = m_localRotations[i];
            }

            if(m_bones[(int)XRHandJointID.Wrist - 1] != null)
                m_bones[(int)XRHandJointID.Wrist - 1].rotation = p_base * (m_left ? Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, 90f, 0f));
        }
    }
}
