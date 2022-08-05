using ABI_RC.Core.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ml_aap
{
    public class AdditionalAvatarParameters : MelonLoader.MelonMod
    {
        static AdditionalAvatarParameters ms_instance = null;

        ParametersHandler m_localHandler = null;

        public override void OnApplicationStart()
        {
            ms_instance = this;

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.ClearAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AdditionalAvatarParameters).GetMethod(nameof(OnLocalAvatarClear_Postfix), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            );

            HarmonyInstance.Patch(
                typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.SetupAvatar)),
                null,
                new HarmonyLib.HarmonyMethod(typeof(AdditionalAvatarParameters).GetMethod(nameof(OnLocalAvatarSetup_Postfix), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            );

            MelonLoader.MelonCoroutines.Start(WaitForLocalPlayer());
        }

        System.Collections.IEnumerator WaitForLocalPlayer()
        {
            while(PlayerSetup.Instance == null)
                yield return null;

            m_localHandler = PlayerSetup.Instance.gameObject.AddComponent<ParametersHandler>();
        }

        static void OnLocalAvatarClear_Postfix() => ms_instance?.OnLocalAvatarClear();
        void OnLocalAvatarClear()
        {
            if(m_localHandler != null)
                m_localHandler.OnAvatarClear();
        }

        static void OnLocalAvatarSetup_Postfix() => ms_instance?.OnLocalAvatarSetup();
        void OnLocalAvatarSetup()
        {
            if(m_localHandler != null)
                m_localHandler.OnAvatarSetup();
        }
    }
}
