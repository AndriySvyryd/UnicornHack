import * as React from 'React';
import { observer } from 'mobx-react';
import { LogEntry } from '../transport/Model';

export const GameLog = observer((props: IGameLogProps) => {
    const messages = Array.from(props.messages.values(), l =>
        <div key={l.id}>{l.message}</div>);
    return <div className="gameLog" role="log">{messages}</div>;
});

interface IGameLogProps {
    messages: Map<string, LogEntry>
}