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
    var canFertilize = FertilizerManager.CanFertilize(__instance);
    var canFertilizeVine = __instance.GetComponent<Vine>() is { } vine && FertilizerManager.CanFertilize(vine);
    if (!canFertilize && !canFertilizeVine) return;
    __result += FertilizerManager.GetFertilizeHoverText(__instance.m_nview);
  }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Pickable.SetPicked))]
  private static void PickableResetFertilizedWith(Pickable __instance, bool picked)
  {
    if (!picked || !__instance.m_nview || !__instance.m_nview.IsValid()) return;
    var wasActuallyPicked = __instance.m_nview.GetZDO().GetBool(ZDOVars.s_picked);
    if (!wasActuallyPicked) return;
    Plugin.Fertilizers.ForEach(fertilizer => FertilizerManager.SetWasFertilizedWith(fertilizer, __instance.m_nview, false));
  }
}
