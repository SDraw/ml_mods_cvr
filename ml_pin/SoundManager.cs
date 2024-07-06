using ABI_RC.Core.AudioEffects;
using System;
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
        int[] m_clipDelays = null;
        int[] m_playTicks = null;

        internal SoundManager()
        {
            m_clips = new AudioClip[4];
            for(int i = 0; i < 4; i++)
                m_clips[i] = null;

            m_clipDelays = new int[4];
            m_clipDelays[(int)SoundType.PlayerJoin] = 708;
            m_clipDelays[(int)SoundType.PlayerLeave] = 380;
            m_clipDelays[(int)SoundType.FriendJoin] = 708;
            m_clipDelays[(int)SoundType.FriendLeave] = 380;

            m_playTicks = new int[4];
            for(int i = 0; i < 4; i++)
                m_playTicks[i] = 0;
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
            m_clipDelays[(int)p_type] = (int)(l_clip.length * 1000f);
        }

        public void PlaySound(SoundType p_type)
        {
            if(m_loaded && (m_clips[(int)p_type] != null))
            {
                if(Settings.Delay)
                {
                    int l_tick = Environment.TickCount;
                    if(l_tick - m_playTicks[(int)p_type] > m_clipDelays[(int)p_type])
                    {
                        m_playTicks[(int)p_type] = l_tick;
                        InterfaceAudio.Instance.UserInterfaceAudio.PlayOneShot(m_clips[(int)p_type], Settings.Volume);
                    }
                }
                else
                {
                    m_playTicks[(int)p_type] = Environment.TickCount;
                    InterfaceAudio.Instance.UserInterfaceAudio.PlayOneShot(m_clips[(int)p_type], Settings.Volume);
                }
            }
        }
    }
}
