/// <reference path='../../node_modules/@types/node/index.d.ts' />

import { observable, computed, action } from "mobx";

export class Level {
    @observable
    currentTick: number;
    @observable
    branchName: string;
    @observable
    depth: number = -1;
    @observable
    height: number = 1;
    @observable
    width: number = 1;
    @observable
    tiles: Array<Array<Tile>>;
    actors: Map<string, Actor> = new Map<string, Actor>();
    items: Map<string, Item> = new Map<string, Item>();
    connections: Map<string, Connection> = new Map<string, Connection>();
    indexToPoint: number[][];

    constructor() {
        this.tiles = [[new Tile()]];
    }

    @action
    static expand(compactLevel: any[], currentLevel: Level | undefined = undefined): (Level | null) {
        let i = 0;
        const state = compactLevel[i++] as EntityState;
        if (state !== EntityState.Added) {
            if (currentLevel == undefined) {
                return null;
            }

            const previousTick = compactLevel[i++];
            const currentTick = compactLevel[i++];
            if (currentLevel.currentTick < previousTick) {
                return null;
            } else if (currentLevel.currentTick > currentTick) {
                return currentLevel;
            }

            currentLevel.currentTick = currentTick;
            for (; i < compactLevel.length;) {
                switch (compactLevel[i++]) {
                    case 2:
                        compactLevel[i++].map(
                            (a: any[]) => Actor.expandToCollection(a, currentLevel.actors, currentLevel.tiles, state));
                        break;
                    case 3:
                        compactLevel[i++].map(
                            (a: any[]) => Item.expandToCollection(a, currentLevel.items, currentLevel.tiles, state));
                        break;
                    case 4:
                        compactLevel[i++].map(
                            (a: any[]) => Connection.expandToCollection(a, currentLevel.connections, currentLevel.tiles, state));
                        break;
                    case 5:
                        const terrainChanges = compactLevel[i++];
                        for (let i = 0; i < terrainChanges.length;) {
                            const changedPoint = currentLevel.indexToPoint[terrainChanges[i++]];
                            currentLevel.tiles[changedPoint[1]][changedPoint[0]].feature = terrainChanges[i++];
                        }
                        break;
                    case 6:
                        const wallNeighboursChanges = compactLevel[i++];
                        for (let i = 0; i < wallNeighboursChanges.length;) {
                            const changedPoint = currentLevel.indexToPoint[wallNeighboursChanges[i++]];
                            currentLevel.tiles[changedPoint[1]][changedPoint[0]].wallNeighbours =
                                wallNeighboursChanges[i++] & DirectionFlags.Cross;
                        }
                        break;
                    case 7:
                        const visibleTerrainChanges = compactLevel[i++];
                        for (let i = 0; i < visibleTerrainChanges.length;) {
                            const changedPoint = currentLevel.indexToPoint[visibleTerrainChanges[i++]];
                            currentLevel.tiles[changedPoint[1]][changedPoint[0]].visibility = visibleTerrainChanges[i++];
                        }
                        break;
                    case 8:
                        currentLevel.branchName = compactLevel[i++];
                        break;
                    case 9:
                        currentLevel.depth = compactLevel[i++];
                        break;
                    case 10:
                        currentLevel.width = compactLevel[i++];
                        break;
                    case 11:
                        currentLevel.height = compactLevel[i++];
                        break;
                }
            }

            return currentLevel;
        }

        const level = new Level();

        level.currentTick = compactLevel[i++];
        const actors = compactLevel[i++];
        const items = compactLevel[i++];
        const connections = compactLevel[i++];
        const terrain = compactLevel[i++];
        const wallNeighbours = compactLevel[i++];
        const visibleTerrain = compactLevel[i++];
        level.branchName = compactLevel[i++];
        level.depth = compactLevel[i++];
        level.width = compactLevel[i++];
        level.height = compactLevel[i++];

        level.tiles = new Array<Array<Tile>>(level.height);
        level.indexToPoint = new Array<Array<number>>(level.height * level.width);

        i = 0;
        for (let y = 0; y < level.height; y++) {
            level.tiles[y] = new Array<Tile>(level.width);
            const row = level.tiles[y];
            for (let x = 0; x < level.width; x++) {
                var tile = new Tile();
                tile.visibility = visibleTerrain[i];
                tile.feature = terrain[i];
                tile.wallNeighbours = wallNeighbours[i] & DirectionFlags.Cross;
                row[x] = tile;
                level.indexToPoint[i] = [x, y];
                i++;
            }
        }

        actors.map((a: any[]) => Actor.expandToCollection(a, level.actors, level.tiles, EntityState.Added));
        items.map((a: any[]) => Item.expandToCollection(a, level.items, level.tiles, EntityState.Added));
        connections.map((a: any[]) => Connection.expandToCollection(a, level.connections, level.tiles, EntityState.Added));

        return level;
    }

