using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant PotionOfOgreness = new ItemVariant
        {
            Name = "potion of ogreness",
            Type = ItemType.Potion,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Material = Material.Glass,
            StackSize = 20,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnConsumption,
                    Effects = new HashSet<Effect> {new ChangeRace {RaceName = "ogre"}}
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 1}}
        };
    }
}