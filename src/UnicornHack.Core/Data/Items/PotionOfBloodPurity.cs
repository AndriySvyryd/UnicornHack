using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant PotionOfBloodPurity = new ItemVariant
        {
            Name = "potion of blood purity",
            Type = ItemType.Potion,
            Weight = 1,
            Material = Material.Glass,
            StackSize = 20,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new ChangeRace {RaceName = "prompt", Remove = true}}
                }
            }
        };
    }
}