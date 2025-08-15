using System.Reflection;
using ml_lme.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(ml_lme.LeapMotionExtension))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(ml_lme.LeapMotionExtension))]

[assembly: MelonLoader.MelonInfo(
    typeof(ml_lme.LeapMotionExtension),
    nameof(ml_lme.LeapMotionExtension),
    AssemblyInfoParams.Version,
    AssemblyInfoParams.Author,
    "https://github.com/SDraw/ml_mods_cvr"
)]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
[assembly: MelonLoader.MelonAdditionalCredits("NotAKidOnSteam")]

namespace ml_lme.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "1.6.2";
    public const string Author = "SDraw";
}
