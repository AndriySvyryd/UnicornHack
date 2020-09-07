import React from 'react';
import scss from '../styles/site.scss'
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { MapStyles, ITileStyle } from '../styles/MapStyles';
import { Level, Tile, AttackSummary } from '../transport/Model';
import { ActorAction } from '../transport/ActorAction';
import { Direction } from '../transport/Direction';
import { GameQueryType } from '../transport/GameQueryType';
import { MapFeature } from '../transport/MapFeature';
import { capitalize, formatTime } from '../Util';
import { IGameContext } from './Game';
import { TooltipTrigger } from './TooltipTrigger';

export const MapDisplay = observer((props: IMapProps) => {
    const level = props.context.player.level;
    const styles = MapStyles.Instance;
    const map = new Array<React.ReactElement<any>>(level.height);
    for (let y = 0; y < level.height; y++) {
        map[y] = <MapRow
            y={y}
            row={level.tiles[y]}
            styles={styles}
            indexToPoint={level.indexToPoint}
            context={props.context}
            key={y} />;
    }

    return <div className="mapContainer">
        <div className="map">{map}</div>
    </div>;
});

interface IMapProps {
    context: IGameContext;
}

const MapRow = observer((props: IRowProps) => {
    const row = props.row.map((t, x) =>
        <MapTile
            x={x} y={props.y}
            tile={t}
            styles={props.styles}
            context={props.context}
            key={x} />
    );
    return <div className="map__row">{row}</div>;
});

enum TileAction {
    None,
    Wait,
    Move,
    Attack,
    PlayerAttributes,
    ActorAttributes,
    ItemAttributes
}

interface IRowProps {
    y: number;
    row: Tile[];
    styles: MapStyles;
    indexToPoint: number[][];
    context: IGameContext;
}

@observer
class MapTile extends React.PureComponent<ITileProps, {}> {
    private _clickAction: TileAction = TileAction.None;
    private _contextMenuAction: TileAction = TileAction.None;

    @action.bound
    handleClick(event: React.MouseEvent<HTMLDivElement>) {
        this.handleEvent(this._clickAction);
        event.preventDefault();
    }

    @action.bound
    handleContextMenu(event: React.MouseEvent<HTMLDivElement>) {
        this.handleEvent(this._contextMenuAction);
        event.preventDefault();
    }

    handleEvent(action: TileAction) {
        switch (action) {
            case TileAction.Wait:
                this.props.context.performAction(ActorAction.Wait, Level.pack(this.props.x, this.props.y), null);
                break;
            case TileAction.Move:
                this.props.context.performAction(ActorAction.MoveToCell, Level.pack(this.props.x, this.props.y), null);
                break;
            case TileAction.Attack:
                this.props.context.performAction(ActorAction.UseAbilitySlot, null, Level.pack(this.props.x, this.props.y));
                break;
            case TileAction.PlayerAttributes:
                this.props.context.showDialog(GameQueryType.PlayerAttributes);
                break;
            case TileAction.ActorAttributes:
                if (this.props.tile.actor != null) {
                    this.props.context.showDialog(GameQueryType.ActorAttributes, this.props.tile.actor.id);
                }
                break;
            case TileAction.ItemAttributes:
                if (this.props.tile.item != null) {
                    this.props.context.showDialog(GameQueryType.ItemAttributes, this.props.tile.item.id);
                }
                break;
        }
    }

    getBackground(heading: Direction, glyph: ITileStyle) {
        var direction = '';
        switch (heading) {
            case Direction.East:
                direction = '90deg';
                break;
            case Direction.Northeast:
                direction = '45deg';
                break;
            case Direction.North:
                direction = '0deg';
                break;
            case Direction.Northwest:
                direction = '315deg';
                break;
            case Direction.West:
                direction = '270deg';
                break;
            case Direction.Southwest:
                direction = '225deg';
                break;
            case Direction.South:
                direction = '180deg';
                break;
            case Direction.Southeast:
                direction = '135deg';
                break;
        }

        const mapBackground = scss.body_bg;
        const highlightBackground = glyph.style.backgroundColor || scss.enemy_bg;
        return `linear-gradient(${direction}, ${mapBackground} 25%, ${highlightBackground})`;
    }

