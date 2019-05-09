import * as React from 'React';
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { GameQueryType } from '../transport/GameQueryType';
import { Item, EquipableSlot } from '../transport/Model';
import { PlayerAction } from "../transport/PlayerAction";
import { IGameContext } from './Game';
import { TooltipTrigger } from './TooltipTrigger';

export const Inventory = observer((props: IInventoryProps) => {
    const items = Array.from(props.context.player.inventory.values(), i =>
        <InventoryLine item={i} key={i.id} context={props.context} />);
    return <div className="inventory">{items}</div>;
});

interface IInventoryProps {
    context: IGameContext;
}

@observer
class InventoryLine extends React.Component<IItemProps, {}> {
    @action.bound
    unequip(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.performAction(PlayerAction.UnequipItem, this.props.item.id, null);
        }
    }

    @action.bound
    drop(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.performAction(PlayerAction.DropItem, this.props.item.id, null);
        }
    }

    @action.bound
    showAttributes(event: React.MouseEvent<HTMLDivElement>) {
        this.props.context.showDialog(GameQueryType.ItemAttributes, this.props.item.id);
        event.preventDefault();
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
                    <a tabIndex={0} role="button" onClick={this.unequip} onKeyPress={this.unequip}>{item.equippedSlot.shortName}</a>
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
        itemLine.push(<a tabIndex={0} role="button" key="drop" onClick={this.drop} onKeyPress={this.drop}>drop</a>);

        return <div onContextMenu={this.showAttributes}>{item.name}{itemLine}</div>;
    }
}

interface IItemProps {
    item: Item;
    context: IGameContext;
}

@observer
class Equip extends React.Component<IEquipProps, {}> {
    @action.bound
    equip(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.performAction(PlayerAction.EquipItem, this.props.item.id, this.props.slot.id);
        }
    }

    render() {
        const { item, slot } = this.props;
        return <TooltipTrigger
            id={`tooltip-equip-${slot.id}-${item.id}`}
            delay={100}
            tooltip={'Equip to ' + slot.name}
        >
            <a tabIndex={0} role="button" onClick={this.equip} onKeyPress={this.equip}>{slot.shortName}</a>
        </TooltipTrigger>;
    }
}

interface IEquipProps {
    item: Item;
    slot: EquipableSlot;
    context: IGameContext;
}