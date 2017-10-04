import * as React from 'React';
import { Item, ItemType } from '../transport/Model';

export class Inventory extends React.Component<IInventoryProps, {}> {
    shouldComponentUpdate(nextProps: IInventoryProps): boolean {
        return this.props.items !== nextProps.items;
    }

    render() {
        const items = this.props.items.map(i => <InventoryLine item={i} key={i.id} performAction={this.props.performAction} />);

        return (<div className="frame">{items}</div>);
    }
}

interface IInventoryProps {
    items: Item[];
    performAction: (action: string, target: (number | null), target2: (number | null)) => void;
}

class InventoryLine extends React.Component<IItemProps, {}> {
    shouldComponentUpdate(nextProps: IItemProps): boolean {
        return this.props.item.name !== nextProps.item.name
            || this.props.item.equippableSlots !== nextProps.item.equippableSlots
            || this.props.item.equippedSlot !== nextProps.item.equippedSlot
            || this.props.item.type !== nextProps.item.type
            || this.props.item.equippableSlots !== nextProps.item.equippableSlots
            || this.props.item.equippableSlots !== nextProps.item.equippableSlots ;
    }

    render() {
        const itemLine: any[] = [];
        if (this.props.item.equippedSlot !== null) {
            itemLine.push(' [');
            itemLine.push(<a className="itemAction" key="eqipped" onClick={
                () => this.props.performAction('UNEQUIP', this.props.item.id, null)
            }>{this.props.item.equippedSlot}</a>);
            itemLine.push('] ');
        } else if (this.props.item.equippableSlots !== null) {
            itemLine.push(' (');
            this.props.item.equippableSlots.map((s, i) => {
                if (i !== 0) {
                    itemLine.push(' ');
                }
                itemLine.push(
                    <a className="itemAction" key={s.id} onClick={
                        () => this.props.performAction('EQUIP', this.props.item.id, s.id)
                    }>{s.name}</a>);
            });
            itemLine.push(')');
        }

        if (this.props.item.type & ItemType.Potion) {
            itemLine.push(' ');
            itemLine.push(<a className="itemAction" key="quaff" onClick={
                () => this.props.performAction('QUAFF', this.props.item.id, null)
            }>quaff</a>);
        }

        itemLine.push(' ');
        itemLine.push(<a className="itemAction" key="drop" onClick={
            () => this.props.performAction('DROP', this.props.item.id, null)
        }>drop</a>);

        return (<div>{this.props.item.name}{itemLine}</div>);
    }
}

interface IItemProps {
    item: Item;
    performAction: (action: string, target: (number | null), target2: (number | null)) => void;
}