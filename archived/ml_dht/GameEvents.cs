using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Core.Player.EyeMovement;
using ABI_RC.Systems.IK;
using System;
using System.Reflection;

namespace ml_dht
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
        internal class GameEvent<T1, T2>
        {
            event Action<T1, T2> m_action;
            public void AddListener(Action<T1, T2> p_listener) => m_action += p_listener;
            public void RemoveListener(Action<T1, T2> p_listener) => m_action -= p_listener;
            public void Invoke(T1 p_objA, T2 p_objB) => m_action?.Invoke(p_objA, p_objB);
        }

        public static readonly GameEvent OnAvatarReuse = new GameEvent();
        public static readonly GameEvent<EyeMovementController> OnEyeControllerUpdate = new GameEvent<EyeMovementController>();
        public static readonly GameEvent<CVRFaceTracking, EventResult> OnFaceTrackingUpdate = new GameEvent<CVRFaceTracking, EventResult>();

        static readonly EventResult ms_result = new EventResult();

        internal static void InitA(HarmonyLib.Harmony p_instance)
        {
            try
            {
                p_instance.Patch(
                    typeof(IKSystem).GetMethod(nameof(IKSystem.ReinitializeAvatar), BindingFlags.Instance | BindingFlags.Public),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnAvatarReinitialize_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
        internal static void InitB(HarmonyLib.Harmony p_instance)
        {
            try
            {
                p_instance.Patch(
                    typeof(EyeMovementController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnEyeControllerUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(CVRFaceTracking).GetMethod("UpdateLocalData", BindingFlags.Instance | BindingFlags.NonPublic),
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnFaceTrackingLocalUpdate_Prefix), BindingFlags.Static | BindingFlags.NonPublic))
                );
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
                OnAvatarReuse.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnEyeControllerUpdate_Postfix(ref EyeMovementController __instance)
        {
            try
            {
                OnEyeControllerUpdate.Invoke(__instance);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static bool OnFaceTrackingLocalUpdate_Prefix(ref CVRFaceTracking __instance)
        {
            try
            {
                ms_result.m_result = false;
                OnFaceTrackingUpdate.Invoke(__instance, ms_result);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
            return !ms_result.m_result;
        }
    }
}