    static pack(x: number, y: number): number {
        return x << 8 | y;
    }
}

export class Tile {
    @observable actor: Actor | null = null;
    @observable item: Item | null = null;
    @observable connection: Connection | null = null;
    @observable visibility: number = 0;
    @observable wallNeighbours: number = 0;
    @observable feature: number = 0;
}

export class Actor {
    protected static actorPropertyCount: number = 6;
    @observable id: number = 0;
    @observable baseName: string = '';
    @observable name: string = '';
    @observable levelX: number = 0;
    @observable levelY: number = 0;
    @observable heading: number = 0;

    @action
    static expandToCollection(compactActor: any[], collection: Map<string, Actor>, tiles: Tile[][], parentState: EntityState) {
        let i = 0;
        if (parentState === EntityState.Added) {
            this.expand(compactActor, i, tiles).addTo(collection).set(tiles);
            return;
        }

        const state = compactActor[i++];
        switch (state) {
        case EntityState.Added:
            this.expand(compactActor, i, tiles).addTo(collection).set(tiles);
            break;
        case EntityState.Deleted:
            const id = compactActor[i++].toString();
            const existingActor = collection.get(id);
            if (existingActor == undefined) {
                throw 'Actor ' + id + ' not deleted';
            }

            existingActor.unset(tiles, existingActor.levelX, existingActor.levelY);
            collection.delete(id);
            break;
        case EntityState.Modified:
        {
            const id = compactActor[i++].toString();
            const existingActor = collection.get(id);
            if (existingActor == undefined) {
                throw 'Actor ' + id + ' not found';
            }
            existingActor.update(compactActor, tiles);
            break;
        }
        }
    }

    @action
    static expand(compactActor: any[], i: number, tiles: Tile[][]): Actor {
        const actor = compactActor.length === Actor.actorPropertyCount + i
            ? new Actor()
            : Player.expand(compactActor, Actor.actorPropertyCount + i, tiles);
        actor.id = compactActor[i++];
        actor.baseName = compactActor[i++];
        actor.name = compactActor[i++];
        actor.levelX = compactActor[i++];
        actor.levelY = compactActor[i++];
        actor.heading = compactActor[i++];
        return actor;
    }

    protected updateImplementation(compactActor: any[], tiles: Tile[][]): number {
        let i = 2;

        let unset = false;
        for (; i < compactActor.length;) {
            switch (compactActor[i++]) {
            case 2:
                this.name = compactActor[i++];
                break;
            case 3:
                this.unset(tiles, this.levelX, this.levelY);
                unset = true;
                this.levelX = compactActor[i++];
                break;
            case 4:
                if (!unset) {
                    this.unset(tiles, this.levelX, this.levelY);
                    unset = true;
                }
                this.levelY = compactActor[i++];
                break;
            case 5:
                this.heading = compactActor[i++];
                break;
            default:
                if (unset) {
                    this.set(tiles);
                }
                return i - 1;
            }
        }

        if (unset) {
            this.set(tiles);
        }

        return i;
    }

    @action.bound
    update(compactActor: any[], tiles: Tile[][]): number {
        return this.updateImplementation(compactActor, tiles);
    }

    addTo(map: Map<string, Actor>): Actor {
        map.set(this.id.toString(), this);
        return this;
    }

    @action.bound
    set(tiles: Tile[][]): Actor {
        return tiles[this.levelY][this.levelX].actor = this;
    }

    @action.bound
    unset(tiles: Tile[][], x: number, y: number): Actor {
        const tile = tiles[y][x];
        if (tile.actor === this) {
            tile.actor = null;
        }
        return this;
    }
}

export class Player extends Actor {
    @observable properties: Map<string, Property> = new Map<string, Property>();
    @observable inventory: Map<string, Item> = new Map<string, Item>();
    @observable abilities: Map<string, Ability> = new Map<string, Ability>();
    @observable nextActionTick: number = 0;
    @observable gold: number = 0;
    @observable log: Map<string, LogEntry> = new Map<string, LogEntry>();
    @observable races: Map<string, PlayerRace> = new Map<string, PlayerRace>();

