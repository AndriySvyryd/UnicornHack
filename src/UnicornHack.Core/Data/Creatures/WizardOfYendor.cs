using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature WizardOfYendor = new Creature
        {
            Name = "Wizard of Yendor",
            Species = Species.Human,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 130}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                }
            },
            InitialLevel = 30,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.RangedPeaceful | AIBehavior.MagicUser,
            Noise = ActorNoiseType.Cuss,
            Sex = Sex.Male,
            Weight = 1000,
            Agility = 16,
            Constitution = 16,
            Intelligence = 16,
            Quickness = 16,
            Strength = 16,
            Willpower = 16,
            EnergyRegeneration = 3,
            Regeneration = 3,
            MagicResistance = 100,
            PhysicalDeflection = 28,
            FireResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Walking | LocomotionType.Teleportation,
            Telepathic = 3,
            Infravisible = true,
            InvisibilityDetection = true
        };
    }
}
