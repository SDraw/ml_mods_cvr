﻿using ABI_RC.Core.Util.AnimatorManager;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ml_prm
{
    class AvatarBoolParameter
    {
        public readonly string m_name;
        public readonly int m_hash = 0;
        public readonly bool m_sync;
        readonly AvatarAnimatorManager m_manager = null;

        public AvatarBoolParameter(string p_name, AvatarAnimatorManager p_manager)
        {
            m_name = p_name;
            m_manager = p_manager;

            Regex l_regex = new Regex("^#?" + p_name + '$');
            foreach(var l_param in m_manager.Animator.parameters)
            {
                if(l_regex.IsMatch(l_param.name) && (l_param.type == AnimatorControllerParameterType.Bool))
                {
                    m_name = l_param.name;
                    m_sync = !l_param.name.StartsWith('#');
                    m_hash = l_param.nameHash;
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
                    m_manager.Animator.SetBool(m_hash, p_value);
            }
        }
    }
}
