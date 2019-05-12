import { action, observable } from 'mobx';
import { capitalize } from '../Util';
import { EntityState } from './EntityState';
import { GameQueryType } from './GameQueryType';
import { Ability, EquipableSlot } from './Model';
import { ItemType } from './ItemType';
import { Material } from './Material';
import { ItemComplexity } from './ItemComplexity';
import { ActivationType } from './ActivationType';
import { TargetingType } from './TargetingType';
import { TargetingShape } from './TargetingShape';
import { AbilitySuccessCondition } from './AbilitySuccessCondition';
import { EffectType } from './EffectType';
import { ValueCombinationFunction } from './ValueCombinationFunction';
import { EffectDuration } from './EffectDuration';

export class DialogData {
    @observable abilitySlot: number | null = null;
    @observable slottableAbilities: Map<number, Ability> = new Map<number, Ability>();
    @observable playerAttributes: ActorAttributes | null = null;
    @observable playerAdaptations: PlayerAdaptations | null = null;
    @observable playerSkills: PlayerSkills | null = null;
    @observable actorAttributes: ActorAttributes | null = null;
    @observable itemAttributes: ItemAttributes | null = null;
    @observable abilityAttributes: AbilityAttributes | null = null;
    @observable staticDescription: string | null = null;

    @action.bound
    update(compactData: any[]): DialogData {
        let i = 0;

        this.clear();
        var queryType = compactData[i++];
        switch (queryType) {
            case GameQueryType.Clear:
                break;
            case GameQueryType.SlottableAbilities:
                this.abilitySlot = compactData[i++];
                compactData[i++].forEach(
                    (s: any[]) => Ability.expandToCollection(s, this.slottableAbilities, EntityState.Added));
                break;
            case GameQueryType.PlayerAttributes:
                this.playerAttributes = ActorAttributes.expand(compactData[i++]);
                break;
            case GameQueryType.PlayerAdaptations:
                this.playerAdaptations = PlayerAdaptations.expand(compactData[i++]);
                break;
            case GameQueryType.PlayerSkills:
                this.playerSkills = PlayerSkills.expand(compactData[i++]);
                break;
            case GameQueryType.ActorAttributes:
                this.actorAttributes = ActorAttributes.expand(compactData[i++]);
                break;
            case GameQueryType.ItemAttributes:
                this.itemAttributes = ItemAttributes.expand(compactData[i++]);
                break;
            case GameQueryType.AbilityAttributes:
                this.abilityAttributes = AbilityAttributes.expand(compactData[i++]);
                break;
            case GameQueryType.StaticDescription:
                this.staticDescription = compactData[i++];
                break;
            default:
                console.error("Unhandled GameQueryType %d", queryType);
                break;
        }

        return this;
    }

    clear() {
        this.abilitySlot = null;
        this.slottableAbilities.clear();
        this.playerAttributes = null;
        this.playerAdaptations = null;
        this.playerSkills = null;
        this.actorAttributes = null;
        this.itemAttributes = null;
        this.abilityAttributes = null;
        this.staticDescription = null;
    }
}

export class ActorAttributes {
    @observable name: string | null = null;
    @observable description: string = '';
    @observable movementDelay: number = 0;
    @observable size: number = 0;
    @observable weight: number = 0;
    @observable primaryFOVQuadrants: number = 0;
    @observable primaryVisionRange: number = 0;
    @observable totalFOVQuadrants: number = 0;
    @observable secondaryVisionRange: number = 0;
    @observable infravision: boolean = false;
    @observable invisibilityDetection: boolean = false;
    @observable infravisible: boolean = false;
    @observable visibility: number = 0;
    @observable hitPoints: number = 0;
    @observable hitPointMaximum: number = 0;
    @observable energyPoints: number = 0;
    @observable energyPointMaximum: number = 0;
    @observable might: number = 0;
    @observable speed: number = 0;
    @observable focus: number = 0;
    @observable perception: number = 0;
    @observable regeneration: number = 0;
    @observable energyRegeneration: number = 0;
    @observable armor: number = 0;
    @observable deflection: number = 0;
    @observable evasion: number = 0;
    @observable physicalResistance: number = 0;
    @observable magicResistance: number = 0;
    @observable bleedingResistance: number = 0;
    @observable acidResistance: number = 0;
    @observable coldResistance: number = 0;
    @observable electricityResistance: number = 0;
    @observable fireResistance: number = 0;
    @observable psychicResistance: number = 0;
    @observable toxinResistance: number = 0;
    @observable voidResistance: number = 0;
    @observable sonicResistance: number = 0;
    @observable stunResistance: number = 0;
    @observable lightResistance: number = 0;
    @observable waterResistance: number = 0;
    @observable abilities: Map<number, AbilityAttributes> = new Map<number, AbilityAttributes>();

