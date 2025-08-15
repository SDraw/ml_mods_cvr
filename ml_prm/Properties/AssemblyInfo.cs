using System.Reflection;
using ml_prm.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_prm.PlayerRagdollMod))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_prm.PlayerRagdollMod))]

[assembly: MelonLoader.MelonInfo(
    typeof(ml_prm.PlayerRagdollMod),
    nameof(ml_prm.PlayerRagdollMod),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    "https://github.com/SDraw/ml_mods_cvr"
)]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPriority(2)]
[assembly: MelonLoader.MelonAdditionalDependencies("BTKUILib")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
[assembly: MelonLoader.MelonAdditionalCredits("kafeijao, NotAKidOnSteam")]

namespace ml_prm.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.2.4";
    public const string Author = "SDraw";
}
