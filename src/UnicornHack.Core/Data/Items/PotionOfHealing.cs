using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant PotionOfHealing = new ItemVariant
        {
            Name = "potion of healing",
            Type = ItemType.Potion,
            GenerationWeight = new DefaultWeight {Multiplier = 10F},
            Material = Material.Glass,
            StackSize = 20,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Heal {Amount = 50}}
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 1}}
        };
    }
}