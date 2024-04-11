using ABI_RC.Core.Util.AnimatorManager;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ml_amt
{
    class AvatarParameter
    {
        public enum ParameterType
        {
            Moving
        }

        readonly ParameterType m_type;
        readonly string m_name;
        readonly int m_hash = 0;
        readonly bool m_sync;
        readonly AnimatorControllerParameterType m_innerType;
        readonly AvatarAnimatorManager m_manager = null;

        public AvatarParameter(ParameterType p_type, AvatarAnimatorManager p_manager)
        {
            m_type = p_type;
            m_name = p_type.ToString();
            m_manager = p_manager;

            Regex l_regex = new Regex("^#?" + m_name + '$');
            foreach(var l_param in m_manager.Animator.parameters)
            {
                if(l_regex.IsMatch(l_param.name))
                {
                    m_hash = l_param.nameHash;
                    m_sync = !l_param.name.StartsWith('#');
                    m_innerType = l_param.type;
                    break;
                }
            }
        }

        public void Update(MotionTweaker p_tweaker)
        {
            switch(m_type)
            {
                case ParameterType.Moving:
                    SetBoolean(p_tweaker.GetMoving());
                    break;
            }
        }

        public bool IsValid() => (m_hash != 0);
        public ParameterType GetParameterType() => m_type;

        void SetFloat(float p_value)
        {
            if(m_innerType == AnimatorControllerParameterType.Float)
            {
                if(m_sync)
                    m_manager.SetParameter(m_name, p_value);
                else
                    m_manager.Animator.SetFloat(m_hash, p_value);
            }
        }

        void SetBoolean(bool p_value)
        {
            if(m_innerType == AnimatorControllerParameterType.Bool)
            {
                if(m_sync)
                    m_manager.SetParameter(m_name, p_value);
                else
                    m_manager.Animator.SetBool(m_hash, p_value);
            }
        }
    }
}
