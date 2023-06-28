using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ml_amt.Fixes
{
    class AnimatorAnalyzer
    {
        bool m_enabled = true;
        List<AnimatorControllerParameter> m_parameters = null;

        public void AnalyzeFrom(Animator p_animator)
        {
            m_enabled = p_animator.enabled;
            m_parameters = p_animator.parameters?.ToList();

            if(m_parameters != null)
            {
                foreach(var l_param in m_parameters)
                {
                    switch(l_param.type)
                    {
                        case AnimatorControllerParameterType.Bool:
                        case AnimatorControllerParameterType.Trigger:
                            l_param.defaultBool = p_animator.GetBool(l_param.nameHash);
                            break;
                        case AnimatorControllerParameterType.Float:
                            l_param.defaultFloat = p_animator.GetFloat(l_param.nameHash);
                            break;
                        case AnimatorControllerParameterType.Int:
                            l_param.defaultInt = p_animator.GetInteger(l_param.nameHash);
                            break;

                    }
                }
            }
        }

        public void ApplyTo(Animator p_animator)
        {
            p_animator.enabled = m_enabled;

            if(m_parameters != null)
            {
                foreach(var l_param in m_parameters)
                {
                    switch(l_param.type)
                    {
                        case AnimatorControllerParameterType.Bool:
                            p_animator.SetBool(l_param.nameHash, l_param.defaultBool);
                            break;
                        case AnimatorControllerParameterType.Float:
                            p_animator.SetFloat(l_param.nameHash, l_param.defaultFloat);
                            break;
                        case AnimatorControllerParameterType.Int:
                            p_animator.SetInteger(l_param.nameHash, l_param.defaultInt);
                            break;
                        case AnimatorControllerParameterType.Trigger:
                        {
                            if(l_param.defaultBool)
                                p_animator.SetTrigger(l_param.nameHash);
                        }
                        break;
                    }
                }
            }
        }

        public bool IsEnabled() => m_enabled;
    }
}
