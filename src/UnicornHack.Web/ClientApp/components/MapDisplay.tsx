import * as React from 'React';
import * as scss from '../styles/site.scss'
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { MapStyles, ITileStyle } from '../styles/MapStyles';
import { Level, Tile } from '../transport/Model';
import { PlayerAction } from '../transport/PlayerAction';
import { Direction } from '../transport/Direction';
import { MapFeature } from '../transport/MapFeature';
import { capitalize } from '../Util';
import { TooltipTrigger } from './TooltipTrigger';

export const MapDisplay = observer((props: IMapProps) => {
    const level = props.level;

    const map = new Array<React.ReactElement<any>>(level.height);
    for (let y = 0; y < level.height; y++) {
        map[y] = <MapRow
            y={y}
            row={level.tiles[y]}
            styles={props.styles}
            indexToPoint={level.indexToPoint}
            performAction={props.performAction}
            key={y} />;
    }

    return <div className="mapContainer">
        <div className="map">{map}</div>
    </div>;
});

interface IMapProps {
    level: Level;
    styles: MapStyles;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}

const MapRow = observer((props: IRowProps) => {
    const row = props.row.map((t, x) =>
        <MapTile
            x={x} y={props.y}
            tile={t}
            styles={props.styles}
            performAction={props.performAction}
            key={x} />
    );
    return <div className="map__row">{row}</div>;
});

interface IRowProps {
    y: number;
    row: Tile[];
    styles: MapStyles;
    indexToPoint: number[][];
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}

@observer
class MapTile extends React.Component<ITileProps, {}> {
    @action.bound
    move(event: React.MouseEvent<HTMLDivElement>) {
        this.props.performAction(PlayerAction.MoveToCell, Level.pack(this.props.x, this.props.y), null);
    }

    @action.bound
    wait(event: React.MouseEvent<HTMLDivElement>) {
        this.props.performAction(PlayerAction.Wait, null, null);
    }

    @action.bound
    attack(event: React.MouseEvent<HTMLDivElement>) {
        this.props.performAction(PlayerAction.UseAbilitySlot, null, Level.pack(this.props.x, this.props.y));
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

    render() {
        const tile = this.props.tile;
        const styles = this.props.styles;
        let glyph: ITileStyle;
        let onClick: undefined | ((event: React.MouseEvent<HTMLDivElement>) => void) = this.move;

        // TODO: Also change pointer
        var content: (JSX.Element | string) = '';
        if (tile.actor != null) {
            glyph = styles.actors[tile.actor.baseName];
            if (glyph == undefined) {
                glyph = Object.assign({}, styles.actors['default'], { char: tile.actor.baseName[0] });
            }

            // TODO: Add more creatures
            if (glyph == undefined) {
                throw `Actor type ${tile.actor.baseName} not supported.`;
            }

            let tooltip: string;
            // TODO: check position instead of base name
            if (tile.actor.baseName == 'player') {
                onClick = this.wait;
                tooltip = capitalize(tile.actor.name)
            } else {
                onClick = this.attack;
                tooltip = 'Attack ' + tile.actor.baseName;
            }
            
            content = <TooltipTrigger
                id={`tooltip-actor-${tile.actor.id}`}
                delay={100}
                tooltip={tooltip}
            >
                <div onClick={onClick} style={{ backgroundImage: this.getBackground(tile.actor.heading, glyph) }}>
                    {glyph.char}
                </div>
            </TooltipTrigger>;
            onClick = undefined;
        } else if (tile.item != null) {
            const type = tile.item.type;
            glyph = styles.items[type];
            if (glyph == undefined) {
                throw `Item type ${type} not supported.`;
            }

            content = <TooltipTrigger
                id={`tooltip-item-${tile.item.id}`}
                delay={100}
                tooltip={capitalize(tile.item.baseName)}
            >
                <span>{glyph.char}</span>
            </TooltipTrigger>
        } else if (tile.connection != null) {
            glyph = styles.connections[tile.connection.isDown ? 1 : 0];
            if (glyph == undefined) {
                throw `Connection type ${tile.connection.isDown} not supported.`;
            }
        } else {
            const feature = tile.feature;
            glyph = styles.features[feature];
            if (glyph == undefined) {
                if (feature === MapFeature.StoneWall) {
                    glyph = styles.walls[tile.wallNeighbours];
                    if (glyph == undefined) {
                        throw `Invalid wall neighbours: ${tile.wallNeighbours}`;
                    }
                } else {
                    throw `Map feature ${feature} not supported.`;
                }
            }

            switch (tile.feature) {
                case MapFeature.RockFloor:
                case MapFeature.StoneFloor:
                case MapFeature.StoneArchway:
                    break;
                default:
                    onClick = undefined;
            }
        }

        if (content == '') {
            content = glyph.char;
        }

        const opacity = 0.3 + ((tile.visibility / 255) * 0.7);
        return <div className="map__tile" style={Object.assign({ opacity: opacity }, glyph.style)} onClick={onClick}>
            {content}
        </div>;
    }
}

interface ITileProps {
    x: number;
    y: number;
    tile: Tile;
    styles: MapStyles;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}