    constructor() {
        super();
        this.baseName = 'player';
    }

    @computed get learningRace(): PlayerRace {
        let learningRace: PlayerRace | null = null;
        this.races.forEach(race => {
            if (learningRace === null || !this.isGreater(race, learningRace)) {
                learningRace = race;
            }
        });

        if (learningRace === null) {
            throw "No race";
        }

        return learningRace;
    }

    @action
    static expand(compactActor: any[], i: number, tiles: Tile[][]): Player {
        const player = new Player();
        compactActor[i++].map((a: any[]) => Property.expandToCollection(a, player.properties, EntityState.Added));
        compactActor[i++].map((a: any[]) => Item.expandToCollection(a, player.inventory, null, EntityState.Added));
        compactActor[i++].map((a: any[]) => Ability.expandToCollection(a, player.abilities, EntityState.Added));
        compactActor[i++].map((a: any[]) => LogEntry.expandToCollection(a, player.log, EntityState.Added));
        compactActor[i++].map((a: any[]) => PlayerRace.expandToCollection(a, player.races, EntityState.Added));
        player.nextActionTick = compactActor[i++];
        player.gold = compactActor[i++];
        return player;
    }

    updateImplementation(compactActor: any[], tiles: Tile[][]): number {
        let i = super.updateImplementation(compactActor, tiles);

        for (; i < compactActor.length;) {
            switch (compactActor[i++]) {
            case 6:
                compactActor[i++].map(
                    (a: any[]) => Property.expandToCollection(a, this.properties, EntityState.Modified));
                break;
            case 7:
                compactActor[i++].map((a: any[]) => Item.expandToCollection(a, this.inventory, null, EntityState.Modified));
                break;
            case 8:
                compactActor[i++].map(
                    (a: any[]) => Ability.expandToCollection(a, this.abilities, EntityState.Modified));
                break;
            case 9:
                compactActor[i++].map((a: any[]) => LogEntry.expandToCollection(a, this.log, EntityState.Modified));
                break;
            case 10:
                compactActor[i++].map((a: any[]) => PlayerRace.expandToCollection(a, this.races, EntityState.Modified));
                break;
            case 11:
                this.nextActionTick = compactActor[i++];
                break;
            case 12:
                this.gold = compactActor[i++];
                break;
            default:
                return i - 1;
            }
        }

        return i;
    }

    private isGreater(race1: PlayerRace, race2: PlayerRace): boolean {
        if (race1.xPLevel > race2.xPLevel) {
            return true;
        }

        if (race1.xPLevel < race2.xPLevel) {
            return false;
        }

        return race1.id > race2.id;
    }
}

export class Property {
    @observable name: string;
    @observable displayName: string;

    @action
    static expandToCollection(compactProperty: any[], collection: Map<string, Property>, parentState: EntityState) {
        let i = 0;
        if (parentState === EntityState.Added) {
            this.expand(compactProperty, i).addTo(collection);
            return;
        }

        const state = compactProperty[i++];
        switch (state) {
        case EntityState.Added:
            this.expand(compactProperty, i).addTo(collection);
            break;
        case EntityState.Deleted:
            const id = compactProperty[i++];
            if (!collection.delete(id)) {
                throw 'Property ' + id + ' not deleted';
            }
            break;
        case EntityState.Modified:
        {
            const id = compactProperty[i++];
            const existingProperty = collection.get(id);
            if (existingProperty == undefined) {
                throw 'Property ' + id + ' not found';
            }
            existingProperty.update(compactProperty);
            break;
        }
        }
    }

    @action
    static expand(compactProperty: any[], i: number): Property {
        const property = new Property();
        property.name = compactProperty[i++];
        property.displayName = compactProperty[i++];

        return property;
    }

    @action.bound
    update(compactProperty: any[]): number {
        let i = 2;

        for (; i < compactProperty.length;) {
            switch (compactProperty[i++]) {
            case 1:
                this.displayName = compactProperty[i++];
                break;
            default:
                return i - 1;
            }
        }

        return i;
    }

    @action.bound
    addTo(map: Map<string, Property>) {
        map.set(this.name, this);
    }
}

export class Item {
    protected static itemPropertyCount: number = 8;
    @observable id: number;
    @observable baseName: string;
    @observable name: string;
    @observable levelX: number;
    @observable levelY: number;
    @observable type: ItemType;
    @observable equippedSlot: string;
    @observable equippableSlots: Map<string, EquipableSlot> = new Map<string, EquipableSlot>();

