using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using System;
using System.Reflection;

namespace ml_amt
{
    static class GameEvents
    {
        internal class GameEvent
        {
            event Action m_action;
            public void AddListener(Action p_listener) => m_action += p_listener;
            public void RemoveListener(Action p_listener) => m_action -= p_listener;
            public void Invoke() => m_action?.Invoke();
        }

        public static readonly GameEvent OnAvatarSetup = new GameEvent();
        public static readonly GameEvent OnAvatarClear = new GameEvent();
        public static readonly GameEvent OnAvatarReuse = new GameEvent();
        public static readonly GameEvent OnPlayspaceScale = new GameEvent();

        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            try
            {
                p_instance.Patch(
                    typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
                );

                p_instance.Patch(
                    typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(IKSystem).GetMethod(nameof(IKSystem.ReinitializeAvatar), BindingFlags.Instance | BindingFlags.Public),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnAvatarReinitialize_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(PlayerSetup).GetMethod("SetPlaySpaceScale", BindingFlags.NonPublic | BindingFlags.Instance),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnPlayspaceScale_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnAvatarClear_Postfix()
        {
            try
            {
                OnAvatarClear.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSetupAvatar_Postfix()
        {
            try
            {
                OnAvatarSetup.Invoke();
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }

        static void OnAvatarReinitialize_Postfix()
        {
            try
            {
                OnAvatarReuse.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnPlayspaceScale_Postfix()
        {
            try
            {
                OnPlayspaceScale.Invoke();
            }
            catch(Exception l_exception)
            {
                MelonLoader.MelonLogger.Error(l_exception);
            }
        }
    }
}
