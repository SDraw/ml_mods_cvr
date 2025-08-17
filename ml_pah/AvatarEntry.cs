using System;

namespace ml_pah
{
    [Serializable]
    class AvatarEntry
    {
        public string m_id;
        public string m_name;
        public string m_imageUrl;
        public DateTime m_lastUsageDate;
        public bool m_cached = false;
    }
}
