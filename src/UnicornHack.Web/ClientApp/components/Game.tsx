import * as React from 'react';
import * as SignalR from '@aspnet/signalr-client';
import { action, observable } from 'mobx';
import { observer } from 'mobx-react';
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
import { Player, Level } from '../transport/Model';
import { GameLog } from './GameLog';
import { PlayerRace } from '../transport/Model';

interface IGameProps {
    playerName: string;
    baseUrl: string;
}

@observer
export class Game extends React.Component<IGameProps, {}> {
    @observable
    level: Level = new Level();
    @observable
    messages: IMessage[] = [];
    @observable
    waiting: boolean = true;
    actionQueue: IAction[] = [];
    keyMap: any;
    keyHandlers: any;
    connection: SignalR.HubConnection;
    styles: MapStyles = new MapStyles();

    constructor(props: IGameProps) {
        super(props);

        const logger = new SignalR.ConsoleLogger(SignalR.LogLevel.Information);
        const http = new SignalR.HttpConnection(`http://${document.location.host}/gameHub`,
            { transport: SignalR.TransportType.WebSockets, logging: logger });
        const connection =
            new SignalR.HubConnection(http,
                { logging: logger, protocol: new SignalR.MessagePackHubProtocol });

        connection.onclose = e => {
            if (e) {
                this.addError('Connection closed with error: ' + (e || '').toString());
            } else {
                this.addMessage(MessageType.Info, 'Disconnected');
            }
        };

        connection.on('ReceiveMessage',
            (userName: string, message: string) => this.addMessage(MessageType.Client, message, userName));
        connection.on('ReceiveState',
            action((level: any[]) => {
                const newLevel = Level.expand(level, this.level);
                if (newLevel == null) {
                    console.log('Desync');
                    this.getFullState();
                    return;
                }

                this.level = newLevel;

                this.performQueuedActions();
            }));

        connection.start()
            .then(() => { this.getFullState() }) // TODO: wait for mount
            .catch(e => this.addError((e || '').toString()));

        const level = new Level();

        const player = new Player();
        player.name = props.playerName;
        new PlayerRace().addTo(player.races);
        player.addTo(level.actors);

        this.connection = connection;
        this.level = level;

        Mousetrap.addKeycodes({
            12: 'numpad5',
            144: 'numlock'
        });
        this.keyMap = {
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
        this.keyHandlers = {
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

        this.performAction = this.performAction.bind(this);
        this.sendMessage = this.sendMessage.bind(this);
    }

    recordKey() {
        console.log('Recording key sequence');
        Mousetrap.record(keys => console.log(keys.join(' ')));
    }

    private getFullState() {
        this.connection.invoke('GetState', this.props.playerName)
            .then(action((level: any[]) => {
                var newLevel = Level.expand(level);
                if (newLevel == null) {
                    return;
                }

                this.level = newLevel;

                this.performQueuedActions();
            }))
            .catch(e => this.addError((e || '').toString()));
    }

    private performAction(action: string, target: (number | null) = null, target2: (number | null) = null) {
        if (this.waiting) {
            this.actionQueue.push({ action: action, target: target, target2: target2 });
        } else {
            this.waiting = true;
            this.connection.invoke('PerformAction', this.props.playerName, action, target, target2)
                .catch(e => this.addError((e || '').toString()));
        }
    }

    private performQueuedActions() {
        this.waiting = false;
        if (this.actionQueue.length > 0) {
            const action = this.actionQueue.shift();
            if (action == undefined) {
                return;
            }
            this.performAction(action.action, action.target, action.target2);
        }
    }

    private sendMessage(outgoingMessage: string) {
        this.connection.invoke('SendMessage', outgoingMessage)
            .catch(e => this.addError((e || '').toString()));
    }

    @action.bound
    private addMessage(type: MessageType, text: string, userName: string | null = null) {
        this.messages.push({ id: this.messages.length, userName: userName, text: text, type: type } as IMessage);
    };

    private addError(error: string) {
        this.addMessage(MessageType.Error, error);
    }

    render() {
        for (let actor of this.level.actors.values()) {
            if (actor.baseName !== 'player' || actor.name !== this.props.playerName) {
                continue;
            }

            const player = actor as Player;
            const firstTimeLoading = this.level.depth === -1;

            return (
                <div>
                    <HotKeys keyMap={this.keyMap} handlers={this.keyHandlers}>
                        <div style={{
                            display: firstTimeLoading ? 'block' : 'none'
                        }}>Establishing connection, please wait...</div>

                        <div className="col-md-9" style={{
                            padding: 5,
                            paddingTop: 0,
                            display: firstTimeLoading ? 'none' : 'block'
                        }}>
                            <Map level={this.level} styles={this.styles} />
                            <StatusBar player={player} levelName={this.level.branchName} levelDepth={
                                this.level.depth} />
                            <GameLog messages={player.log} />
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
                        <Chat sendMessage={this.sendMessage} messages={this.messages} />
                    </div>
                </div>
            );
        }

        return <div />;
    }
}

interface IAction {
    action: string;
    target: (number | null);
    target2: (number | null);
}