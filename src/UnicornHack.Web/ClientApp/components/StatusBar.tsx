import * as React from 'React';
import { observer } from 'mobx-react';
import { Player, PlayerRace } from '../transport/Model';

@observer
export class StatusBar extends React.Component<IStatusBarProps, {}> {
    render() {
        const learningRace = this.props.player.learningRace;
        const racesStatus = Array.from(this.props.player.races.values(),
            r => <RaceStatus race={r} isLearning={r === learningRace} key={r.id} />);
        const player = this.props.player;

        var ep = `${player.ep}/${player.maxEp}`;
        if (player.reservedEp != 0) {
            ep = `${player.ep}/${player.maxEp - player.reservedEp}(${player.maxEp})`;
        }
        //TODO: Add status effects
        return (<div className="statusBar__wrapper"><div className="statusBar">
            <div className="statusBar__row">
                {player.name} {racesStatus}
            </div>
            <div className="statusBar__row">
                HP: {player.hp}/{player.maxHp} 
            </div>
            <div className="statusBar__row">
                EP: {ep} 
            </div>
            <div className="statusBar__row">
                XP:{player.xP}/{player.nextLevelXP}
            </div>
            <div className="statusBar__row">
                {this.props.levelName}:{this.props.levelDepth} 
            </div>
            <div className="statusBar__row">
                AUT:{player.nextActionTick / 100}
            </div>
        </div></div>);
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