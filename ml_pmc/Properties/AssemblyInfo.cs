using System.Reflection;

[assembly: AssemblyTitle("PlayerMovementCopycat")]
[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.0.0")]

[assembly: MelonLoader.MelonInfo(typeof(ml_pmc.PlayerMovementCopycat), "PlayerMovementCopycat", "1.0.0", "SDraw", "https://github.com/SDraw/ml_mods_cvr")]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPriority(3)]
[assembly: MelonLoader.MelonAdditionalDependencies("BTKUILib")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]