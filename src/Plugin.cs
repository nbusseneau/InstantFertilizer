using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using InstantFertilizer.Model;

namespace InstantFertilizer;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
[NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Minor)]
public class Plugin : BaseUnityPlugin
{
  internal const string ModGUID = "nbusseneau.InstantFertilizer";
  private const string ModName = "InstantFertilizer";
  private const string ModVersion = "0.1.1";

  internal static new ManualLogSource Logger { get; private set; }

  private static readonly List<Fertilizer> s_defaultFertilizers = [
    new("$item_ancientseed", 3, "defeated_eikthyr"),
    new("$item_ymirremains", 1, "defeated_gdking"),
  ];
  private static ConfigEntry<string> s_fertilizers;
  private static List<Fertilizer> ParseFertilizers(string serializedFertilizers) => serializedFertilizers.Split(',').Select(Fertilizer.FromString).Where(f => f is not null).ToList();
  public static List<Fertilizer> Fertilizers { get; private set; }

  private static ConfigEntry<int> s_fertilizePercentage;
  public static float FertilizePercentage => s_fertilizePercentage.Value / 100f;

  public void Awake()
  {
    Logger = base.Logger;

    ConfigurationManagerAttributes isAdminOnly = new() { IsAdminOnly = true };
    var fertilizersConfigDescription = @$"Comma-separated list of fertilizers that may be used.
Fertilizer format: {Fertilizer.SerializedFormat}
See https://valheim.fandom.com/wiki/Global_Keys for quick reference of available global keys, or use `{FertilizerManager.GlobalKeyIgnore}` if you do not want to gate fertilizing behind a global key.
Note that the mod is not able to determine in advance if an item or global key actually exists. If a fertilizer appears to be ignored, double check item names and global keys.";
    s_fertilizers = Config.Bind("Behaviour", "Fertilizer list", s_defaultFertilizers.Join(), new ConfigDescription(fertilizersConfigDescription, tags: isAdminOnly));
    Fertilizers = ParseFertilizers(s_fertilizers.Value);
    s_fertilizers.SettingChanged += (_, _) => Fertilizers = ParseFertilizers(s_fertilizers.Value);
    var fertilizePercentageConfigDescription = @"Reduce remaining time by this amount when fertilizing (in percentage of total growing / respawning time).
Default value of 100%: grow / respawn instantaneously.
A single plant / pickable can be fertilized multiple times, but not more than once with the same fertilizer.";
    AcceptableValueRange<int> fertilizePercentageAcceptableValues = new(1, 100);
    s_fertilizePercentage = Config.Bind("Behaviour", "Fertilize percentage", 100, new ConfigDescription(fertilizePercentageConfigDescription, fertilizePercentageAcceptableValues, tags: isAdminOnly));
    SetUpConfigWatcher();

    var assembly = Assembly.GetExecutingAssembly();
    Harmony harmony = new(ModGUID);
    harmony.PatchAll(assembly);
  }

  public void OnDestroy() => Config.Save();

  private void SetUpConfigWatcher()
  {
    FileSystemWatcher watcher = new(BepInEx.Paths.ConfigPath, Path.GetFileName(Config.ConfigFilePath));
    watcher.Changed += ReadConfigValues;
    watcher.Created += ReadConfigValues;
    watcher.Renamed += ReadConfigValues;
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }

  private void ReadConfigValues(object sender, FileSystemEventArgs e)
  {
    if (!File.Exists(Config.ConfigFilePath)) return;
    try
    {
      Logger.LogDebug("Attempting to reload configuration...");
      Config.Reload();
    }
    catch
    {
      Logger.LogError($"There was an issue loading {Config.ConfigFilePath}");
    }
  }
}
