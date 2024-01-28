namespace ml_dht
{
    class DataParser
    {
        MemoryMapReader m_mapReader = null;
        byte[] m_buffer = null;
        TrackingData m_trackingData;

        public DataParser()
        {
            m_buffer = new byte[1024];
            m_mapReader = new MemoryMapReader();
            m_mapReader.Open("head/data");
        }
        ~DataParser()
        {
            m_mapReader.Close();
            m_mapReader = null;
        }

        public void Update()
        {
            if(m_mapReader.Read(ref m_buffer))
                m_trackingData = TrackingData.ToObject(m_buffer);
        }

        public ref TrackingData GetLatestTrackingData() => ref m_trackingData;
    }
}
