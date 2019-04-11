import * as React from 'React';
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { Item, EquipableSlot } from '../transport/Model';
import { PlayerAction } from "../transport/PlayerAction";
import { TooltipTrigger } from './TooltipTrigger';

export const Inventory = observer((props: IInventoryProps) => {
    const items = Array.from(props.items.values(), i =>
        <InventoryLine item={i} key={i.id} performAction={props.performAction} />);

    return <div className="inventory">{items}</div>;
});

interface IInventoryProps {
    items: Map<string, Item>;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}

@observer
class InventoryLine extends React.Component<IItemProps, {}> {
    @action.bound
    unequip(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.performAction(PlayerAction.UnequipItem, this.props.item.id, null);
        }
    }

    @action.bound
    drop(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.performAction(PlayerAction.DropItem, this.props.item.id, null);
        }
    }

    render() {
        const item = this.props.item;
        const itemLine: any[] = [];
        if (item.equippedSlot !== null) {
            itemLine.push(' [');
            itemLine.push(
                <TooltipTrigger
                    key="eqipped"
                    id={`tooltip-unequip-${item.id}`}
                    delay={100}
                    tooltip={'Unequip from ' + item.equippedSlot.name}
                >
                    <a tabIndex={0} onClick={this.unequip} onKeyPress={this.unequip}>{item.equippedSlot.shortName}</a>
                </TooltipTrigger>);
            itemLine.push('] ');
        } else if (item.equippableSlots.size !== 0) {
            itemLine.push(' (');

            let first = true;
            item.equippableSlots.forEach(s => {
                if (!first) {
                    itemLine.push(' ');
                }
                first = false;
                itemLine.push(<Equip key={s.id} slot={s} {...this.props} />);
            });
            itemLine.push(')');
        }

        itemLine.push(' ');
        itemLine.push(<a tabIndex={0} key="drop" onClick={this.drop} onKeyPress={this.drop}>drop</a>);

        return <div>{item.name}{itemLine}</div>;
    }
}

interface IItemProps {
    item: Item;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}

@observer
class Equip extends React.Component<IEquipProps, {}> {
    @action.bound
    equip(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.performAction(PlayerAction.EquipItem, this.props.item.id, this.props.slot.id);
        }
    }

    render() {
        const { item, slot } = this.props;
        return <TooltipTrigger
            id={`tooltip-equip-${slot.id}-${item.id}`}
            delay={100}
            tooltip={'Equip to ' + slot.name}
        >
            <a tabIndex={0} onClick={this.equip} onKeyPress={this.equip}>{slot.shortName}</a>
        </TooltipTrigger>;
    }
}

interface IEquipProps {
    item: Item;
    slot: EquipableSlot;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}