    @action
    static expandToCollection(compactItem: any[], collection: Map<string, Item>, tiles: Tile[][] | null, parentState: EntityState) {
        let i = 0;
        if (parentState === EntityState.Added) {
            this.expand(compactItem, i).addTo(collection).set(tiles);
            return;
        }

        const state = compactItem[i++];
        switch (state) {
        case EntityState.Added:
            this.expand(compactItem, i).addTo(collection).set(tiles);
            break;
        case EntityState.Deleted:
            const id = compactItem[i++].toString();
            const existingItem = collection.get(id);
            if (existingItem == undefined) {
                throw 'Item ' + id + ' not deleted';
            }

            existingItem.unset(tiles, existingItem.levelX, existingItem.levelY);
            collection.delete(id);
            break;
        case EntityState.Modified:
        {
            const id = compactItem[i++].toString();
            const existingItem = collection.get(id);
            if (existingItem == undefined) {
                throw 'Item ' + id + ' not found';
            }
            existingItem.update(compactItem, tiles);
            break;
        }
        }
    }

    @action
    static expand(compactItem: any[], i: number): Item {
        const item = new Item();

        item.id = compactItem[i++];
        item.baseName = compactItem[i++];
        const slots = compactItem[i++];
        if (slots != null) {
            slots.map((s: any[]) => EquipableSlot.expandToCollection(s, item.equippableSlots, EntityState.Added));
        }
        item.name = compactItem[i++];
        item.levelX = compactItem[i++];
        item.levelY = compactItem[i++];
        item.type = compactItem[i++];
        item.equippedSlot = compactItem[i++];
        return item;
    }

    updateImplementation(compactItem: any[], tiles: Tile[][] | null): number {
        let i = 2;

        let unset = false;
        for (; i < compactItem.length;) {
            switch (compactItem[i++]) {
            case 2:
                this.equippableSlots.clear();
                compactItem[i++].map(
                    (s: any[]) => EquipableSlot.expandToCollection(s, this.equippableSlots, EntityState.Modified));
                break;
            case 3:
                this.name = compactItem[i++];
                break;
            case 4:
                this.unset(tiles, this.levelX, this.levelY);
                unset = true;
                this.levelX = compactItem[i++];
                break;
            case 5:
                if (!unset) {
                    this.unset(tiles, this.levelX, this.levelY);
                    unset = true;
                }
                this.levelY = compactItem[i++];
                break;
            case 6:
                this.equippedSlot = compactItem[i++];
                break;
            default:
                if (unset) {
                    this.set(tiles);
                }

                return i - 1;
            }
        }

        if (unset) {
            this.set(tiles);
        }

        return i;
    }

    @action.bound
    update(compactItem: any[], tiles: Tile[][] | null): number {
        return this.updateImplementation(compactItem, tiles);
    }

    addTo(map: Map<string, Item>): Item {
        map.set(this.id.toString(), this);
        return this;
    }

    @action.bound
    set(tiles: Tile[][] | null): Item {
        if (tiles == null) {
            return this;
        }
        return tiles[this.levelY][this.levelX].item = this;
    }

    @action.bound
    unset(tiles: Tile[][] | null, x: number, y: number): Item {
        if (tiles == null) {
            return this;
        }
        const tile = tiles[y][x];
        if (tile.item === this) {
            tile.item = null;
        }
        return this;
    }
}

export class EquipableSlot {
    @observable id: number;
    @observable name: string;

    @action
    static expandToCollection(compactSlot: any[], collection: Map<string, EquipableSlot>, parentState: EntityState) {
        this.expand(compactSlot, 0).addTo(collection);
    }

    @action
    static expand(compactSlot: any[], i: number): EquipableSlot {
        const slot = new EquipableSlot();
        slot.id = compactSlot[i++];
        slot.name = compactSlot[i++];

        return slot;
    }

    @action.bound
    addTo(map: Map<string, EquipableSlot>) {
        map.set(this.id.toString(), this);
    }
}

export class Ability {
    @observable id: number;
    @observable name: string;
    @observable isDefault: boolean | undefined;

    @action
    static expandToCollection(compactAbility: any[], collection: Map<string, Ability>, parentState: EntityState) {
        let i = 0;
        if (parentState === EntityState.Added) {
            this.expand(compactAbility, i).addTo(collection);
            return;
        }

        const state = compactAbility[i++];
        switch (state) {
        case EntityState.Added:
            this.expand(compactAbility, i).addTo(collection);
            break;
        case EntityState.Deleted:
            const id = compactAbility[i++].toString();
            if (!collection.delete(id)) {
                throw 'Ability ' + id + ' not deleted';
            }
            break;
        case EntityState.Modified:
        {
            const id = compactAbility[i++].toString();
            const existingAbility = collection.get(id);
            if (existingAbility == undefined) {
                throw 'Ability ' + id + ' not found';
            }
            existingAbility.update(compactAbility);
            break;
        }
        }
    }

