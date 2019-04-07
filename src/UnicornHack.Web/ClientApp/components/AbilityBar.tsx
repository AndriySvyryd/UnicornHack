import * as React from 'React';
import { observer } from 'mobx-react';
import { Ability, Player } from '../transport/Model';
import { PlayerAction } from '../transport/PlayerAction';
import { GameQueryType } from '../transport/GameQueryType';
import { ActivationType } from '../transport/ActivationType';
import { action } from 'mobx';

export const AbilityBar = observer((props: IAbilityBarProps) => {
    const slots = new Array(9);
    for (var i = 1; i < slots.length; i++) {
        const slot = i - 1;
        var slottedAbility = null;
        for (var ability of props.player.abilities.values()) {
            if (ability.slot === slot) {
                slottedAbility = ability;
                break;
            }
        }

        slots[i] = <AbilityLine ability={slottedAbility} player={props.player} slot={slot} key={i}
            performAction={props.performAction} queryGame={props.queryGame} />
    }

    return <div className="abilityBar">{slots}</div>;
});

interface IAbilityBarProps {
    player: Player
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
}

@observer
class AbilityLine extends React.Component<IAbilityProps, {}> {
    @action.bound
    showAbilities(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.queryGame(GameQueryType.SlottableAbilities, this.props.slot);
        }
    }

    @action.bound
    useAbilitySlot(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.performAction(PlayerAction.UseAbilitySlot, this.props.slot, null);
        }
    }

    render() {
        const slot = <a tabIndex={(this.props.slot + 1) * 2} onClick={this.showAbilities} onKeyPress={this.showAbilities}>
            <span className="abilityBar__slot">{this.props.slot === -1 ? 'D' : this.props.slot + 1}:</span>
        </a>;

        // TODO: Activate targeting mode for targetted abilities        

        var ability = <span>{this.props.ability === null ? "" : this.props.ability.name}</span>;
        if (this.props.ability !== null
            && (this.props.ability.activation & ActivationType.ManualActivation) !== 0) {
            if (this.props.ability.cooldownTick == null
                && this.props.ability.cooldownXpLeft == null) {
                ability = <a tabIndex={(this.props.slot + 1) * 2 + 1} onClick={this.useAbilitySlot} onKeyPress={this.useAbilitySlot}
                >{ability}</a>;
            } else {
                var timeout = '[';
                if (this.props.ability.cooldownTick != null) {
                    timeout += (this.props.ability.cooldownTick - this.props.player.currentTick) / 100.0 + ' AUT';
                }

                if (this.props.ability.cooldownXpLeft != null) {
                    timeout += this.props.ability.cooldownXpLeft + ' XP';
                }

                timeout = timeout.trimRight();
                timeout += ']';

                ability = <span>{ability} {timeout}</span>;
            }
        }

        return <div>{slot} {ability}</div>;
    }
}

interface IAbilityProps {
    slot: number;
    ability: Ability | null;
    player: Player
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
}