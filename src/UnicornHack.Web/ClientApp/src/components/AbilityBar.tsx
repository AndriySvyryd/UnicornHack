import React from 'react';
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { Ability } from '../transport/Model';
import { ActorAction } from '../transport/ActorAction';
import { GameQueryType } from '../transport/GameQueryType';
import { ActivationType } from '../transport/ActivationType';
import { IGameContext } from './Game';

export const AbilityBar = observer((props: IAbilityBarProps) => {
    const slots = new Array(8);
    for (var i = 0; i < slots.length; i++) {
        var slottedAbility = null;
        for (var ability of props.context.player.abilities.values()) {
            if (ability.slot === i) {
                slottedAbility = ability;
                break;
            }
        }

        slots[i] = <AbilityLine ability={slottedAbility} context={props.context} slot={i} key={i} />
    }

    return <div className="abilityBar">{slots}</div>;
});

interface IAbilityBarProps {
    context: IGameContext;
}

@observer
class AbilityLine extends React.PureComponent<IAbilityProps, {}> {
    @action.bound
    showAbilities(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.showDialog(GameQueryType.SlottableAbilities, this.props.slot);
            event.preventDefault();
        }
    }

    @action.bound
    useAbilitySlot(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.performAction(ActorAction.UseAbilitySlot, this.props.slot, null);
            event.preventDefault();
        }
    }

    @action.bound
    useTargetedAbility(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.startTargeting(this.props.slot);
            event.preventDefault();
        }
    }

    @action.bound
    showAttributes(event: React.MouseEvent<HTMLAnchorElement>) {
        if (this.props.ability !== null) {
            this.props.context.showDialog(GameQueryType.AbilityAttributes, this.props.ability.id);
        }
        event.preventDefault();
    }

    render() {
        const slot = <a tabIndex={(this.props.slot + 1) * 2} role="button" onClick={this.showAbilities} onKeyPress={this.showAbilities}>
            <span className="abilityBar__slot">{this.props.slot === -1 ? 'D' : this.props.slot + 1}:</span>
        </a>;

        var ability = <span>{this.props.ability?.name ?? ''}</span>;
        if (this.props.ability !== null
            && (this.props.ability.activation & ActivationType.WhileToggled) == 0) {
            if (this.props.ability.cooldownTick == null
                && this.props.ability.cooldownXpLeft == null) {
                const action = (this.props.ability.activation & ActivationType.Targeted) !== 0
                    ? this.useTargetedAbility
                    : this.useAbilitySlot;
                ability = <a tabIndex={(this.props.slot + 1) * 2 + 1} role="button"
                    onClick={action} onKeyPress={action} onContextMenu={this.showAttributes}
                >
                    {ability}
                </a>;
            } else {
                var timeout = '[';
                if (this.props.ability.cooldownTick != null) {
                    timeout += (this.props.ability.cooldownTick - this.props.context.player.currentTick) / 100.0 + ' AUT';
                }

                if (this.props.ability.cooldownXpLeft != null) {
                    timeout += this.props.ability.cooldownXpLeft + ' XP';
                }

                timeout = timeout.trimRight();
                timeout += ']';

                ability = <span className="contextLink" onContextMenu={this.showAttributes}>{ability} {timeout}</span>;
            }
        } else {
            ability = <span className="contextLink" onContextMenu={this.showAttributes}>{ability}</span>;
        }

        return <div>{slot} {ability}</div>;
    }
}

interface IAbilityProps {
    slot: number;
    ability: Ability | null;
    context: IGameContext;
}