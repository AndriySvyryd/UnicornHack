import { CSSProperties } from 'React';
import { MapFeature } from '../transport/MapFeature';
import { ItemType } from '../transport/ItemType';
import { DirectionFlags } from '../transport/DirectionFlags';

export class MapStyles {
    actors = new MapActorStyles();
    features = new MapFeatureStyles();
    walls = new WallStyles();
    connections = new ConnectionsStyles();
    items = new ItemStyles();

    private static _instance: MapStyles;

    private constructor() {
        Object.freeze(this);
    }

    public static get Instance() {
        return this._instance || (this._instance = new this());
    }
}

export interface ITileStyle {
    style: CSSProperties;
    char?: string;
}

class MapActorStyles {
    [index: string]: ITileStyle;

    '': ITileStyle = { char: '?', style: { color: 'lightgray' } };
    'player': ITileStyle = { char: '@', style: { color: 'black', backgroundColor: 'lightgrey' } };
    'human': ITileStyle = { char: 'H', style: { color: 'lightgray' } };
    'unicorn': ITileStyle = { char: 'U', style: { color: 'white' } };
    'firefly': ITileStyle = { char: 'f', style: { color: 'darkorange' } };
    'giant spider': ITileStyle = { char: 's', style: { color: 'gray' } };
    'default': ITileStyle = { style: { color: 'red' } };
}

export class MapFeatureStyles {
    [index: number]: ITileStyle;

    constructor() {
        this[MapFeature.Default] = { char: '█', style: { color: '#666' } };
        this[MapFeature.StoneFloor] = { char: '·', style: { color: 'grey' } };
        this[MapFeature.RockFloor] = { char: '◦', style: { color: 'dimgrey' } };
        this[MapFeature.StoneArchway] = { char: '∩', style: { color: 'grey' } };
        this[MapFeature.Pool] = { char: '≈', style: { color: 'aqua' } };
        this[MapFeature.Unexplored] = { char: ' ', style: { } };
    }
}

export class WallStyles {
    [index: number]: ITileStyle;

    constructor() {
        const defaultStyle = { color: 'grey', backgroundColor: 'darkslategray' };
        this[DirectionFlags.None] = { char: '●', style: { color: 'grey' } };
        this[DirectionFlags.North] = { char: '╹', style: defaultStyle };
        this[DirectionFlags.East] = { char: '╺', style: defaultStyle };
        this[DirectionFlags.NorthAndEast] = { char: '┗', style: defaultStyle };
        this[DirectionFlags.South] = { char: '╻', style: defaultStyle };
        this[DirectionFlags.Longitudinal] = { char: '┃', style: defaultStyle };
        this[DirectionFlags.SouthAndEast] = { char: '┏', style: defaultStyle };
        this[DirectionFlags.NorthEastSouth] = { char: '┣', style: defaultStyle };
        this[DirectionFlags.West] = { char: '╸', style: defaultStyle };
        this[DirectionFlags.NorthAndWest] = { char: '┛', style: defaultStyle };
        this[DirectionFlags.Latitudinal] = { char: '━', style: defaultStyle };
        this[DirectionFlags.NorthEastWest] = { char: '┻', style: defaultStyle };
        this[DirectionFlags.SouthAndWest] = { char: '┓', style: defaultStyle };
        this[DirectionFlags.NorthWestSouth] = { char: '┫', style: defaultStyle };
        this[DirectionFlags.SouthEastWest] = { char: '┳', style: defaultStyle };
        this[DirectionFlags.Cross] = { char: '╋', style: defaultStyle };
    }
}

export class ConnectionsStyles {
    [index: number]: ITileStyle;

    constructor() {
        const defaultStyle = { color: 'lightgray' };
        this[0] = { char: '<', style: defaultStyle };
        this[1] = { char: '>', style: defaultStyle };
    }
}

export class ItemStyles {
    [index: number]: ITileStyle;

    constructor() {
        this[ItemType.None] = { char: '?', style: { color: 'gray' } };

        this[ItemType.Coin] = { char: '$', style: { color: 'gold' } };

        this[ItemType.AccessoryNeck] = { char: '"', style: { color: 'limegreen' } };

        const armor = { char: '(', style: { color: 'teal' }  };
        this[ItemType.AccessoryBack] = armor;
        this[ItemType.Shield] = armor;
        this[ItemType.ArmorTorso] = armor;
        this[ItemType.ArmorFeet] = armor;
        this[ItemType.ArmorHands] = armor;
        this[ItemType.ArmorHead] = armor;

        const weapon = { char: ')', style: { color: 'chocolate' }  };
        this[ItemType.WeaponMeleeHand] = weapon;
        this[ItemType.WeaponMeleeShort] = weapon;
        this[ItemType.WeaponMeleeMedium] = weapon;
        this[ItemType.WeaponMeleeLong] = weapon;
        this[ItemType.WeaponRangedClose] = weapon;
        this[ItemType.WeaponRangedShort] = weapon;
        this[ItemType.WeaponRangedMedium] = weapon;
        this[ItemType.WeaponRangedLong] = weapon;
        this[ItemType.WeaponAmmoContainer] = weapon;
        this[ItemType.WeaponProjectile] = weapon;

        this[ItemType.Potion] = { char: '!', style: { color: 'yellow' } };

        this[ItemType.SkillBook] = { char: '+', style: { color: 'magenta' } };
    }
}