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

        var ep = `EP: ${player.ep}/${player.maxEp - player.reservedEp}`;
        if (player.reservedEp != 0) {
            ep += `(${player.maxEp})`;
        }

        var currentHpClass = 'statusBar__currentHp';
        if (player.hp * 5 <= player.maxHp) {
            currentHpClass = 'statusBar__currentHp_critical';
        } else if (player.hp * 3 <= player.maxHp) {
            currentHpClass = 'statusBar__currentHp_warning';
        }

        //TODO: Add status effects
        return (<div className="statusBar__wrapper"><div className="statusBar">
            <div className="statusBar__element">
                <div className="statusBar__maxHp">
                    <div className="statusBar__recentHp" role="progressbar" aria-valuenow={player.hp} aria-valuemin={0}
                        aria-valuemax={player.maxHp} style={{ maxWidth: (100 * player.hp / player.maxHp) + '%' }}>
                    </div>
                    <div className={currentHpClass} role="progressbar" aria-labelledby="hpLabel" aria-valuenow={player.hp} aria-valuemin={0}
                        aria-valuemax={player.maxHp} style={{ maxWidth: (100 * player.hp / player.maxHp) + '%' }}>
                    </div>
                    <div className="statusBar__hpLabel" id="hpLabel">
                        HP: {player.hp}/{player.maxHp}
                    </div>
                </div>
            </div>
            <div className="statusBar__element">
                <div className="statusBar__maxHp">
                    <div className="statusBar__reservedEp" role="progressbar" aria-valuenow={player.ep + player.reservedEp} aria-valuemin={0}
                        aria-valuemax={player.maxEp} style={{ maxWidth: (100 * (player.ep + player.reservedEp) / player.maxEp) + '%' }}>
                    </div>
                    <div className="statusBar__currentEp" role="progressbar" aria-labelledby="epLabel" aria-valuenow={player.ep} aria-valuemin={0}
                        aria-valuemax={player.maxEp} style={{ maxWidth: (100 * player.ep / player.maxEp) + '%' }}>
                    </div>
                    <div className="statusBar__epLabel" id="epLabel">
                        {ep}
                    </div>
                </div>
            </div>
            <div className="statusBar__element">
                <div className="statusBar__nextLevelXp">
                    <div className="statusBar__currentXp" role="progressbar" aria-labelledby="xpLabel" aria-valuenow={player.xP} aria-valuemin={0}
                        aria-valuemax={player.nextLevelXP} style={{ maxWidth: (100 * player.xP / player.nextLevelXP) + '%' }}>
                    </div>
                    <div className="statusBar__xpLabel" id="xpLabel">
                        XP: {player.xP}/{player.nextLevelXP}
                    </div>
                </div>
            </div>
            <div className="statusBar__element">
                {player.name} {racesStatus}
            </div>
            <div className="statusBar__element">
                {this.props.levelName}:{this.props.levelDepth} AUT: {player.nextActionTick / 100}
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
        const raceString = `${this.props.race.shortName}(${this.props.race.xpLevel}) `;
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