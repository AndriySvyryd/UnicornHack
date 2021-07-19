import React from 'react';
import { action, computed } from 'mobx';
import { observer } from 'mobx-react';
import { Ability } from '../transport/Model';
import { GameQueryType } from '../transport/GameQueryType';
import { ActorAction } from "../transport/ActorAction";
import { DialogData } from '../transport/DialogData';
import { Dialog } from './Dialog';
import { IGameContext } from './Game';

export const AbilitySelectionDialog = observer((props: IAbilitySelectionProps) => {
    const { data, context } = props;
    return <Dialog context={context} show={computed(() => data.abilitySlot != null)} className="abilitySlotSelection"
        title={computed(() => 'Select ability for ' +
            (data.abilitySlot === 0
                ? 'default melee attack'
                : data.abilitySlot === 1
                    ? 'default ranged attack'
                    : "slot " + (data.abilitySlot ?? 0 - 1)))}
    >
        <AbilitySelection {...props} />
    </Dialog>;
});

const AbilitySelection = observer(({ context, data }: IAbilitySelectionProps) => {
    const slot = data.abilitySlot;
    if (slot == null) {
        return <></>;
    }

    var abilities = Array.from(data.slottableAbilities.values(),
        i => <AbilitySelectionLine ability={i} slot={slot} key={i.id} context={context} />);

    abilities.push(<AbilitySelectionLine ability={null} slot={slot} key={-1} context={context} />);

    return <ul>{abilities}</ul>;
});

interface IAbilitySelectionProps {
    data: DialogData;
    context: IGameContext;
}

@observer
class AbilitySelectionLine extends React.PureComponent<IAbilityLineProps, {}> {
    @action.bound
    setAbilitySlot(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.showDialog(GameQueryType.Clear);
            this.props.context.performAction(
                ActorAction.SetAbilitySlot, this.props.ability?.id ?? 0, this.props.slot);
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
        var name = "none";
        if (this.props.ability !== null) {
            name = this.props.ability.name;
            const abilitySlot = this.props.ability.slot;

            if (abilitySlot !== null) {
                name = `[${(abilitySlot - 1)}] ` + name;
            }

            if (abilitySlot == this.props.slot) {
                return <li>{name}</li>;
            }
        }

        return <li><a className="contextLink" tabIndex={(100 + (this.props.ability?.id ?? 0))} role="button"
            onClick={this.setAbilitySlot} onKeyPress={this.setAbilitySlot} onContextMenu={this.showAttributes}
        >
            {name}
        </a></li>;
    }
}

interface IAbilityLineProps {
    slot: number;
    ability: Ability | null;
    context: IGameContext;
}