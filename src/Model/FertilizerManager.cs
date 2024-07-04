using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InstantFertilizer.Model;

public static class FertilizerManager
{
  public const string GlobalKeyIgnore = "none";
  private static readonly Dictionary<string, bool> s_cachedGlobalKeys = [];
  public static void ClearCachedGlobalKeys() => s_cachedGlobalKeys.Clear();
  private static bool HasGlobalKey(string globalKey)
  {
    if (globalKey == GlobalKeyIgnore) return true; // special case for those crazy enough not to gate fertilizing behind global keys
    if (s_cachedGlobalKeys.TryGetValue(globalKey, out var hasKey)) return hasKey;
    hasKey = ZoneSystem.instance.GetGlobalKey(globalKey);
    if (hasKey) s_cachedGlobalKeys[globalKey] = true;
    return hasKey;
  }

  private static readonly string WasFertilizedWithZDOKey = $"{Plugin.ModGUID}.WasFertilizedWith";
  public static void SetWasFertilizedWith(Fertilizer fertilizer, ZNetView nview, bool value) => nview.GetZDO().Set($"{WasFertilizedWithZDOKey}.{fertilizer.ItemName}", value);
  private static bool WasFertilizedWith(Fertilizer fertilizer, ZNetView nview) => nview.GetZDO().GetBool($"{WasFertilizedWithZDOKey}.{fertilizer.ItemName}");

  public static string GetFertilizeHoverText(ZNetView nview)
  {
    var fertilizeHoverText = string.Empty;

    var wasFertilizedWith = Plugin.Fertilizers
      .Where(fertilizer => WasFertilizedWith(fertilizer, nview))
      .Select(fertilizer => fertilizer.ItemName);
    if (wasFertilizedWith.Any()) fertilizeHoverText += $"\n$InstantFertilizer_FertilizedWith {string.Join(" / ", wasFertilizedWith)}";

    var availableFertilizers = Plugin.Fertilizers
      .Where(fertilizer => HasGlobalKey(fertilizer.RequiredGlobalKey) && !wasFertilizedWith.Contains(fertilizer.ItemName))
      .Select(fertilizer => $"{fertilizer.RequiredAmount} {fertilizer.ItemName}");
    if (availableFertilizers.Any()) fertilizeHoverText += $"\n[<color=yellow><b>$KEY_Use</b></color>] $InstantFertilizer_Fertilize ({string.Join(" / ", availableFertilizers)})";

    return Localization.instance.Localize(fertilizeHoverText);
  }

  public static bool TryFertilize(Player player, Pickable pickable)
  {
    if (!pickable.m_nview.IsValid() || !pickable.m_picked || pickable.m_enabled == 0) return false;
    return TryFertilizeInternal(player, pickable.m_nview, () =>
    {
      pickable.m_nview.ClaimOwnership();
      if (pickable.m_pickedTime == 0L) pickable.UpdateRespawn(); // kludge to force initialize pickedTime before fertilizing if the pickable was just created
      DateTime pickedTime = new(pickable.m_nview.GetZDO().GetLong(ZDOVars.s_pickedTime));
      pickedTime -= TimeSpan.FromMinutes(pickable.m_respawnTimeMinutes * Plugin.FertilizePercentage);
      pickable.m_nview.GetZDO().Set(ZDOVars.s_pickedTime, pickedTime.Ticks);
      pickable.UpdateRespawn();
    });
  }

  public static bool TryFertilize(Player player, Plant plant)
  {
    if (!plant.m_nview.IsValid() || plant.m_status != Plant.Status.Healthy) return false;
    return TryFertilizeInternal(player, plant.m_nview, () =>
    {
      plant.m_nview.ClaimOwnership();
      DateTime plantTime = new(plant.m_nview.GetZDO().GetLong(ZDOVars.s_plantTime));
      plantTime -= TimeSpan.FromSeconds(plant.GetGrowTime() * Plugin.FertilizePercentage);
      plant.m_nview.GetZDO().Set(ZDOVars.s_plantTime, plantTime.Ticks);
      plant.m_updateTime -= 100f; // kludge to force SUpdate to re-run right away
      plant.m_spawnTime -= 100f; // kludge to force Grow to be able to run right away if suitable
      plant.SUpdate();
    });
  }

  private static bool TryFertilizeInternal(Player player, ZNetView nview, Action onFertilize)
  {
    player.m_lastHoverInteractTime = Time.time;
    var availableFertilizers = Plugin.Fertilizers.Where(fertilizer => HasGlobalKey(fertilizer.RequiredGlobalKey) && !WasFertilizedWith(fertilizer, nview));
    if (!availableFertilizers.Any()) return false;

    var wasAnyFertilizerUsed = false;
    foreach (var fertilizer in availableFertilizers)
    {
      wasAnyFertilizerUsed = player.m_inventory.GetItem(fertilizer.ItemName) is { } item && player.m_inventory.CountItems(fertilizer.ItemName) >= fertilizer.RequiredAmount && player.m_inventory.RemoveItem(item, fertilizer.RequiredAmount);
      if (wasAnyFertilizerUsed)
      {
        SetWasFertilizedWith(fertilizer, nview, true);
        break;
      }
    }

    if (!wasAnyFertilizerUsed)
    {
      player.Message(MessageHud.MessageType.Center, "$InstantFertilizer_FertilizerRequired");
      return false;
    }

    onFertilize();
    player.DoInteractAnimation(nview.transform.position);
    return true;
  }
}
