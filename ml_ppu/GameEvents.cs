using ABI_RC.Core;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using System;
using System.Reflection;

namespace ml_ppu
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
        internal class GameEvent<T1>
        {
            event Action<T1> m_action;
            public void AddListener(Action<T1> p_listener) => m_action += p_listener;
            public void RemoveListener(Action<T1> p_listener) => m_action -= p_listener;
            public void Invoke(T1 p_obj) => m_action?.Invoke(p_obj);
        }

        public static readonly GameEvent<float> OnIKScaling = new GameEvent<float>();
        public static readonly GameEvent OnWorldPreSpawn = new GameEvent();
        public static readonly GameEvent<CVRSeat> OnSeatPreSit = new GameEvent<CVRSeat>();

        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            try
            {
                p_instance.Patch(
                    typeof(PlayerSetup).GetMethod("SetupIKScaling", BindingFlags.Instance | BindingFlags.NonPublic),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnSetupIKScaling_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(RootLogic).GetMethod(nameof(RootLogic.SpawnOnWorldInstance), BindingFlags.Instance | BindingFlags.Public),
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnWorldSpawn_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                    null
                );

                p_instance.Patch(
                    typeof(CVRSeat).GetMethod(nameof(CVRSeat.SitDown), BindingFlags.Instance | BindingFlags.Public),
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnCVRSeatSitDown_Prefix), BindingFlags.Static | BindingFlags.NonPublic)),
                    null
                );
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
    }
}