    private getAttackSummary(attack: AttackSummary, attackName: string, victim: string): React.ReactElement {
        return <div>
            {`${attackName} - toHit: ${attack.hitProbability}, dmg: ${attack.damage}, delay: ${
                formatTime(attack.delay / 100)}, expected time to defeat: `}<b>{formatTime(attack.ticksToKill / 100)}</b>
            </div>;
    }

    render() {
        const { x, y, tile, styles, context } = this.props;
        let glyph: ITileStyle;
        let inlineStyle: React.CSSProperties = {};
        let tooltip: string | React.ReactElement | null = null;
        this._clickAction = TileAction.Move;
        this._contextMenuAction = TileAction.None;

        if (tile.actor !== null) {
            glyph = styles.actors[tile.actor.baseName];
            if (glyph === undefined) {
                glyph = Object.assign({ char: tile.actor.baseName[0] }, styles.actors['default']);
                // TODO: Add more creatures
                console.log(`Actor type ${tile.actor.baseName} not supported.`);
            }

            if (tile.actor.baseName === 'player'
                && tile.actor.name === context.player.name) {
                this._clickAction = TileAction.Wait;
                this._contextMenuAction = TileAction.PlayerAttributes;
                tooltip = capitalize(tile.actor.name);
            } else {
                this._clickAction = TileAction.Attack;
                this._contextMenuAction = TileAction.ActorAttributes;
                tooltip = <div>
                    <span>{'Attack ' + tile.actor.name}</span>
                    {tile.actor.meleeAttack === null
                        ? <></>
                        : this.getAttackSummary(tile.actor.meleeAttack, `${capitalize(tile.actor.name)}'s melee attack`, 'you')}
                    {tile.actor.rangeAttack === null
                        ? <></>
                        : this.getAttackSummary(tile.actor.rangeAttack, `${capitalize(tile.actor.name)}'s range attack`, 'you')}
                    {tile.actor.meleeDefense === null
                        ? <></>
                        : this.getAttackSummary(tile.actor.meleeDefense, 'Your melee attack', tile.actor.name)}
                    {tile.actor.rangeDefense === null
                        ? <></>
                        : this.getAttackSummary(tile.actor.rangeDefense, 'Your range attack', tile.actor.name)}
                </div>;
            }

            inlineStyle = { backgroundImage: this.getBackground(tile.actor.heading, glyph) };
        } else if (tile.item !== null) {
            const type = tile.item.type;
            glyph = styles.items[type];
            if (glyph === undefined) {
                throw `Item type ${type} not supported.`;
            }

            this._contextMenuAction = TileAction.ItemAttributes;
            tooltip = capitalize(tile.item.baseName || 'Unknown item');
        } else if (tile.connection !== null) {
            glyph = styles.connections[tile.connection.isDown ? 1 : 0];
            if (glyph === undefined) {
                throw `Connection type ${tile.connection.isDown} not supported.`;
            }
        } else {
            const feature = tile.feature;
            glyph = styles.features[feature];
            if (glyph === undefined) {
                if (feature === MapFeature.StoneWall) {
                    glyph = styles.walls[tile.wallNeighbours];
                    if (glyph == undefined) {
                        throw `Invalid wall neighbours: ${tile.wallNeighbours}`;
                    }
                } else {
                    throw `Map feature ${feature} not supported.`;
                }
            }
        }

        // TODO: Change pointer depending on the action
        const className = 'map__tile' + (this._contextMenuAction === TileAction.None ? '' : ' contextLink')
            + ' ' + context.player.level.tileClasses[y][x].join(' ');
        const opacity = 0.3 + ((tile.visibility / 255) * 0.7);
        const content = <div className={className} style={Object.assign({ opacity: opacity }, glyph.style, inlineStyle)}
            onClick={this.handleClick} onContextMenu={this.handleContextMenu}
        >
            {glyph.char}
        </div>;

        if (tooltip == null) {
            return <>{ content }</>;
        }

        return <TooltipTrigger
            id={`tooltip-tile-${x}-${y}`}
            tooltip={tooltip}
        >
            {content}
        </TooltipTrigger>
    }
}

interface ITileProps {
    x: number;
    y: number;
    tile: Tile;
    styles: MapStyles;
    context: IGameContext;
}