using HarmonyLib;
using InstantFertilizer.Model;

namespace InstantFertilizer.Patches;

[HarmonyPatch(typeof(Plant))]
public class PlantPatches
{
  [HarmonyPostfix]
  [HarmonyPatch(nameof(Plant.GetHoverText))]
  private static void PlantGetFertilizeHoverText(Plant __instance, ref string __result)
  {
    if (__instance.m_status != Plant.Status.Healthy) return;
    __result += FertilizerManager.GetFertilizeHoverText();
  }
}
