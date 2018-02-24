import * as React from 'React';
import { observer } from 'mobx-react';
import { Player, PlayerRace } from '../transport/Model';

@observer
export class StatusBar extends React.Component<IStatusBarProps, {}> {
    render() {
        const learningRace = this.props.player.learningRace;
        const racesStatus = Array.from(this.props.player.races.values(),
            r => <RaceStatus race={r} isLearning={r === learningRace} key={r.id} />);

        return (<div className="frame">
            {this.props.player.name} {racesStatus}XP:{learningRace.xP}/{learningRace.nextLevelXP
            } $:{this.props.player.gold} {this.props.levelName}:{this.props.levelDepth} AUT:{ this.props.player.nextActionTick / 100 }
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
            return (<span><b>{raceString}</b></span>);
        }
        return (<span>{raceString}</span>);
    }
}

interface IRaceStatusProps {
    race: PlayerRace;
    isLearning: boolean;
}