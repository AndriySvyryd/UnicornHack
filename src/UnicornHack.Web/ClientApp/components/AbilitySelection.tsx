import * as React from 'React';
import { observer } from 'mobx-react';
import { coalesce } from '../Util';
import { Ability } from '../transport/Model';
import { GameQueryType } from '../transport/GameQueryType';
import { PlayerAction } from "../transport/PlayerAction";
import { UIData } from '../transport/UIData';

@observer
export class AbilitySelection extends React.Component<IAbilitySelectionProps, {}> {
    container: React.RefObject<HTMLDivElement>;

    constructor(props: IAbilitySelectionProps) {
        super(props);

        this.container = React.createRef();
    }

    componentDidUpdate(prevProps: any) {
        if (this.props.data.abilitySlot !== null
            && this.container.current !== null) {
            this.container.current.focus();
        }
    }

    render() {
        var abilities = Array.from(this.props.data.slottableAbilities.values(),
            i => <AbilitySelectionLine ability={i} slot={coalesce(this.props.data.abilitySlot, -3)} key={i.id} performAction={this.props.performAction} queryGame={this.props.queryGame} />);

        abilities.push(<AbilitySelectionLine ability={null} slot={coalesce(this.props.data.abilitySlot, -3)} key={-1} performAction={this.props.performAction} queryGame={this.props.queryGame} />);

        return <div className="dialog__overlay" ref={this.container} tabIndex={100} style={{
            display: this.props.data.abilitySlot === null ? 'none' : 'block'
        }} onClick={() => this.props.queryGame(GameQueryType.Clear)}>
            <div className="abilitySlotSelection__wrapper">
                <div className="abilitySlotSelection" onClick={(e) => e.stopPropagation()}>
                    <div><h4>Select ability:</h4></div>
                    <br />
                    <ul>{abilities}</ul>
                </div>
            </div>
        </div>;
    }
}

interface IAbilitySelectionProps {
    data: UIData;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
}

@observer
export class AbilitySelectionLine extends React.Component<IAbilityLineProps, {}> {
    render() {
        var name = "none";
        if (this.props.ability !== null) {
            name = this.props.ability.name;
            const abilitySlot = this.props.ability.slot;

            if (abilitySlot !== null) {
                name = `(${abilitySlot + 1}) ` + name;
            }

            if (abilitySlot == this.props.slot) {
                return <li>{name}</li>;
            }
        }

        const setAbilitySlot: (() => void) = () => {
            this.props.queryGame(GameQueryType.Clear);
            this.props.performAction(
                PlayerAction.SetAbilitySlot, this.props.ability === null ? 0 : this.props.ability.id, this.props.slot);
        };
        return <li><a tabIndex={(this.props.ability === null ? 0 : 100 + this.props.ability.id)}
            onClick={setAbilitySlot} onKeyPress={(e) => { if (e.key == 'Enter') { setAbilitySlot() } }}>{name}</a></li>;
    }
}

interface IAbilityLineProps {
    slot: number;
    ability: Ability | null;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
}