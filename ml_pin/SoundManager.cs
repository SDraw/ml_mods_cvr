using ABI_RC.Core.AudioEffects;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ml_pin
{
    class SoundManager
    {
        public enum SoundType
        {
            PlayerJoin = 0,
            PlayerLeave,
            FriendJoin,
            FriendLeave
        }

        const string c_modName = "PlayersInstanceNotifier";

        bool m_loaded = false;
        readonly AudioClip[] m_clips = null;

        internal SoundManager()
        {
            m_clips = new AudioClip[4];
            for(int i = 0; i < 4; i++)
                m_clips[i] = null;
        }
        public void LoadSounds()
        {
            if(!m_loaded)
            {
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.PlayerJoin, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "player_join.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.PlayerLeave, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "player_leave.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.FriendJoin, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "friend_join.wav")));
                MelonLoader.MelonCoroutines.Start(LoadAudioClip(SoundType.FriendLeave, Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, c_modName, "friend_leave.wav")));

                m_loaded = true;
            }
        }

        IEnumerator LoadAudioClip(SoundType p_type, string p_path)
        {
            using UnityWebRequest l_uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + p_path, AudioType.WAV);
            ((DownloadHandlerAudioClip)l_uwr.downloadHandler).streamAudio = true;
            yield return l_uwr.SendWebRequest();

            if(l_uwr.isNetworkError || l_uwr.isHttpError)
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

        public void PlaySound(SoundType p_type)
        {
            if(m_loaded && (m_clips[(int)p_type] != null))
                InterfaceAudio.Instance.UserInterfaceAudio.PlayOneShot(m_clips[(int)p_type], Settings.Volume);
        }
    }
}
