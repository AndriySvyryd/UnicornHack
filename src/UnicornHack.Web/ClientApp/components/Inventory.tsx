import * as React from 'React';
import { observer } from 'mobx-react';
import { Item } from '../transport/Model';
import { PlayerAction } from "../transport/PlayerAction";

@observer
export class Inventory extends React.Component<IInventoryProps, {}> {
    render() {
        const items = Array.from(this.props.items.values(),
            i => <InventoryLine item={i} key={i.id} performAction={this.props.performAction} />);

        return (<div className="inventory">{items}</div>);
    }
}

interface IInventoryProps {
    items: Map<string, Item>;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}

@observer
class InventoryLine extends React.Component<IItemProps, {}> {
    render() {
        const itemLine: any[] = [];
        if (this.props.item.equippedSlot !== null) {
            itemLine.push(' [');
            itemLine.push(<a tabIndex={0} key="eqipped" onClick={
                () => this.props.performAction(PlayerAction.UnequipItem, this.props.item.id, null)
            }>{this.props.item.equippedSlot}</a>);
            itemLine.push('] ');
        } else if (this.props.item.equippableSlots.size !== 0) {
            itemLine.push(' (');

            let first = true;
            this.props.item.equippableSlots.forEach(s => {
                if (!first) {
                    itemLine.push(' ');
                }
                first = false;
                itemLine.push(
                    <a tabIndex={0} key={s.id} onClick={
                        () => this.props.performAction(PlayerAction.EquipItem, this.props.item.id, s.id)
                    }>{s.name}</a>);
            });
            itemLine.push(')');
        }

        itemLine.push(' ');
        itemLine.push(<a tabIndex={0} key="drop" onClick={
            () => this.props.performAction(PlayerAction.DropItem, this.props.item.id, null)
        }>drop</a>);

        return (<div>{this.props.item.name}{itemLine}</div>);
    }
}

interface IItemProps {
    item: Item;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}