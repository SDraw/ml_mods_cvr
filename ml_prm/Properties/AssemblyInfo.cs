using System.Reflection;

[assembly: AssemblyTitle("PlayerRagdollMod")]
[assembly: AssemblyVersion("1.0.5")]
[assembly: AssemblyFileVersion("1.0.5")]

[assembly: MelonLoader.MelonInfo(typeof(ml_prm.PlayerRagdollMod), "PlayerRagdollMod", "1.0.5", "SDraw", "https://github.com/SDraw/ml_mods_cvr")]
[assembly: MelonLoader.MelonGame(null, "ChilloutVR")]
[assembly: MelonLoader.MelonPriority(2)]
[assembly: MelonLoader.MelonOptionalDependencies("BTKUILib")]
[assembly: MelonLoader.MelonPlatform(MelonLoader.MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.MONO)]