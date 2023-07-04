using UnityEngine;

namespace ml_lme
{
    class VisualHand
    {
        Transform m_root = null;
        Transform m_wrist = null;
        Transform[] m_fingers = null;

        public VisualHand(Transform p_root, bool p_left)
        {
            m_root = p_root;

            if(m_root != null)
            {
                m_wrist = m_root.Find(p_left ? "LeftHand/Wrist" : "RightHand/Wrist");
                if(m_wrist != null)
                {
                    m_fingers = new Transform[20];

                    m_fingers[0] = null; // Actual thumb-meta, look at Leap Motion docs, dummy
                    m_fingers[1] = m_wrist.Find("thumb_meta");
                    m_fingers[2] = m_wrist.Find("thumb_meta/thumb_a");
                    m_fingers[3] = m_wrist.Find("thumb_meta/thumb_a/thumb_b");

                    m_fingers[4] = m_wrist.Find("index_meta");
                    m_fingers[5] = m_wrist.Find("index_meta/index_a");
                    m_fingers[6] = m_wrist.Find("index_meta/index_a/index_b");
                    m_fingers[7] = m_wrist.Find("index_meta/index_a/index_b/index_c");

                    m_fingers[8] = m_wrist.Find("middle_meta");
                    m_fingers[9] = m_wrist.Find("middle_meta/middle_a");
                    m_fingers[10] = m_wrist.Find("middle_meta/middle_a/middle_b");
                    m_fingers[11] = m_wrist.Find("middle_meta/middle_a/middle_b/middle_c");

                    m_fingers[12] = m_wrist.Find("ring_meta");
                    m_fingers[13] = m_wrist.Find("ring_meta/ring_a");
                    m_fingers[14] = m_wrist.Find("ring_meta/ring_a/ring_b");
                    m_fingers[15] = m_wrist.Find("ring_meta/ring_a/ring_b/ring_c");

                    m_fingers[16] = m_wrist.Find("pinky_meta");
                    m_fingers[17] = m_wrist.Find("pinky_meta/pinky_a");
                    m_fingers[18] = m_wrist.Find("pinky_meta/pinky_a/pinky_b");
                    m_fingers[19] = m_wrist.Find("pinky_meta/pinky_a/pinky_b/pinky_c");
                }
            }
        }

        public void Update(GestureMatcher.HandData p_data)
        {
            if(m_wrist != null)
            {
                m_wrist.position = p_data.m_position;
                m_wrist.rotation = p_data.m_rotation;

                for(int i = 0; i < 20; i++)
                {
                    if(m_fingers[i] != null)
                    {
                        //m_fingers[i].position = p_data.m_fingerPosition[i];
                        m_fingers[i].rotation = p_data.m_fingerRotation[i];
                    }
                }

                m_wrist.localPosition = p_data.m_position;
                m_wrist.localRotation = p_data.m_rotation;
            }
        }
    }
}
