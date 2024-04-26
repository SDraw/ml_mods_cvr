using System.Collections.Generic;
using UnityEngine;

namespace ml_bft
{
    class HandHandler
    {
        protected bool m_left = false;
        protected List<Transform> m_bones = null;
        protected List<Quaternion> m_localRotations = null;
        protected Transform m_prefabRoot = null;
        protected List<Renderer> m_renderers = null;

        protected HandHandler(bool p_left)
        {
            m_left = p_left;
            m_bones = new List<Transform>();
            m_localRotations = new List<Quaternion>();
            m_renderers = new List<Renderer>();

            Settings.OnShowHandsChanged.AddHandler(this.OnShowHandsChanged);
        }

        public virtual void Cleanup()
        {
            if(m_prefabRoot != null)
                Object.Destroy(m_prefabRoot.gameObject);
            m_prefabRoot = null;

            m_bones.Clear();
            m_localRotations.Clear();
            m_renderers.Clear();

            Settings.OnShowHandsChanged.RemoveHandler(this.OnShowHandsChanged);
        }

        public virtual void Update()
        {
        }

        public virtual Transform GetSourceForBone(HumanBodyBones p_bone)
        {
            return default;
        }

        public virtual void Rebind(Quaternion p_base)
        {
        }

        protected void OnShowHandsChanged(bool p_state)
        {
            foreach(var l_render in m_renderers)
            {
                if(l_render != null)
                    l_render.enabled = p_state;
            }
        }
    }
}
