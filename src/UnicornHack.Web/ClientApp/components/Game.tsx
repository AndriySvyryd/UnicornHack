import * as React from 'react';
import * as SignalR from '@aspnet/signalr-client';
import 'isomorphic-fetch';
import { HotKeys } from 'react-hotkeys';
import * as Mousetrap from 'mousetrap';
import 'mousetrap/plugins/record/mousetrap-record';
import { Chat, IMessage, MessageType } from './Chat';
import { StatusBar } from './StatusBar';
import { Inventory } from './Inventory';
import { AbilityBar } from './AbilityBar';
import { PropertyList } from './PropertyList';
import { Map } from './Map';
import { MapStyles } from '../styles/MapStyles';
import { Player, Level, ICompactLevel } from '../transport/Model';
import { GameLog } from './GameLog';

interface IGameProps {
    playerName: string;
    baseUrl: string;
}

class GameState {
    level: Level = new Level();
    connection: SignalR.HubConnection;
    messages: IMessage[] = [];
    styles: MapStyles = new MapStyles();
    waiting: boolean = true;
    keyMap: any;
    keyHandlers: any;
}

export class Game extends React.Component<IGameProps, GameState> {
    constructor(props: IGameProps) {
        super(props);

        const logger = new SignalR.ConsoleLogger(SignalR.LogLevel.Information);
        const http = new SignalR.HttpConnection(`http://${document.location.host}/gameHub`,
            { transport: SignalR.TransportType.WebSockets, logging: logger });
        const connection =
            new SignalR.HubConnection(http,
                { logging: logger, protocol: new SignalR.JsonHubProtocol }); // MessagePackHubProtocol

        connection.onClosed = e => {
            if (e) {
                this.addError('Connection closed with error: ' + (e || '').toString());
            } else {
                this.addMessage(MessageType.Info, 'Disconnected');
            }
        };

        connection.on('ReceiveMessage', (userName: string, message: string) => this.addMessage(MessageType.Client, message, userName));
        connection.on('ReceiveState', (level: ICompactLevel) => {
            let start = 0;
            if (this.isInDev()) {
                start = performance.now();
            }

            const s = this.setState({ level: Level.expand(level), waiting: false });

            if (this.isInDev()) {
                const end = performance.now();
                console.log('ReceiveState time: ', end - start);
            }
            return s;
        });

        connection.start()
            .then(() => { this.getFullState() }) // TODO: wait for mount
            .catch(e => this.addError((e || '').toString()));

        const level = new Level();

        const player = new Player();
        player.name = props.playerName;
        level.actors = [player];

        const state = new GameState();
        state.connection = connection;
        state.level = level;

        Mousetrap.addKeycodes({
            12: 'numpad5',
            144: 'numlock'
        });
        state.keyMap = {
            'moveNorth': ['up', '8', 'k'],
            'moveEast': ['right', '6', 'l'],
            'moveSouth': ['down', '2', 'j'],
            'moveWest': ['left', '4', 'h'],
            'moveNorthEast': ['pageup', '9', 'u'],
            'moveSouthEast': ['pagedown', '3', 'n'],
            'moveSouthWest': ['end', '1', 'b'],
            'moveNorthWest': ['home', '7', 'y'],
            'moveUp': 'shift+,',
            'moveDown': 'shift+.',
            'wait': ['numpad5', '5', '.'],
        };
        state.keyHandlers = {
            'moveNorth': (event: ExtendedKeyboardEvent) => {
                this.performAction("N");
                event.preventDefault();
            },
            'moveEast': (event: ExtendedKeyboardEvent) => {
                this.performAction("E");
                event.preventDefault();
            },
            'moveSouth': (event: ExtendedKeyboardEvent) => {
                this.performAction("S");
                event.preventDefault();
            },
            'moveWest': (event: ExtendedKeyboardEvent) => {
                this.performAction("W");
                event.preventDefault();
            },
            'moveNorthEast': (event: ExtendedKeyboardEvent) => {
                this.performAction("NE");
                event.preventDefault();
            },
            'moveSouthEast': (event: ExtendedKeyboardEvent) => {
                this.performAction("SE");
                event.preventDefault();
            },
            'moveSouthWest': (event: ExtendedKeyboardEvent) => {
                this.performAction("SW");
                event.preventDefault();
            },
            'moveNorthWest': (event: ExtendedKeyboardEvent) => {
                this.performAction("NW");
                event.preventDefault();
            },
            'moveUp': (event: ExtendedKeyboardEvent) => {
                this.performAction("U");
                event.preventDefault();
            },
            'moveDown': (event: ExtendedKeyboardEvent) => {
                this.performAction("D");
                event.preventDefault();
            },
            'wait': (event: ExtendedKeyboardEvent) => {
                this.performAction("H");
                event.preventDefault();
            },
            'record': (event: ExtendedKeyboardEvent) => this.recordKey()
        };
        this.state = state;

        this.performAction = this.performAction.bind(this);
        this.sendMessage = this.sendMessage.bind(this);
    }

