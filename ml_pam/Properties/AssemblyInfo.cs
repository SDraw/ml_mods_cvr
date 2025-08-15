using System.Reflection;
using ml_pam.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_pam.PickupArmMovement))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_pam.PickupArmMovement))]

[assembly: MelonLoader.MelonInfo(
    typeof(ml_pam.PickupArmMovement),
    nameof(ml_pam.PickupArmMovement),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    "https://github.com/SDraw/ml_mods_cvr"
)]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPriority(1)]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]

namespace ml_pam.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.2.3";
    public const string Author = "SDraw";
}
