import * as React from 'react';

interface IChatProps {
    sendMessage: (outgoingMessage: string) => void;
    messages: IMessage[];
}

interface IChatState {
    outgoingMessage: string;
}

export class Chat extends React.Component<IChatProps, IChatState> {
    constructor(props: IChatProps) {
        super(props);

        this.state = { outgoingMessage: '' };

        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleChange(event: React.ChangeEvent<HTMLInputElement>) {
        this.setState({ outgoingMessage: event.target.value });
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        this.props.sendMessage(this.state.outgoingMessage);
        this.setState({ outgoingMessage: '' });
        event.preventDefault();
    }

    render() {
        return (
            <div className="frame">
                <ul className="chat__messages" ref={(list: HTMLUListElement) => {if (list) {
                     list.scrollTop = list.scrollHeight;
                }}}>
                    {this.props.messages.map(m => <MessageLine {...m} key={m.id} />)}
                </ul>
                <InputForm currentMessage={this.state.outgoingMessage} handleChange={this.handleChange} handleSubmit={this.handleSubmit} />
            </div>
        );
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

interface IInputFormProps {
    currentMessage: string;
    handleChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
    handleSubmit: (event: React.FormEvent<HTMLFormElement>) => void;
}

class InputForm extends React.Component<IInputFormProps, {}> {
    constructor(props: IInputFormProps) {
        super(props);

        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleChange(event: React.ChangeEvent<HTMLInputElement>) {
        this.props.handleChange(event);
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        this.props.handleSubmit(event);
    }

    render() {
        return (
            <form onSubmit={this.handleSubmit} autoComplete="off">
                <input type="text" style={{ backgroundColor: 'black' }} value={this.props.currentMessage} onChange={this.handleChange} />
                <input type="submit" style={{ backgroundColor: 'gray' }} value="Send" />
            </form>
        );
    }
}