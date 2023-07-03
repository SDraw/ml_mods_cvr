using System.Reflection;

[assembly: AssemblyTitle("LeapMotionExtension")]
[assembly: AssemblyVersion("1.3.8")]
[assembly: AssemblyFileVersion("1.3.8")]

[assembly: MelonLoader.MelonInfo(typeof(ml_lme.LeapMotionExtension), "LeapMotionExtension", "1.3.8", "SDraw", "https://github.com/SDraw/ml_mods_cvr")]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonOptionalDependencies("ml_pmc")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
