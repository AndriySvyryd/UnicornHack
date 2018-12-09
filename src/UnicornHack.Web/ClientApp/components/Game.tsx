import * as React from 'react';
import * as signalR from '@aspnet/signalr';
import { MessagePackHubProtocol } from '@aspnet/signalr-protocol-msgpack';
import * as Mousetrap from 'mousetrap';
import 'mousetrap/plugins/record/mousetrap-record';
import { action, observable } from 'mobx';
import { observer } from 'mobx-react';
import { HotKeys } from 'react-hotkeys';
import { Chat, IMessage, MessageType } from './Chat';
import { StatusBar } from './StatusBar';
import { Inventory } from './Inventory';
import { AbilityBar } from './AbilityBar';
import { AbilitySelection } from './AbilitySelection';
import { PropertyList } from './PropertyList';
import { MapDisplay } from './MapDisplay';
import { GameLog } from './GameLog';
import { MapStyles } from '../styles/MapStyles';
import { Player, PlayerRace } from '../transport/Model';
import { PlayerAction } from "../transport/PlayerAction";
import { Direction } from "../transport/Direction";
import { UIData } from '../transport/UIData';
import { GameQueryType } from '../transport/GameQueryType';

interface IGameProps {
    playerName: string;
    baseUrl: string;
}

@observer
export class Game extends React.Component<IGameProps, {}> {
    @observable player: Player = new Player();
    @observable messages: IMessage[] = [];
    @observable dialogData: UIData = new UIData();
    @observable waiting: boolean = true;
    actionQueue: IAction[] = [];
    keyMap: any;
    keyHandlers: any;
    connection: signalR.HubConnection;
    styles: MapStyles = new MapStyles();

    constructor(props: IGameProps) {
        super(props);

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`http://${(document.location || new Location()).host}/gameHub`,
                { transport: signalR.HttpTransportType.WebSockets, skipNegotiation: true })
            .configureLogging(signalR.LogLevel.Information)
            .withHubProtocol(new MessagePackHubProtocol())
            .build();

        connection.onclose = e => {
            if (e) {
                this.addError('Connection closed with error: ' + (e || '').toString());
            } else {
                this.addMessage(MessageType.Info, 'Disconnected');
            }
        };

        connection.on('ReceiveMessage',
            (userName: string, message: string) => this.addMessage(MessageType.Client, message, userName));
        connection.on('ReceiveState', this.processServerState);
        connection.on('ReceiveUIRequest', this.handleUIRequest);

        connection.start()
            .then(() => { this.getFullState() }) // TODO: wait for mount
            .catch(e => this.addError((e || '').toString()));

        this.connection = connection;

        this.player.name = props.playerName;
        new PlayerRace().addTo(this.player.races);

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
                this.performAction(PlayerAction.MoveOneCell, Direction.North);
                event.preventDefault();
            },
            'moveEast': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.East);
                event.preventDefault();
            },
            'moveSouth': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.South);
                event.preventDefault();
            },
            'moveWest': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.West);
                event.preventDefault();
            },
            'moveNorthEast': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Northeast);
                event.preventDefault();
            },
            'moveSouthEast': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Southeast);
                event.preventDefault();
            },
            'moveSouthWest': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Southwest);
                event.preventDefault();
            },
            'moveNorthWest': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Northwest);
                event.preventDefault();
            },
            'moveUp': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Up);
                event.preventDefault();
            },
            'moveDown': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Down);
                event.preventDefault();
            },
            'wait': (event: ExtendedKeyboardEvent) => {
                this.performAction(PlayerAction.Wait);
                event.preventDefault();
            },
            'record': (event: ExtendedKeyboardEvent) => this.recordKey()
        };

        this.queryGame = this.queryGame.bind(this);
        this.performAction = this.performAction.bind(this);
        this.sendMessage = this.sendMessage.bind(this);
    }

    recordKey() {
        console.log('Recording key sequence');
        Mousetrap.record(keys => console.log(keys.join(' ')));
    }

    private getFullState() {
        this.connection.invoke('GetState', this.props.playerName)
            .then(this.processServerState)
            .catch(e => this.addError((e || '').toString()));
    }

    @action.bound
    processServerState(compactPlayer: any[]) {
        const newPlayer = Player.expand(compactPlayer, this.player);
        if (newPlayer == null) {
            console.log('Desync, reloading.');
            this.getFullState();
            return;
        }

        this.player = newPlayer;

        this.performQueuedActions();
    }

    private queryGame(queryType: GameQueryType, ...args: Array<number>) {
        this.connection.invoke('QueryGame', this.props.playerName, queryType, args)
            .catch(e => this.addError((e || '').toString()));
    }

    @action.bound
    handleUIRequest(compactRequest: any[]) {
        this.dialogData = this.dialogData.update(compactRequest);
    }

    private performAction(action: PlayerAction, target: (number | null) = null, target2: (number | null) = null) {
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
        if (this.player.level.depth === -1) {
            this.player.level.depth = 0;
        }
        this.messages.push({ id: this.messages.length, userName: userName, text: text, type: type } as IMessage);
    };

    private addError(error: string) {
        this.addMessage(MessageType.Error, error);
    }

    render() {
        const level = this.player.level;
        const firstTimeLoading = level.depth === -1;

        return (
            <div className="row">
                <div style={{
                    display: firstTimeLoading ? 'block' : 'none'
                }}>Loading, please wait...</div>
                <div className="col-9" style={{
                    display: firstTimeLoading ? 'none' : 'block'
                }}>
                    <HotKeys keyMap={this.keyMap} handlers={this.keyHandlers}>
                        <MapDisplay level={level} styles={this.styles} performAction={this.performAction} />
                        <StatusBar player={this.player}
                            levelName={level.branchName} levelDepth={level.depth} />
                        <GameLog messages={this.player.log} />
                    </HotKeys>
                </div>

                <div className="col-3" style={{
                    display: firstTimeLoading ? 'none' : 'block'
                }}>
                    <HotKeys keyMap={this.keyMap} handlers={this.keyHandlers}>
                        <Inventory items={this.player.inventory} performAction={this.performAction} />
                        <AbilityBar abilities={this.player.abilities} performAction={this.performAction} queryGame={this.queryGame} />
                        <PropertyList player={this.player} />
                    </HotKeys>
                    <Chat sendMessage={this.sendMessage} messages={this.messages} />
                </div>

                <AbilitySelection data={this.dialogData} performAction={this.performAction} queryGame={this.queryGame} />
            </div>
        );
    }
}

interface IAction {
    action: PlayerAction;
    target: (number | null);
    target2: (number | null);
}
