/// <reference path='../../node_modules/@types/node/index.d.ts' />

export class Level {
    branchName: string;
    depth: number = -1;
    height: number = 1;
    width: number = 1;
    terrain: Uint8Array;
    wallNeighbours: Uint8Array;
    visibleTerrain: Uint8Array;
    actors: Actor[] = [];
    items: Item[] = [];
    connections: Connection[] = [];

    constructor() {
        // '\0'.repeat(level.height * level.width)
        //new Array<number>(this.height * this.width).fill(0)
        this.terrain = new Uint8Array(this.height * this.width);
        this.wallNeighbours = this.terrain;
        this.visibleTerrain = this.terrain;
    }

    static expand(compactLevel: ICompactLevel): Level {
        const level = new Level();
        let i = 0;
        level.branchName = compactLevel.Properties[i++];
        level.depth = compactLevel.Properties[i++];
        level.width = compactLevel.Properties[i++];
        level.height = compactLevel.Properties[i++];
        level.terrain = new Uint8Array(Buffer.from(compactLevel.Properties[i++], 'base64'));
        level.wallNeighbours = new Uint8Array(Buffer.from(compactLevel.Properties[i++], 'base64'));
        level.visibleTerrain = new Uint8Array(Buffer.from(compactLevel.Properties[i++], 'base64'));
        level.actors = compactLevel.Properties[i++].map((a: ICompactActor) => Actor.expand(a));
        level.items = compactLevel.Properties[i++].map((a: ICompactItem) => Item.expand(a));
        level.connections = compactLevel.Properties[i++].map((a: ICompactConnection) => Connection.expand(a));

        return level;
    }
}

export interface ICompactLevel {
    // ReSharper disable once InconsistentNaming
    Properties: any[];
}

export class Actor {
    protected static actorPropertyCount: number = 6;
    id: number = 0;
    baseName: string = '';
    name: string = '';
    levelX: number = 0;
    levelY: number = 0;
    heading: number = 0;

    static expand(compactActor: ICompactActor): Actor {
        const actor = compactActor.Properties.length === Actor.actorPropertyCount
            ? new Actor()
            : Player.expand(compactActor);
        let i = 0;
        actor.id = compactActor.Properties[i++];
        actor.baseName = compactActor.Properties[i++];
        actor.name = compactActor.Properties[i++];
        actor.levelX = compactActor.Properties[i++];
        actor.levelY = compactActor.Properties[i++];
        actor.heading = compactActor.Properties[i++];
        return actor;
    }
}

export class Player extends Actor {
    properties: Property[] = [];
    inventory: Item[] = [];
    abilities: Ability[] = [];
    nextActionTick: number = 0;
    xP: number = 0;
    nextLevelXP: number = 0;
    xPLevel: number = 0;
    gold: number = 0;
    log: LogEntry[] = [];
    races: PlayerRace[] = [];

    constructor() {
        super();
        this.baseName = 'player';
    }

    static expand(compactActor: ICompactActor): Player {
        const player = new Player();
        let i = Actor.actorPropertyCount;
        player.properties = compactActor.Properties[i++].map((a: ICompactProperty) => Property.expand(a));
        player.inventory = compactActor.Properties[i++].map((a: ICompactItem) => Item.expand(a));
        player.abilities = compactActor.Properties[i++].map((a: ICompactAbility) => Ability.expand(a));
        player.nextActionTick = compactActor.Properties[i++];
        player.xP = compactActor.Properties[i++];
        player.nextLevelXP = compactActor.Properties[i++];
        player.xPLevel = compactActor.Properties[i++];
        player.gold = compactActor.Properties[i++];
        player.log = compactActor.Properties[i++].map((a: ICompactLogEntry) => LogEntry.expand(a));
        player.races = compactActor.Properties[i++].map((a: ICompactPlayerRace) => PlayerRace.expand(a));
        return player;
    }
}

export interface ICompactActor {
    // ReSharper disable once InconsistentNaming
    Properties: any[];
}

export class Property {
    name: string;
    displayName: string;

    static expand(compactProperty: ICompactProperty): Property {
        const property = new Property();
        let i = 0;
        property.name = compactProperty.Properties[i++];
        property.displayName = compactProperty.Properties[i++];

        return property;
    }
}

export interface ICompactProperty {
    // ReSharper disable once InconsistentNaming
    Properties: any[];
}

export class Item {
    protected static itemPropertyCount: number = 8;
    id: number;
    baseName: string;
    name: string;
    levelX: number;
    levelY: number;
    type: ItemType;
    equippedSlot: string;
    equippableSlots: IEquipableSlot[];

    static expand(compactItem: ICompactItem): Item {
        const item = compactItem.Properties.length === Item.itemPropertyCount
            ? new Item()
            : compactItem.Properties[5] === ItemType.Coin
                ? Gold.expand(compactItem)
                : Container.expand(compactItem);

        let i = 0;
        item.id = compactItem.Properties[i++];
        item.baseName = compactItem.Properties[i++];
        item.name = compactItem.Properties[i++];
        item.levelX = compactItem.Properties[i++];
        item.levelY = compactItem.Properties[i++];
        item.type = compactItem.Properties[i++];
        item.equippedSlot = compactItem.Properties[i++];
        const slots = compactItem.Properties[i++];
        item.equippableSlots = slots == null
            ? null
            : slots.map((s: any[]) => { return { id: s[0], name: s[1] }; });
        return item;
    }
}

export class Gold extends Item {
    quantity: number;

    static expand(compactItem: ICompactItem): Gold {
        const item = new Gold();
        item.quantity = compactItem.Properties[Item.itemPropertyCount];
        return item;
    }
}

export class Container extends Item {
    quantity: number;

