import { action } from 'mobx';
import { observer } from 'mobx-react';
import React from 'react';
import { MapStyles } from '../styles/MapStyles';
import { GameQueryType } from '../transport/GameQueryType';
import { MapActor, Player, PlayerRace, MapItem } from '../transport/Model';
import { capitalize, formatTime } from '../Util';
import { IGameContext } from './Game';
import { MeterBar } from './MeterBar';
import { TooltipTrigger } from './TooltipTrigger';

export const StatusBar = observer((props: IStatusBarProps) => {
    const player = props.context.player;
    const actorPanels = new Array<[number, JSX.Element]>();

    player.level.actors.forEach((actor: MapActor, id: number) => {
        if (actor.isCurrentlyPerceived) {
            actorPanels.push([actor.nextActionTick, <ActorPanel
                actor={actor}
                context={props.context}
                key={id} />]);
        }
    });

    const sortedActors = actorPanels.sort((a, b) => a[0] - b[0]).map(a => a[1]);

    sortedActors.unshift(<CharacterPanel {...props} key={-1} />);

    const itemPanels = new Array<JSX.Element>();

    player.level.items.forEach((item: MapItem, id: number) => {
        if (item.isCurrentlyPerceived) {
            itemPanels.push(<ItemPanel
                item={item}
                context={props.context}
                key={id} />);
        }
    });

    return <div className="statusBar__wrapper">
        {sortedActors}
        {itemPanels}
    </div>;
});

interface IStatusBarProps {
    context: IGameContext;
}

@observer
class CharacterPanel extends React.PureComponent<IStatusBarProps, {}> {
    @action.bound
    showCharacterScreen(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || event.type == 'contextmenu' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.showDialog(GameQueryType.PlayerAttributes);
            event.preventDefault();
        }
    }

    @action.bound
    highlight(event: React.MouseEvent<HTMLDivElement>) {
        const player = this.props.context.player;
        const mapPlayer = player.level.actors.get(player.id);
        if (mapPlayer != undefined) {
            player.level.tileClasses[mapPlayer.levelY][mapPlayer.levelX].push('map__tile_highlight');
        }
    }

    @action.bound
    clear(event: React.MouseEvent<HTMLDivElement>) {
        const player = this.props.context.player;
        const mapPlayer = player.level.actors.get(player.id);
        if (mapPlayer !== undefined) {
            const classes = player.level.tileClasses[mapPlayer.levelY][mapPlayer.levelX]
            let index = classes.indexOf('map__tile_highlight');
            if (index !== -1) {
                classes.splice(index, 1);
            }
            // Clear any lingering highlights
            index = classes.indexOf('map__tile_highlight');
            if (index !== -1) {
                classes.splice(index, 1);
            }
        }
    }

    componentDidUpdate(prevProps: any) {
    }

    render() {
        const player = this.props.context.player;
        const styles = MapStyles.Instance;
        const playerGlyph = styles.actors['player'];

        //TODO: Add status effects
        return <div className="statusBar__panel" role="status" onMouseEnter={this.highlight} onMouseLeave={this.clear}>
            <div className="statusBar__element">
                <PlayerLocation player={player} />
            </div>
            <div className="statusBar__element">
                {playerGlyph.char}: <RaceList player={player} showCharacterScreen={this.showCharacterScreen} />
            </div>
            <div className="statusBar__element">
                <HpBar player={player} />
            </div>
            <div className="statusBar__element">
                <EpBar player={player} />
            </div>
            <div className="statusBar__element">
                <XpBar player={player} />
            </div>
        </div>;
    }
}

