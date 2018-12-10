import * as React from 'React';
import * as scss from '../styles/site.scss'
import { observer } from 'mobx-react';
import { Level, MapFeature, Tile } from '../transport/Model';
import { MapStyles, ITileStyle } from '../styles/MapStyles';
import { PlayerAction } from '../transport/PlayerAction';
import { Direction } from 'ClientApp/transport/Direction';

@observer
export class MapDisplay extends React.Component<IMapProps, {}> {
    render() {
        const level = this.props.level;

        const map = new Array<React.ReactElement<any>>(level.height);
        for (let y = 0; y < level.height; y++) {
            map[y] = <MapRow
                y={y}
                row={level.tiles[y]}
                styles={this.props.styles}
                indexToPoint={level.indexToPoint}
                performAction={this.props.performAction}
                key={y} />;
        }

        return (
            <div className={scss.mapContainer + " " + scss.frame} >
                <div className={scss.map}>{map}</div>
            </div>);
    }
}

interface IMapProps {
    level: Level;
    styles: MapStyles;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}

@observer
class MapRow extends React.Component<IRowProps, {}> {
    render() {
        const row = this.props.row.map((t, x) =>
            <MapTile
                x={x} y={this.props.y}
                tile={t}
                styles={this.props.styles}
                performAction={this.props.performAction}
                key={x} />
        );
        return (<div className={scss.map__row}>{row}</div>);
    }
}

interface IRowProps {
    y: number;
    row: Tile[];
    styles: MapStyles;
    indexToPoint: number[][];
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}

@observer
class MapTile extends React.Component<ITileProps, {}> {
    render() {
        const tile = this.props.tile;
        const styles = this.props.styles;
        let glyph: ITileStyle;
        let onClick: ((() => void) | undefined) =
            () => this.props.performAction(PlayerAction.MoveToCell, Level.pack(this.props.x, this.props.y), null);
        // TODO: Also change pointer

        var content: (JSX.Element | string) = "";
        if (tile.actor != null) {
            glyph = styles.actors[tile.actor.baseName];
            if (glyph == undefined) {
                glyph = Object.assign({}, styles.actors['default'], { char: tile.actor.baseName[0] });
            }

            // TODO: Add more creatures
            if (glyph == undefined) {
                throw `Actor type ${tile.actor.baseName} not supported.`;
            }

            // TODO: check position instead of base name
            if (tile.actor.baseName == 'player') {
                onClick = () => this.props.performAction(
                    PlayerAction.Wait, null, null);
            } else {
                onClick = () => this.props.performAction(
                    PlayerAction.PerformDefaultAttack, Level.pack(this.props.x, this.props.y), null);
            }

            var direction = '';
            switch (tile.actor.heading) {
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
            const inlineStyle = {
                backgroundImage: `linear-gradient(${direction}, ${mapBackground} 25%, ${highlightBackground})`
            };
            content = <div onClick={onClick} style={inlineStyle}>{glyph.char}</div>;
            onClick = undefined;
        } else if (tile.item != null) {
            const type = tile.item.type;
            glyph = styles.items[type];
            if (glyph == undefined) {
                throw `Item type ${type} not supported.`;
            }
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
        return (<div className={scss.map__tile} style={Object.assign({ opacity: opacity }, glyph.style)} onClick={onClick}>
            {content}
        </div>);
    }
}

interface ITileProps {
    x: number;
    y: number;
    tile: Tile;
    styles: MapStyles;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
}