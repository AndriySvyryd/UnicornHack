import * as React from 'React';
import { observer } from 'mobx-react';
import { Level, MapFeature, Tile } from '../transport/Model';
import { MapStyles, ITileStyle } from '../styles/MapStyles';
import { PlayerAction } from 'ClientApp/transport/PlayerAction';

@observer
export class Map extends React.Component<IMapProps, {}> {
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

        return (<div className="mapContainer frame">
                    <div className="map"><div className="map__inner-container">{map}</div></div>
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
        return (<div>{row}</div>);
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
        // TODO: also change pointer

        if (tile.actor != null) {
            glyph = styles.actors[tile.actor.baseName];
            if (glyph == undefined) {
                glyph = Object.assign({}, styles.actors['default'], { char: tile.actor.baseName[0] });
            }
            // TODO: Add more creatures
            if (glyph == undefined) {
                throw `Actor type ${tile.actor.baseName} not supported.`;
            }

            if (tile.actor.baseName == 'player') {
                onClick = () => this.props.performAction(
                    PlayerAction.Wait, null, null);
            }
            else {
                onClick = () => this.props.performAction(
                    PlayerAction.PerformDefaultAttack, Level.pack(this.props.x, this.props.y), null);
            }
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

        const opacity = 0.3 + ((tile.visibility / 255) * 0.7);
        return (<div className="map__tile" style={Object.assign({ opacity: opacity }, glyph.style)} onClick={onClick}>
            {glyph.char}
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