using ABI.CCK.Components;
using ABI_RC.Core;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.IK.SubSystems;
using ABI_RC.Systems.Movement;
using System;
using System.Linq;
using System.Reflection;

namespace ml_prm
{
    static class GameEvents
    {
        internal class EventResult
        {
            public bool m_result = false;
        }
        internal class GameEvent
        {
            event Action m_action;
            public void AddListener(Action p_listener) => m_action += p_listener;
            public void RemoveListener(Action p_listener) => m_action -= p_listener;
            public void Invoke() => m_action?.Invoke();
        }
        internal class GameEvent<T1>
        {
            event Action<T1> m_action;
            public void AddListener(Action<T1> p_listener) => m_action += p_listener;
            public void RemoveListener(Action<T1> p_listener) => m_action -= p_listener;
            public void Invoke(T1 p_obj) => m_action?.Invoke(p_obj);
        }

        public static readonly GameEvent OnAvatarSetup = new GameEvent();
        public static readonly GameEvent OnAvatarClear = new GameEvent();
        public static readonly GameEvent OnAvatarPreReuse = new GameEvent();
        public static readonly GameEvent OnAvatarPostReuse = new GameEvent();
        public static readonly GameEvent<float> OnIKScaling = new GameEvent<float>();
        public static readonly GameEvent<CVRSeat> OnSeatPreSit = new GameEvent<CVRSeat>();
        public static readonly GameEvent OnCalibrationStart = new GameEvent();
        public static readonly GameEvent OnWorldPreSpawn = new GameEvent();
        public static readonly GameEvent OnCombatPreDown = new GameEvent();
        public static readonly GameEvent OnFlightChange = new GameEvent();
        public static readonly GameEvent<EventResult> OnIKOffsetUpdate = new GameEvent<EventResult>();

        static readonly EventResult ms_result = new EventResult();

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
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnAvatarReinitialize_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnAvatarReinitialize_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(PlayerSetup).GetMethod("SetupIKScaling", BindingFlags.NonPublic | BindingFlags.Instance),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnSetupIKScaling_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(CVRSeat).GetMethod(nameof(CVRSeat.SitDown)),
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnCVRSeatSitDown_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                    null
                );

                p_instance.Patch(
                    typeof(BodySystem).GetMethod(nameof(BodySystem.StartCalibration)),
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnStartCalibration_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                    null
                );

                p_instance.Patch(
                    typeof(RootLogic).GetMethod(nameof(RootLogic.SpawnOnWorldInstance)),
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnWorldSpawn_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                    null
                );

                p_instance.Patch(
                    typeof(CombatSystem).GetMethods().First(m => (!m.IsGenericMethod && m.Name == nameof(CombatSystem.Down))),
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnCombatDown_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                    null
                );

                p_instance.Patch(
                    typeof(BetterBetterCharacterController).GetMethod(nameof(BetterBetterCharacterController.ChangeFlight)),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnChangeFlight_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(IKSystem).GetMethod("OnPreSolverUpdateActiveOffset", BindingFlags.Instance | BindingFlags.NonPublic),
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnOffsetUpdate_Prefix), BindingFlags.Static | BindingFlags.NonPublic))
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
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnAvatarReinitialize_Prefix()
        {
            try
            {
                OnAvatarPreReuse.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnAvatarReinitialize_Postfix()
        {
            try
            {
                OnAvatarPostReuse.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSetupIKScaling_Postfix(ref UnityEngine.Vector3 ___scaleDifference)
        {
            try
            {
                OnIKScaling.Invoke(1f + ___scaleDifference.y);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnCVRSeatSitDown_Prefix(ref CVRSeat __instance)
        {
            try
            {
                OnSeatPreSit.Invoke(__instance);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnStartCalibration_Prefix()
        {
            try
            {
                OnCalibrationStart.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnWorldSpawn_Prefix()
        {
            try
            {
                OnWorldPreSpawn.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnCombatDown_Prefix()
        {
            try
            {
                OnCombatPreDown.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnChangeFlight_Postfix()
        {
            try
            {
                OnFlightChange.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static bool OnOffsetUpdate_Prefix()
        {
            try
            {
                ms_result.m_result = false;
                OnIKOffsetUpdate.Invoke(ms_result);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
            return !ms_result.m_result;
        }

    }
}
