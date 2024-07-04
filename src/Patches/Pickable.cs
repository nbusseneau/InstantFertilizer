using System;
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
    if (!__instance.m_nview.IsValid() || !__instance.m_picked || __instance.m_enabled == 0) return;
    var timeSincePicked = ZNet.instance.GetTime() - new DateTime(__instance.m_nview.GetZDO().GetLong(ZDOVars.s_pickedTime));
    var canFertilize = timeSincePicked.TotalMinutes <= __instance.m_respawnTimeMinutes;
    if (!canFertilize) return;
    __result += FertilizerManager.GetFertilizeHoverText(__instance.m_nview);
  }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Pickable.SetPicked))]
  private static void PickableResetFertilizedWith(Pickable __instance, bool picked)
  {
    if (!picked || !__instance.m_picked || __instance.m_enabled == 0 || !__instance.m_nview.IsValid()) return;
    Plugin.Fertilizers.ForEach(fertilizer => FertilizerManager.SetWasFertilizedWith(fertilizer, __instance.m_nview, false));
  }
}
