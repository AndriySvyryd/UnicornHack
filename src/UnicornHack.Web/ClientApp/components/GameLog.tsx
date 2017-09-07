import * as React from 'React';
import { LogEntry } from './Model';

export class GameLog extends React.Component<IGameLogProps, {}> {
    render() {
        const messages = this.props.properties.map(l => <GameLogLine {...l} key={l.id} />);
        return (<div className="frame">{messages}</div>);
    }
}

interface IGameLogProps {
    properties: LogEntry[]
}

export class GameLogLine extends React.Component<LogEntry, {}> {
    render() {
        return <div>{this.props.message}</div>;
    }
}