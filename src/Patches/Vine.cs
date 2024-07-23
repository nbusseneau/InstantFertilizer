using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using InstantFertilizer.Model;

namespace InstantFertilizer.Patches;

[HarmonyPatch(typeof(Vine))]
public class VinePatches
{
  [HarmonyTranspiler]
  [HarmonyPatch(nameof(Vine.Grow))]
  private static IEnumerable<CodeInstruction> SetWasVineFertilizedOnChildVines(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
        .MatchEndForward(
          new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZNetView), nameof(ZNetView.GetZDO))),
          new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ZDOVars), nameof(ZDOVars.s_plantTime))),
          new CodeMatch(OpCodes.Call),
          new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZNet), nameof(ZNet.GetTime))),
          new CodeMatch(OpCodes.Stloc_2),
          new CodeMatch(OpCodes.Ldloca_S),
          new CodeMatch(OpCodes.Call),
          new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZDO), nameof(ZDO.Set), [typeof(int), typeof(long)])))
        .ThrowIfInvalid("Could not inject WasVineFertilized status to child vine in Vine.Grow(...)")
        .Advance(1)
        // load existing Vine (arg 0) and new Vine (index 0) to be consumed as arguments to the delegate
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
        .InsertAndAdvance(Transpilers.EmitDelegate<Action<Vine, Vine>>((parentVine, childVine) =>
        {
          if (FertilizerManager.WasVineFertilized(parentVine)) FertilizerManager.SetWasVineFertilized(childVine, true);
        }))
        .InstructionEnumeration();
  }
}
