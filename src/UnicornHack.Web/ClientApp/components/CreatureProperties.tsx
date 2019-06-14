import * as React from 'React';
import { computed } from 'mobx';
import { observer } from 'mobx-react';
import { ActorAttributes } from '../transport/DialogData';
import { capitalize } from '../Util';
import { AbilitiesList } from './AbilityProperties';
import { Dialog } from './Dialog';
import { IGameContext } from './Game';
import { PropertyRow } from './PropertyRow';

export const CreaturePropertiesDialog = observer((props: ICreaturePropertiesProps) => {
    const { data, context } = props;
    return <Dialog context={context} show={computed(() => data.actorAttributes != null)} className="creatureProperties"
        title={computed(() =>
            capitalize(data.actorAttributes == null || data.actorAttributes.name == null ? "Unknown creature" : data.actorAttributes.name))}
    >
        <CreatureProperties {...props} />
    </Dialog>;
});

const CreatureProperties = observer((props: ICreaturePropertiesProps) => {
    const actorAttributes = props.data.actorAttributes;
    if (actorAttributes == null) {
        return <></>;
    }

    return <>
        <div>{actorAttributes.description}</div>
        <ActorAttributesScreen actorAttributes={actorAttributes} />
    </>;
});

interface ICreaturePropertiesProps {
    data: IActorPropertiesData;
    context: IGameContext;
}

export const ActorAttributesScreen = observer((props: IActorPropertiesData) => {
    const actorAttributes = props.actorAttributes;
    if (actorAttributes == null) {
        return <></>;
    }

    return <div className="characterScreen__content">
        <PropertyRow label="Hit points" value={actorAttributes.hitPoints + '/' + actorAttributes.hitPointMaximum} />
        <PropertyRow label="Energy points" value={actorAttributes.energyPoints + '/' + actorAttributes.energyPointMaximum} />
        <PropertyRow label="Regeneration" value={actorAttributes.regeneration} />
        <PropertyRow label="Energy regeneration" value={actorAttributes.energyRegeneration} />
        <PropertyRow label="Might" value={actorAttributes.might} />
        <PropertyRow label="Focus" value={actorAttributes.focus} />
        <PropertyRow label="Speed" value={actorAttributes.speed} />
        <PropertyRow label="Perception" value={actorAttributes.perception} />
        <PropertyRow label="Primary FOV" value={actorAttributes.primaryFOVQuadrants * 90} />
        <PropertyRow label="Primary vision range" value={actorAttributes.primaryVisionRange} />
        <PropertyRow label="Total FOV" value={actorAttributes.totalFOVQuadrants * 90} />
        <PropertyRow label="Secondary vision range" value={actorAttributes.secondaryVisionRange} />
        <PropertyRow label="Infravision" value={actorAttributes.infravision} />
        <PropertyRow label="Invisibility detection" value={actorAttributes.invisibilityDetection} />
        <PropertyRow label="Infravisible" value={actorAttributes.infravisible} />
        <PropertyRow label="Visibility" value={actorAttributes.visibility} />
        <PropertyRow label="Movement delay" value={actorAttributes.movementDelay} />
        <PropertyRow label="Size" value={actorAttributes.size} />
        <PropertyRow label="Weight" value={actorAttributes.weight} />
        <PropertyRow label="Armor" value={actorAttributes.armor} />
        <PropertyRow label="Deflection" value={actorAttributes.deflection} />
        <PropertyRow label="Evasion" value={actorAttributes.evasion} />
        <PropertyRow label="Hindrance" value={actorAttributes.hindrance} />
        <PropertyRow label="Physical resistance" value={actorAttributes.physicalResistance} />
        <PropertyRow label="Magic resistance" value={actorAttributes.magicResistance} />
        <PropertyRow label="Bleeding resistance" value={actorAttributes.bleedingResistance} />
        <PropertyRow label="Acid resistance" value={actorAttributes.acidResistance} />
        <PropertyRow label="Cold resistance" value={actorAttributes.coldResistance} />
        <PropertyRow label="Electricity resistance" value={actorAttributes.electricityResistance} />
        <PropertyRow label="Fire resistance" value={actorAttributes.fireResistance} />
        <PropertyRow label="Psychic resistance" value={actorAttributes.psychicResistance} />
        <PropertyRow label="Toxin resistance" value={actorAttributes.toxinResistance} />
        <PropertyRow label="Void resistance" value={actorAttributes.voidResistance} />
        <PropertyRow label="Sonic resistance" value={actorAttributes.sonicResistance} />
        <PropertyRow label="Stun resistance" value={actorAttributes.stunResistance} />
        <PropertyRow label="Light resistance" value={actorAttributes.lightResistance} />
        <PropertyRow label="Water resistance" value={actorAttributes.waterResistance} />
        {actorAttributes.abilities.size > 0 ? <AbilitiesList abilities={actorAttributes.abilities} /> : ""}
    </div>;
});

export interface IActorPropertiesData {
    actorAttributes: ActorAttributes | null;
}