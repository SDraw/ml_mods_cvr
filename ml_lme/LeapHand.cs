using UnityEngine;

namespace ml_lme
{
    class LeapHand
    {
        enum FingerBone
        {
            ThumbMetacarpal = 0,
            ThumbProximal,
            ThumbIntermediate,
            ThumbDistal,
            IndexMetacarpal,
            IndexProximal,
            IndexIntermediate,
            IndexDistal,
            MiddleMetacarpal,
            MiddleProximal,
            MiddleIntermediate,
            MiddleDistal,
            RingMetacarpal,
            RingProximal,
            RingIntermediate,
            RingDistal,
            PinkyMetacarpal,
            PinkyProximal,
            PinkyIntermediate,
            PinkyDistal,

            Count
        };

        readonly bool m_left = false;
        readonly Transform m_root = null;
        readonly Transform m_wrist = null;
        readonly GameObject m_mesh = null;
        readonly Transform[] m_fingersBones = null;
        readonly Quaternion[] m_initialRotations = null;

        public LeapHand(Transform p_root, bool p_left)
        {
            m_left = p_left;
            m_fingersBones = new Transform[(int)FingerBone.Count];
            m_initialRotations = new Quaternion[(int)FingerBone.Count];

            m_root = p_root;
            if(m_root != null)
            {
                m_mesh = m_root.Find(m_left ? "GenericHandL" : "GenericHandR")?.gameObject;
                m_wrist = m_root.Find(m_left ? "LeftHand/Wrist" : "RightHand/Wrist");
                if(m_wrist != null)
                {
                    m_fingersBones[0] = null; // Actual thumb-meta, look at Leap Motion docs, dummy, it's zero point
                    m_fingersBones[1] = m_wrist.Find("thumb_meta");
                    m_fingersBones[2] = m_wrist.Find("thumb_meta/thumb_a");
                    m_fingersBones[3] = m_wrist.Find("thumb_meta/thumb_a/thumb_b");

                    m_fingersBones[4] = m_wrist.Find("index_meta");
                    m_fingersBones[5] = m_wrist.Find("index_meta/index_a");
                    m_fingersBones[6] = m_wrist.Find("index_meta/index_a/index_b");
                    m_fingersBones[7] = m_wrist.Find("index_meta/index_a/index_b/index_c");

                    m_fingersBones[8] = m_wrist.Find("middle_meta");
                    m_fingersBones[9] = m_wrist.Find("middle_meta/middle_a");
                    m_fingersBones[10] = m_wrist.Find("middle_meta/middle_a/middle_b");
                    m_fingersBones[11] = m_wrist.Find("middle_meta/middle_a/middle_b/middle_c");

                    m_fingersBones[12] = m_wrist.Find("ring_meta");
                    m_fingersBones[13] = m_wrist.Find("ring_meta/ring_a");
                    m_fingersBones[14] = m_wrist.Find("ring_meta/ring_a/ring_b");
                    m_fingersBones[15] = m_wrist.Find("ring_meta/ring_a/ring_b/ring_c");

                    m_fingersBones[16] = m_wrist.Find("pinky_meta");
                    m_fingersBones[17] = m_wrist.Find("pinky_meta/pinky_a");
                    m_fingersBones[18] = m_wrist.Find("pinky_meta/pinky_a/pinky_b");
                    m_fingersBones[19] = m_wrist.Find("pinky_meta/pinky_a/pinky_b/pinky_c");
                }
            }

            for(int i = 0; i < 20; i++)
            {
                if(m_fingersBones[i] != null)
                    m_initialRotations[i] = m_fingersBones[i].localRotation;
            }
        }

        public void Update(LeapParser.HandData p_data)
        {
            if(m_wrist != null)
            {
                m_wrist.position = p_data.m_position;
                m_wrist.rotation = p_data.m_rotation;

                for(int i = 0; i < 20; i++)
                {
                    if(m_fingersBones[i] != null)
                    {
                        //m_fingersBones[i].position = p_data.m_fingerPosition[i];
                        m_fingersBones[i].rotation = p_data.m_fingerRotation[i];
                    }
                }

                m_wrist.localPosition = Vector3.zero;
                m_wrist.localRotation = Quaternion.identity;
            }
        }