    @action
    static expand(compactAttributes: any[]): ActorAttributes {
        var i = 0;
        const attributes = new ActorAttributes();
        if (compactAttributes.length == 0) {
            return attributes;
        }

        attributes.name = compactAttributes[i++];
        attributes.description = compactAttributes[i++];
        attributes.movementDelay = compactAttributes[i++];
        attributes.size = compactAttributes[i++];
        attributes.weight = compactAttributes[i++];
        attributes.primaryFOVQuadrants = compactAttributes[i++];
        attributes.primaryVisionRange = compactAttributes[i++];
        attributes.totalFOVQuadrants = compactAttributes[i++];
        attributes.secondaryVisionRange = compactAttributes[i++];
        attributes.infravision = compactAttributes[i++];
        attributes.invisibilityDetection = compactAttributes[i++];
        attributes.infravisible = compactAttributes[i++];
        attributes.visibility = compactAttributes[i++];
        attributes.hitPoints = compactAttributes[i++];
        attributes.hitPointMaximum = compactAttributes[i++];
        attributes.energyPoints = compactAttributes[i++];
        attributes.energyPointMaximum = compactAttributes[i++];
        attributes.might = compactAttributes[i++];
        attributes.speed = compactAttributes[i++];
        attributes.focus = compactAttributes[i++];
        attributes.perception = compactAttributes[i++];
        attributes.regeneration = compactAttributes[i++];
        attributes.energyRegeneration = compactAttributes[i++];
        attributes.armor = compactAttributes[i++];
        attributes.deflection = compactAttributes[i++];
        attributes.evasion = compactAttributes[i++];
        attributes.physicalResistance = compactAttributes[i++];
        attributes.magicResistance = compactAttributes[i++];
        attributes.bleedingResistance = compactAttributes[i++];
        attributes.acidResistance = compactAttributes[i++];
        attributes.coldResistance = compactAttributes[i++];
        attributes.electricityResistance = compactAttributes[i++];
        attributes.fireResistance = compactAttributes[i++];
        attributes.psychicResistance = compactAttributes[i++];
        attributes.toxinResistance = compactAttributes[i++];
        attributes.voidResistance = compactAttributes[i++];
        attributes.sonicResistance = compactAttributes[i++];
        attributes.stunResistance = compactAttributes[i++];
        attributes.lightResistance = compactAttributes[i++];
        attributes.waterResistance = compactAttributes[i++];

        if (compactAttributes.length > i) {
            (<any[]>compactAttributes[i++]).forEach((a: any[]) => AbilityAttributes.expand(a).addTo(attributes.abilities));
        }

        return attributes;
    }
}

export class PlayerSkills {
    @observable skillPoints: number = 0;
    @observable handWeapons: number = 0;
    @observable shortWeapons: number = 0;
    @observable mediumWeapons: number = 0;
    @observable longWeapons: number = 0;
    @observable closeRangeWeapons: number = 0;
    @observable shortRangeWeapons: number = 0;
    @observable mediumRangeWeapons: number = 0;
    @observable longRangeWeapons: number = 0;
    @observable oneHanded: number = 0;
    @observable twoHanded: number = 0;
    @observable dualWielding: number = 0;
    @observable acrobatics: number = 0;
    @observable lightArmor: number = 0;
    @observable heavyArmor: number = 0;
    @observable airSourcery: number = 0;
    @observable bloodSourcery: number = 0;
    @observable earthSourcery: number = 0;
    @observable fireSourcery: number = 0;
    @observable spiritSourcery: number = 0;
    @observable waterSourcery: number = 0;
    @observable conjuration: number = 0;
    @observable enchantment: number = 0;
    @observable evocation: number = 0;
    @observable malediction: number = 0;
    @observable illusion: number = 0;
    @observable transmutation: number = 0;
    @observable assassination: number = 0;
    @observable stealth: number = 0;
    @observable artifice: number = 0;
    @observable leadership: number = 0;

