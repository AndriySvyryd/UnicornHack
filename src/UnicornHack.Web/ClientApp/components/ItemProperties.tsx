import * as React from 'React';
import { computed } from 'mobx';
import { observer } from 'mobx-react';
import { ItemAttributes } from '../transport/DialogData';
import { ItemType } from '../transport/ItemType';
import { ItemComplexity } from '../transport/ItemComplexity';
import { Material } from '../transport/Material';
import { capitalize } from '../Util';
import { AbilitiesList } from './AbilityProperties';
import { Dialog } from './Dialog';
import { IGameContext } from './Game';
import { PropertyRow } from './PropertyRow';

export const ItemPropertiesDialog = observer((props: IItemPropertiesProps) => {
    const { data, context } = props;
    return <Dialog context={context} show={computed(() => data.itemAttributes != null)}>
        <ItemProperties {...props} />
    </Dialog>;
});

const ItemProperties = observer((props: IItemPropertiesProps) => {
    const itemAttributes = props.data.itemAttributes;
    if (itemAttributes == null) {
        return <></>
    }

    const title = capitalize(itemAttributes.name || "Unknown item");
    return <div className="itemProperties" role="dialog" aria-label="Item properties">
        <h2>{title}</h2>
        <div>{itemAttributes.description}</div>
        {itemAttributes.name == null ? <></> : <ItemAttributesScreen itemAttributes={itemAttributes} />}
    </div>;
});

interface IItemPropertiesProps {
    data: IItemPropertiesData;
    context: IGameContext;
}

const ItemAttributesScreen = observer((props: IItemPropertiesData) => {
    const itemAttributes = props.itemAttributes;
    if (itemAttributes == null) {
        return <></>;
    }

    const slots: string[] = [];
    if (itemAttributes.equippableSlots.size !== 0) {
        let first = true;
        itemAttributes.equippableSlots.forEach(slot => {
            if (!first) {
                slots.push(', ');
            }
            first = false;
            slots.push(slot.name);
        });
    }

    // TODO: get localized enum strings, convert enums to const
    return <div className="characterScreen__content">
        <PropertyRow label="Type" value={ItemType[itemAttributes.type]} />
        <PropertyRow label="Material" value={Material[itemAttributes.material]} />
        <PropertyRow label="Size" value={itemAttributes.size} />
        <PropertyRow label="Weight" value={itemAttributes.weight} />
        <PropertyRow label="Hindrance" value={itemAttributes.hindrance} />
        <PropertyRow label="Complexity" value={ItemComplexity[itemAttributes.complexity]} />
        <PropertyRow label="Required might" value={itemAttributes.requiredMight} />
        <PropertyRow label="Required speed" value={itemAttributes.requiredSpeed} />
        <PropertyRow label="Required focus" value={itemAttributes.requiredFocus} />
        <PropertyRow label="Required perception" value={itemAttributes.requiredPerception} />
        <div className="property__row property__row_wide">
            <div className="property__label">Equippable slots:</div><div className="property__value">{slots}</div>
        </div>
        <AbilitiesList abilities={itemAttributes.abilities} />
    </div>;
});

export interface IItemPropertiesData {
    itemAttributes: ItemAttributes | null;
}