using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using System;
using System.Reflection;
using UnityEngine;

namespace ml_pam
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
        internal class GameEvent<T1, T2>
        {
            event Action<T1, T2> m_action;
            public void AddListener(Action<T1, T2> p_listener) => m_action += p_listener;
            public void RemoveListener(Action<T1, T2> p_listener) => m_action -= p_listener;
            public void Invoke(T1 p_objA, T2 p_objB) => m_action?.Invoke(p_objA, p_objB);
        }

        public static readonly GameEvent OnAvatarSetup = new GameEvent();
        public static readonly GameEvent OnAvatarClear = new GameEvent();
        public static readonly GameEvent OnAvatarReuse = new GameEvent();
        public static readonly GameEvent<float> OnIKScaling = new GameEvent<float>();
        public static readonly GameEvent<CVRPickupObject, Vector3> OnPickupGrab = new GameEvent<CVRPickupObject, Vector3>();
        public static readonly GameEvent<CVRPickupObject> OnPickupDrop = new GameEvent<CVRPickupObject>();

        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            try
            {
                p_instance.Patch(
                    typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
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
                    typeof(PlayerSetup).GetMethod("SetupIKScaling", BindingFlags.NonPublic | BindingFlags.Instance),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnSetupIKScaling_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(CVRPickupObject).GetMethod("OnGrab", BindingFlags.Instance | BindingFlags.NonPublic),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnCVRPickupObjectGrab_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(CVRPickupObject).GetMethod("OnDrop", BindingFlags.Instance | BindingFlags.NonPublic),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnCVRPickupObjectDrop_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
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

        static void OnCVRPickupObjectGrab_Postfix(ref CVRPickupObject __instance, Vector3 __0)
        {
            try
            {
                OnPickupGrab.Invoke(__instance, __0);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnCVRPickupObjectDrop_Postfix(ref CVRPickupObject __instance)
        {
            try
            {
                OnPickupDrop.Invoke(__instance);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}
