import * as React from 'react';
import * as signalR from '@aspnet/signalr';
import { MessagePackHubProtocol } from '@aspnet/signalr-protocol-msgpack';
import { action, observable } from 'mobx';
import { observer } from 'mobx-react';
import { HotKeys, IgnoreKeys } from 'react-hotkeys';
import { Player, PlayerRace } from '../transport/Model';
import { PlayerAction } from "../transport/PlayerAction";
import { Direction } from "../transport/Direction";
import { DialogData } from '../transport/DialogData';
import { GameQueryType } from '../transport/GameQueryType';
import { AbilityBar } from './AbilityBar';
import { AbilitySelectionDialog } from './AbilitySelection';
import { Chat, IMessage, MessageType } from './Chat';
import { CharacterScreenDialog } from './CharacterScreen';
import { CreaturePropertiesDialog } from './CreatureProperties';
import { Inventory } from './Inventory';
import { GameLog } from './GameLog';
import { MapDisplay } from './MapDisplay';
import { StatusBar } from './StatusBar';
import { ItemPropertiesDialog } from './ItemProperties';
import { AbilityPropertiesDialog } from './AbilityProperties';
import { PostGameStatisticsDialog } from './PostGameStatistics';

@observer
export class Game extends React.Component<IGameProps, {}> {
    @observable player: Player = new Player();
    @observable private _messages: IMessage[] = [];
    @observable private _dialogData: DialogData = new DialogData();
    @observable private _waiting: boolean = true;
    @observable private _firstTimeLoading: boolean = true;
    private _actionQueue: IAction[] = [];
    private _keyMap: any;
    private _keyHandlers: any;
    private _connection: signalR.HubConnection;
    private _hotKeyContainer: React.RefObject<HTMLInputElement>;

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

        this._connection = connection;

        this.player.name = props.playerName;
        new PlayerRace().addTo(this.player.races);

        // https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key/Key_Values
        this._keyMap = {
            'moveNorth': ['up', '8', 'k'],
            'moveEast': ['right', '6', 'l'],
            'moveSouth': ['down', '2', 'j'],
            'moveWest': ['left', '4', 'h'],
            'moveNorthEast': ['pageup', '9', 'u'],
            'moveSouthEast': ['pagedown', '3', 'n'],
            'moveSouthWest': ['end', '1', 'b'],
            'moveNorthWest': ['home', '7', 'y'],
            'wait': ['Clear', '5', '.'],
            'clear': 'Escape',
            'back': 'Backspace'
        };

