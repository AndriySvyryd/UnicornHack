import * as React from 'react';
import * as scss from '../styles/site.scss'
import { action, observable } from 'mobx';
import { observer } from 'mobx-react';

@observer
export class Chat extends React.Component<IChatProps, {}> {
    render() {
        return (
            <div className={scss.frame}>
                <ul className={scss.chat__messages} ref={(list: HTMLUListElement) => {
                    if (list) {
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
                color = 'text-light';
                break;
            case MessageType.Error:
                color = 'text-danger';
                break;
            case MessageType.Warning:
                color = 'text-warning';
                break;
            case MessageType.Info:
                color = 'text-muted font-italic';
                break;
        }

        return (<li className={color} key={this.props.id}>{line}</li>);
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
    @observable outgoingMessage: string = '';

    @action.bound
    handleChange(event: React.ChangeEvent<HTMLInputElement>) {
        this.outgoingMessage = event.target.value;
    }

    @action.bound
    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        this.props.sendMessage(this.outgoingMessage);
        this.outgoingMessage = '';
        event.preventDefault();
    }

    render() {
        return (
            <form className="form-inline" onSubmit={this.handleSubmit} autoComplete="off">
                <input className="form-control col-10" type="text" value={this.outgoingMessage} onChange={this.handleChange} />
                <input className="btn btn-secondary" type="submit" value="Send" />
            </form>
        );
    }
}

interface IInputFormProps {
    sendMessage: (outgoingMessage: string) => void;
}