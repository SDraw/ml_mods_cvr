using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using ABI_RC.Systems.InputManagement;
using System;
using System.Reflection;
using UnityEngine;

namespace ml_bft
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
        internal class GameEvent<T1, T2>
        {
            event Action<T1, T2> m_action;
            public void AddListener(Action<T1, T2> p_listener) => m_action += p_listener;
            public void RemoveListener(Action<T1, T2> p_listener) => m_action -= p_listener;
            public void Invoke(T1 p_objA, T2 p_objB) => m_action?.Invoke(p_objA, p_objB);
        }

        public static readonly GameEvent OnAvatarReuse = new GameEvent();
        public static readonly GameEvent OnInputUpdate = new GameEvent();
        public static readonly GameEvent<HumanPoseHandler, Transform> OnIKSystemLateUpdate = new GameEvent<HumanPoseHandler, Transform>();

        internal static void Init(HarmonyLib.Harmony p_instance)
        {
            try
            {
                p_instance.Patch(
                    typeof(IKSystem).GetMethod(nameof(IKSystem.ReinitializeAvatar), BindingFlags.Instance | BindingFlags.Public),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnAvatarReinitialize_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(CVRInputManager).GetMethod("UpdateInput", BindingFlags.Instance | BindingFlags.NonPublic),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnInputUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(IKSystem).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnIKSystemLateUpdate_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
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

        static void OnInputUpdate_Postfix()
        {
            try
            {
                OnInputUpdate.Invoke();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnIKSystemLateUpdate_Postfix(HumanPoseHandler ____humanPoseHandler, Transform ____hipTransform)
        {
            try
            {
                OnIKSystemLateUpdate.Invoke(____humanPoseHandler, ____hipTransform);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}
