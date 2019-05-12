import * as React from 'React';
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { Player, PlayerRace } from '../transport/Model';
import { GameQueryType } from '../transport/GameQueryType';
import { capitalize } from '../Util';
import { IGameContext } from './Game';
import { MeterBar } from './MeterBar';
import { TooltipTrigger } from './TooltipTrigger';

@observer
export class StatusBar extends React.Component<IStatusBarProps, {}> {
    @action.bound
    showCharacterScreen(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.context.showDialog(GameQueryType.PlayerAttributes);
        }
    }

    render() {
        const player = this.props.context.player;

        //TODO: Add status effects
        return <div className="statusBar__wrapper"><div className="statusBar" role="status">
            <div className="statusBar__element">
                <HpBar player={player} />
            </div>
            <div className="statusBar__element">
                <EpBar player={player} />
            </div>
            <div className="statusBar__element">
                <XpBar player={player} />
            </div>
            <div className="statusBar__element">
                <RaceList player={player} showCharacterScreen={this.showCharacterScreen} />
            </div>
            <div className="statusBar__element">
                <PlayerLocation player={player} />
            </div>
        </div></div>;
    }
}

interface IStatusBarProps {
    context: IGameContext;
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

const RaceList = observer((props: IPlayerRacesProps) => {
    const player = props.player;
    const races = Array.from(player.races.values(), r => <RaceStatus race={r} player={player} key={r.id} />);
    return <>
        <a tabIndex={1} role="button" onClick={props.showCharacterScreen} onKeyPress={props.showCharacterScreen}>
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

const PlayerLocation = observer((props: IPlayerStatusProps) => {
    const player = props.player;
    const level = player.level;
    return <span>{level.branchName}: {level.depth} AUT: {player.nextActionTick / 100}</span>;
});