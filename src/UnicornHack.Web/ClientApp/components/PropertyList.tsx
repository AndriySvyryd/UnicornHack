import * as React from 'React';
import { observer } from 'mobx-react';
import { Player } from '../transport/Model';

@observer
export class PropertyList extends React.Component<IPropertyListProps, {}> {
    render() {
        //TODO: Add status effects
        return (<div className="frame">
        </div>);
    }
}

interface IPropertyListProps {
    player: Player
}