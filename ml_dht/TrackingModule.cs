using ABI_RC.Systems.FaceTracking;
using System;
using UnityEngine;
using ViveSR.anipal.Lip;

namespace ml_dht
{
    class TrackingModule : ITrackingModule
    {
        bool m_registered = false;
        bool m_activeAsModule = false;
        MemoryMapReader m_mapReader = null;
        byte[] m_buffer = null;
        TrackingData m_trackingData;
        LipData_v2 m_lipData;

        public TrackingModule()
        {
            m_lipData = new LipData_v2();
            m_lipData.frame = 0;
            m_lipData.time = 0;
            m_lipData.image = IntPtr.Zero;
            m_lipData.prediction_data = new PredictionData_v2();
            m_lipData.prediction_data.blend_shape_weight = new float[(int)LipShape_v2.Max];

            m_buffer = new byte[1024];
            m_mapReader = new MemoryMapReader();
            m_mapReader.Open("head/data");
        }
        ~TrackingModule()
        {
            m_mapReader.Close();
            m_mapReader = null;
        }

        public (bool, bool) Initialize(bool useEye, bool useLip)
        {
            m_registered = true;
            m_activeAsModule = true;
            return (false, true);
        }

        public void Shutdown()
        {
            m_activeAsModule = false;
        }

        public bool IsEyeDataAvailable() => false;
        public bool IsLipDataAvailable() => true;

        internal void Update()
        {
            if(m_mapReader.Read(ref m_buffer))
            {
                m_trackingData = TrackingData.ToObject(m_buffer);

                float l_weight = Mathf.Clamp01(Mathf.InverseLerp(0.25f, 1f, Mathf.Abs(m_trackingData.m_mouthShape)));
                m_lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Jaw_Open] = m_trackingData.m_mouthOpen;
                m_lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Pout] = ((m_trackingData.m_mouthShape > 0f) ? l_weight : 0f);
                m_lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Smile_Left] = ((m_trackingData.m_mouthShape < 0f) ? l_weight : 0f);
                m_lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Smile_Right] = ((m_trackingData.m_mouthShape < 0f) ? l_weight : 0f);

                if(m_registered && m_activeAsModule && Settings.FaceTracking)
                    FaceTrackingManager.Instance.SubmitNewFacialData(m_lipData);
            }
        }

        internal ref TrackingData GetLatestTrackingData() => ref m_trackingData;
    }
}
