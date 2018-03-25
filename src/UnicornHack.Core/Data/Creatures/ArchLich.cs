using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature ArchLich = new Creature
        {
            Name = "arch-lich",
            Species = Species.Lich,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 133,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Freeze {Damage = 170}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                }
            },
            InitialLevel = 25,
            GenerationWeight = new BranchWeight
            {
                Matched = new DefaultWeight(),
                Name = "hell"
            },
            PreviousStageName = "master lich",
            Behavior = AIBehavior.MagicUser,
            Noise = ActorNoiseType.Mumble,
            Weight = 600,
            Agility = 13,
            Constitution = 13,
            Intelligence = 13,
            Quickness = 13,
            Strength = 13,
            Willpower = 18,
            Regeneration = 3,
            MagicResistance = 90,
            PhysicalDeflection = 26,
            ColdResistance = 75,
            ElectricityResistance = 75,
            FireResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Infravision = true
        };
    }
}
