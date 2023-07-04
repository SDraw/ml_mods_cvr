using UnityEngine;

namespace ml_lme
{
    static class GestureMatcher
    {
        readonly static Vector2[] ms_fingerLimits =
        {
            new Vector2(-50f, 0f),
            new Vector2(-20f, 30f),
            new Vector2(-15f, 15f),
            new Vector2(-10f, 20f),
            new Vector2(-10f, 25f)
        };

        public class HandData
        {
            public bool m_present = false;
            public Vector3 m_position = Vector3.zero;
            public Quaternion m_rotation = Quaternion.identity;
            public Vector3 m_elbowPosition = Vector3.zero;
            public readonly float[] m_spreads = null;
            public readonly float[] m_bends = null;
            public float m_grabStrength = 0f;
            public Vector3[] m_fingerPosition;
            public Quaternion[] m_fingerRotation;

            public HandData()
            {
                m_spreads = new float[5];
                m_bends = new float[5];
                m_fingerPosition = new Vector3[20];
                m_fingerRotation = new Quaternion[20];
            }

            public void Reset()
            {
                m_present = false;
                for(int i = 0; i < 5; i++)
                {
                    m_bends[i] = 0f;
                    m_spreads[i] = 0f;
                }
                for(int i = 0; i < 20; i++)
                {
                    m_fingerPosition[i].Set(0f, 0f, 0f);
                    m_fingerRotation[i].Set(0f, 0f, 0f, 1f);
                }
                m_grabStrength = 0f;
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

        public static void GetFrameData(Leap.Frame p_frame, LeapData p_data)
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

            // Bends
            foreach(Leap.Finger l_finger in p_hand.Fingers)
            {
                Quaternion l_prevSegment = Quaternion.identity;

                float l_angle = 0f;
                foreach(Leap.Bone l_bone in l_finger.bones)
                {
                    p_data.m_fingerPosition[(int)l_finger.Type * 4 + (int)l_bone.Type] = l_bone.PrevJoint;
                    p_data.m_fingerRotation[(int)l_finger.Type * 4 + (int)l_bone.Type] = l_bone.Rotation;

                    if(l_bone.Type == Leap.Bone.BoneType.TYPE_METACARPAL)
                    {
                        l_prevSegment = l_bone.Rotation;
                        continue;
                    }

                    Quaternion l_diff = Quaternion.Inverse(l_prevSegment) * l_bone.Rotation;
                    l_prevSegment = l_bone.Rotation;

                    float l_angleDiff = l_diff.eulerAngles.x;
                    if(l_angleDiff > 180f)
                        l_angleDiff -= 360f;
                    l_angle += l_angleDiff;
                }

                p_data.m_bends[(int)l_finger.Type] = Utils.InverseLerpUnclamped(0f, (l_finger.Type == Leap.Finger.FingerType.TYPE_THUMB) ? 90f : 180f, l_angle);
            }

            // Spreads
            foreach(Leap.Finger l_finger in p_hand.Fingers)
            {
                Leap.Bone l_parent = l_finger.Bone(Leap.Bone.BoneType.TYPE_METACARPAL);
                Leap.Bone l_child = l_finger.Bone(Leap.Bone.BoneType.TYPE_PROXIMAL);
                Quaternion l_diff = Quaternion.Inverse(l_parent.Rotation) * l_child.Rotation;

                // Spread - local Y rotation, but thumb is obnoxious
                float l_angle = 360f - l_diff.eulerAngles.y;
                if(l_angle > 180f)
                    l_angle -= 360f;

                // Pain
                if(p_hand.IsRight)
                    l_angle *= -1f;

                if(l_finger.Type != Leap.Finger.FingerType.TYPE_THUMB)
                {
                    if(l_angle < 0f)
                        p_data.m_spreads[(int)l_finger.Type] = 0.5f * Utils.InverseLerpUnclamped(ms_fingerLimits[(int)l_finger.Type].x, 0f, l_angle);
                    else
                        p_data.m_spreads[(int)l_finger.Type] = 0.5f + 0.5f * Utils.InverseLerpUnclamped(0f, ms_fingerLimits[(int)l_finger.Type].y, l_angle);
                }
                else
                    p_data.m_spreads[(int)l_finger.Type] = Utils.InverseLerpUnclamped(ms_fingerLimits[(int)l_finger.Type].x, ms_fingerLimits[(int)l_finger.Type].y, l_angle);
            }

            p_data.m_grabStrength = Mathf.Clamp01((p_data.m_bends[1] + p_data.m_bends[2] + p_data.m_bends[3] + p_data.m_bends[4]) * 0.25f);
        }
    }
}
