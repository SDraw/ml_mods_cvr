using ABI_RC.Core;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ml_prm
{
    public class SoundManager
    {
        enum SoundType
        {
            ImpactHard1 = 0,
            ImpactHard2,
            ImpactHard3,
            ImpactHard4,
            ImpactHard5,
            ImpactHard6,
            ImpactSoft1,
            ImpactSoft2,
            ImpactSoft3,
            ImpactSoft4,
            ImpactSoft5,
            ImpactSoft6,
            ImpactSoft7,

            Count
        }
        public enum ImpactType
        {
            Hard = 0,
            Soft
        }

        const string c_modName = "PlayerRagdollMod";

        public static SoundManager Instance { get; private set; } = null;

        bool m_loaded = false;
        readonly AudioClip[] m_clips = null;
        AudioSource m_audioSource = null;

        public SoundManager(Transform p_root)
        {
            m_clips = new AudioClip[(int)SoundType.Count];
            for(int i = 0; i < (int)SoundType.Count; i++)
                m_clips[i] = null;

            GameObject l_audioSource = new GameObject("[ImpactSource]");
            l_audioSource.transform.parent = p_root;
            l_audioSource.transform.localPosition = Vector3.zero;
            l_audioSource.transform.localRotation = Quaternion.identity;

            m_audioSource = l_audioSource.AddComponent<AudioSource>();
            m_audioSource.playOnAwake = false;
            m_audioSource.loop = false;
            m_audioSource.minDistance = 2f;
            m_audioSource.maxDistance = 5f;
            m_audioSource.dopplerLevel = 0f;
            m_audioSource.panStereo = 0f;
            m_audioSource.spatialBlend = 0f; // 2D
            m_audioSource.spread = 0f;
            m_audioSource.rolloffMode = AudioRolloffMode.Linear;
            m_audioSource.outputAudioMixerGroup = RootLogic.Instance.mainSfx;

            Instance = this;

        }
        ~SoundManager()
        {
            if(Instance == this)
                Instance = null;

            if(m_audioSource != null)
                Object.Destroy(m_audioSource);
            m_audioSource = null;
        }

        internal void LoadSounds()
        {
            if(!m_loaded)
            {
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactHard1, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_hard1.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactHard2, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_hard2.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactHard3, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_hard3.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactHard4, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_hard4.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactHard5, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_hard5.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactHard6, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_hard6.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactSoft1, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_soft1.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactSoft2, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_soft2.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactSoft3, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_soft3.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactSoft4, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_soft4.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactSoft5, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_soft5.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactSoft6, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_soft6.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.ImpactSoft7, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "body_medium_impact_soft7.wav")));
                m_loaded = true;
            }
        }

        IEnumerator LoadAudioClip(SoundType p_type, string p_path)
        {
            using UnityWebRequest l_uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + p_path, AudioType.WAV);
            ((DownloadHandlerAudioClip)l_uwr.downloadHandler).streamAudio = true;
            yield return l_uwr.SendWebRequest();

            if((l_uwr.result == UnityWebRequest.Result.ConnectionError) || (l_uwr.result == UnityWebRequest.Result.ProtocolError))
            {
                MelonLoader.MelonLogger.Warning(l_uwr.error);
                yield break;
            }

            AudioClip l_content;
            AudioClip l_clip = (l_content = DownloadHandlerAudioClip.GetContent(l_uwr));
            yield return l_content;
            if(!l_uwr.isDone || (l_clip == null))
                yield break;

            m_clips[(int)p_type] = l_clip;
        }

        public void PlaySound(ImpactType p_type)
        {
            if(m_loaded)
            {
                int l_index = -1;
                switch(p_type)
                {
                    case ImpactType.Hard:
                        l_index = (int)SoundType.ImpactHard1 + Random.Range(0, 6);
                        break;
                    case ImpactType.Soft:
                        l_index = (int)SoundType.ImpactSoft1 + Random.Range(0, 7);
                        break;
                }

                if((l_index != -1) && (m_clips[l_index] != null))
                    m_audioSource.PlayOneShot(m_clips[l_index], Settings.ImpactVolume);
            }
        }
    }
}
