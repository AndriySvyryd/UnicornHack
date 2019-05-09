import * as React from 'React';
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { Ability } from '../transport/Model';
import { PlayerAction } from '../transport/PlayerAction';
import { GameQueryType } from '../transport/GameQueryType';
import { ActivationType } from '../transport/ActivationType';
import { IGameContext } from './Game';

export const AbilityBar = observer((props: IAbilityBarProps) => {
    const slots = new Array(9);
    for (var i = 1; i < slots.length; i++) {
        const slot = i - 1;
        var slottedAbility = null;
        for (var ability of props.context.player.abilities.values()) {
            if (ability.slot === slot) {
                slottedAbility = ability;
                break;
            }
        }

        slots[i] = <AbilityLine ability={slottedAbility} context={props.context} slot={slot} key={i} />
    }

    return <div className="abilityBar">{slots}</div>;
});

interface IAbilityBarProps {
    context: IGameContext;
}

@observer
class AbilityLine extends React.Component<IAbilityProps, {}> {
    @action.bound
    showAbilities(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.showDialog(GameQueryType.SlottableAbilities, this.props.slot);
        }
    }

    @action.bound
    useAbilitySlot(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.performAction(PlayerAction.UseAbilitySlot, this.props.slot, null);
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

        // TODO: Activate targeting mode for targetted abilities        

        var ability = <span>{this.props.ability === null ? "" : this.props.ability.name}</span>;
        if (this.props.ability !== null
            && (this.props.ability.activation & ActivationType.Manual) !== 0) {
            if (this.props.ability.cooldownTick == null
                && this.props.ability.cooldownXpLeft == null) {
                ability = <a tabIndex={(this.props.slot + 1) * 2 + 1} role="button"
                    onClick={this.useAbilitySlot} onKeyPress={this.useAbilitySlot} onContextMenu={this.showAttributes}
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

                ability = <span onContextMenu={this.showAttributes}>{ability} {timeout}</span>;
            }
        } else {
            ability = <span onContextMenu={this.showAttributes}>{ability}</span>;
        }

        return <div>{slot} {ability}</div>;
    }
}

interface IAbilityProps {
    slot: number;
    ability: Ability | null;
    context: IGameContext;
}