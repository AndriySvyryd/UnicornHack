import * as React from 'React';
import { Property } from '../transport/Model';

export class PropertyList extends React.Component<IPropertyListProps, {}> {
    shouldComponentUpdate(nextProps: IPropertyListProps): boolean {
        return this.props.properties !== nextProps.properties;
    }

    render() {
        const abilities = this.props.properties.map(p => <PropertyLine property={p} key={p.name} />);
        return (<div className="frame">{abilities}</div>);
    }
}

interface IPropertyListProps {
    properties: Property[]
}

export class PropertyLine extends React.Component<IPropertyLineProps, {}> {
    shouldComponentUpdate(nextProps: IPropertyLineProps): boolean {
        return this.props.property.displayName !== nextProps.property.displayName;
    }

    render() {
        return <div>{this.props.property.displayName}</div>;
    }
}

interface IPropertyLineProps {
    property: Property
}