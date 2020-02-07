import React from 'react';
import { observer } from 'mobx-react';
import { LogEntry } from '../transport/Model';
import { unshift } from '../Util';
import { parseMetadata } from './MetadataDisplay';

export const GameLog = observer((props: IGameLogProps) => {
    const messages = Array.from(props.messages.values(), l =>
        <div key={l.id}>{unshift(l.ticks + ": ", parseMetadata(l.message))}</div>);
    return <div className="gameLog" role="log">{messages}</div>;
});

interface IGameLogProps {
    messages: Map<number, LogEntry>
}