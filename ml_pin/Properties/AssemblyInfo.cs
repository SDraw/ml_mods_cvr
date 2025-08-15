using System.Reflection;
using ml_pin.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_pin.PlayersInstanceNotifier))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_pin.PlayersInstanceNotifier))]

[assembly: MelonLoader.MelonInfo(
    typeof(ml_pin.PlayersInstanceNotifier),
    nameof(ml_pin.PlayersInstanceNotifier),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    "https://github.com/SDraw/ml_mods_cvr"
)]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace ml_pin.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.1.2";
    public const string Author = "SDraw";
}