    @action
    static expand(compactAbility: any[], i: number): Ability {
        const ability = new Ability();
        ability.id = compactAbility[i++];
        ability.name = compactAbility[i++];
        if (compactAbility.length > i) {
            ability.isDefault = compactAbility[i++];
        }

        return ability;
    }

    @action.bound
    update(compactAbility: any[]): number {
        let i = 2;

        for (; i < compactAbility.length;) {
            switch (compactAbility[i++]) {
            case 1:
                this.name = compactAbility[i++];
                break;
            case 2:
                this.isDefault = compactAbility[i++];
                break;
            }
        }

        return i;
    }

    @action.bound
    addTo(map: Map<string, Ability>) {
        map.set(this.id.toString(), this);
    }
}

export class LogEntry {
    @observable id: number;
    @observable message: string;

    @action
    static expandToCollection(compactLogEntry: any[], collection: Map<string, LogEntry>, parentState: EntityState) {
        let i = 0;
        if (parentState === EntityState.Added) {
            this.expand(compactLogEntry, i).addTo(collection);
            return;
        }

        const state = compactLogEntry[i++];
        switch (state) {
        case EntityState.Added:
            this.expand(compactLogEntry, i).addTo(collection);
            break;
        case EntityState.Deleted:
            const id = compactLogEntry[i++].toString();
            if (!collection.delete(id)) {
                throw 'LogEntry ' + id + ' not deleted';
            }
            break;
        case EntityState.Modified:
        {
            const id = compactLogEntry[i++].toString();
            const existingLogEntry = collection.get(id);
            if (existingLogEntry == undefined) {
                throw 'LogEntry ' + id + ' not found';
            }
            existingLogEntry.update(compactLogEntry);
            break;
        }
        }
    }

    @action
    static expand(compactLogEntry: any[], i: number): LogEntry {
        const logEntry = new LogEntry();
        logEntry.id = compactLogEntry[i++];
        logEntry.message = compactLogEntry[i++];

        return logEntry;
    }

    @action.bound
    update(compactLogEntry: any[]): number {
        let i = 2;

        for (; i < compactLogEntry.length;) {
            switch (compactLogEntry[i++]) {
            case 1:
                this.message = compactLogEntry[i++];
                break;
            default:
                return i - 1;
            }
        }

        return i;
    }

    @action.bound
    addTo(map: Map<string, LogEntry>) {
        map.set(this.id.toString(), this);
    }
}

export class PlayerRace {
    @observable id: number = 0;
    @observable name: string;
    @observable xPLevel: number = 0;
    @observable xP: number = 0;
    @observable nextLevelXP: number = 0;

    @action
    static expandToCollection(compactPlayerRace: any[], collection: Map<string, PlayerRace>, parentState: EntityState) {
        let i = 0;
        if (parentState === EntityState.Added) {
            this.expand(compactPlayerRace, i).addTo(collection);
            return;
        }

        const state = compactPlayerRace[i++];
        switch (state) {
        case EntityState.Added:
            this.expand(compactPlayerRace, i).addTo(collection);
            break;
        case EntityState.Deleted:
            const id = compactPlayerRace[i++].toString();
            if (!collection.delete(id)) {
                throw 'PlayerRace ' + id + ' not deleted';
            }
            break;
        case EntityState.Modified:
        {
            const id = compactPlayerRace[i++].toString();
            const existingPlayerRace = collection.get(id);
            if (existingPlayerRace == undefined) {
                throw 'PlayerRace ' + id + ' not found';
            }
            existingPlayerRace.update(compactPlayerRace);
            break;
        }
        }
    }

    @action
    static expand(compactPlayerRace: any[], i: number): PlayerRace {
        const playerRace = new PlayerRace();
        playerRace.id = compactPlayerRace[i++];
        playerRace.name = compactPlayerRace[i++];
        playerRace.xPLevel = compactPlayerRace[i++];
        playerRace.xP = compactPlayerRace[i++];
        playerRace.nextLevelXP = compactPlayerRace[i++];

        return playerRace;
    }

