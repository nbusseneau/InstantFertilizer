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
  private static bool WasFertilizedWith(Fertilizer fertilizer, ZNetView nview) => nview.GetZDO().GetBool($"{WasFertilizedWithZDOKey}.{fertilizer.ItemName}");
  public static void SetWasFertilizedWith(Fertilizer fertilizer, ZNetView nview, bool value) => nview.GetZDO().Set($"{WasFertilizedWithZDOKey}.{fertilizer.ItemName}", value);
  public static bool WasVineFertilized(Vine vine) => vine.m_nview.GetZDO().GetBool(WasFertilizedWithZDOKey);
  public static void SetWasVineFertilized(Vine vine, bool value) => vine.m_nview.GetZDO().Set(WasFertilizedWithZDOKey, value);

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

  public static bool CanFertilize(Pickable pickable)
  {
    if (pickable.CanBePicked() || !pickable.m_nview || !pickable.m_nview.IsValid()) return false;
    if (pickable.GetComponent<Vine>() is { } vine && !vine.CanSpawnPickable(pickable)) return false;
    var timeSincePicked = ZNet.instance.GetTime() - new DateTime(pickable.m_nview.GetZDO().GetLong(ZDOVars.s_pickedTime));
    return timeSincePicked.TotalMinutes <= pickable.m_respawnTimeMinutes;
  }
  public static bool CanFertilize(Vine vine) => !vine.IsDoneGrowing && vine.m_nview && vine.m_nview.IsValid() && !WasVineFertilized(vine) && !vine.m_pickable.CanBePicked();
  public static bool CanFertilize(Plant plant) => plant.m_status == Plant.Status.Healthy && plant.m_nview && plant.m_nview.IsValid() && plant.TimeSincePlanted() <= plant.GetGrowTime();

  public static bool TryFertilize(Player player, Pickable pickable)
  {
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

  public static bool TryFertilize(Player player, Vine vine)
  {
    return TryFertilizeInternal(player, vine.m_nview, () =>
    {
      vine.m_nview.ClaimOwnership();
      SetWasVineFertilized(vine, true);
      vine.m_initialGrowItterations = 25;
    }, setWasFertilizedWith: false);
  }

  public static bool TryFertilize(Player player, Plant plant)
  {
    return TryFertilizeInternal(player, plant.m_nview, () =>
    {
      plant.m_nview.ClaimOwnership();
      DateTime plantTime = new(plant.m_nview.GetZDO().GetLong(ZDOVars.s_plantTime));
      plantTime -= TimeSpan.FromSeconds(plant.GetGrowTime() * Plugin.FertilizePercentage);
      plant.m_nview.GetZDO().Set(ZDOVars.s_plantTime, plantTime.Ticks);
      plant.m_updateTime = float.MaxValue; // kludge to force SUpdate to re-run right away
      plant.m_spawnTime = 0f; // kludge to force Grow to be able to run right away if suitable
      plant.SUpdate(Time.time, ZoneSystem.GetZone(ZNet.instance.GetReferencePosition()));
    });
  }

  private static bool TryFertilizeInternal(Player player, ZNetView nview, Action onFertilize, bool setWasFertilizedWith = true)
  {
    player.m_lastHoverInteractTime = Time.time;
    var availableFertilizers = Plugin.Fertilizers.Where(fertilizer => HasGlobalKey(fertilizer.RequiredGlobalKey) && !WasFertilizedWith(fertilizer, nview));
    if (!availableFertilizers.Any()) return false;

    var wasAnyFertilizerUsed = false;
    foreach (var fertilizer in availableFertilizers)
    {
      wasAnyFertilizerUsed = player.m_inventory.GetItem(fertilizer.ItemName) is { } item && player.m_inventory.CountItems(fertilizer.ItemName) >= fertilizer.RequiredAmount && player.m_inventory.RemoveItem(item, fertilizer.RequiredAmount);
      if (wasAnyFertilizerUsed && setWasFertilizedWith)
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
    player.DoInteractAnimation(nview.gameObject);
    return true;
  }
}
