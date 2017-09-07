import * as React from 'React';
import { Property } from './Model';

export class PropertyList extends React.Component<IPropertyListProps, {}> {
    render() {
        const abilities = this.props.properties.map(p => <PropertyLine property={p} key={p.name} />);
        return (<div className="frame">{abilities}</div>);
    }
}

interface IPropertyListProps {
    properties: Property[]
}

export class PropertyLine extends React.Component<IPropertyLineProps, {}> {
    render() {
        return <div>{this.props.property.displayName}</div>;
    }
}

interface IPropertyLineProps {
    property: Property
}