using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant PotionOfBloodPurity = new ItemVariant
        {
            Name = "potion of inhumanity",
            Type = ItemType.Potion,
            Material = Material.Glass,
            StackSize = 20,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new ChangeRace {RaceName = "human", Remove = true}}
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 1}}
        };
    }
}