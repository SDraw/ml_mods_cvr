using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ml_pah
{
    static class HistoryManager
    {
        internal class EntriesUpdateEvent
        {
            event Action m_action;
            public void AddListener(Action p_listener) => m_action += p_listener;
            public void RemoveListener(Action p_listener) => m_action -= p_listener;
            public void Invoke() => m_action?.Invoke();
        }

        public static readonly EntriesUpdateEvent OnEntriesUpdated = new EntriesUpdateEvent();

        static bool ms_initialized = false;
        static string ms_historyPath;
        readonly static List<AvatarEntry> ms_avatarEntries = new List<AvatarEntry>();

        static int ms_lastTick = 0;

        // Init
        internal static void Initialize()
        {
            if(!ms_initialized)
            {
                ms_historyPath = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "PlayerAvatarHistory.json");

                try
                {
                    if(File.Exists(ms_historyPath))
                    {
                        string l_json = File.ReadAllText(ms_historyPath);
                        List<AvatarEntry> l_entries = JsonConvert.DeserializeObject<List<AvatarEntry>>(l_json);
                        if(l_entries != null)
                        {
                            ms_avatarEntries.AddRange(l_entries);
                            LimitEntries();
                            ms_avatarEntries.Sort((a, b) => a.m_lastUsageDate.CompareTo(b.m_lastUsageDate));
                        }
                    }
                }
                catch(Exception e)
                {
                    MelonLoader.MelonLogger.Error(e);
                }

                ms_lastTick = Environment.TickCount;
                Settings.OnAutosaveTimeChanged.AddListener(OnAutosaveTimeChanged);

                ms_initialized = true;
            }
        }

        internal static void Shutdown()
        {
            if(ms_initialized)
            {
                SaveHistory();

                Settings.OnAutosaveTimeChanged.RemoveListener(OnAutosaveTimeChanged);

                ms_initialized = false;
            }
        }

        // Update
        public static void Update()
        {
            if(ms_initialized && (Settings.AutosaveTime > 0))
            {
                int l_tick = Environment.TickCount;
                if((l_tick - ms_lastTick) >= (Settings.AutosaveTime * 60000))
                {
                    MelonLoader.MelonCoroutines.Start(AutosaveCoroutine());
                    ms_lastTick = l_tick;
                }
            }
        }

        // Entries
        internal static List<AvatarEntry> GetAvatarEntries() => ms_avatarEntries;

        internal static void AddEntry(string p_id)
        {
            if(ms_initialized)
            {
                int l_index = ms_avatarEntries.FindIndex(l_entry => l_entry.m_id == p_id);
                if(l_index != -1)
                {
                    ms_avatarEntries[l_index].m_lastUsageDate = DateTime.Now;

                    if(l_index != 0)
                    {
                        // Move in list
                        AvatarEntry l_entry = ms_avatarEntries[l_index];
                        ms_avatarEntries.RemoveAt(l_index);
                        ms_avatarEntries.Insert(0, l_entry);

                        OnEntriesUpdated?.Invoke();
                    }
                }
                else
                {
                    AvatarEntry l_entry = new AvatarEntry();
                    l_entry.m_id = p_id;
                    l_entry.m_name = "Loading ...";
                    l_entry.m_lastUsageDate = DateTime.Now;

                    MelonLoader.MelonCoroutines.Start(RequestAvatarInfo(l_entry));
                }
            }
        }

        // History
        internal static void ClearHistory() => ms_avatarEntries.Clear();

        internal static void SaveHistory()
        {
            if(ms_initialized)
            {
                try
                {
                    string l_json = JsonConvert.SerializeObject(ms_avatarEntries, Formatting.Indented);
                    File.WriteAllText(ms_historyPath, l_json);
                }
                catch(Exception e)
                {
                    MelonLoader.MelonLogger.Error(e);
                }
            }
        }

        static IEnumerator AutosaveCoroutine()
        {
            List<AvatarEntry> l_listCopy = new List<AvatarEntry>();
            l_listCopy.AddRange(ms_avatarEntries);

            Task l_task = Task.Run(() => AutosaveTask(l_listCopy));
            while(!l_task.IsCompleted)
                yield return null;
        }

        static async Task AutosaveTask(List<AvatarEntry> p_entries)
        {
            try
            {
                string l_json = JsonConvert.SerializeObject(p_entries, Formatting.Indented);
                File.WriteAllText(ms_historyPath, l_json);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }

            await Task.Delay(1);
        }

        // Network request
        static IEnumerator RequestAvatarInfo(AvatarEntry p_entry)
        {
            Task l_task = Task.Run(() => RequestAvatarInfoTask(p_entry));
            while(!l_task.IsCompleted)
                yield return null;

            ms_avatarEntries.Insert(0, p_entry);
            LimitEntries();
            OnEntriesUpdated?.Invoke();
        }

        static async Task RequestAvatarInfoTask(AvatarEntry p_entry)
        {
            BaseResponse<AvatarDetailsResponse> l_baseResponse = await ApiConnection.MakeRequest<AvatarDetailsResponse>(ApiConnection.ApiOperation.AvatarDetail, new { avatarID = p_entry.m_id });
            if(l_baseResponse != null)
            {
                if(!l_baseResponse.IsSuccessStatusCode) return;
                p_entry.m_name = l_baseResponse.Data.Name;
                p_entry.m_imageUrl = l_baseResponse.Data.ImageUrl;
                p_entry.m_cached = true;
            }
        }

        // Settings
        static void OnAutosaveTimeChanged(int p_value)
        {
            ms_lastTick = Environment.TickCount;
        }

        // Utility
        static void LimitEntries()
        {
            int l_currentLimit = Settings.AvatarsLimit;
            while(ms_avatarEntries.Count > l_currentLimit)
                ms_avatarEntries.RemoveAt(ms_avatarEntries.Count - 1);
        }
    }
}