@observer
class ActorPanel extends React.PureComponent<IActorPanelProps, {}> {
    @action.bound
    showActorAttributes(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type === 'click' || event.type === 'contextmenu' || (event as React.KeyboardEvent<HTMLAnchorElement>).key === 'Enter') {
            this.props.context.showDialog(GameQueryType.ActorAttributes, this.props.actor.id);
            event.preventDefault();
        }
    }

    @action.bound
    highlight(event: React.MouseEvent<HTMLDivElement>) {
        const actor = this.props.actor;
        this.props.context.player.level.tileClasses[actor.levelY][actor.levelX].push('map__tile_highlight');
    }

    @action.bound
    clear(event: React.MouseEvent<HTMLDivElement>) {
        const actor = this.props.actor;
        const classes = this.props.context.player.level.tileClasses[actor.levelY][actor.levelX];
        let index = classes.indexOf('map__tile_highlight');
        if (index !== -1) {
            classes.splice(index, 1);
        }

        // Clear any lingering highlights
        index = classes.indexOf('map__tile_highlight');
        if (index !== -1) {
            classes.splice(index, 1);
        }
    }

    render() {
        const actor = this.props.actor;
        const styles = MapStyles.Instance;
        let glyph = styles.actors[actor.baseName];
        if (glyph === undefined) {
            glyph = Object.assign({ char: actor.baseName[0] }, styles.actors['default']);
        }

        const action = actor.nextAction.type === null ? "Do nothing" : actor.nextAction.name;
        return <div className="statusBar__panel" onMouseEnter={this.highlight} onMouseLeave={this.clear}>
            <div className="statusBar__element">
                <span style={glyph.style}>{glyph.char}</span>: <ActorName actor={actor} showActorAttributes={this.showActorAttributes} />
                {' +'}{formatTime(actor.nextActionTick - this.props.context.player.nextActionTick)} [{action}]
            </div>
            <div className="statusBar__smallElement">
                <ActorHpBar actor={actor} />
            </div>
            <div className="statusBar__smallElement">
                <ActorEpBar actor={actor} />
            </div>
            {/*TODO: Add status effects*/}
        </div>;
    }
}

interface IActorPanelProps {
    context: IGameContext;
    actor: MapActor;
}

const HpBar = observer((props: IPlayerStatusProps) => {
    const player = props.player;

    let hpClass = 'bg-success';
    if (player.hp * 5 <= player.maxHp) {
        hpClass = 'bg-danger';
    } else if (player.hp * 3 <= player.maxHp) {
        hpClass = 'bg-warning';
    }

    return <MeterBar label={`HP: ${player.hp}/${player.maxHp}`}>
        <MeterBar className={hpClass} now={player.hp} max={player.maxHp} />
        <MeterBar className="bg-recentHp progress-bar-striped" now={0} max={player.maxHp} />
        <MeterBar className="bg-dark" now={player.maxHp - player.hp} max={player.maxHp} />
    </MeterBar>;
});

const EpBar = observer((props: IPlayerStatusProps) => {
    const player = props.player;

    let ep = `EP: ${player.ep}/${player.maxEp - player.reservedEp}`;
    if (player.reservedEp !== 0) {
        ep += `(${player.maxEp})`;
    }

    return <MeterBar label={ep}>
        <MeterBar className="bg-primary" now={player.ep} max={player.maxEp} />
        <MeterBar className="bg-dark" now={player.maxEp - player.ep - player.reservedEp} max={player.maxEp} />
        <MeterBar className="bg-secondary" now={player.reservedEp} max={player.maxEp} />
    </MeterBar>;
});

const XpBar = observer((props: IPlayerStatusProps) => {
    const player = props.player;
    return <MeterBar label={`XP: ${player.xP}/${player.nextLevelXP}`}>
        <MeterBar className="bg-currentXp" now={player.xP} max={player.nextLevelXP} />
        <MeterBar className="bg-dark" now={player.nextLevelXP - player.xP} max={player.nextLevelXP} />
    </MeterBar>;
});

interface IPlayerStatusProps {
    player: Player;
}

const ActorHpBar = observer((props: IActorStatusProps) => {
    const actor = props.actor;

    return <MeterBar label={`HP: ${actor.hp}/${actor.maxHp}`}>
        <MeterBar className="bg-danger" now={actor.hp} max={actor.maxHp} />
        <MeterBar className="bg-dark" now={actor.maxHp - actor.hp} max={actor.maxHp} />
    </MeterBar>;
});

const ActorEpBar = observer((props: IActorStatusProps) => {
    const actor = props.actor;

    return <MeterBar label={`EP: ${actor.ep}/${actor.maxEp}`}>
        <MeterBar className="bg-primary" now={actor.ep} max={actor.maxEp} />
        <MeterBar className="bg-dark" now={actor.maxEp - actor.ep} max={actor.maxEp} />
    </MeterBar>;
});