    static expand(compactItem: ICompactItem): Container {
        const item = new Container();
        item.quantity = compactItem.Properties[Item.itemPropertyCount];
        return item;
    }
}

interface IEquipableSlot {
    id: number;
    name: string;
}

export interface ICompactItem {
    // ReSharper disable once InconsistentNaming
    Properties: any[];
}

export class Ability {
    id: number;
    name: string;
    isDefault: boolean | undefined;

    static expand(compactAbility: ICompactAbility): Ability {
        const ability = new Ability();
        let i = 0;
        ability.id = compactAbility.Properties[i++];
        ability.name = compactAbility.Properties[i++];
        if (compactAbility.Properties.length === 3) {
            ability.isDefault = compactAbility.Properties[i++];
        }

        return ability;
    }
}

export interface ICompactAbility {
    // ReSharper disable once InconsistentNaming
    Properties: any[];
}

export class LogEntry {
    id: string;
    message: string;

    static expand(compactLogEntry: ICompactLogEntry): LogEntry {
        const logEntry = new LogEntry();
        let i = 0;
        logEntry.id = compactLogEntry.Properties[i++];
        logEntry.message = compactLogEntry.Properties[i++];

        return logEntry;
    }
}

export interface ICompactLogEntry {
    // ReSharper disable once InconsistentNaming
    Properties: any[];
}

export class PlayerRace {
    name: string;
    xPLevel: number;
    isLearning: boolean;

    static expand(compactPlayerRace: ICompactPlayerRace): PlayerRace {
        let playerRace = new PlayerRace();
        let i = 0;
        playerRace.name = compactPlayerRace.Properties[i++];
        playerRace.xPLevel = compactPlayerRace.Properties[i++];
        playerRace.isLearning = compactPlayerRace.Properties[i++];

        return playerRace;
    }
}

export interface ICompactPlayerRace {
    // ReSharper disable once InconsistentNaming
    Properties: any[];
}

export class Connection {
    levelX: number;
    levelY: number;
    isDown: boolean;

    static expand(compactConnection: ICompactConnection): Connection {
        let connection = new Connection();
        let i = 0;
        connection.levelX = compactConnection.Properties[i++];
        connection.levelY = compactConnection.Properties[i++];
        connection.isDown = compactConnection.Properties[i++];

        return connection;
    }
}

export interface ICompactConnection {
    // ReSharper disable once InconsistentNaming
    Properties: any[];
}

export const enum MapFeature {
    Default = 0,
    RockFloor = 1,
    StoneFloor = 2,
    RockWall = 3,
    StoneWall = 4,
    StoneArchway = 5,
    Pool = 14
}

export const enum DirectionFlags {
    None = 0,
    East = 1 << 0,
    Northeast = 1 << 1,
    North = 1 << 2,
    Northwest = 1 << 3,
    West = 1 << 4,
    Southwest = 1 << 5,
    South = 1 << 6,
    Southeast = 1 << 7,
    Up = 1 << 8,
    Down = 1 << 9,
    Center = 1 << 10,
    NorthAndEast = North | East,
    NorthAndWest = North | West,
    SouthAndWest = South | West,
    SouthAndEast = South | East,
    Longitudinal = North | South,
    Latitudinal = West | East,
    Diagonal = Northeast | Southwest,
    AntiDiagonal = Northwest | Southeast,
    NorthEastWest = North | East | West,
    NorthEastSouth = North | East | South,
    NorthWestSouth = North | West | South,
    SouthEastWest = South | East | West,
    NorthwestCorner = West | Northwest | North,
    NortheastCorner = North | Northeast | East,
    SoutheastCorner = East | Southeast | South,
    SouthwestCorner = South | Southwest | West,
    Cross = Longitudinal | Latitudinal,
    DiagonalCross = Diagonal | AntiDiagonal,
    NorthSemicircle = NorthwestCorner | NortheastCorner,
    EastSemicircle = NortheastCorner | SoutheastCorner,
    SouthSemicircle = SoutheastCorner | SouthwestCorner,
    WestSemicircle = SouthwestCorner | NorthwestCorner,
    Circle = NorthSemicircle | SouthSemicircle
}

export const enum ItemType {
    None = 0,
    Coin = 1 << 0,
    WeaponMeleeFist = 1 << 2,
    WeaponMeleeShort = 1 << 3,
    WeaponMeleeMedium = 1 << 4,
    WeaponMeleeLong = 1 << 5,
    WeaponMagicFocus = 1 << 6,
    WeaponMagicStaff = 1 << 7,
    WeaponRangedThrown = 1 << 8,
    WeaponRangedSlingshot = 1 << 9,
    WeaponRangedBow = 1 << 10,
    WeaponRangedCrossbow = 1 << 11,
    WeaponAmmoContainer = 1 << 12,
    Shield = 1 << 13,
    ArmorBody = 1 << 14,
    ArmorHead = 1 << 15,
    ArmorFeet = 1 << 16,
    ArmorHands = 1 << 17,
    ArmorBack = 1 << 18,
    Accessory = 1 << 19,
    Container = 1 << 20,
    Potion = 1 << 21,
    Wand = 1 << 22,
    Figurine = 1 << 23,
    Trinket = 1 << 25,
    SkillBook = 1 << 26,
    Intricate = 1 << 30,
    Exotic = 1 << 31,

    WeaponRanged = WeaponRangedThrown | WeaponRangedSlingshot | WeaponRangedBow | WeaponRangedCrossbow
        | WeaponMagicStaff,

    WeaponMelee = WeaponMeleeFist | WeaponMeleeShort | WeaponMeleeMedium | WeaponMeleeLong
        | WeaponMagicFocus,

    Wepon = WeaponMelee | WeaponRanged
}