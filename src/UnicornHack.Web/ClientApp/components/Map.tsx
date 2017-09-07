import * as React from 'React';
import { Level, MapFeature, DirectionFlags, ItemType } from './Model';
import { MapStyles, ITileStyle } from './MapStyles';

export class Map extends React.Component<IMapProps, {}> {
    render() {
        const rows = new Array<ITileProps[]>(this.props.level.height);
        const styles = this.props.styles;
        const terrain = this.props.level.terrain;
        const wallNeighbours = this.props.level.wallNeighbours;
        const visibleTerrain = this.props.level.visibleTerrain;

        let i = 0;
        for (let y = 0; y < this.props.level.height; y++) {
            const row = new Array<ITileProps>(this.props.level.width);
            rows[y] = row;
            for (let x = 0; x < this.props.level.width; x++) {
                const feature = terrain[i];
                let glyph = styles.features[feature];
                if (glyph === undefined) {
                    if (feature === MapFeature.StoneWall) {
                        const neighbours = wallNeighbours[i] & DirectionFlags.Cross;

                        glyph = styles.walls[neighbours];
                        if (glyph === undefined) {
                            throw `Invalid wall neighbours: ${neighbours}`;
                        }
                    } else {
                        throw `Map feature ${feature} not supported.`;
                    }
                }

                row[x] = { glyph: glyph, visibility: visibleTerrain[i] };
                i++;
            }
        }

        this.props.level.connections.map(c => rows[c.levelY][c.levelX].glyph = styles.connections[c.isDown ? 1 : 0]);

        this.props.level.items.map(t => {
            const type = t.type & ~ItemType.Intricate & ~ItemType.Exotic;
            const glyph = styles.items[type];
            if (glyph === undefined) {
                throw `Item type ${type} not supported.`;
            }
            return rows[t.levelY][t.levelX].glyph = glyph;
        });

        this.props.level.actors.map(a => {
            let glyph = styles.actors[a.baseName];
            if (glyph === undefined) {
                // TODO: Add more creatures
                //throw `Actor type ${a.baseName} not supported.`;
                glyph = Object.assign({}, styles.actors['default'], { char: a.baseName[0] });
            }
            return rows[a.levelY][a.levelX].glyph = glyph;
        });

        const map = new Array<React.ReactElement<any>>(rows.length);
        for (let y = 0; y < rows.length; y++) {
            map[y] = <Row row={rows[y]} key={y} />;
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

class Row extends React.Component<IRowProps, {}> {
    render() {
        const row = this.props.row.map((t, i) =>
            <Tile {...t} key={i} />
        );
        return (<div>{row}</div>);
    }
}

interface IRowProps {
    row: ITileProps[];
}

class Tile extends React.Component<ITileProps, {}> {
    render() {
        const opacity = 0.3 + ((this.props.visibility / 255) * 0.7);
        return (<div className="map__row" style={Object.assign({ opacity: opacity, float: 'left' }, this.props.glyph.style)}>
            {this.props.glyph.char}
        </div>);
    }
}

interface ITileProps {
    glyph: ITileStyle;
    visibility: number;
}