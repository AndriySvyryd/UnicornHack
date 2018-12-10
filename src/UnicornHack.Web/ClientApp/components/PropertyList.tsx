import * as React from 'React';
import * as scss from '../styles/site.scss'
import { observer } from 'mobx-react';
import { Player } from '../transport/Model';

@observer
export class PropertyList extends React.Component<IPropertyListProps, {}> {
    render() {
        //TODO: Add status effects
        return (<div className={scss.frame}>
        </div>);
    }
}

interface IPropertyListProps {
    player: Player
}