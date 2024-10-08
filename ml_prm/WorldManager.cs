﻿using ABI.CCK.Components;
using ABI_RC.Systems.GameEventSystem;
using System;
using UnityEngine;

namespace ml_prm
{
    static class WorldManager
    {
        static bool ms_safeWorld = true;
        static bool ms_restrictedWorld = false;
        static float ms_movementLimit = 1f;

        internal static void Init()
        {
            CVRGameEventSystem.World.OnLoad.AddListener(OnWorldLoad);
        }

        internal static void DeInit()
        {
            CVRGameEventSystem.World.OnLoad.RemoveListener(OnWorldLoad);
        }

        static void OnWorldLoad(string p_id)
        {
            try
            {
                ms_safeWorld = ((CVRWorld.Instance != null) && CVRWorld.Instance.allowFlying);
                ms_movementLimit = 1f;

                GameObject l_restrictObj = GameObject.Find("[RagdollRestriction]");
                ms_restrictedWorld = ((l_restrictObj != null) && (l_restrictObj.scene.name != "DontDestroyOnLoad"));

                if(CVRWorld.Instance != null)
                {
                    ms_movementLimit = CVRWorld.Instance.baseMovementSpeed;
                    ms_movementLimit *= CVRWorld.Instance.sprintMultiplier;
                    ms_movementLimit *= CVRWorld.Instance.inAirMovementMultiplier;
                    ms_movementLimit *= CVRWorld.Instance.flyMultiplier;
                }
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        public static bool IsSafeWorld() => ms_safeWorld;
        public static bool IsRestrictedWorld() => ms_restrictedWorld;
        public static float GetMovementLimit() => ms_movementLimit;
    }
}
