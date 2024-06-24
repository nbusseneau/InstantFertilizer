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

  public static string GetFertilizeHoverText()
  {
    var availableFertilizers = Plugin.Fertilizers.Where(fertilizer => HasGlobalKey(fertilizer.RequiredGlobalKey)).Select(fertilizer => fertilizer.HoverText);
    if (!availableFertilizers.Any()) return string.Empty;

    var fertilizeHoverText = $"\n[<color=yellow><b>$KEY_Use</b></color>] $InstantFertilizer_Fertilize ({string.Join(" / ", availableFertilizers)})";
    return Localization.instance.Localize(fertilizeHoverText);
  }

  public static bool TryFertilize(Player player, Pickable pickable)
  {
    if (!pickable.m_nview.IsValid() || !pickable.m_picked || pickable.m_enabled == 0) return false;
    return TryFertilizeInternal(player, pickable, () =>
    {
      pickable.m_nview.ClaimOwnership();
      pickable.m_nview.InvokeRPC(ZNetView.Everybody, nameof(Pickable.RPC_SetPicked), false);
    });
  }

  public static bool TryFertilize(Player player, Plant plant)
  {
    if (!plant.m_nview.IsValid() || plant.m_status != Plant.Status.Healthy) return false;
    return TryFertilizeInternal(player, plant, () =>
    {
      plant.m_nview.ClaimOwnership();
      plant.Grow();
    });
  }

  private static bool TryFertilizeInternal(Player player, Component component, Action onFertilize)
  {
    player.m_lastHoverInteractTime = Time.time;
    var availableFertilizers = Plugin.Fertilizers.Where(fertilizer => HasGlobalKey(fertilizer.RequiredGlobalKey));
    if (!availableFertilizers.Any()) return false;

    var wasAnyFertilizerUsed = false;
    foreach (var fertilizer in availableFertilizers)
    {
      wasAnyFertilizerUsed = player.m_inventory.GetItem(fertilizer.ItemName) is { } item && player.m_inventory.CountItems(fertilizer.ItemName) >= fertilizer.RequiredAmount && player.m_inventory.RemoveItem(item, fertilizer.RequiredAmount);
      if (wasAnyFertilizerUsed) break;
    }

    if (!wasAnyFertilizerUsed)
    {
      player.Message(MessageHud.MessageType.Center, "$InstantFertilizer_FertilizerRequired");
      return false;
    }

    onFertilize();
    player.DoInteractAnimation(component.transform.position);
    return true;
  }
}
