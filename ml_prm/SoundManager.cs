using ABI_RC.Core;
using ABI_RC.Core.Player;
using ABI_RC.Systems.ModNetwork;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ml_prm
{
    public class SoundManager
    {
        enum SoundType : int
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
        AudioSource m_audioSourceLocal = null;
        AudioSource m_audioSourceRemote = null;

        public SoundManager(Transform p_root)
        {
            Instance = this;

            m_clips = new AudioClip[(int)SoundType.Count];
            for(int i = 0; i < (int)SoundType.Count; i++)
                m_clips[i] = null;

            // Local
            GameObject l_audioSource = new GameObject("[ImpactSourceLocal]");
            l_audioSource.transform.parent = p_root;
            l_audioSource.transform.localPosition = Vector3.zero;
            l_audioSource.transform.localRotation = Quaternion.identity;

            m_audioSourceLocal = l_audioSource.AddComponent<AudioSource>();
            m_audioSourceLocal.playOnAwake = false;
            m_audioSourceLocal.loop = false;
            m_audioSourceLocal.minDistance = 2f;
            m_audioSourceLocal.maxDistance = 5f;
            m_audioSourceLocal.dopplerLevel = 0f;
            m_audioSourceLocal.panStereo = 0f;
            m_audioSourceLocal.spatialBlend = 0f; // 2D
            m_audioSourceLocal.spread = 0f;
            m_audioSourceLocal.rolloffMode = AudioRolloffMode.Linear;
            m_audioSourceLocal.outputAudioMixerGroup = RootLogic.Instance.mainSfx;

            // Remote
            l_audioSource = new GameObject("[ImpactSourceRemote]");
            l_audioSource.transform.parent = p_root;
            l_audioSource.transform.localPosition = Vector3.zero;
            l_audioSource.transform.localRotation = Quaternion.identity;

            m_audioSourceRemote = l_audioSource.AddComponent<AudioSource>();
            m_audioSourceRemote.playOnAwake = false;
            m_audioSourceRemote.loop = false;
            m_audioSourceRemote.minDistance = 2f;
            m_audioSourceRemote.maxDistance = 5f;
            m_audioSourceRemote.dopplerLevel = 0f;
            m_audioSourceRemote.panStereo = 0f;
            m_audioSourceRemote.spatialBlend = 1f; // 3D
            m_audioSourceRemote.spread = 0f;
            m_audioSourceRemote.rolloffMode = AudioRolloffMode.Linear;
            m_audioSourceRemote.outputAudioMixerGroup = RootLogic.Instance.mainSfx;

            // Network events
            ModNetworkManager.Subscribe(PlayerRagdollMod.ms_modGuid, this.OnNetworkMessage);
        }
        ~SoundManager()
        {
            if(Instance == this)
                Instance = null;

            ModNetworkManager.Unsubscribe(PlayerRagdollMod.ms_modGuid);

            if(m_audioSourceLocal != null)
                UnityEngine.Object.Destroy(m_audioSourceLocal);
            m_audioSourceLocal = null;
        }

        internal void LoadSounds()
        {
            if(m_loaded)
                return;

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

        public void PlayLocalSound(ImpactType p_type)
        {
            if(!m_loaded)
                return;

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

            if(m_clips[l_index] != null)
                m_audioSourceLocal.PlayOneShot(m_clips[l_index], Settings.ImpactVolume);

            if(Settings.ImpactSync)
            {
                ModNetworkMessage l_message = new ModNetworkMessage(PlayerRagdollMod.ms_modGuid);
                l_message.Write(l_index);
                l_message.Send();
            }
        }

        public void PlayRemoteSound(int p_soundIndex, Vector3 p_pos)
        {
            if(!m_loaded)
                return;

            p_soundIndex = Mathf.Clamp(p_soundIndex, (int)SoundType.ImpactHard1, (int)SoundType.ImpactSoft7);
            if(m_clips[p_soundIndex] != null)
            {
                m_audioSourceRemote.transform.position = p_pos;
                m_audioSourceRemote.PlayOneShot(m_clips[p_soundIndex], Settings.ImpactVolume);
            }
        }

        private void OnNetworkMessage(ModNetworkMessage p_message)
        {
            if(!m_loaded || !Settings.ImpactSounds || !Settings.ImpactSync)
                return;

            try
            {
                if(CVRPlayerManager.Instance.UserIdToPlayerEntity.TryGetValue(p_message.Sender, out var l_player))
                {
                    if(l_player.PlayerObject != null)
                    {
                        p_message.Read(out int l_index);
                        PlayRemoteSound(l_index, l_player.PlayerObject.transform.position);
                    }
                }
            }
            catch(System.Exception _) { }
        }
    }
}
