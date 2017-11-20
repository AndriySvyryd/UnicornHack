import * as React from 'React';
import { observer } from 'mobx-react';
import { Level, MapFeature, ItemType, Tile } from '../transport/Model';
import { MapStyles, ITileStyle } from '../styles/MapStyles';

@observer
export class Map extends React.Component<IMapProps, {}> {
    render() {
        const level = this.props.level;

        const map = new Array<React.ReactElement<any>>(level.height);
        for (let y = 0; y < level.height; y++) {
            map[y] = <MapRow row={level.tiles[y]} styles={this.props.styles} indexToPoint={level.indexToPoint} key={y} />;
        }

        return (<div className="mapContainer frame">
                    <div className="map"><div className="map__inner-container">{map}</div></div>
                </div>);
    }
}

interface IMapProps {
    level: Level;
    styles: MapStyles;
}

@observer
class MapRow extends React.Component<IRowProps, {}> {
    render() {
        const row = this.props.row.map((t, x) =>
            <MapTile tile={t} styles={this.props.styles} key={x} />
        );
        return (<div>{row}</div>);
    }
}

interface IRowProps {
    row: Tile[];
    styles: MapStyles;
    indexToPoint: number[][];
}

@observer
class MapTile extends React.Component<ITileProps, {}> {
    render() {
        const tile = this.props.tile;
        const styles = this.props.styles;
        let glyph: ITileStyle;
        if (tile.actor != null) {
            glyph = styles.actors[tile.actor.baseName];
            if (glyph == undefined) {
                glyph = Object.assign({}, styles.actors['default'], { char: tile.actor.baseName[0] });
            }
            // TODO: Add more creatures
            if (glyph == undefined) {
                throw `Actor type ${tile.actor.baseName} not supported.`;
            }
        } else if (tile.item != null) {
            const type = tile.item.type & ~ItemType.Intricate & ~ItemType.Exotic;
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
        }

        const opacity = 0.3 + ((tile.visibility / 255) * 0.7);
        return (<div className="map__row" style={Object.assign({ opacity: opacity, float: 'left' }, glyph.style)}>
            {glyph.char}
        </div>);
    }
}

interface ITileProps {
    tile: Tile;
    styles: MapStyles;
}