    @action
    static expand(compactSkills: any[]): PlayerSkills {
        var i = 0;
        const skills = new PlayerSkills();
        skills.skillPoints = compactSkills[i++];
        skills.handWeapons = compactSkills[i++];
        skills.shortWeapons = compactSkills[i++];
        skills.mediumWeapons = compactSkills[i++];
        skills.longWeapons = compactSkills[i++];
        skills.closeRangeWeapons = compactSkills[i++];
        skills.shortRangeWeapons = compactSkills[i++];
        skills.mediumRangeWeapons = compactSkills[i++];
        skills.longRangeWeapons = compactSkills[i++];
        skills.oneHanded = compactSkills[i++];
        skills.twoHanded = compactSkills[i++];
        skills.dualWielding = compactSkills[i++];
        skills.acrobatics = compactSkills[i++];
        skills.lightArmor = compactSkills[i++];
        skills.heavyArmor = compactSkills[i++];
        skills.airSourcery = compactSkills[i++];
        skills.bloodSourcery = compactSkills[i++];
        skills.earthSourcery = compactSkills[i++];
        skills.fireSourcery = compactSkills[i++];
        skills.spiritSourcery = compactSkills[i++];
        skills.waterSourcery = compactSkills[i++];
        skills.conjuration = compactSkills[i++];
        skills.enchantment = compactSkills[i++];
        skills.evocation = compactSkills[i++];
        skills.malediction = compactSkills[i++];
        skills.illusion = compactSkills[i++];
        skills.transmutation = compactSkills[i++];
        skills.assassination = compactSkills[i++];
        skills.stealth = compactSkills[i++];
        skills.artifice = compactSkills[i++];
        skills.leadership = compactSkills[i++];

        return skills;
    }
}

export class PlayerAdaptations {
    @observable traitPoints: number = 0;
    @observable mutationPoints: number = 0;
    @observable traits: Map<string, number> = new Map<string, number>();
    @observable mutations: Map<string, number> = new Map<string, number>();

    @action
    static expand(compactSkills: any[]): PlayerAdaptations {
        var i = 0;
        const adaptations = new PlayerAdaptations();
        adaptations.traitPoints = compactSkills[i++];
        adaptations.mutationPoints = compactSkills[i++];
        adaptations.traits = compactSkills[i++];
        adaptations.mutations = compactSkills[i++];

        return adaptations;
    }
}

export class ItemAttributes {
    @observable name: string | null = null;
    @observable description: string = '';
    @observable type: ItemType = ItemType.None;
    @observable material: Material = Material.Default;
    @observable size: number = 0;
    @observable weight: number = 0;
    @observable hindrance: number = 0;
    @observable complexity: ItemComplexity = 0;
    @observable requiredMight: number = 0;
    @observable requiredSpeed: number = 0;
    @observable requiredFocus: number = 0;
    @observable requiredPerception: number = 0;
    @observable equippableSlots: Map<number, EquipableSlot> = new Map<number, EquipableSlot>();
    @observable abilities: Map<number, AbilityAttributes> = new Map<number, AbilityAttributes>();

    @action
    static expand(compactAttributes: any[]): ItemAttributes {
        var i = 0;
        const attributes = new ItemAttributes();
        attributes.name = compactAttributes[i++];
        attributes.description = compactAttributes[i++];
        attributes.type = compactAttributes[i++];
        attributes.material = compactAttributes[i++];
        attributes.size = compactAttributes[i++];
        attributes.weight = compactAttributes[i++];
        attributes.hindrance = compactAttributes[i++];
        attributes.complexity = compactAttributes[i++];
        attributes.requiredMight = compactAttributes[i++];
        attributes.requiredSpeed = compactAttributes[i++];
        attributes.requiredFocus = compactAttributes[i++];
        attributes.requiredPerception = compactAttributes[i++];
        const equippableSlots = compactAttributes[i++];
        if (equippableSlots != null) {
            equippableSlots.forEach((s: any[]) => EquipableSlot.expandToCollection(s, attributes.equippableSlots));
        }

        if (compactAttributes.length > i) {
            (<any[]>compactAttributes[i++]).forEach((a: any[]) => AbilityAttributes.expand(a).addTo(attributes.abilities));
        }

        return attributes;
    }
}

