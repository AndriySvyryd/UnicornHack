import * as React from 'React';
import { Ability } from './Model';

export class AbilityBar extends React.Component<IAbilityBarProps, {}> {
    render() {
        const abilities = this.props.abilities.map(a => <AbilityLine ability={a} performAction={this.props.performAction} key={a.id} />);
        return (<div className="frame">{abilities}</div>);
    }
}

interface IAbilityBarProps {
    abilities: Ability[];
    performAction: (action: string, target: (number | null), target2: (number | null)) => void;
}

export class AbilityLine extends React.Component<IAbilityProps, {}> {
    render() {
        if (this.props.ability.isDefault) {
            return <div>{'*' + this.props.ability.name}</div>;
        }
        else {
            return (<div><a onClick={
                        () => this.props.performAction('MAKEDEFAULT', this.props.ability.id, null)}>{this.props.ability.name}</a>
                    </div>);
        }
    }
}

interface IAbilityProps {
    ability: Ability;
    performAction: (action: string, target: (number | null), target2: (number | null)) => void;
}