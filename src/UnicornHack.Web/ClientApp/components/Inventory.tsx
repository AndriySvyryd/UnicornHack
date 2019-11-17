import React from 'react';
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { GameQueryType } from '../transport/GameQueryType';
import { Item, EquipableSlot, PlayerInventory } from '../transport/DialogData';
import { PlayerAction } from "../transport/PlayerAction";
import { IGameContext } from './Game';
import { TooltipTrigger } from './TooltipTrigger';

export const InventoryScreen = observer((props: IInventoryProps) => {
    const items = Array.from(props.playerInventory.items.values(), i =>
        <InventoryLine item={i} key={i.id} context={props.context} />);
    return <div className="inventory">{items}</div>;
});

interface IInventoryProps {
    context: IGameContext;
    playerInventory: PlayerInventory;
}

@observer
class InventoryLine extends React.PureComponent<IItemProps, {}> {
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
                    tooltip={'Unequip from ' + item.equippedSlot.name}
                >
                    <a className="annotatedText" tabIndex={0} role="button" onClick={this.unequip} onKeyPress={this.unequip}>
                        {item.equippedSlot.shortName}
                    </a>
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

        return <div className="contextLink" onContextMenu={this.showAttributes}>{item.name}{itemLine}</div>;
    }
}

interface IItemProps {
    item: Item;
    context: IGameContext;
}

@observer
class Equip extends React.PureComponent<IEquipProps, {}> {
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
            tooltip={'Equip to ' + slot.name}
        >
            <a className="annotatedText" tabIndex={0} role="button" onClick={this.equip} onKeyPress={this.equip}>{slot.shortName}</a>
        </TooltipTrigger>;
    }
}

interface IEquipProps {
    item: Item;
    slot: EquipableSlot;
    context: IGameContext;
}