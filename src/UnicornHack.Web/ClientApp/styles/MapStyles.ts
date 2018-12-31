import { CSSProperties } from 'React';
import { MapFeature, DirectionFlags, ItemType } from '../transport/Model';

export class MapStyles {
    actors = new MapActorStyles();
    features = new MapFeatureStyles();
    walls = new WallStyles();
    connections = new ConnectionsStyles();
    items = new ItemStyles();
}

export interface ITileStyle {
    style: CSSProperties;
    char: string;
}

class MapActorStyles {
    [index: string]: ITileStyle;

    '': ITileStyle = { char: '?', style: { color: 'lightgray' } };
    'player': ITileStyle = { char: '@', style: { color: 'black', backgroundColor: 'lightgrey' } };
    'human': ITileStyle = { char: 'H', style: { color: 'lightgray' } };
    'unicorn': ITileStyle = { char: 'U', style: { color: 'white' } };
    'lightning bug': ITileStyle = { char: 'b', style: { color: 'aqua' } };
    'firefly': ITileStyle = { char: 'F', style: { color: 'darkorange' } };
    'giant ant': ITileStyle = { char: 'a', style: { color: 'gray' } };
    'jaguar': ITileStyle = { char: 'j', style: { color: 'orange' } };
    'default': ITileStyle = { char: 'x', style: { color: 'red' } };
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
        const defaultStyle = { color: 'grey' };
        this[DirectionFlags.None] = { char: '●', style: defaultStyle };
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
        this[ItemType.Coin] = { char: '$', style: { color: 'gold' } };

        const armor = { char: '(', style: { color: 'teal' }  };
        this[ItemType.AccessoryBack] = armor;
        this[ItemType.ArmorTorso] = armor;
        this[ItemType.ArmorFeet] = armor;
        this[ItemType.ArmorHands] = armor;
        this[ItemType.ArmorHead] = armor;

        const weapon = { char: ')', style: { color: 'chocolate' }  };
        this[ItemType.WeaponMeleeFist] = weapon;
        this[ItemType.WeaponMeleeShort] = weapon;
        this[ItemType.WeaponMeleeMedium] = weapon;
        this[ItemType.WeaponMeleeLong] = weapon;
        this[ItemType.WeaponMagicFocus] = weapon;
        this[ItemType.WeaponMagicStaff] = weapon;
        this[ItemType.WeaponRangedBow] = weapon;
        this[ItemType.WeaponRangedCrossbow] = weapon;
        this[ItemType.WeaponRangedSlingshot] = weapon;
        this[ItemType.WeaponRangedThrown] = weapon;
        this[ItemType.WeaponAmmoContainer] = weapon;

        this[ItemType.Potion] = { char: '!', style: { color: 'yellow' } };

        this[ItemType.SkillBook] = { char: '+', style: { color: 'magenta' } };
    }
}