using System;
using System.IO;
using System.Reflection;
using BTKUILib.UIObjects;
using BTKUILib.UIObjects.Components;

namespace ml_ppu
{
    static class ModUi
    {
        enum UiIndex
        {
            Enabled = 0,
            FriendsOnly,
            VelocityMultiplier
        } 
        readonly static string ms_namespace = typeof(ModUi).Namespace;

        static Page ms_page = null;
        static Category ms_category = null;

        static ToggleButton ms_enabledToggle = null;
        static ToggleButton ms_friendsOnlyToggle = null;
        static SliderFloat ms_velocityMultiplierSlider = null;

        internal static void Init()
        {
            BTKUILib.QuickMenuAPI.PrepareIcon("PlayerPickUp", "PPU-Person", GetIconStream("person.png"));

            ms_page = new Page("PlayerPickUp", "MainPage", true, "PPU-Person");
            ms_page.MenuTitle = "Player Pick Up";
            ms_page.MenuSubtitle = "Let people pick you up and carry you around";

            ms_category = ms_page.AddCategory("Settings");

            ms_enabledToggle = ms_category.AddToggle("Enabled", "Set mod's activity as enabled or disabled", Settings.Enabled);
            ms_enabledToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.Enabled, state);

            ms_friendsOnlyToggle = ms_category.AddToggle("Friends only", "Allow only friends to pick you up", Settings.FriendsOnly);
            ms_friendsOnlyToggle.OnValueUpdated += (state) => OnToggleUpdate(UiIndex.FriendsOnly, state);

            ms_velocityMultiplierSlider = ms_category.AddSlider("Velocity multiplier", "Velocity multiplier upon drop", Settings.VelocityMultiplier, 0f, 50f);
            ms_velocityMultiplierSlider.OnValueUpdated += (value) => OnSliderUpdate(UiIndex.VelocityMultiplier, value);
        }

        static void OnToggleUpdate(UiIndex p_index, bool p_state)
        {
            try
            {
                switch(p_index)
                {
                    case UiIndex.Enabled:
                        Settings.SetSetting(Settings.ModSetting.Enabled, p_state);
                        break;

                    case UiIndex.FriendsOnly:
                        Settings.SetSetting(Settings.ModSetting.FriendsOnly, p_state);
                        break;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSliderUpdate(UiIndex p_index, float p_value)
        {
            try
            {
                switch(p_index)
                {
                    case UiIndex.VelocityMultiplier:
                        Settings.SetSetting(Settings.ModSetting.VelocityMultiplier, p_value);
                        break;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static Stream GetIconStream(string p_name) => Assembly.GetExecutingAssembly().GetManifestResourceStream(ms_namespace + ".resources." + p_name);
    }
}
