using System.Reflection;
using ml_dht.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_dht.DesktopHeadTracking))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_dht.DesktopHeadTracking))]

[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace ml_dht.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.3.4";
    public const string Author = "SDraw";
}
