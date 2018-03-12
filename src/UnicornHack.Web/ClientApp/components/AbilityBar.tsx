import * as React from 'React';
import { observer } from 'mobx-react';
import { Ability } from '../transport/Model';
import { PlayerAction } from "../transport/PlayerAction";

@observer
export class AbilityBar extends React.Component<IAbilityBarProps, {}> {
    render() {
        const abilities = Array.from(this.props.abilities.values(),
            a => <AbilityLine ability={a} performAction={this.props.performAction} key={a.id} />);
        return (<div className="frame">{abilities}</div>);
    }
}

interface IAbilityBarProps {
    abilities: Map<string, Ability>;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}

@observer
export class AbilityLine extends React.Component<IAbilityProps, {}> {
    render() {
        if (this.props.ability.isDefault) {
            return <div className="font-weight-bold">{this.props.ability.name}</div>;
        } else {
            return (<div><a tabIndex={0} onClick={
                        () => this.props.performAction(PlayerAction.ChooseDefaultAttack, this.props.ability.id, null)
}>{this.props.ability.name}</a>
                    </div>);
        }
    }
}

interface IAbilityProps {
    ability: Ability;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}