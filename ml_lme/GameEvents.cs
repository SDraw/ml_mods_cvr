﻿using ABI.CCK.Components;
using ABI_RC.Core.Player;
using ABI_RC.Systems.IK;
using System;
using System.Reflection;

namespace ml_lme
{
    static class GameEvents
    {
        internal class GameEvent
        {
            event Action m_action;
            public void AddHandler(Action p_listener) => m_action += p_listener;
            public void RemoveHandler(Action p_listener) => m_action -= p_listener;
            public void Invoke() => m_action?.Invoke();
        }
        internal class GameEvent<T1>
        {
            event Action<T1> m_action;
            public void AddHandler(Action<T1> p_listener) => m_action += p_listener;
            public void RemoveHandler(Action<T1> p_listener) => m_action -= p_listener;
            public void Invoke(T1 p_obj) => m_action?.Invoke(p_obj);
        }

        public static readonly GameEvent OnAvatarSetup = new GameEvent();
        public static readonly GameEvent OnAvatarClear = new GameEvent();
        public static readonly GameEvent OnAvatarReuse = new GameEvent();
        public static readonly GameEvent<float> OnRayScale = new GameEvent<float>();
        public static readonly GameEvent<float> OnPlayspaceScale = new GameEvent<float>();
        public static readonly GameEvent<CVRPickupObject> OnPickupGrab = new GameEvent<CVRPickupObject>();

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
                    typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetControllerRayScale)),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnRayScale_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(PlayerSetup).GetMethod("SetPlaySpaceScale", BindingFlags.NonPublic | BindingFlags.Instance),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnPlayspaceScale_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
                );

                p_instance.Patch(
                    typeof(CVRPickupObject).GetMethod(nameof(CVRPickupObject.Grab), BindingFlags.Public | BindingFlags.Instance),
                    null,
                    new HarmonyLib.HarmonyMethod(typeof(GameEvents).GetMethod(nameof(OnPickupGrab_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
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

        static void OnRayScale_Postfix(float __0)
        {
            try
            {
                OnRayScale.Invoke(__0);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnPlayspaceScale_Postfix(float ____avatarScaleRelation)
        {
            try
            {
                OnPlayspaceScale.Invoke(____avatarScaleRelation);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnPickupGrab_Postfix(ref CVRPickupObject __instance)
        {
            try
            {
                OnPickupGrab.Invoke(__instance);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}
