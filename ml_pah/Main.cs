using ABI.CCK.Components;
using ABI_RC.Core;
using System;
using System.Collections;
using ABI_RC.Systems.GameEventSystem;

namespace ml_pah
{
    public class PlayerAvatarHistory : MelonLoader.MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.Init();
            HistoryManager.Initialize();
        }

        public override void OnLateInitializeMelon()
        {
            ModUi.Initialize();
            MelonLoader.MelonCoroutines.Start(WaitForRootLogic());
        }

        IEnumerator WaitForRootLogic()
        {
            while(RootLogic.Instance == null)
                yield return null;

            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(this.OnLocalAvatarLoad);
            HistoryManager.OnEntriesUpdated.AddListener(this.OnHistoryEntriesUpdated);
        }

        public override void OnDeinitializeMelon()
        {
            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.RemoveListener(this.OnLocalAvatarLoad);
            HistoryManager.OnEntriesUpdated.RemoveListener(this.OnHistoryEntriesUpdated);

            ModUi.Shutdown();
            HistoryManager.Shutdown();
        }

        public override void OnUpdate()
        {
            HistoryManager.Update();
        }

        // Game events
        void OnLocalAvatarLoad(CVRAvatar p_avatar)
        {
            try
            {
                if((p_avatar.AssetInfo != null) && (p_avatar.AssetInfo.objectId.Length > 0))
                    HistoryManager.AddEntry(p_avatar.AssetInfo.objectId);
            }
            catch(Exception e)
            {
                MelonLoader.MelonLogger.Error(e);
            }
        }

        // Mod events
        void OnHistoryEntriesUpdated() => ModUi.UpdateAvatarsList();
    }
}
