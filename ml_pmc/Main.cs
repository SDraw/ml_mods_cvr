using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using ABI_RC.Systems.MovementSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ml_pmc
{
    public class PlayerMovementCopycat : MelonLoader.MelonMod
    {
        static PlayerMovementCopycat ms_instance = null;

        PoseCopycat m_localCopycat = null;

        public override void OnInitializeMelon()
        {
            if(ms_instance == null)
                ms_instance = this;

            Settings.Init();
            ModUi.Init();

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(PlayerMovementCopycat).GetMethod(nameof(OnAvatarClear_Postfix), BindingFlags.NonPublic | BindingFlags.Static))
            );
            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(PlayerMovementCopycat).GetMethod(nameof(OnSetupAvatar_Postfix), BindingFlags.Static | BindingFlags.NonPublic))
            );

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        public override void OnDeinitializeMelon()
        {
            if(ms_instance == this)
                ms_instance = null;

            m_localCopycat = null;
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localCopycat = PlayerSetup.Instance.gameObject.AddComponent<PoseCopycat>();
            ModUi.CopySwitch += this.OnTargetSelect;
        }

        void OnTargetSelect(string p_id)
        {
            if(m_localCopycat != null)
            {
                if(m_localCopycat.IsActive())
                    m_localCopycat.SetTarget(null);
                else
                {
                    if(Friends.FriendsWith(p_id))
                    {
                        if(CVRPlayerManager.Instance.GetPlayerPuppetMaster(p_id, out PuppetMaster l_puppetMaster))
                        {
                            if(IsInSight(MovementSystem.Instance.proxyCollider, l_puppetMaster.GetComponent<CapsuleCollider>(), Utils.GetWorldMovementLimit()))
                                m_localCopycat.SetTarget(l_puppetMaster.gameObject);
                            else
                                ModUi.ShowAlert("Selected player is too far away or obstructed");
                        }
                        else
                            ModUi.ShowAlert("Selected player isn't connected or ready yet");
                    }
                    else
                        ModUi.ShowAlert("Selected player isn't your friend");
                }
            }
        }

        static bool IsInSight(CapsuleCollider p_source, CapsuleCollider p_target, float p_limit)
        {
            bool l_result = false;
            if((p_source != null) && (p_target != null))
            {
                Ray l_ray = new Ray();
                l_ray.origin = (p_source.transform.position + p_source.transform.rotation * p_source.center);
                l_ray.direction = (p_target.transform.position + p_target.transform.rotation * p_target.center) - l_ray.origin;
                List<RaycastHit> l_hits = Physics.RaycastAll(l_ray, p_limit, LayerMask.NameToLayer("UI Internal")).ToList();
                if(l_hits.Count > 0)
                {
                    l_hits.Sort((a, b) => ((a.distance < b.distance) ? -1 : 1));
                    l_result = (l_hits.First().collider.gameObject.transform.root == p_target.transform.root);
                }
            }
            return l_result;
        }

        // Patches
        static void OnAvatarClear_Postfix() => ms_instance?.OnAvatarClear();
        void OnAvatarClear()
        {
            try
            {
                if(m_localCopycat != null)
                    m_localCopycat.OnAvatarClear();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        static void OnSetupAvatar_Postfix() => ms_instance?.OnSetupAvatar();
        void OnSetupAvatar()
        {
            try
            {
                if(m_localCopycat != null)
                    m_localCopycat.OnAvatarSetup();
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }
    }
}