        this._keyHandlers = {
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
                this.showDialog(GameQueryType.Clear);
                event.preventDefault();
            },
            'back': (event: KeyboardEvent) => {
                this.showDialog(GameQueryType.Back);
                event.preventDefault();
            }
        };

        this.sendMessage = this.sendMessage.bind(this);
        this._hotKeyContainer = React.createRef();
    }

    componentDidMount() {
        this._connection.start()
            .then(() => { this.getFullState() })
            .catch(e => this.addError((e || '').toString()));

        if (this._hotKeyContainer.current !== null) {
            this._hotKeyContainer.current.focus();
        }
    }

    private getFullState() {
        this._connection.invoke('GetState', this.props.playerName)
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
        this._firstTimeLoading = false;

        this.performQueuedActions();
        // TODO: Get additional static data
    }

    @action.bound
    handleUIRequest(compactRequest: any[]) {
        this._dialogData = this._dialogData.update(compactRequest);
    }

    showDialog = (queryType: GameQueryType, ...args: Array<number>) => {
        if (queryType == GameQueryType.Back) {
            var lastQuery = this._dialogData.dialogHistory.pop();
            if (lastQuery != undefined) {
                queryType = lastQuery[0];
                args = lastQuery[1];
            } else {
                queryType = GameQueryType.Clear;
            }
            this._dialogData.currentDialog = lastQuery;
        } else if (queryType == GameQueryType.Clear) {
            this._dialogData.dialogHistory = [];
            this._dialogData.currentDialog = undefined;
        } else {
            if (this._dialogData.currentDialog != undefined) {
                this._dialogData.dialogHistory.push(this._dialogData.currentDialog);
            }
            this._dialogData.currentDialog = [queryType, args];
        }

        this._connection.invoke('ShowDialog', this.props.playerName, queryType, args)
            .catch(e => this.addError((e || '').toString()));

        if (queryType === GameQueryType.Clear && this._hotKeyContainer.current !== null) {
            this._hotKeyContainer.current.focus();
        }
    };

    queryGame = (queryType: GameQueryType, ...args: Array<number>) => {
        this._connection.invoke('QueryGame', this.props.playerName, queryType, args)
            .catch(e => this.addError((e || '').toString()));
    };

    performAction = (action: PlayerAction, target: (number | null) = null, target2: (number | null) = null) => {
        if (this._waiting) {
            this._actionQueue.push({ action: action, target: target, target2: target2 });
        } else {
            this._waiting = true;
            this._connection.invoke('PerformAction', this.props.playerName, action, target, target2)
                .catch(e => this.addError((e || '').toString()));
        }

        if (this._hotKeyContainer.current !== null) {
            this._hotKeyContainer.current.focus();
        }
    };

    private performQueuedActions() {
        this._waiting = false;
        if (this._actionQueue.length > 0) {
            const action = this._actionQueue.shift();
            if (action == undefined) {
                return;
            }
            this.performAction(action.action, action.target, action.target2);
        }
    }

    private sendMessage(outgoingMessage: string) {
        this._connection.invoke('SendMessage', outgoingMessage)
            .catch(e => this.addError((e || '').toString()));
    }

    @action.bound
    private addMessage(type: MessageType, text: string, userName: string | null = null) {
        if (this.player.level.depth === -1) {
            this.player.level.depth = 0;
        }
        this._messages.push({ id: this._messages.length, userName: userName, text: text, type: type } as IMessage);
    };

    private addError(error: string) {
        this.addMessage(MessageType.Error, error);
    }

    render() {
        const loading = !this._firstTimeLoading
            ? ''
            : <div className="loading__screen">
                <div className="loading__text">
                    <div className="spinner-border spinner-border-sm" /> Loading, please wait...
                </div>
            </div>;

        return <HotKeys innerRef={this._hotKeyContainer} keyMap={this._keyMap} handlers={this._keyHandlers}>
            {loading}
            <div className="game" aria-hidden={this._firstTimeLoading} style={{
                display: this._firstTimeLoading ? 'none' : 'flex'
            }}>
                <div className="game__map">
                    <MapDisplay context={this} />
                    <GameLog messages={this.player.log} />
                </div>

                <div className="game__sidepanel">
                    <div className="sidepanel">
                        <StatusBar context={this} />
                    </div>
                    <div className="sidepanel">
                        <AbilityBar context={this} />
                    </div>
                    <div className="sidepanel">
                        <Inventory context={this} />
                    </div>
                    <div className="sidepanel">
                        <Chat sendMessage={this.sendMessage} messages={this._messages} />
                    </div>
                </div>

                <IgnoreKeys only={[]} except={["Escape", "Backspace"]}>
                    <PostGameStatisticsDialog context={this} data={this._dialogData} />
                    <AbilitySelectionDialog context={this} data={this._dialogData} />
                    <CharacterScreenDialog context={this} data={this._dialogData} />
                    <CreaturePropertiesDialog context={this} data={this._dialogData} />
                    <ItemPropertiesDialog context={this} data={this._dialogData} />
                    <AbilityPropertiesDialog context={this} data={this._dialogData} />
                </IgnoreKeys>
            </div>
        </HotKeys>;
    }
}

interface IGameProps {
    playerName: string;
    baseUrl: string;
}

interface IAction {
    action: PlayerAction;
    target: (number | null);
    target2: (number | null);
}

export interface IGameContext {
    player: Player;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    showDialog: (intQueryType: GameQueryType, ...args: Array<number>) => void;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
}