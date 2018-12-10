import * as React from 'React';
import * as scss from '../styles/site.scss'
import { observer } from 'mobx-react';
import { LogEntry } from '../transport/Model';

@observer
export class GameLog extends React.Component<IGameLogProps, {}> {
    render() {
        const messages = Array.from(this.props.messages.values(),
            l => <GameLogLine entry={l} key={l.id} />);
        return (<div className={scss.frame}>{messages}</div>);
    }
}

interface IGameLogProps {
    messages: Map<string, LogEntry>
}

@observer
export class GameLogLine extends React.Component<IGameLogLineProps, {}> {
    render() {
        return <div>{this.props.entry.message}</div>;
    }
}

interface IGameLogLineProps {
    entry: LogEntry
}