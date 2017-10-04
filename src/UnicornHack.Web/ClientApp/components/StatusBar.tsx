import * as React from 'React';
import { Player, PlayerRace } from '../transport/Model';

export class StatusBar extends React.Component<IStatusBarProps, {}> {
    shouldComponentUpdate(nextProps: IStatusBarProps): boolean {
        return this.props.player !== nextProps.player
            || this.props.levelDepth !== nextProps.levelDepth
            || this.props.levelName !== nextProps.levelName;
    }

    render() {
        const racesStatus = this.props.player.races.map(r => <RaceStatus {...r} key={r.name} />);

        return (<div className="frame">
            {this.props.player.name} {racesStatus}XP:{this.props.player.xP}/{this.props.player.nextLevelXP
            } $:{this.props.player.gold} {this.props.levelName}:{this.props.levelDepth} {
                this.props.player.levelX},{this.props.player.levelY} AUT:{ this.props.player.nextActionTick / 100 }
</div>);
    }
}

interface IStatusBarProps {
    player: Player;
    levelName: string;
    levelDepth: number;
}

export class RaceStatus extends React.Component<PlayerRace, {}> {
    shouldComponentUpdate(nextProps: PlayerRace): boolean {
        return this.props.isLearning !== nextProps.isLearning
            || this.props.xPLevel !== nextProps.xPLevel;
    }

    render() {
        const raceString = `${this.props.name}(${this.props.xPLevel}) `;
        if (this.props.isLearning) {
            return (<span><b>{raceString}</b></span>);
        }
        return (<span>{raceString}</span>);
    }
}