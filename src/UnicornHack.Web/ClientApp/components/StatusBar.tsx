import * as React from 'React';
import { observer } from 'mobx-react';
import { Player, PlayerRace } from '../transport/Model';
import { GameQueryType } from '../transport/GameQueryType';
import { MeterBar } from './MeterBar';
import { action } from 'mobx';

@observer
export class StatusBar extends React.Component<IStatusBarProps, {}> {
    @action.bound
    showCharacterScreen(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.queryGame(GameQueryType.PlayerAttributes);
        }
    }

    render() {
        const player = this.props.player;
        const playerName = <a tabIndex={1} onClick={this.showCharacterScreen} onKeyPress={this.showCharacterScreen}
        >
            {player.name}
        </a>;

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
                {playerName} {<RaceList player={player} />}
            </div>
            <div className="statusBar__element">
                {this.props.levelName}:{this.props.levelDepth} AUT: {player.nextActionTick / 100}
            </div>
        </div></div>;
    }
}

interface IStatusBarProps {
    player: Player;
    levelName: string;
    levelDepth: number;
    queryGame: (intQueryType: GameQueryType, ...args: Array<number>) => void;
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

const RaceList = observer((props: IPlayerStatusProps) => {
    const player = props.player;
    const learningRace = player.learningRace;
    return <>
        {Array.from(player.races.values(), r => <RaceStatus race={r} isLearning={r === learningRace} key={r.id} />)}
    </>;
});

interface IPlayerStatusProps {
    player: Player;
}

const RaceStatus = observer((props: IRaceStatusProps) => {
    const raceString = `${props.race.shortName}(${props.race.xpLevel}) `;
    if (props.isLearning) {
        return <span className="font-weight-bold">{raceString}</span>;
    }
    return <span>{raceString}</span>;
});

interface IRaceStatusProps {
    race: PlayerRace;
    isLearning: boolean;
}