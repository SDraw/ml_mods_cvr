using System.Reflection;
using ml_ppu.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_ppu.PlayerPickUp))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_ppu.PlayerPickUp))]

[assembly: MelonLoader.MelonInfo(
    typeof(ml_ppu.PlayerPickUp),
    nameof(ml_ppu.PlayerPickUp),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    "https://github.com/SDraw/ml_mods_cvr"
)]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonOptionalDependencies("PlayerRagdollMod")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace ml_ppu.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.0.1";
    public const string Author = "SDraw";
}