    shouldComponentUpdate(nextProps: IGameProps, nextState: GameState): boolean {
        return this.props !== nextProps || this.state !== nextState;
    }

    recordKey() {
        console.log('Recording key sequence');
        Mousetrap.record(keys => console.log(keys.join(' ')));
    }

    private getFullState() {
        this.state.connection.invoke('GetState', this.props.playerName)
            .then((level: ICompactLevel) => {
                let start = 0;
                if (this.isInDev()) {
                    start = performance.now();
                }

                const s = this.setState({ level: Level.expand(level), waiting: false });

                if (this.isInDev()) {
                    const end = performance.now();
                    console.log('GetState time: ', end - start);
                }
                return s;
            })
            .catch(e => this.addError((e || '').toString()));
    }

    private performAction(action: string, target: (number | null) = null, target2: (number | null) = null) {
        this.setState({ waiting: true });
        this.state.connection.invoke('PerformAction', this.props.playerName, action, target, target2)
            .catch(e => this.addError((e || '').toString()));
    }

    private sendMessage(outgoingMessage: string) {
        this.state.connection.invoke('SendMessage', outgoingMessage)
            .catch(e => this.addError((e || '').toString()));
    }

    private addMessage(type: MessageType, text: string, userName: string | null = null) {
        this.setState((prevState: GameState) => ({
            messages: [
                ...prevState.messages,
                { id: prevState.messages.length, userName: userName, text: text, type: type } as IMessage
            ]
        }));
    };

    private addError(error: string) {
        this.addMessage(MessageType.Error, error);
    }

    private isInDev(): boolean {
        return process.env.NODE_ENV !== 'production';
    }

    render() {
        const player = this.state.level.actors.find(v => v.baseName === 'player' && v.name === this.props.playerName) as Player;
        const firstTimeLoading = this.state.waiting && this.state.level.depth === -1;

        return (
            <div>
                <HotKeys keyMap={this.state.keyMap} handlers={this.state.keyHandlers}>
                    <div style={{
                        display: firstTimeLoading ? 'block' : 'none'
                    }}>Establishing connection, please wait...</div>

                    <div className="col-md-9" style={{
                        padding: 5,
                        paddingTop: 0,
                        display: firstTimeLoading ? 'none' : 'block'
                    }}>
                        <Map level={this.state.level} styles={this.state.styles} />
                        <StatusBar player={player} levelName={this.state.level.branchName} levelDepth={this.state.level
                            .depth} />
                        <GameLog properties={player.log} />
                    </div>

                    <div className="col-md-3" style={{
                        padding: 5,
                        paddingTop: 0,
                        display: firstTimeLoading ? 'none' : 'block'
                    }}>
                        <Inventory items={player.inventory} performAction={this.performAction} />
                    </div>

                    <div className="col-md-3" style={{
                        padding: 5,
                        paddingTop: 0,
                        display: firstTimeLoading ? 'none' : 'block'
                    }}>
                        <AbilityBar abilities={player.abilities} performAction={this.performAction} />
                    </div>

                    <div className="col-md-3" style={{
                        padding: 5,
                        paddingTop: 0,
                        display: firstTimeLoading ? 'none' : 'block'
                    }}>
                        <PropertyList properties={player.properties} />
                    </div>
                </HotKeys>

                <div className="col-md-3" style={{
                    padding: 5,
                    paddingTop: 0,
                    display: firstTimeLoading ? 'none' : 'block'
                }}>
                    <Chat sendMessage={this.sendMessage} messages={this.state.messages} />
                </div>
            </div>
        );
    }
}