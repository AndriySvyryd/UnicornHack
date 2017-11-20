import * as React from 'react';
import { action, observable } from 'mobx';
import { observer } from 'mobx-react';

@observer
export class Chat extends React.Component<IChatProps, {}> {
    render() {
        return (
            <div className="frame">
                <ul className="chat__messages" ref={(list: HTMLUListElement) => {if (list) {
                     list.scrollTop = list.scrollHeight;
                }}}>
                    {this.props.messages.map(m => <MessageLine {...m} key={m.id} />)}
                </ul>
                <InputForm sendMessage={this.props.sendMessage} />
            </div>
        );
    }
}

interface IChatProps {
    sendMessage: (outgoingMessage: string) => void;
    messages: IMessage[];
}

@observer
class MessageLine extends React.Component<IMessage, {}> {
    render() {
        const line: React.ReactElement<any>[] = [];

        if (this.props.userName) {
            line.push(<b key={this.props.id}>{this.props.userName}: </b>);
        }

        line.push(<span key={this.props.id}>{this.props.text}</span>);

        let color = '';
        switch (this.props.type) {
            case MessageType.Client:
                color = 'lightgray';
                break;
            case MessageType.Error:
                color = 'red';
                break;
            case MessageType.Warning:
                color = 'yellow';
                break;
            case MessageType.Info:
                color = 'gray';
                break;
        }
        return (<li key={this.props.id} style={{ color: color }}>{line}</li>);
    }
}

export interface IMessage {
    id: number;
    userName: string | null;
    text: string;
    type: MessageType;
}

export enum MessageType {
    Client,
    Error,
    Warning,
    Info
}

@observer
class InputForm extends React.Component<IInputFormProps, {}> {
    @observable
    outgoingMessage: string;

    constructor(props: IInputFormProps) {
        super(props);

        this.handleChange = action.bound(this.handleChange.bind(this));
        this.handleSubmit = action.bound(this.handleSubmit.bind(this));

        this.outgoingMessage = '';
    }

    handleChange(event: React.ChangeEvent<HTMLInputElement>) {
        this.outgoingMessage = event.target.value;
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        this.props.sendMessage(this.outgoingMessage);
        this.outgoingMessage = '';
        event.preventDefault();
    }

    render() {
        return (
            <form onSubmit={this.handleSubmit} autoComplete="off">
                <input type="text" style={{ backgroundColor: 'black' }} value={this.outgoingMessage} onChange={this.handleChange} />
                <input type="submit" style={{ backgroundColor: 'gray' }} value="Send" />
            </form>
        );
    }
}

interface IInputFormProps {
    sendMessage: (outgoingMessage: string) => void;
}