using BTKUILib.UIObjects;
using BTKUILib.UIObjects.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ml_pmc
{
    static class ModUi
    {
        enum UiIndex
        {
            Toggle,
            Position,
            Rotation,
            Gestures,
            LookAtMix,
            MirrorPose,
            MirrorPosition,
            MirrorRotation,
            Reset
        }

        internal static Action<string> CopySwitch;

        static List<QMUIElement> ms_uiElements = null;
        static string ms_selectedPlayer;

        internal static void Init()
        {
            ms_uiElements = new List<QMUIElement>();

            BTKUILib.QuickMenuAPI.PrepareIcon("PlayerMovementCopycat", "PMC-Dancing", GetIconStream("dancing.png"));
            BTKUILib.QuickMenuAPI.PrepareIcon("PlayerMovementCopycat", "PMC-Dancing-On", GetIconStream("dancing_on.png"));

            var l_category = BTKUILib.QuickMenuAPI.PlayerSelectPage.AddCategory("Player Movement Copycat", "PlayerMovementCopycat");

            ms_uiElements.Add(l_category.AddButton("Copy movement", "PMC-Dancing", "Start/stop copy of player's movement"));
            (ms_uiElements[(int)UiIndex.Toggle] as Button).OnPress += OnCopySwitch;

            ms_uiElements.Add(l_category.AddToggle("Apply position", "Apply local position change of target player", Settings.Position));
            (ms_uiElements[(int)UiIndex.Position] as ToggleButton).OnValueUpdated += (value) => OnToggleUpdate(UiIndex.Position, value);

            ms_uiElements.Add(l_category.AddToggle("Apply rotation", "Apply local rotation change of target player", Settings.Rotation));
            (ms_uiElements[(int)UiIndex.Rotation] as ToggleButton).OnValueUpdated += (value) => OnToggleUpdate(UiIndex.Rotation, value);

            ms_uiElements.Add(l_category.AddToggle("Copy gestures", "Copy gestures of target player", Settings.Gestures));
            (ms_uiElements[(int)UiIndex.Gestures] as ToggleButton).OnValueUpdated += (value) => OnToggleUpdate(UiIndex.Gestures, value);

            ms_uiElements.Add(l_category.AddToggle("Apply LookAtIK", "Mix target player pose and camera view direction (desktop only)", Settings.LookAtMix));
            (ms_uiElements[(int)UiIndex.LookAtMix] as ToggleButton).OnValueUpdated += (value) => OnToggleUpdate(UiIndex.LookAtMix, value);

            ms_uiElements.Add(l_category.AddToggle("Mirror pose", "Mirror target player pose", Settings.MirrorPose));
            (ms_uiElements[(int)UiIndex.MirrorPose] as ToggleButton).OnValueUpdated += (value) => OnToggleUpdate(UiIndex.MirrorPose, value);

            ms_uiElements.Add(l_category.AddToggle("Mirror position", "Mirror target player movement against 0YZ plane", Settings.MirrorPosition));
            (ms_uiElements[(int)UiIndex.MirrorPosition] as ToggleButton).OnValueUpdated += (value) => OnToggleUpdate(UiIndex.MirrorPosition, value);

            ms_uiElements.Add(l_category.AddToggle("Mirror rotation", "Mirror target player rotation against 0YZ plane", Settings.MirrorRotation));
            (ms_uiElements[(int)UiIndex.MirrorRotation] as ToggleButton).OnValueUpdated += (value) => OnToggleUpdate(UiIndex.MirrorRotation, value);

            ms_uiElements.Add(l_category.AddButton("Reset settings", "", "Reset mod's settings to default"));
            (ms_uiElements[(int)UiIndex.Reset] as Button).OnPress += Reset;

            BTKUILib.QuickMenuAPI.OnPlayerSelected += (_, id) => ms_selectedPlayer = id;
            PoseCopycat.OnActivityChange += UpdateToggleColor;
        }

        static void OnCopySwitch() => CopySwitch?.Invoke(ms_selectedPlayer);

        static void OnToggleUpdate(UiIndex p_index, bool p_value, bool p_force = false)
        {
            switch(p_index)
            {
                case UiIndex.Position:
                    Settings.SetSetting(Settings.ModSetting.Position, p_value);
                    break;

                case UiIndex.Rotation:
                    Settings.SetSetting(Settings.ModSetting.Rotation, p_value);
                    break;

                case UiIndex.Gestures:
                    Settings.SetSetting(Settings.ModSetting.Gestures, p_value);
                    break;

                case UiIndex.LookAtMix:
                    Settings.SetSetting(Settings.ModSetting.LookAtMix, p_value);
                    break;

                case UiIndex.MirrorPose:
                    Settings.SetSetting(Settings.ModSetting.MirrorPose, p_value);
                    break;

                case UiIndex.MirrorPosition:
                    Settings.SetSetting(Settings.ModSetting.MirrorPosition, p_value);
                    break;

                case UiIndex.MirrorRotation:
                    Settings.SetSetting(Settings.ModSetting.MirrorRotation, p_value);
                    break;
            }

            if(p_force)
                (ms_uiElements[(int)p_index] as ToggleButton).ToggleValue = p_value;
        }

        static void Reset()
        {
            OnToggleUpdate(UiIndex.Position, true, true);
            OnToggleUpdate(UiIndex.Rotation, true, true);
            OnToggleUpdate(UiIndex.Gestures, true, true);
            OnToggleUpdate(UiIndex.LookAtMix, true, true);
            OnToggleUpdate(UiIndex.MirrorPose, false, true);
            OnToggleUpdate(UiIndex.MirrorPosition, false, true);
            OnToggleUpdate(UiIndex.MirrorRotation, false, true);
        }

        internal static void ShowAlert(string p_text) => BTKUILib.QuickMenuAPI.ShowAlertToast(p_text, 2);

        // Currently broken in BTKUILib, waiting for fix
        static void UpdateToggleColor(bool p_state)
        {
            //(ms_uiElements[(int)UiIndex.Toggle] as Button).ButtonIcon = (p_state ? "PMC-Dancing-On" : "PMC-Dancing");
            //(ms_uiElements[(int)UiIndex.Toggle] as Button).ButtonText = (p_state ? "PMC-Dancing-On" : "PMC-Dancing");
        }

        static Stream GetIconStream(string p_name)
        {
            Assembly l_assembly = Assembly.GetExecutingAssembly();
            string l_assemblyName = l_assembly.GetName().Name;
            return l_assembly.GetManifestResourceStream(l_assemblyName + ".resources." + p_name);
        }
    }
}