        public void Rebind(Quaternion p_base)
        {
            if(m_wrist != null)
            {
                m_wrist.localPosition = Vector3.zero;
                m_wrist.localRotation = Quaternion.identity;

                m_wrist.rotation = p_base * Quaternion.Euler(0f, m_left ? -90f : 90f, 0f);
            }

            for(int i = 0; i < 20; i++)
            {
                if(m_fingersBones[i] != null)
                    m_fingersBones[i].localRotation = m_initialRotations[i];
            }
        }

        public Transform GetRoot() => m_root;
        public Transform GetLinkedBone(HumanBodyBones p_bone)
        {
            Transform l_result = null;
            switch(p_bone)
            {
                case HumanBodyBones.LeftHand:
                case HumanBodyBones.RightHand:
                    l_result = m_wrist;
                    break;

                case HumanBodyBones.LeftThumbProximal:
                case HumanBodyBones.RightThumbProximal:
                    l_result = m_fingersBones[(int)FingerBone.ThumbProximal];
                    break;

                case HumanBodyBones.LeftThumbIntermediate:
                case HumanBodyBones.RightThumbIntermediate:
                    l_result = m_fingersBones[(int)FingerBone.ThumbIntermediate];
                    break;

                case HumanBodyBones.LeftThumbDistal:
                case HumanBodyBones.RightThumbDistal:
                    l_result = m_fingersBones[(int)FingerBone.ThumbDistal];
                    break;

                case HumanBodyBones.LeftIndexProximal:
                case HumanBodyBones.RightIndexProximal:
                    l_result = m_fingersBones[(int)FingerBone.IndexProximal];
                    break;

                case HumanBodyBones.LeftIndexIntermediate:
                case HumanBodyBones.RightIndexIntermediate:
                    l_result = m_fingersBones[(int)FingerBone.IndexIntermediate];
                    break;

                case HumanBodyBones.LeftIndexDistal:
                case HumanBodyBones.RightIndexDistal:
                    l_result = m_fingersBones[(int)FingerBone.IndexDistal];
                    break;

                case HumanBodyBones.LeftMiddleProximal:
                case HumanBodyBones.RightMiddleProximal:
                    l_result = m_fingersBones[(int)FingerBone.MiddleProximal];
                    break;

                case HumanBodyBones.LeftMiddleIntermediate:
                case HumanBodyBones.RightMiddleIntermediate:
                    l_result = m_fingersBones[(int)FingerBone.MiddleIntermediate];
                    break;

                case HumanBodyBones.LeftMiddleDistal:
                case HumanBodyBones.RightMiddleDistal:
                    l_result = m_fingersBones[(int)FingerBone.MiddleDistal];
                    break;

                case HumanBodyBones.LeftRingProximal:
                case HumanBodyBones.RightRingProximal:
                    l_result = m_fingersBones[(int)FingerBone.RingProximal];
                    break;

                case HumanBodyBones.LeftRingIntermediate:
                case HumanBodyBones.RightRingIntermediate:
                    l_result = m_fingersBones[(int)FingerBone.RingIntermediate];
                    break;

                case HumanBodyBones.LeftRingDistal:
                case HumanBodyBones.RightRingDistal:
                    l_result = m_fingersBones[(int)FingerBone.RingDistal];
                    break;

                case HumanBodyBones.LeftLittleProximal:
                case HumanBodyBones.RightLittleProximal:
                    l_result = m_fingersBones[(int)FingerBone.PinkyProximal];
                    break;

                case HumanBodyBones.LeftLittleIntermediate:
                case HumanBodyBones.RightLittleIntermediate:
                    l_result = m_fingersBones[(int)FingerBone.PinkyIntermediate];
                    break;

                case HumanBodyBones.LeftLittleDistal:
                case HumanBodyBones.RightLittleDistal:
                    l_result = m_fingersBones[(int)FingerBone.PinkyDistal];
                    break;
            }
            return l_result;
        }

        public void SetMeshActive(bool p_state)
        {
            if(m_mesh != null)
                m_mesh.SetActive(p_state);
        }

        public bool IsLeft() => m_left;
    }
}
