import * as React from 'React';
import * as scss from '../styles/site.scss'
import { observer } from 'mobx-react';
import { Player, PlayerRace } from '../transport/Model';

@observer
export class StatusBar extends React.Component<IStatusBarProps, {}> {
    render() {
        const learningRace = this.props.player.learningRace;
        const racesStatus = Array.from(this.props.player.races.values(),
            r => <RaceStatus race={r} isLearning={r === learningRace} key={r.id} />);
        const player = this.props.player;

        return (<div className={scss.frame}>
            {player.name} {racesStatus}HP: {player.hp}/{player.maxHp} EP: {player.ep}/{player.maxEp} XP:{player.xP}/{player.nextLevelXP
            } {this.props.levelName}:{this.props.levelDepth} AUT:{player.nextActionTick / 100}
        </div>);
    }
}

interface IStatusBarProps {
    player: Player;
    levelName: string;
    levelDepth: number;
}

@observer
export class RaceStatus extends React.Component<IRaceStatusProps, {}> {
    render() {
        const raceString = `${this.props.race.name}(${this.props.race.xPLevel}) `;
        if (this.props.isLearning) {
            return (<span className="font-weight-bold">{raceString}</span>);
        }
        return (<span>{raceString}</span>);
    }
}

interface IRaceStatusProps {
    race: PlayerRace;
    isLearning: boolean;
}