    @action.bound
    update(compactPlayerRace: any[]): number {
        let i = 2;

        for (; i < compactPlayerRace.length;) {
            switch (compactPlayerRace[i++]) {
            case 1:
                this.name = compactPlayerRace[i++];
                break;
            case 2:
                this.xPLevel = compactPlayerRace[i++];
                break;
            case 3:
                this.xP = compactPlayerRace[i++];
                break;
            case 4:
                this.nextLevelXP = compactPlayerRace[i++];
                break;
            default:
                return i - 1;
            }
        }

        return i;
    }

    @action.bound
    addTo(map: Map<string, PlayerRace>) {
        map.set(this.id.toString(), this);
    }
}

export class Connection {
    @observable id: number;
    @observable levelX: number;
    @observable levelY: number;
    @observable isDown: boolean;

    @action
    static expandToCollection(compactConnection: any[], collection: Map<string, Connection>, tiles: Tile[][], parentState: EntityState) {
        let i = 0;
        if (parentState === EntityState.Added) {
            this.expand(compactConnection, i).addTo(collection).set(tiles);
            return;
        }

        const state = compactConnection[i++];
        switch (state) {
        case EntityState.Added:
            this.expand(compactConnection, i).addTo(collection).set(tiles);
            break;
        case EntityState.Deleted:
            const id = compactConnection[i++].toString();
            const existingConnection = collection.get(id);
            if (existingConnection == undefined) {
                throw 'Connection ' + id + ' not deleted';
            }
            collection.delete(id);
            existingConnection.unset(tiles, existingConnection.levelX, existingConnection.levelY);
            break;
        case EntityState.Modified:
        {
            const id = compactConnection[i++].toString();
            const existingConnection = collection.get(id);
            if (existingConnection == undefined) {
                throw 'Connection ' + id + ' not found';
            }
            existingConnection.update(compactConnection, tiles);
            break;
        }
        }
    }

    @action
    static expand(compactConnection: any[], i: number): Connection {
        const connection = new Connection();
        connection.id = compactConnection[i++];
        connection.levelX = compactConnection[i++];
        connection.levelY = compactConnection[i++];
        connection.isDown = compactConnection[i++];

        return connection;
    }

    @action.bound
    update(compactConnection: any[], tiles: Tile[][]): number {
        let i = 2;

        let unset = false;
        for (; i < compactConnection.length;) {
            switch (compactConnection[i++]) {
            case 1:
                this.unset(tiles, this.levelX, this.levelY);
                unset = true;
                this.levelX = compactConnection[i++];
                break;
            case 2:
                if (!unset) {
                    this.unset(tiles, this.levelX, this.levelY);
                    unset = true;
                }
                this.levelY = compactConnection[i++];
                break;
            case 3:
                this.isDown = compactConnection[i++];
                break;
            default:
                if (unset) {
                    this.set(tiles);
                }

                return i - 1;
            }
        }

        if (unset) {
            this.set(tiles);
        }

        return i;
    }

    addTo(map: Map<string, Connection>): Connection {
        map.set(this.id.toString(), this);
        return this;
    }

    @action.bound
    set(tiles: Tile[][]): Connection {
        return tiles[this.levelY][this.levelX].connection = this;
    }

    @action.bound
    unset(tiles: Tile[][], x: number, y: number): Connection {
        const tile = tiles[y][x];
        if (tile.connection === this) {
            tile.connection = null;
        }
        return this;
    }
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
    WeaponProjectile = 1 << 13,
    Shield = 1 << 14,
    ArmorBody = 1 << 15,
    ArmorHead = 1 << 16,
    ArmorFeet = 1 << 17,
    ArmorHands = 1 << 18,
    ArmorBack = 1 << 19,
    Accessory = 1 << 20,
    Container = 1 << 21,
    Potion = 1 << 22,
    Wand = 1 << 23,
    Figurine = 1 << 24,
    Trinket = 1 << 25,
    SkillBook = 1 << 26,

    WeaponRanged = WeaponRangedThrown | WeaponRangedSlingshot | WeaponRangedBow | WeaponRangedCrossbow
        | WeaponMagicStaff,

    WeaponMelee = WeaponMeleeFist | WeaponMeleeShort | WeaponMeleeMedium | WeaponMeleeLong
        | WeaponMagicFocus,

    Weapon = WeaponMelee | WeaponRanged
}

export const enum ItemComplexity {
    Normal = 0,
    Intricate,
    Exotic
}

export const enum EntityState {
    Detached = 0,
    Unchanged,
    Deleted,
    Modified,
    Added,
}