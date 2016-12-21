using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Effects;
using UnicornHack.Events;

namespace UnicornHack
{
    public class Ability
    {
        public Ability()
        {
            Effects = new HashSet<Effect>();
        }

        public Ability(Game game)
            : this()
        {
            Game = game;
            Id = game.NextAbilityId++;
            game.Abilities.Add(this);
        }

        public virtual Ability Instantiate(Game game)
        {
            if (Game != null)
            {
                throw new InvalidOperationException("This ability is already part of a game.");
            }

            var abilityInstance = new Ability(game)
            {
                Activation = Activation,
                Action = Action,
                ActionPointCost = ActionPointCost,
                EnergyPointCost = EnergyPointCost,
                Timeout = Timeout,
                EffectDuration = EffectDuration
            };
            foreach (var effect in Effects)
            {
                abilityInstance.Effects.Add(effect.Instantiate(game));
            }

            return abilityInstance;
        }

        public string Name { get; set; }
        public int Id { get; private set; }
        public int GameId { get; private set; }
        public Game Game { get; set; }
        public virtual AbilityActivation Activation { get; set; }
        public virtual AbilityAction Action { get; set; }
        public virtual int Timeout { get; set; }
        public virtual int TimeoutTurnsLeft { get; set; }
        public virtual EquipmentSlot FreeSlotsRequired { get; set; }
        public virtual bool IsActive { get; set; }
        // If more than one turn - can be interrupted
        public virtual int ActionPointCost { get; set; }
        public virtual int EnergyPointCost { get; set; }
        // Targeting mode
        // Success condition
        public virtual int EffectDuration { get; set; }
        public virtual ISet<Effect> Effects { get; set; }

        public virtual bool IsUsable { get; set; }

        public virtual bool Activate(Actor activator, Actor target, bool pretend = false)
        {
            if (!IsUsable)
            {
                return false;
            }

            if (!target.IsAlive)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            var ability = this;
            var weapon = Effects.OfType<MeleeAttack>().FirstOrDefault()?.Weapon;
            if (weapon != null)
            {
                ability = weapon.Abilities.FirstOrDefault(a => a.Activation == AbilityActivation.OnMeleeAttack);
            }

            var damage = ability.Effects.OfType<PhysicalDamage>().FirstOrDefault()?.Damage
                         ?? ability.Effects.OfType<ElectricityDamage>().FirstOrDefault()?.Damage
                         ?? ability.Effects.OfType<FireDamage>().FirstOrDefault()?.Damage;
            if (damage == null)
            {
                return false;
            }

            // TODO: Calculate AP cost
            activator.ActionPoints -= Actor.ActionPointsPerTurn;

            if (Game.NextRandom(maxValue: 3) == 0)
            {
                AttackEvent.New(activator, target, ability.Action, hit: false);
            }
            else
            {
                AttackEvent.New(activator, target, ability.Action, hit: true, damage: damage.Value, weapon: weapon);
                target.ChangeCurrentHP(-1*damage.Value);
            }

            if (!target.IsAlive)
            {
                activator.XP += target.XP;
            }
            else if (Activation == AbilityActivation.OnTarget)
            {
                var abilityTrigger = AbilityActivation.Default;

                switch (ability.Action)
                {
                    case AbilityAction.Slash:
                    case AbilityAction.Chop:
                    case AbilityAction.Stab:
                    case AbilityAction.Poke:
                    case AbilityAction.Bludgeon:
                    case AbilityAction.Punch:
                    case AbilityAction.Kick:
                    case AbilityAction.Touch:
                    case AbilityAction.Headbutt:
                    case AbilityAction.Claw:
                    case AbilityAction.Bite:
                    case AbilityAction.Suck:
                    case AbilityAction.Sting:
                    case AbilityAction.Hug:
                    case AbilityAction.Trample:
                    case AbilityAction.Digestion:
                        abilityTrigger = AbilityActivation.OnMeleeAttack;
                        break;
                    case AbilityAction.Spit:
                    case AbilityAction.Breath:
                    case AbilityAction.Gaze:
                    case AbilityAction.Scream:
                    case AbilityAction.Explosion:
                        abilityTrigger = AbilityActivation.OnRangedAttack;
                        break;
                    case AbilityAction.Spell:
                        abilityTrigger = AbilityActivation.OnSpellCast;
                        break;
                    case AbilityAction.Modifier:
                    case AbilityAction.Default:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (abilityTrigger != AbilityActivation.Default)
                {
                    foreach (var triggeredAbility in
                        activator.Abilities.Where(a => a.IsUsable && a.Activation == abilityTrigger))
                    {
                        triggeredAbility.Activate(activator, target, pretend);
                    }
                }
            }

            return true;
        }
    }
}