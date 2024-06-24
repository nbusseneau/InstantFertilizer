using HarmonyLib;
using InstantFertilizer.Model;

namespace InstantFertilizer.Patches;

[HarmonyPatch(typeof(Pickable))]
public class PickablePatches
{
  [HarmonyPostfix]
  [HarmonyPatch(nameof(Pickable.GetHoverText))]
  private static void PickableGetFertilizeHoverText(Pickable __instance, ref string __result)
  {
    if (!__instance.m_picked || __instance.m_enabled == 0) return;
    __result += FertilizerManager.GetFertilizeHoverText();
  }
}
