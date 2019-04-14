import * as React from 'React';
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { Ability } from '../transport/Model';
import { GameQueryType } from '../transport/GameQueryType';
import { PlayerAction } from "../transport/PlayerAction";
import { DialogData } from '../transport/DialogData';
import { coalesce } from '../Util';

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

    @action.bound
    clear(event: React.MouseEvent<HTMLDivElement>) {
        this.props.queryGame(GameQueryType.Clear);
        event.preventDefault();
    }

    stopPropagation(e: React.SyntheticEvent<{}>) {
        e.stopPropagation;
    }

    render() {
        if (this.props.data.abilitySlot === null) {
            return <></>
        }

        var abilities = Array.from(this.props.data.slottableAbilities.values(),
            i => <AbilitySelectionLine ability={i} slot={coalesce(this.props.data.abilitySlot, -3)}
                key={i.id} performAction={this.props.performAction} queryGame={this.props.queryGame} />);

        abilities.push(<AbilitySelectionLine ability={null} slot={coalesce(this.props.data.abilitySlot, -3)}
            key={-1} performAction={this.props.performAction} queryGame={this.props.queryGame} />);

        return <div className="dialog__overlay" ref={this.container} tabIndex={100} onClick={this.clear} onContextMenu={this.clear}>
            <div className="abilitySlotSelection" onClick={this.stopPropagation} role="dialog" aria-labelledby="abilitySelection">
                <h4 id="abilitySelection">Select ability for slot {this.props.data.abilitySlot}:</h4>
                <br />
                <ul>{abilities}</ul>
            </div>
        </div>;
    }
}

interface IAbilitySelectionProps {
    data: DialogData;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
}

@observer
class AbilitySelectionLine extends React.Component<IAbilityLineProps, {}> {
    @action.bound
    setAbilitySlot(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.queryGame(GameQueryType.Clear);
            this.props.performAction(
                PlayerAction.SetAbilitySlot, this.props.ability === null ? 0 : this.props.ability.id, this.props.slot);
        }
    }

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

        return <li><a tabIndex={(this.props.ability === null ? 0 : 100 + this.props.ability.id)}
            onClick={this.setAbilitySlot} onKeyPress={this.setAbilitySlot}
        >
            {name}
        </a></li>;
    }
}

interface IAbilityLineProps {
    slot: number;
    ability: Ability | null;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
}