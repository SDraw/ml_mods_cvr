using System.Reflection;
using ml_pmc.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_pmc.PlayerMovementCopycat))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_pmc.PlayerMovementCopycat))]

[assembly: MelonLoader.MelonInfo(
    typeof(ml_pmc.PlayerMovementCopycat),
    nameof(ml_pmc.PlayerMovementCopycat),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    "https://github.com/SDraw/ml_mods_cvr"
)]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPriority(3)]
[assembly: MelonLoader.MelonAdditionalDependencies("BTKUILib")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace ml_pmc.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.1.2";
    public const string Author = "SDraw";
}
