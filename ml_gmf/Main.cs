namespace ml_gmf
{
    public class GameMainFixes : MelonLoader.MelonMod
    {
        public override void OnInitializeMelon()
        {
            Fixes.ViveControls.Init(HarmonyInstance);
            Fixes.AvatarOverrides.Init(HarmonyInstance);
            Fixes.PostProccesVolumes.Init();
        }
    }
}
