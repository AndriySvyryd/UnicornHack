import * as React from 'React';
import { observer } from 'mobx-react';
import { Property } from '../transport/Model';

@observer
export class PropertyList extends React.Component<IPropertyListProps, {}> {
    render() {
        const abilities: Array<Object> = [];
        const values = this.props.properties.values();
        let v = values.next();
        while (!v.done) {
            if (v.value.displayName !== "") {
                abilities.push(<PropertyLine property={v.value} key={v.value.name} />);
            }
            v = values.next();
        }
        return (<div className="frame">{abilities}</div>);
    }
}

interface IPropertyListProps {
    properties: Map<string, Property>
}

@observer
export class PropertyLine extends React.Component<IPropertyLineProps, {}> {
    render() {
        return <div>{this.props.property.displayName}</div>;
    }
}

interface IPropertyLineProps {
    property: Property
}