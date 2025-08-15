using System.Reflection;
using ml_vei.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_vei.ViveExtendedInput))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_vei.ViveExtendedInput))]

[assembly: MelonLoader.MelonInfo(
    typeof(ml_vei.ViveExtendedInput),
    nameof(ml_vei.ViveExtendedInput),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    "https://github.com/SDraw/ml_mods_cvr"
)]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace ml_vei.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.1.2";
    public const string Author = "SDraw";
}
