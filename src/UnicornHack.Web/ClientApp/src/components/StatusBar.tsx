import React from 'react';
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { MapStyles } from '../styles/MapStyles';
import { Player, PlayerRace, MapActor, Level } from '../transport/Model';
import { GameQueryType } from '../transport/GameQueryType';
import { capitalize } from '../Util';
import { IGameContext } from './Game';
import { MeterBar } from './MeterBar';
import { TooltipTrigger } from './TooltipTrigger';
import { PlayerAction } from '../transport/PlayerAction';

export const StatusBar = observer((props: IStatusBarProps) => {
    const player = props.context.player;
    const actors = player.level.actors;
    const actorPanels = new Array<[number, JSX.Element]>();

    actorPanels.push([player.nextActionTick, <CharacterPanel {...props} key={-1} />]);

    actors.forEach((actor: MapActor, id: number) => {
        if (actor.isCurrentlyPerceived) {
            actorPanels.push([actor.nextActionTick, <ActorPanel
                actor={actor}
                context={props.context}
                key={id} />]);
        }
    });

    const sortedActors = actorPanels.sort((a, b) => a[0] - b[0]).map(a => a[1]);

    return <div className="statusBar__wrapper">
        {sortedActors}
    </div>;
});

interface IStatusBarProps {
    context: IGameContext;
}

@observer
export class CharacterPanel extends React.PureComponent<IStatusBarProps, {}> {
    @action.bound
    showCharacterScreen(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || event.type == 'contextmenu' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.showDialog(GameQueryType.PlayerAttributes);
            event.preventDefault();
        }
    }

    render() {
        const player = this.props.context.player;
        const styles = MapStyles.Instance;
        const playerGlyph = styles.actors['player'];

        //TODO: Add status effects
        return <div className="statusBar__panel" role="status">
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
export class ActorPanel extends React.PureComponent<IActorPanelProps, {}> {
    @action.bound
    attack(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || event.type == 'contextmenu' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.performAction(PlayerAction.UseAbilitySlot, null, Level.pack(this.props.actor.levelX, this.props.actor.levelY));
            event.preventDefault();
        }
    }

    @action.bound
    showActorAttributes(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || event.type == 'contextmenu' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.showDialog(GameQueryType.ActorAttributes, this.props.actor.id);
            event.preventDefault();
        }
    }

    render() {
        const actor = this.props.actor;
        const styles = MapStyles.Instance;
        var glyph = styles.actors[actor.baseName];
        if (glyph == undefined) {
            glyph = Object.assign({ char: actor.baseName[0] }, styles.actors['default']);
        }

        return <div className="statusBar__panel">
            <div className="statusBar__smallElement">
                {glyph.char}: <ActorName actor={actor} attack={this.attack} showActorAttributes={this.showActorAttributes} />
            </div>
            <div className="statusBar__smallElement">
                +{actor.nextActionTick - this.props.context.player.nextActionTick} AUT
            </div>
            {/*TODO: Show next action*/}
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

    var hpClass = 'bg-success';
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

    var ep = `EP: ${player.ep}/${player.maxEp - player.reservedEp}`;
    if (player.reservedEp != 0) {
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
    var className = '';
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
    return <a tabIndex={1} role="button" onClick={props.attack} onContextMenu={props.showActorAttributes} onKeyPress={props.showActorAttributes}>
        <TooltipTrigger
            id={`tooltip-actor-status-${actor.id}`}
            tooltip={<p>{'Attack ' + actor.name}</p>}
        >
            <span>{actor.name}</span>
        </TooltipTrigger>
    </a>;
});

interface IActorNameProps extends IActorStatusProps {
    showActorAttributes: ((event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) => void)
    attack: ((event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) => void)
};

const PlayerLocation = observer((props: IPlayerStatusProps) => {
    const player = props.player;
    const level = player.level;
    var currentTime = <></>;
    if (player.nextActionTick < 1000) {
        currentTime = <span>{player.nextActionTick} AUT</span>;
    } else if (player.nextActionTick < 1000000) {
        currentTime = <TooltipTrigger id='tooltip-current-time' tooltip={`${player.nextActionTick.toLocaleString('en')} AUT`}>
            <span className="annotatedText">{(player.nextActionTick / 1000).toPrecision(3)} kAUT</span>
        </TooltipTrigger>
    } else {
        currentTime = <TooltipTrigger id='tooltip-current-time' tooltip={`${player.nextActionTick.toLocaleString('en')} AUT`}>
            <span className="annotatedText">{(player.nextActionTick / 1000000).toPrecision(3)} mAUT</span>
        </TooltipTrigger>
    }
    
    return <span>{level.branchName}({level.depth}) {currentTime}</span>;
});