using UnityEngine;

namespace ml_lme
{
    static class LeapParser
    {
        readonly static Vector2[] ms_bendLimits =
        {
            new Vector2(0f, 90f),
            new Vector2(0f, 180f),
            new Vector2(0f, 180f),
            new Vector2(0f, 180f),
            new Vector2(0f, 180f)
        };

        public class HandData
        {
            public bool m_present = false;
            public Vector3 m_position = Vector3.zero;
            public Quaternion m_rotation = Quaternion.identity;
            public Vector3 m_elbowPosition = Vector3.zero;
            public readonly float[] m_normalizedCurls = null;
            public float m_grabStrength = 0f;
            public Vector3[] m_fingerPosition;
            public Quaternion[] m_fingerRotation;

            public HandData()
            {
                m_normalizedCurls = new float[5];
                m_fingerPosition = new Vector3[20];
                m_fingerRotation = new Quaternion[20];
            }

            public void Reset()
            {
                m_present = false;
                m_grabStrength = 0f;

                for(int i = 0; i < 5; i++)
                    m_normalizedCurls[i] = 0f;

                for(int i = 0; i < 20; i++)
                {
                    m_fingerPosition[i].Set(0f, 0f, 0f);
                    m_fingerRotation[i].Set(0f, 0f, 0f, 1f);
                }
            }
        }

        public class LeapData
        {
            public readonly HandData m_leftHand = null;
            public readonly HandData m_rightHand = null;

            public LeapData()
            {
                m_leftHand = new HandData();
                m_rightHand = new HandData();
            }

            public void Reset()
            {
                m_leftHand.Reset();
                m_rightHand.Reset();
            }
        }

        public static void ParseFrame(Leap.Frame p_frame, LeapData p_data)
        {
            p_data.Reset();

            // Fill hands data
            foreach(Leap.Hand l_hand in p_frame.Hands)
            {
                if(l_hand.IsLeft && !p_data.m_leftHand.m_present)
                    FillHandData(l_hand, p_data.m_leftHand);
                if(l_hand.IsRight && !p_data.m_rightHand.m_present)
                    FillHandData(l_hand, p_data.m_rightHand);
            }
        }

        static void FillHandData(Leap.Hand p_hand, HandData p_data)
        {
            // Unity's IK and FinalIK move hand bones to target, therefore - wrist
            p_data.m_present = true;
            p_data.m_position = p_hand.WristPosition;
            p_data.m_rotation = p_hand.Rotation;
            p_data.m_elbowPosition = p_hand.Arm.ElbowPosition;

            // Curls
            foreach(Leap.Finger l_finger in p_hand.Fingers)
            {
                Quaternion l_parentRot = Quaternion.identity;

                float l_angle = 0f;
                foreach(Leap.Bone l_bone in l_finger.bones)
                {
                    int l_index = (int)l_finger.Type * 4 + (int)l_bone.Type;
                    p_data.m_fingerPosition[l_index] = l_bone.PrevJoint;
                    p_data.m_fingerRotation[l_index] = l_bone.Rotation;

                    if(l_bone.Type == Leap.Bone.BoneType.TYPE_METACARPAL)
                    {
                        l_parentRot = l_bone.Rotation;
                        continue;
                    }

                    Quaternion l_localRot = Quaternion.Inverse(l_parentRot) * l_bone.Rotation;
                    float l_angleDiff = l_localRot.eulerAngles.x;
                    if(l_angleDiff > 180f)
                        l_angleDiff -= 360f;
                    l_angle += l_angleDiff;

                    l_parentRot = l_bone.Rotation;
                }

                p_data.m_normalizedCurls[(int)l_finger.Type] = Utils.InverseLerpUnclamped(ms_bendLimits[(int)l_finger.Type].x, ms_bendLimits[(int)l_finger.Type].y, l_angle);
            }

            p_data.m_grabStrength = Mathf.Clamp01((p_data.m_normalizedCurls[1] + p_data.m_normalizedCurls[2] + p_data.m_normalizedCurls[3] + p_data.m_normalizedCurls[4]) * 0.25f);
        }
    }
}
