using System.Reflection;
using ml_vet.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_vet.ViveEyeTracking))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_vet.ViveEyeTracking))]

[assembly: MelonLoader.MelonInfo(
    typeof(ml_vet.ViveEyeTracking),
    nameof(ml_vet.ViveEyeTracking),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    "https://github.com/SDraw/ml_mods_cvr"
)]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace ml_vet.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.0.2";
    public const string Author = "SDraw";
}
