import * as React from 'React';
import { computed } from 'mobx';
import { observer } from 'mobx-react';
import { ActivationType } from '../transport/ActivationType';
import { AbilitySuccessCondition } from '../transport/AbilitySuccessCondition';
import { EffectDuration } from '../transport/EffectDuration';
import { EffectType } from '../transport/EffectType';
import { AbilityAttributes, EffectAttributes } from '../transport/DialogData';
import { TargetingType } from '../transport/TargetingType';
import { TargetingShape } from '../transport/TargetingShape';
import { ValueCombinationFunction } from '../transport/ValueCombinationFunction';
import { Accordion, AccordionToggle, AccordionCollapse } from './Accordion';
import { IGameContext } from './Game';
import { Dialog } from './Dialog';
import { KeyContext } from './KeyContext';
import { PropertyRow } from './PropertyRow';

export const AbilityPropertiesDialog = observer((props: IAbilityPropertiesProps) => {
    const { data, context } = props;
    return <Dialog context={context} show={computed(() => data.abilityAttributes != null)}>
        <AbilityProperties {...props} />
    </Dialog>;
});

const AbilityProperties = observer((props: IAbilityPropertiesProps) => {
    const abilityAttributes = props.data.abilityAttributes;
    if (abilityAttributes == null) {
        throw "Rendered with no data";
    }

    return <div className="abilityProperties" role="dialog" aria-label="Ability properties">
        <h2>{abilityAttributes.getName()}</h2>
        <AbilityAttributesScreen abilityAttributes={abilityAttributes} />
    </div>;
});

interface IAbilityPropertiesProps {
    data: IAbilityPropertiesData;
    context: IGameContext;
}

export const AbilityAttributesScreen = observer((props: IAbilityPropertiesData) => {
    const abilityAttributes = props.abilityAttributes;
    if (abilityAttributes == null) {
        throw "Rendered with no data";
    }

    return <div className="characterScreen__content">
        {abilityAttributes.description != null
            ? <div className="property__row property__row_multi-line">{abilityAttributes.description}</div>
            : <></>}
        <PropertyRow label="Activation" value={ActivationType[abilityAttributes.activation]} />
        <PropertyRow label="Activation condition" value={abilityAttributes.activationCondition} show={abilityAttributes.activationCondition != null} />
        <PropertyRow label="Targeting type" value={TargetingType[abilityAttributes.targetingType]}
            show={abilityAttributes.activation == ActivationType.Targeted} />
        <PropertyRow label="Targeting shape" value={TargetingShape[abilityAttributes.targetingShape]}
            show={abilityAttributes.activation == ActivationType.Targeted} />
        <PropertyRow label="Range" value={abilityAttributes.range} show={abilityAttributes.activation == ActivationType.Targeted} />
        <PropertyRow label="Heading deviation" value={abilityAttributes.headingDeviation}
            show={abilityAttributes.activation == ActivationType.Targeted} />
        <PropertyRow label="Energy cost" value={abilityAttributes.energyCost} show={abilityAttributes.energyCost != 0} />
        <PropertyRow label="Delay" value={abilityAttributes.delay} show={abilityAttributes.delay != 0} />
        <PropertyRow label="Cooldown" value={abilityAttributes.cooldown} show={abilityAttributes.cooldown != 0} />
        <PropertyRow label="Cooldown ticks left" value={abilityAttributes.cooldownTicksLeft} show={abilityAttributes.cooldownTicksLeft != 0} />
        <PropertyRow label="XP cooldown" value={abilityAttributes.xpCooldown} show={abilityAttributes.xpCooldown != 0} />
        <PropertyRow label="XP cooldown left" value={abilityAttributes.xpCooldownLeft} show={abilityAttributes.xpCooldownLeft != 0} />
        <PropertyRow label="Success condition" value={AbilitySuccessCondition[abilityAttributes.successCondition]}
            show={abilityAttributes.successCondition != AbilitySuccessCondition.Default} />
        <EffectsList effects={abilityAttributes.effects} />
    </div>;
});

export interface IAbilityPropertiesData {
    abilityAttributes: AbilityAttributes | null;
}

export const AbilitiesList = observer((props: IAbilitiesListData) => {
    var abilityKeyContext = new KeyContext();
    const abilities = Array.from(props.abilities.values(), a =>
        <div className="card" key={a.id}>
            <h4 className="card-header abilityScreen__header">
                <AccordionToggle className="btn abilityScreen__name"
                    bodyId={"ability-" + a.id} eventKey={a.id} keyContext={abilityKeyContext}
                >
                    {a.getName()}
                </AccordionToggle>
            </h4>
            <AccordionCollapse id={"ability-" + a.id} eventKey={a.id} keyContext={abilityKeyContext}>
                <div className="card-body">
                    <AbilityAttributesScreen abilityAttributes={a} />
                </div>
            </AccordionCollapse>
        </div>);
    return <div className="property__row_multi-line abilityList">
        <h3 className="abilityScreen__title">Abilities:</h3>
        <Accordion>
            {abilities}
        </Accordion>
    </div>;
});

export interface IAbilitiesListData {
    abilities: Map<string, AbilityAttributes>;
}

const EffectsList = observer(({ effects }: IEffectsListData) => {
    return <div className="abilityScreen__effects">
        <h5 className="abilityScreen__effectsTitle">Effects:</h5>
        {Array.from(effects.values(), e => <EffectPropertiesScreen key={e.id} effect={e} />)}
    </div>;
});

interface IEffectsListData {
    effects: Map<string, EffectAttributes>;
}

const EffectPropertiesScreen = observer(({ effect }: IEffectPropertiesData) => {
    return <div className="effectScreen__content">
        <PropertyRow label="Type" value={EffectType[effect.type]} />
        <PropertyRow label="Targets activator" value={effect.shouldTargetActivator} show={effect.shouldTargetActivator} />
        <PropertyRow label="Amount" value={effect.amount}
            show={effect.amount != null && effect.type != EffectType.AddAbility && effect.type != EffectType.ChangeRace} />
        <PropertyRow label="Secondary amount" value={effect.secondaryAmount}
            show={effect.secondaryAmount != null && effect.secondaryAmount != 0} />
        <PropertyRow label="Combination" value={ValueCombinationFunction[effect.function]} show={effect.function != ValueCombinationFunction.Sum} />
        <PropertyRow label="Name" value={effect.targetName}
            show={effect.type == EffectType.ChangeProperty || effect.type == EffectType.AddAbility || effect.type == EffectType.ChangeRace} />
        <PropertyRow label="Duration" value={effect.durationAmount} show={effect.duration == EffectDuration.UntilTimeout} />
    </div>
});

interface IEffectPropertiesData {
    effect: EffectAttributes;
}