import * as React from 'react';
import * as signalR from '@aspnet/signalr';
import { MessagePackHubProtocol } from '@aspnet/signalr-protocol-msgpack';
import { action, observable } from 'mobx';
import { observer } from 'mobx-react';
import { HotKeys, IgnoreKeys } from 'react-hotkeys';
import { MapStyles } from '../styles/MapStyles';
import { Player, PlayerRace } from '../transport/Model';
import { PlayerAction } from "../transport/PlayerAction";
import { Direction } from "../transport/Direction";
import { DialogData } from '../transport/DialogData';
import { GameQueryType } from '../transport/GameQueryType';
import { AbilityBar } from './AbilityBar';
import { AbilitySelection } from './AbilitySelection';
import { Chat, IMessage, MessageType } from './Chat';
import { CharacterScreen } from './CharacterScreen';
import { CreatureProperties } from './CreatureProperties';
import { Inventory } from './Inventory';
import { GameLog } from './GameLog';
import { MapDisplay } from './MapDisplay';
import { StatusBar } from './StatusBar';

interface IGameProps {
    playerName: string;
    baseUrl: string;
}

@observer
export class Game extends React.Component<IGameProps, {}> {
    @observable player: Player = new Player();
    @observable messages: IMessage[] = [];
    @observable dialogData: DialogData = new DialogData();
    @observable waiting: boolean = true;
    actionQueue: IAction[] = [];
    keyMap: any;
    keyHandlers: any;
    connection: signalR.HubConnection;
    styles: MapStyles = new MapStyles();
    hotKeyContainer: React.RefObject<HTMLInputElement>;

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

        this.connection = connection;

        this.player.name = props.playerName;
        new PlayerRace().addTo(this.player.races);

        // https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key/Key_Values
        this.keyMap = {
            'moveNorth': ['up', '8', 'k'],
            'moveEast': ['right', '6', 'l'],
            'moveSouth': ['down', '2', 'j'],
            'moveWest': ['left', '4', 'h'],
            'moveNorthEast': ['pageup', '9', 'u'],
            'moveSouthEast': ['pagedown', '3', 'n'],
            'moveSouthWest': ['end', '1', 'b'],
            'moveNorthWest': ['home', '7', 'y'],
            'wait': ['Clear', '5', '.'],
            'clear': 'Escape'
        };

        this.keyHandlers = {
            'moveNorth': (event: KeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.North);
                event.preventDefault();
            },
            'moveEast': (event: KeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.East);
                event.preventDefault();
            },
            'moveSouth': (event: KeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.South);
                event.preventDefault();
            },
            'moveWest': (event: KeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.West);
                event.preventDefault();
            },
            'moveNorthEast': (event: KeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Northeast);
                event.preventDefault();
            },
            'moveSouthEast': (event: KeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Southeast);
                event.preventDefault();
            },
            'moveSouthWest': (event: KeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Southwest);
                event.preventDefault();
            },
            'moveNorthWest': (event: KeyboardEvent) => {
                this.performAction(PlayerAction.MoveOneCell, Direction.Northwest);
                event.preventDefault();
            },
            'wait': (event: KeyboardEvent) => {
                this.performAction(PlayerAction.Wait);
                event.preventDefault();
            },
            'clear': (event: KeyboardEvent) => {
                this.queryGame(GameQueryType.Clear);
                event.preventDefault();
            }
        };

        this.queryGame = this.queryGame.bind(this);
        this.performAction = this.performAction.bind(this);
        this.sendMessage = this.sendMessage.bind(this);
        this.hotKeyContainer = React.createRef();
    }

    componentDidMount() {
        this.connection.start()
            .then(() => { this.getFullState() })
            .catch(e => this.addError((e || '').toString()));

        if (this.hotKeyContainer.current !== null) {
            this.hotKeyContainer.current.focus();
        }
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
        if (queryType === GameQueryType.Clear && this.hotKeyContainer.current !== null) {
            this.hotKeyContainer.current.focus();
        }
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

        if (this.hotKeyContainer.current !== null) {
            this.hotKeyContainer.current.focus();
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

        return <HotKeys innerRef={this.hotKeyContainer} keyMap={this.keyMap} handlers={this.keyHandlers}>
            <div className="dialog__overlay" aria-hidden={!firstTimeLoading} style={{
                display: firstTimeLoading ? 'flex' : 'none', background: 'transparent'
            }}>
                <div className="loading">
                    <div className="spinner-border spinner-border-sm" /> Loading, please wait...
                </div>    
            </div>
            <div className="game" aria-hidden={firstTimeLoading} style={{
                display: firstTimeLoading ? 'none' : 'flex'
            }}>
                <div className="game__map">
                    <MapDisplay level={level} styles={this.styles} performAction={this.performAction} queryGame={this.queryGame}/>
                    <GameLog messages={this.player.log} />
                </div>

                <div className="game__sidepanel">
                    <div className="sidepanel">
                        <StatusBar player={this.player} levelName={level.branchName} levelDepth={level.depth} queryGame={this.queryGame} />
                    </div>
                    <div className="sidepanel">
                        <AbilityBar player={this.player} performAction={this.performAction} queryGame={this.queryGame} />
                    </div>
                    <div className="sidepanel">
                        <Inventory items={this.player.inventory} performAction={this.performAction} />
                    </div>
                    <div className="sidepanel">
                        <Chat sendMessage={this.sendMessage} messages={this.messages} />
                    </div>
                </div>

                <IgnoreKeys only='' except='Escape'>
                    <AbilitySelection data={this.dialogData} performAction={this.performAction} queryGame={this.queryGame} />
                    <CharacterScreen data={this.dialogData} player={this.player} performAction={this.performAction} queryGame={this.queryGame} />
                    <CreatureProperties data={this.dialogData} queryGame={this.queryGame} />
                </IgnoreKeys>
            </div>
        </HotKeys>;
    }
}

interface IAction {
    action: PlayerAction;
    target: (number | null);
    target2: (number | null);
}
