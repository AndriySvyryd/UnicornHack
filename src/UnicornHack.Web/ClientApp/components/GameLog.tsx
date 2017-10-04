import * as React from 'React';
import { LogEntry } from '../transport/Model';

export class GameLog extends React.Component<IGameLogProps, {}> {
    shouldComponentUpdate(nextProps: IGameLogProps): boolean {
        return this.props.properties !== nextProps.properties;
    }

    render() {
        const messages = this.props.properties.map(l => <GameLogLine {...l} key={l.id} />);
        return (<div className="frame">{messages}</div>);
    }
}

interface IGameLogProps {
    properties: LogEntry[]
}

export class GameLogLine extends React.Component<LogEntry, {}> {
    shouldComponentUpdate(nextProps: LogEntry): boolean {
        return false;
    }

    render() {
        return <div>{this.props.message}</div>;
    }
}