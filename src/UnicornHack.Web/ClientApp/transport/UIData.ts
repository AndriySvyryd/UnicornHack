import { action, observable } from 'mobx';
import { EntityState } from './EntityState';
import { Ability } from './Model';
import { GameQueryType } from './GameQueryType';

export class UIData {
    @observable abilitySlot: number | null = null;
    @observable slottableAbilities: Map<string, Ability> = new Map<string, Ability>();
    @observable playerAttributes: PlayerAttributes | null = null;
    @observable playerAdaptations: PlayerAdaptations | null = null;
    @observable playerSkills: PlayerSkills | null = null;

    @action.bound
    update(compactData: any[]): UIData {
        let i = 0;

        this.clear();
        var queryType = compactData[i++];
        switch (queryType) {
            case GameQueryType.Clear:
                break;
            case GameQueryType.SlottableAbilities:
                this.abilitySlot = compactData[i++];
                compactData[i++].map(
                    (s: any[]) => Ability.expandToCollection(s, this.slottableAbilities, EntityState.Added));
                break;
            case GameQueryType.PlayerAttributes:
                this.playerAttributes = PlayerAttributes.expand(compactData[i++]);
                break;
            case GameQueryType.PlayerAdaptations:
                this.playerAdaptations = PlayerAdaptations.expand(compactData[i++]);
                break;
            case GameQueryType.PlayerSkills:
                this.playerSkills = PlayerSkills.expand(compactData[i++]);
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
    }
}

export class PlayerAttributes {
    @observable Might: number = 0;
    @observable Speed: number = 0;
    @observable Focus: number = 0;
    @observable Perception: number = 0;
    @observable Regeneration: number = 0;
    @observable EnergyRegeneration: number = 0;
    @observable Armor: number = 0;
    @observable Deflection: number = 0;
    @observable Evasion: number = 0;
    @observable PhysicalResistance: number = 0;
    @observable MagicResistance: number = 0;
    @observable BleedingResistance: number = 0;
    @observable AcidResistance: number = 0;
    @observable ColdResistance: number = 0;
    @observable ElectricityResistance: number = 0;
    @observable FireResistance: number = 0;
    @observable PsychicResistance: number = 0;
    @observable ToxinResistance: number = 0;
    @observable VoidResistance: number = 0;
    @observable SonicResistance: number = 0;
    @observable StunResistance: number = 0;
    @observable LightResistance: number = 0;
    @observable WaterResistance: number = 0;

    @action
    static expand(compactAttributes: any[]): PlayerAttributes {
        var i = 0;
        const attributes = new PlayerAttributes();
        attributes.Might = compactAttributes[i++];
        attributes.Speed = compactAttributes[i++];
        attributes.Focus = compactAttributes[i++];
        attributes.Perception = compactAttributes[i++];
        attributes.Regeneration = compactAttributes[i++];
        attributes.EnergyRegeneration = compactAttributes[i++];
        attributes.Armor = compactAttributes[i++];
        attributes.Deflection = compactAttributes[i++];
        attributes.Evasion = compactAttributes[i++];
        attributes.PhysicalResistance = compactAttributes[i++];
        attributes.MagicResistance = compactAttributes[i++];
        attributes.BleedingResistance = compactAttributes[i++];
        attributes.AcidResistance = compactAttributes[i++];
        attributes.ColdResistance = compactAttributes[i++];
        attributes.ElectricityResistance = compactAttributes[i++];
        attributes.FireResistance = compactAttributes[i++];
        attributes.PsychicResistance = compactAttributes[i++];
        attributes.ToxinResistance = compactAttributes[i++];
        attributes.VoidResistance = compactAttributes[i++];
        attributes.SonicResistance = compactAttributes[i++];
        attributes.StunResistance = compactAttributes[i++];
        attributes.LightResistance = compactAttributes[i++];
        attributes.WaterResistance = compactAttributes[i++];

        return attributes;
    }
}

export class PlayerSkills {
    @observable SkillPoints: number = 0;
    @observable HandWeapons: number = 0;
    @observable ShortWeapons: number = 0;
    @observable MediumWeapons: number = 0;
    @observable LongWeapons: number = 0;
    @observable CloseRangeWeapons: number = 0;
    @observable ShortRangeWeapons: number = 0;
    @observable MediumRangeWeapons: number = 0;
    @observable LongRangeWeapons: number = 0;
    @observable OneHanded: number = 0;
    @observable TwoHanded: number = 0;
    @observable DualWielding: number = 0;
    @observable Acrobatics: number = 0;
    @observable LightArmor: number = 0;
    @observable HeavyArmor: number = 0;
    @observable AirSourcery: number = 0;
    @observable BloodSourcery: number = 0;
    @observable EarthSourcery: number = 0;
    @observable FireSourcery: number = 0;
    @observable SpiritSourcery: number = 0;
    @observable WaterSourcery: number = 0;
    @observable Conjuration: number = 0;
    @observable Enchantment: number = 0;
    @observable Evocation: number = 0;
    @observable Malediction: number = 0;
    @observable Illusion: number = 0;
    @observable Transmutation: number = 0;
    @observable Assassination: number = 0;
    @observable Stealth: number = 0;
    @observable Artifice: number = 0;
    @observable Leadership: number = 0;

    @action
    static expand(compactSkills: any[]): PlayerSkills {
        var i = 0;
        const skills = new PlayerSkills();
        skills.SkillPoints = compactSkills[i++];
        skills.HandWeapons = compactSkills[i++];
        skills.ShortWeapons = compactSkills[i++];
        skills.MediumWeapons = compactSkills[i++];
        skills.LongWeapons = compactSkills[i++];
        skills.CloseRangeWeapons = compactSkills[i++];
        skills.ShortRangeWeapons = compactSkills[i++];
        skills.MediumRangeWeapons = compactSkills[i++];
        skills.LongRangeWeapons = compactSkills[i++];
        skills.OneHanded = compactSkills[i++];
        skills.TwoHanded = compactSkills[i++];
        skills.DualWielding = compactSkills[i++];
        skills.Acrobatics = compactSkills[i++];
        skills.LightArmor = compactSkills[i++];
        skills.HeavyArmor = compactSkills[i++];
        skills.AirSourcery = compactSkills[i++];
        skills.BloodSourcery = compactSkills[i++];
        skills.EarthSourcery = compactSkills[i++];
        skills.FireSourcery = compactSkills[i++];
        skills.SpiritSourcery = compactSkills[i++];
        skills.WaterSourcery = compactSkills[i++];
        skills.Conjuration = compactSkills[i++];
        skills.Enchantment = compactSkills[i++];
        skills.Evocation = compactSkills[i++];
        skills.Malediction = compactSkills[i++];
        skills.Illusion = compactSkills[i++];
        skills.Transmutation = compactSkills[i++];
        skills.Assassination = compactSkills[i++];
        skills.Stealth = compactSkills[i++];
        skills.Artifice = compactSkills[i++];
        skills.Leadership = compactSkills[i++];

        return skills;
    }
}

export class PlayerAdaptations {
    @observable TraitPoints: number = 0;
    @observable MutationPoints: number = 0;
    @observable Traits: Map<string, number> = new Map<string, number>();
    @observable Mutations: Map<string, number> = new Map<string, number>();

    @action
    static expand(compactSkills: any[]): PlayerAdaptations {
        var i = 0;
        const adaptations = new PlayerAdaptations();
        adaptations.TraitPoints = compactSkills[i++];
        adaptations.MutationPoints = compactSkills[i++];
        adaptations.Traits = compactSkills[i++];
        adaptations.Mutations = compactSkills[i++];

        return adaptations;
    }
}