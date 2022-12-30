using ABI_RC.Core.Player;

namespace ml_amt
{
    class AvatarParameter
    {
        public enum ParameterType
        {
            Upright,
            GroundedRaw,
            Moving
        }

        public enum ParameterSyncType
        {
            Synced,
            Local
        }

        public readonly ParameterType m_type;
        public readonly ParameterSyncType m_sync;
        public readonly string m_name;
        public readonly int m_hash; // For local only


        public AvatarParameter(ParameterType p_type, string p_name, ParameterSyncType p_sync = ParameterSyncType.Synced, int p_hash = 0)
        {
            m_type = p_type;
            m_sync = p_sync;
            m_name = p_name;
            m_hash = p_hash;
        }

        public void Update(MotionTweaker p_tweaker)
        {
            switch(m_type)
            {
                case ParameterType.Upright:
                    SetFloat(p_tweaker.GetUpright());
                    break;

                case ParameterType.GroundedRaw:
                    SetBoolean(p_tweaker.GetGroundedRaw());
                    break;

                case ParameterType.Moving:
                    SetBoolean(p_tweaker.GetMoving());
                    break;
            }
        }

        void SetFloat(float p_value)
        {
            switch(m_sync)
            {
                case ParameterSyncType.Local:
                    PlayerSetup.Instance._animator.SetFloat(m_hash, p_value);
                    break;
                case ParameterSyncType.Synced:
                    PlayerSetup.Instance.animatorManager.SetAnimatorParameterFloat(m_name, p_value);
                    break;
            }
        }

        void SetBoolean(bool p_value)
        {
            switch(m_sync)
            {
                case ParameterSyncType.Local:
                    PlayerSetup.Instance._animator.SetBool(m_hash, p_value);
                    break;
                case ParameterSyncType.Synced:
                    PlayerSetup.Instance.animatorManager.SetAnimatorParameterBool(m_name, p_value);
                    break;
            }
        }
    }
}
