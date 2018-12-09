import * as React from 'React';
import { observer } from 'mobx-react';
import { Ability } from '../transport/Model';
import { PlayerAction } from "../transport/PlayerAction";
import { GameQueryType } from '../transport/GameQueryType';
import { ActivationType } from '../transport/ActivationType';

@observer
export class AbilityBar extends React.Component<IAbilityBarProps, {}> {
    render() {
        const slots = new Array(9);
        for (var i = 0; i < slots.length; i++) {
            const slot = i - 1;
            var slottedAbility = null;
            for (var ability of this.props.abilities.values()) {
                if (ability.slot === slot) {
                    slottedAbility = ability;
                    break;
                }
            }

            slots[i] = <AbilityLine ability={slottedAbility} slot={slot} key={i} performAction={this.props.performAction} queryGame={this.props.queryGame} />
        }

        return (<div className="frame">{slots}</div>);
    }
}

interface IAbilityBarProps {
    abilities: Map<string, Ability>;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
}

@observer
export class AbilityLine extends React.Component<IAbilityProps, {}> {
    render() {
        const slot = <a tabIndex={(this.props.slot + 1) * 2}
            onClick={() => this.props.queryGame(GameQueryType.SlottableAbilities, this.props.slot)}>
            <span className="ability__slot">{this.props.slot === -1 ? "D" : this.props.slot + 1}:</span></a>;

        // TODO: Activate targetting mode for targetted abilities        

        var ability = <span>{this.props.ability === null ? "" : this.props.ability.name}</span>;
        if (this.props.ability !== null
            && (this.props.ability.activation & ActivationType.ManualActivation) !== 0) {
            ability = <a tabIndex={(this.props.slot + 1) * 2 + 1}
                onClick={() => this.props.performAction(PlayerAction.UseAbilitySlot, this.props.slot, null)
                }>{ability}</a>;
        }

        return <div>{slot} {ability}</div>;
    }
}

interface IAbilityProps {
    slot: number;
    ability: Ability | null;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
}