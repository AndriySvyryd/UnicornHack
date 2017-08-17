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
            Weight = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 10F},
            Material = Material.Glass,
            StackSize = 20,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new Heal {Amount = 50}}
                }
            }
        };
    }
}