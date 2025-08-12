using ABI_RC.Core.Util.AnimatorManager;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ml_prm
{
    class AvatarParameter
    {
        public readonly string m_name;
        public readonly int m_hash = 0;
        public readonly bool m_sync;
        public readonly AnimatorControllerParameterType m_type;
        readonly AvatarAnimatorManager m_manager = null;

        public AvatarParameter(string p_name, AvatarAnimatorManager p_manager)
        {
            m_name = p_name;
            m_manager = p_manager;

            Regex l_regex = new Regex("^#?" + p_name + '$');
            foreach(var l_param in m_manager.Animator.parameters)
            {
                if(l_regex.IsMatch(l_param.name))
                {
                    m_name = l_param.name;
                    m_sync = !l_param.name.StartsWith('#');
                    m_hash = l_param.nameHash;
                    m_type = l_param.type;
                    break;
                }
            }
        }

        public void SetValue(bool p_value)
        {
            if(m_hash != 0)
            {
                if(m_sync)
                    m_manager.SetParameter(m_name, p_value);
                else
                {
                    switch(m_type)
                    {
                        case AnimatorControllerParameterType.Bool:
                        case AnimatorControllerParameterType.Trigger:
                            m_manager.Animator.SetBool(m_hash, p_value);
                            break;
                        case AnimatorControllerParameterType.Int:
                            m_manager.Animator.SetInteger(m_hash, p_value ? 1 : 0);
                            break;
                        case AnimatorControllerParameterType.Float:
                            m_manager.Animator.SetFloat(m_hash, p_value ? 1f : 0f);
                            break;
                    }
                }
            }
        }

        public void SetValue(int p_value)
        {
            if(m_hash != 0)
            {
                if(m_sync)
                    m_manager.SetParameter(m_name, p_value);
                else
                {
                    switch(m_type)
                    {
                        case AnimatorControllerParameterType.Bool:
                        case AnimatorControllerParameterType.Trigger:
                            m_manager.Animator.SetBool(m_hash, p_value > 0);
                            break;
                        case AnimatorControllerParameterType.Int:
                            m_manager.Animator.SetInteger(m_hash, p_value);
                            break;
                        case AnimatorControllerParameterType.Float:
                            m_manager.Animator.SetFloat(m_hash, p_value);
                            break;
                    }
                }
            }
        }

        public void SetValue(float p_value)
        {
            if(m_hash != 0)
            {
                if(m_sync)
                    m_manager.SetParameter(m_name, p_value);
                else
                {
                    switch(m_type)
                    {
                        case AnimatorControllerParameterType.Bool:
                        case AnimatorControllerParameterType.Trigger:
                            m_manager.Animator.SetBool(m_hash, p_value > 0f);
                            break;
                        case AnimatorControllerParameterType.Int:
                            m_manager.Animator.SetInteger(m_hash, (int)p_value);
                            break;
                        case AnimatorControllerParameterType.Float:
                            m_manager.Animator.SetFloat(m_hash, p_value);
                            break;
                    }
                }
            }
        }
    }
}