interface IActorStatusProps {
    actor: MapActor;
};

const RaceList = observer((props: IPlayerRacesProps) => {
    const player = props.player;
    const races = Array.from(player.races.values(), r => <RaceStatus race={r} player={player} key={r.id} />);
    return <>
        <a tabIndex={1} role="button" onClick={props.showCharacterScreen} onContextMenu={props.showCharacterScreen} onKeyPress={props.showCharacterScreen}>
            {player.name}
        </a> {races}
    </>;
});

interface IPlayerRacesProps extends IPlayerStatusProps {
    showCharacterScreen: ((event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) => void)
};

const RaceStatus = observer(({ race, player }: IRaceStatusProps) => {
    const raceString = `${race.shortName}(${race.xpLevel}) `;
    let className = '';
    if (race.id === player.learningRace.id) {
        className = 'font-weight-bold';
    }

    return <TooltipTrigger
        id={`tooltip-race-${race.id}`}
        tooltip={capitalize(race.name)}
    >
        <span className={className}>{raceString}</span>
    </TooltipTrigger>;
});

interface IRaceStatusProps extends IPlayerStatusProps {
    race: PlayerRace;
}

const ActorName = observer((props: IActorNameProps) => {
    const actor = props.actor;
    return <a tabIndex={1} role="button" onClick={props.showActorAttributes} onContextMenu={props.showActorAttributes} onKeyPress={props.showActorAttributes}>
        <span>{actor.name}</span>
    </a>;
});

interface IActorNameProps extends IActorStatusProps {
    showActorAttributes: ((event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) => void);
};

const PlayerLocation = observer((props: IPlayerStatusProps) => {
    const player = props.player;
    const level = player.level;
    let currentTime = <span>{formatTime(player.nextActionTick, true)}</span>;

    return <span>{level.branchName}({level.depth}) {currentTime}</span>;
});

@observer
class ItemPanel extends React.PureComponent<IItemPanelProps, {}> {
    @action.bound
    showItemAttributes(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type === 'click' || event.type === 'contextmenu' || (event as React.KeyboardEvent<HTMLAnchorElement>).key === 'Enter') {
            this.props.context.showDialog(GameQueryType.ItemAttributes, this.props.item.id);
            event.preventDefault();
        }
    }

    @action.bound
    highlight(event: React.MouseEvent<HTMLDivElement>) {
        const item = this.props.item;
        this.props.context.player.level.tileClasses[item.levelY][item.levelX].push('map__tile_highlight');
    }

    @action.bound
    clear(event: React.MouseEvent<HTMLDivElement>) {
        const item = this.props.item;
        const classes = this.props.context.player.level.tileClasses[item.levelY][item.levelX];
        let index = classes.indexOf('map__tile_highlight');
        if (index !== -1) {
            classes.splice(index, 1);
        }

        // Clear any lingering highlights
        index = classes.indexOf('map__tile_highlight');
        if (index !== -1) {
            classes.splice(index, 1);
        }
    }

    render() {
        const item = this.props.item;
        const styles = MapStyles.Instance;
        const glyph = styles.items[item.type];
        if (glyph === undefined) {
            throw `Item type ${item.type} not supported.`;
        }

        return <div className="statusBar__panel" onMouseEnter={this.highlight} onMouseLeave={this.clear}>
            <div className="statusBar__element">
                <span style={glyph.style}>{glyph.char}</span>: <ItemName item={item} showItemAttributes={this.showItemAttributes} />
            </div>
        </div>;
    }
}

interface IItemPanelProps {
    context: IGameContext;
    item: MapItem;
}

const ItemName = observer((props: IItemNameProps) => {
    return <a tabIndex={1} role="button" onClick={props.showItemAttributes} onContextMenu={props.showItemAttributes} onKeyPress={props.showItemAttributes}>
        <span>{props.item.name}</span>
    </a>;
});

interface IItemNameProps {
    item: MapItem;
    showItemAttributes: ((event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) => void);
};