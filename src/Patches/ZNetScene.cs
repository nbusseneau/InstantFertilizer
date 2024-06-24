using HarmonyLib;
using InstantFertilizer.Model;

namespace InstantFertilizer.Patches;

[HarmonyPatch(typeof(ObjectDB))]
public class ZNetScenePatches
{
  /// <summary>
  /// Reset keys on game start in case the user switched to another character with different keys.
  /// </summary>
  [HarmonyPostfix]
  [HarmonyPatch(nameof(ObjectDB.Awake))]
  [HarmonyPatch(nameof(ObjectDB.CopyOtherDB))]
  private static void ClearCachedGlobalKeys() => FertilizerManager.ClearCachedGlobalKeys();
}
