using ABI_RC.Core.EventSystem;
using ABI_RC.Core.InteractionSystem;
using BTKUILib.UIObjects;
using BTKUILib.UIObjects.Components;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ml_pah
{
    static class ModUi
    {
        readonly static string ms_namespace = typeof(ModUi).Namespace;
        static bool ms_initialized = false;

        static Page ms_page = null;

        static Category ms_settingsCategory = null;
        static Button ms_settingsClearButton = null;
        static Button ms_settingSaveButton = null;
        static SliderFloat ms_settingsEntriesLimit = null;
        static SliderFloat ms_settingsAutosaveTime = null;

        static Category ms_buttonsCategory = null;
        static readonly List<Button> ms_avatarButtons = new List<Button>();

        // Init
        internal static void Initialize()
        {
            if(!ms_initialized)
            {
                BTKUILib.QuickMenuAPI.PrepareIcon("PlayerAvatarHistory", "guardian", GetIconStream("guardian.png"));
                BTKUILib.QuickMenuAPI.PrepareIcon("PlayerAvatarHistory", "delete", GetIconStream("delete.png"));
                BTKUILib.QuickMenuAPI.PrepareIcon("PlayerAvatarHistory", "save", GetIconStream("save.png"));

                ms_page = new Page("PlayerAvatarHistory", "MainPage", true, "guardian");
                ms_page.MenuTitle = "Player Avatar History";
                ms_page.MenuSubtitle = "List of last used avatars";

                ms_settingsCategory = ms_page.AddCategory("Settings");

                ms_settingsClearButton = ms_settingsCategory.AddButton("Clear history", "delete", "Clear current history");
                ms_settingsClearButton.OnPress += ClearHistory;

                ms_settingSaveButton = ms_settingsCategory.AddButton("Save history", "save", "Manually save current history");
                ms_settingSaveButton.OnPress += SaveHistory;

                ms_settingsEntriesLimit = ms_settingsCategory.AddSlider("History limit", "Number of saved avatar history entries.<p>Warning: Large value can impact performance.", Settings.AvatarsLimit, 10f, 100f, 0);
                ms_settingsEntriesLimit.OnValueUpdated += OnAvatarLimitChange;

                ms_settingsAutosaveTime = ms_settingsCategory.AddSlider("Autosave period", "Automatic history saving in minutes.", Settings.AutosaveTime, 0f, 60f, 0);
                ms_settingsAutosaveTime.OnValueUpdated += OnAutosaveTimeChange;

                ms_buttonsCategory = ms_page.AddCategory("Avatars");
                RegenerateAvatarButtons();

                ms_initialized = true;
            }
        }

        internal static void Shutdown()
        {
            if(ms_initialized)
            {
                ms_page = null;

                ms_buttonsCategory = null;
                ms_avatarButtons.Clear();

                ms_settingsCategory = null;
                ms_settingSaveButton = null;
                ms_settingsClearButton = null;

                ms_initialized = false;
            }
        }


        // History
        static void ClearHistory()
        {
            BTKUILib.QuickMenuAPI.ShowConfirm(
                "Clear history", "Are you sure want to clear all avatar history?",
                () =>
                {
                    HistoryManager.ClearHistory();
                    RegenerateAvatarButtons();
                }
            );
        }

        static void SaveHistory() => HistoryManager.SaveHistory();

        // Update
        public static void UpdateAvatarsList()
        {
            if(ms_initialized)
                RegenerateAvatarButtons();
        }

        // Settings
        static void OnAvatarLimitChange(float p_value) => Settings.SetSetting(Settings.ModSetting.AvatarsLimit, (int)p_value);
        static void OnAutosaveTimeChange(float p_value) => Settings.SetSetting(Settings.ModSetting.AutosaveTime, (int)p_value);

        // Utility
        static void RegenerateAvatarButtons()
        {
            if(ms_avatarButtons.Count > 0)
            {
                foreach(Button l_button in ms_avatarButtons)
                    l_button.Delete();
                ms_avatarButtons.Clear();
            }

            foreach(AvatarEntry l_entry in HistoryManager.GetAvatarEntries())
            {
                Button l_button = ms_buttonsCategory.AddButton("", "", "", ButtonStyle.FullSizeImage);
                l_button.ButtonText = (l_entry.m_cached ? l_entry.m_name : "Loading ...");
                l_button.ButtonIcon = (l_entry.m_cached ? l_entry.m_imageUrl : "");
                l_button.ButtonTooltip = string.Format("Click to open avatar page, hold to switch avatar.<p>Last used time: {0}", l_entry.m_lastUsageDate.ToString("g"));

                l_button.OnPress += () => ViewManager.Instance.RequestAvatarDetailsPage(l_entry.m_id);
                l_button.OnHeld += () => AssetManagement.Instance.LoadLocalAvatarFromNetwork(l_entry.m_id);

                ms_avatarButtons.Add(l_button);
            }
        }

        static Stream GetIconStream(string p_name) => Assembly.GetExecutingAssembly().GetManifestResourceStream(ms_namespace + ".resources." + p_name);
    }
}
