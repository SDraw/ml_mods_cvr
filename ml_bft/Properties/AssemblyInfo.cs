using System.Reflection;
using ml_bft.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_bft.BetterFingersTracking))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_bft.BetterFingersTracking))]

[assembly: MelonLoader.MelonInfo(
    typeof(ml_bft.BetterFingersTracking),
    nameof(ml_bft.BetterFingersTracking),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    "https://github.com/SDraw/ml_mods_cvr"
)]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace ml_bft.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.1.3";
    public const string Author = "SDraw";
}
