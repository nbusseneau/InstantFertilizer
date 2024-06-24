namespace InstantFertilizer.Model;

public class Fertilizer(string itemName, int requiredAmount, string requiredGlobalKey)
{
  public string ItemName { get; } = itemName;
  public int RequiredAmount { get; } = requiredAmount;
  public string RequiredGlobalKey { get; } = requiredGlobalKey;
  public string HoverText { get; } = $"{requiredAmount} {itemName}";

  public override string ToString() => $"{this.ItemName}{Delimiter}{this.RequiredAmount}{Delimiter}{this.RequiredGlobalKey}";

  public const char Delimiter = ':';
  public static readonly string SerializedFormat = $"itemName{Delimiter}requiredAmount{Delimiter}requiredGlobalKey";
  public static Fertilizer FromString(string serializedFertilizer)
  {
    var components = serializedFertilizer.Trim().Split(Delimiter);
    if (components.Length != 3)
    {
      Plugin.Logger.LogError(@$"Could not deserialize the following fertilizer entry: {serializedFertilizer}
Invalid format: must be `{SerializedFormat}`");
      return null;
    }

    var (itemName, requiredAmountString, requiredGlobalKey) = (components[0], components[1], components[2]);

    if (!int.TryParse(requiredAmountString, out var requiredAmount))
    {
      Plugin.Logger.LogError(@$"Could not deserialize the following fertilizer entry: {serializedFertilizer}
Invalid amount: must be a valid integer");
      return null;
    }

    return new(itemName, requiredAmount, requiredGlobalKey);
  }
}
