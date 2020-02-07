import React from 'react';
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
    return <Dialog context={context} show={computed(() => data.itemAttributes != null)} className="itemProperties"
        title={computed(() => capitalize(data.itemAttributes == null || data.itemAttributes.name == null ? "Unknown item" : data.itemAttributes.name))}
    >
        <ItemAttributesScreen {...props} />
    </Dialog>;
});

interface IItemPropertiesProps {
    data: IItemPropertiesData;
    context: IGameContext;
}

interface IItemPropertiesData {
    itemAttributes: ItemAttributes | null;
}

const ItemAttributesScreen = observer((props: IItemPropertiesProps) => {
    const itemAttributes = props.data.itemAttributes;
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
    return <>
        <div className="property__row_multi-line">{itemAttributes.description}</div>
        <div className="characterScreen__content">
            <PropertyRow label="Type" value={ItemType[itemAttributes.type]} />
            <PropertyRow label="Material" value={Material[itemAttributes.material]} />
            <PropertyRow label="Weight" value={itemAttributes.weight} />
            <PropertyRow label="Complexity" value={ItemComplexity[itemAttributes.complexity]} show={itemAttributes.complexity != ItemComplexity.Normal} />
            <PropertyRow label="Required might" value={itemAttributes.requiredMight} show={itemAttributes.requiredMight != 0} />
            <PropertyRow label="Required speed" value={itemAttributes.requiredSpeed} show={itemAttributes.requiredSpeed != 0} />
            <PropertyRow label="Required focus" value={itemAttributes.requiredFocus} show={itemAttributes.requiredFocus != 0} />
            <PropertyRow label="Required perception" value={itemAttributes.requiredPerception} show={itemAttributes.requiredPerception != 0} />
            <PropertyRow label="Equippable slots" classname="property__row property__row_wide" value={slots} show={slots.length != 0} />
            <AbilitiesList abilities={itemAttributes.abilities} />
        </div>
    </>;
});