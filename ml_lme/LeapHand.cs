using UnityEngine;

namespace ml_lme
{
    class LeapHand
    {
        public enum FingerBone
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
            PinkyDistal
        };

        readonly Transform m_root = null;
        readonly Transform m_wrist = null;
        readonly GameObject m_mesh = null;
        readonly Transform[] m_fingersBones = null;
        readonly Quaternion[] m_initialRotations = null;

        public LeapHand(Transform p_root, bool p_left)
        {
            m_fingersBones = new Transform[20];
            m_initialRotations = new Quaternion[20];

            m_root = p_root;
            if(m_root != null)
            {
                m_mesh = m_root.Find(p_left ? "GenericHandL" : "GenericHandR")?.gameObject;
                m_wrist = m_root.Find(p_left ? "LeftHand/Wrist" : "RightHand/Wrist");
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
                        //m_fingers[i].position = p_data.m_fingerPosition[i];
                        m_fingersBones[i].rotation = p_data.m_fingerRotation[i];
                    }
                }

                m_wrist.localPosition = Vector3.zero;
                m_wrist.localRotation = Quaternion.identity;
            }
        }

        public void Reset()
        {
            if(m_wrist != null)
            {
                m_wrist.localPosition = Vector3.zero;
                m_wrist.localRotation = Quaternion.identity;
            }

            for(int i = 0; i < 20; i++)
            {
                if(m_fingersBones[i] != null)
                    m_fingersBones[i].localRotation = m_initialRotations[i];
            }
        }

        public Transform GetRoot() => m_root;
        public Transform GetWrist() => m_wrist;
        public Transform GetFingersBone(FingerBone p_bone) => m_fingersBones[(int)p_bone];

        public void SetMeshActive(bool p_state)
        {
            if(m_mesh != null)
                m_mesh.SetActive(p_state);
        }
    }
}
