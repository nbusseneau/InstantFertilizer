using HarmonyLib;
using InstantFertilizer.Model;
using UnityEngine;

namespace InstantFertilizer.Patches;

[HarmonyPatch(typeof(Player))]
public class PlayerPatches
{
  [HarmonyPrefix]
  [HarmonyPatch(nameof(Player.Interact))]
  private static void Fertilize(Player __instance, GameObject go, bool hold, ref bool __runOriginal)
  {
    // copy vanilla safeguards, except we ignore any hold interaction instead of checking time since last interact to
    // avoid fertilizing pickables multiple times in a row
    if (__instance.InAttack() || __instance.InDodge() || hold) return;
    var wasAnythingFertilized = false;
    if (go.GetComponentInParent<Plant>() is { } plant && FertilizerManager.CanFertilize(plant)) wasAnythingFertilized = FertilizerManager.TryFertilize(__instance, plant);
    else if (go.GetComponentInParent<Vine>() is { } vine && FertilizerManager.CanFertilize(vine)) wasAnythingFertilized = FertilizerManager.TryFertilize(__instance, vine);
    else if (go.GetComponentInParent<Pickable>() is { } pickable && FertilizerManager.CanFertilize(pickable)) wasAnythingFertilized = FertilizerManager.TryFertilize(__instance, pickable);
    if (wasAnythingFertilized) __runOriginal = false;
  }
}