export class AbilityAttributes {
    id: number = -1;
    @observable name: string | null = null;
    @observable level: number = 0;
    @observable activation: ActivationType = ActivationType.Default;
    @observable activationCondition: number | null = null;
    @observable targetingType: TargetingType = TargetingType.Single;
    @observable targetingShape: TargetingShape = TargetingShape.Line;
    @observable range: number = 0;
    @observable headingDeviation: number = 0;
    @observable energyCost: number = 0;
    @observable delay: number = 0;
    @observable cooldown: number = 0;
    @observable cooldownTicksLeft: number = 0;
    @observable xpCooldown: number = 0;
    @observable xpCooldownLeft: number = 0;
    @observable successCondition: AbilitySuccessCondition = AbilitySuccessCondition.Default;
    @observable effects: Map<string, EffectAttributes> = new Map<string, EffectAttributes>();
    @observable description: string | null = null;
    //@observable type: AbilityType = AbilityType.Default;
    //@observable cost: number = 0;
    //@observable accuracy: number = 0;

    @action
    static expand(compactAttributes: any[]): AbilityAttributes {
        var i = 0;
        const attributes = new AbilityAttributes();
        attributes.id = compactAttributes[i++];
        attributes.name = compactAttributes[i++];
        attributes.level = compactAttributes[i++];
        attributes.activation = compactAttributes[i++];
        attributes.activationCondition = compactAttributes[i++];
        attributes.targetingType = compactAttributes[i++];
        attributes.targetingShape = compactAttributes[i++];
        attributes.range = compactAttributes[i++];
        attributes.headingDeviation = compactAttributes[i++];
        attributes.energyCost = compactAttributes[i++];
        attributes.delay = compactAttributes[i++];
        attributes.cooldown = compactAttributes[i++];
        attributes.cooldownTicksLeft = compactAttributes[i++];
        attributes.xpCooldown = compactAttributes[i++];
        attributes.xpCooldownLeft = compactAttributes[i++];
        attributes.successCondition = compactAttributes[i++];
        (<any[]>compactAttributes[i++]).forEach((a: any[]) => EffectAttributes.expand(a).addTo(attributes.effects));

        if (compactAttributes.length > i) {
            attributes.description = compactAttributes[i++];
            //attributes.type = compactAttributes[i++];
            //attributes.cost = compactAttributes[i++];
            //attributes.accuracy = compactAttributes[i++];
        }

        return attributes;
    }

    @action.bound
    addTo(map: Map<number, AbilityAttributes>) {
        map.set(this.id, this);
    }

    getName(): string {
        if (this.name == null) {
            return "Ability";
        }

        return capitalize(this.name) + (this.level == 0 ? '' : ' ' + this.level.toString());
    } 
}

export class EffectAttributes {
    id: number = -1;
    @observable type: EffectType = EffectType.Default;
    @observable shouldTargetActivator: boolean = false;
    @observable amount: string | null = null;
    @observable function: ValueCombinationFunction = ValueCombinationFunction.Sum;
    @observable targetName: string | null = null;
    @observable secondaryAmount: number | null = null;
    @observable duration: EffectDuration = EffectDuration.Instant;
    @observable durationAmount: string | null = null;

    @action
    static expand(compactAttributes: any[]): EffectAttributes {
        var i = 0;
        const attributes = new EffectAttributes();
        attributes.id = compactAttributes[i++];
        attributes.type = compactAttributes[i++];
        attributes.shouldTargetActivator = compactAttributes[i++];
        attributes.amount = compactAttributes[i++];
        attributes.function = compactAttributes[i++];
        attributes.targetName = compactAttributes[i++];
        attributes.secondaryAmount = compactAttributes[i++];
        attributes.duration = compactAttributes[i++];
        attributes.durationAmount = compactAttributes[i++];

        return attributes;
    }

    @action.bound
    addTo(map: Map<string, EffectAttributes>) {
        map.set(this.id.toString(), this);
    }
}