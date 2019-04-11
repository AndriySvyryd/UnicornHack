import { observable, computed, action } from "mobx";
import { ActivationType } from "./ActivationType";
import { Direction } from "./Direction";
import { EntityState } from "./EntityState";
import { ItemType } from "./ItemType";
import { MapFeature } from "./MapFeature";
import { DirectionFlags } from "./DirectionFlags";

export class Player {
    id: number = 0;
    name: string = '';
    @observable currentTick: number = -1;
    @observable nextActionTick: number = 0;
    @observable hp: number = 0;
    @observable maxHp: number = 0;
    @observable ep: number = 0;
    @observable maxEp: number = 0;
    @observable reservedEp: number = 0;
    @observable level: Level = new Level();
    @observable xP: number = 0;
    @observable nextLevelXP: number = 0;
    @observable inventory: Map<string, Item> = new Map<string, Item>();
    @observable abilities: Map<string, Ability> = new Map<string, Ability>();
    @observable log: Map<string, LogEntry> = new Map<string, LogEntry>();
    @observable races: Map<string, PlayerRace> = new Map<string, PlayerRace>();

    @computed get learningRace(): PlayerRace {
        let learningRace: PlayerRace | null = null;
        this.races.forEach(race => {
            if (learningRace === null || !Player.isGreater(race, learningRace)) {
                learningRace = race;
            }
        });

        if (learningRace === null) {
            throw "No race";
        }

        return learningRace;
    }

    private static isGreater(race1: PlayerRace, race2: PlayerRace): boolean {
        if (race1.xpLevel > race2.xpLevel) {
            return true;
        }

        if (race1.xpLevel < race2.xpLevel) {
            return false;
        }

        return race1.id > race2.id;
    }

    @action
    static expand(compactPlayer: any[], currentPlayer: Player): (Player | null) {
        let i = 0;

        const state = compactPlayer[i++];
        if (state === EntityState.Added) {
            return Player.expandImplementation(compactPlayer, currentPlayer, i);
        }

        const previousTick = compactPlayer[i++];
        const currentTick = compactPlayer[i++];
        if (currentPlayer.currentTick < previousTick) {
            return null;
        } else if (currentPlayer.currentTick > currentTick) {
            return currentPlayer;
        }

        currentPlayer.currentTick = currentTick;
        currentPlayer.update(compactPlayer, i);
        return currentPlayer;
    }

    @action
    private static expandImplementation(compactPlayer: any[], currentPlayer: Player, i: number): Player {
        currentPlayer.id = compactPlayer[i++];
        currentPlayer.name = compactPlayer[i++];
        currentPlayer.currentTick = compactPlayer[i++];
        currentPlayer.level = Level.expand(compactPlayer[i++], currentPlayer.level, EntityState.Added);
        currentPlayer.races.clear();
        compactPlayer[i++].map((a: any[]) => PlayerRace.expandToCollection(a, currentPlayer.races, EntityState.Added));
        currentPlayer.inventory.clear();
        compactPlayer[i++].map((a: any[]) => Item.expandToCollection(a, currentPlayer.inventory, EntityState.Added));
        currentPlayer.abilities.clear();
        compactPlayer[i++].map((a: any[]) => Ability.expandToCollection(a, currentPlayer.abilities, EntityState.Added));
        currentPlayer.log.clear();
        compactPlayer[i++].map((a: any[]) => LogEntry.expandToCollection(a, currentPlayer.log, EntityState.Added));
        currentPlayer.nextActionTick = compactPlayer[i++];
        currentPlayer.nextLevelXP = compactPlayer[i++];
        currentPlayer.xP = compactPlayer[i++];
        currentPlayer.hp = compactPlayer[i++];
        currentPlayer.maxHp = compactPlayer[i++];
        currentPlayer.ep = compactPlayer[i++];
        currentPlayer.maxEp = compactPlayer[i++];
        currentPlayer.reservedEp = compactPlayer[i++];
        return currentPlayer;
    }

    @action.bound
    update(compactPlayer: any[], i: number): number {
        for (; i < compactPlayer.length;) {
            switch (compactPlayer[i++]) {
                case 2:
                    this.level = Level.expand(compactPlayer[i++], this.level, EntityState.Modified);
                    break;
                case 3:
                    compactPlayer[i++].map((a: any[]) => PlayerRace.expandToCollection(a, this.races, EntityState.Modified));
                    break;
                case 4:
                    compactPlayer[i++].map((a: any[]) => Item.expandToCollection(a, this.inventory, EntityState.Modified));
                    break;
                case 5:
                    compactPlayer[i++].map(
                        (a: any[]) => Ability.expandToCollection(a, this.abilities, EntityState.Modified));
                    break;
                case 6:
                    compactPlayer[i++].map((a: any[]) => LogEntry.expandToCollection(a, this.log, EntityState.Modified));
                    break;
                case 7:
                    this.nextActionTick = compactPlayer[i++];
                    break;
                case 8:
                    this.nextLevelXP = compactPlayer[i++];
                    break;
                case 9:
                    this.xP = compactPlayer[i++];
                    break;
                case 10:
                    this.hp = compactPlayer[i++];
                    break;
                case 11:
                    this.maxHp = compactPlayer[i++];
                    break;
                case 12:
                    this.ep = compactPlayer[i++];
                    break;
                case 13:
                    this.maxEp = compactPlayer[i++];
                    break;
                case 14:
                    this.reservedEp = compactPlayer[i++];
                    break;
                default:
                    return i - 1;
            }
        }

        return i;
    }
}

export class Level {
    @observable branchName: string = '';
    @observable depth: number = -1;
    @observable height: number = 1;
    @observable width: number = 1;
    @observable tiles: Array<Array<Tile>> = [[new Tile()]];

    actors: Map<string, MapActor> = new Map<string, MapActor>();
    items: Map<string, MapItem> = new Map<string, MapItem>();
    connections: Map<string, Connection> = new Map<string, Connection>();
    indexToPoint: number[][] = [[0]];

    @action
    static expand(compactLevel: any[], currentLevel: Level, parentState: EntityState): Level {
        let i = 0;

        if (parentState === EntityState.Added) {
            return Level.expandImplementation(compactLevel, currentLevel, i);
        }

        const state = compactLevel[i++] as EntityState;
        if (state === EntityState.Added) {
            return Level.expandImplementation(compactLevel, currentLevel, i);
        }

        currentLevel.update(compactLevel, i);
        return currentLevel;
    }

    @action
    private static expandImplementation(compactLevel: any[], currentLevel: Level, i: number): Level {
        const actors = compactLevel[i++];
        const items = compactLevel[i++];
        const connections = compactLevel[i++];
        const terrain = compactLevel[i++];
        const wallNeighbours = compactLevel[i++];
        const visibleTerrain = compactLevel[i++];
        currentLevel.branchName = compactLevel[i++];
        currentLevel.depth = compactLevel[i++];
        currentLevel.width = compactLevel[i++];
        currentLevel.height = compactLevel[i++];

        let dimensionChanged = false;
        if (currentLevel.tiles.length != currentLevel.height
            || currentLevel.tiles[0].length != currentLevel.width) {
            currentLevel.tiles = new Array<Array<Tile>>(currentLevel.height);
            currentLevel.indexToPoint = new Array<Array<number>>(currentLevel.height * currentLevel.width);
            dimensionChanged = true;
        }

        let terrainIndex = -1;
        let terrainChangeIndex = 0;
        if (terrainChangeIndex < terrain.length) {
            terrainIndex = terrain[terrainChangeIndex++];
        }

        let wallNeighboursIndex = -1;
        let wallNeighboursChangeIndex = 0;
        if (wallNeighboursChangeIndex < wallNeighbours.length) {
            wallNeighboursIndex = wallNeighbours[wallNeighboursChangeIndex++];
        }

        let visibleIndex = -1;
        let visibleChangeIndex = 0;
        if (visibleChangeIndex < visibleTerrain.length) {
            visibleIndex = visibleTerrain[visibleChangeIndex++];
        }

        i = 0;
        for (let y = 0; y < currentLevel.height; y++) {
            if (dimensionChanged) {
                currentLevel.tiles[y] = new Array<Tile>(currentLevel.width);
            }
            const row = currentLevel.tiles[y];
            for (let x = 0; x < currentLevel.width; x++) {
                if (dimensionChanged) {
                    row[x] = new Tile();
                }
                const tile = row[x];

                if (i == terrainIndex) {
                    tile.feature = terrain[terrainChangeIndex++];
                    if (terrainChangeIndex < terrain.length) {
                        terrainIndex = terrain[terrainChangeIndex++];
                    }
                } else {
                    tile.feature = MapFeature.Unexplored;
                }

                if (i == wallNeighboursIndex) {
                    tile.wallNeighbours = wallNeighbours[wallNeighboursChangeIndex++];
                    if (wallNeighboursChangeIndex < wallNeighbours.length) {
                        wallNeighboursIndex = wallNeighbours[wallNeighboursChangeIndex++];
                    }
                } else {
                    tile.wallNeighbours = DirectionFlags.None;
                }

                if (i == visibleIndex) {
                    tile.visibility = visibleTerrain[visibleChangeIndex++];
                    if (visibleChangeIndex < visibleTerrain.length) {
                        visibleIndex = visibleTerrain[visibleChangeIndex++];
                    }
                } else {
                    tile.visibility = 0;
                }

                tile.actor = null;
                tile.connection = null;
                tile.item = null;
                currentLevel.indexToPoint[i] = [x, y];
                i++;
            }
        }

        currentLevel.actors.clear();
        actors.map((a: any[]) => MapActor.expandToCollection(a, currentLevel.actors, currentLevel.tiles, EntityState.Added));
        currentLevel.items.clear();
        items.map((a: any[]) => MapItem.expandToCollection(a, currentLevel.items, currentLevel.tiles, EntityState.Added));
        currentLevel.connections.clear();
        connections.map((a: any[]) => Connection.expandToCollection(a, currentLevel.connections, currentLevel.tiles, EntityState.Added));

        return currentLevel;
    }

    @action.bound
    update(compactLevel: any[], i: number): number {
        for (; i < compactLevel.length;) {
            switch (compactLevel[i++]) {
                case 2:
                    compactLevel[i++].map(
                        (a: any[]) => MapActor.expandToCollection(a, this.actors, this.tiles, EntityState.Modified));
                    break;
                case 3:
                    compactLevel[i++].map(
                        (a: any[]) => MapItem.expandToCollection(a, this.items, this.tiles, EntityState.Modified));
                    break;
                case 4:
                    compactLevel[i++].map(
                        (a: any[]) => Connection.expandToCollection(a, this.connections, this.tiles, EntityState.Modified));
                    break;
                case 5:
                    const terrainChanges = compactLevel[i++];
                    for (let i = 0; i < terrainChanges.length;) {
                        const changedPoint = this.indexToPoint[terrainChanges[i++]];
                        this.tiles[changedPoint[1]][changedPoint[0]].feature = terrainChanges[i++];
                    }
                    break;
                case 6:
                    const wallNeighboursChanges = compactLevel[i++];
                    for (let i = 0; i < wallNeighboursChanges.length;) {
                        const changedPoint = this.indexToPoint[wallNeighboursChanges[i++]];
                        this.tiles[changedPoint[1]][changedPoint[0]].wallNeighbours =
                            wallNeighboursChanges[i++];
                    }
                    break;
                case 7:
                    const visibleTerrainChanges = compactLevel[i++];
                    for (let i = 0; i < visibleTerrainChanges.length;) {
                        const changedPoint = this.indexToPoint[visibleTerrainChanges[i++]];
                        this.tiles[changedPoint[1]][changedPoint[0]].visibility = visibleTerrainChanges[i++];
                    }
                    break;
            }
        }

        return i;
    }

    static pack(x: number, y: number): number {
        return x << 8 | y;
    }
}

export class Tile {
    @observable actor: MapActor | null = null;
    @observable item: MapItem | null = null;
    @observable connection: Connection | null = null;
    @observable visibility: number = 0;
    @observable wallNeighbours: number = 0;
    @observable feature: number = 0;
}

export class MapActor {
    protected static actorPropertyCount: number = 6;
    id: number = 0;
    @observable baseName: string = '';
    @observable name: string = '';
    @observable levelX: number = 0;
    @observable levelY: number = 0;
    @observable heading: Direction = 0;

    @action
    static expandToCollection(compactActor: any[], collection: Map<string, MapActor>, tiles: Tile[][], parentState: EntityState) {
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
    static expand(compactActor: any[], i: number, tiles: Tile[][]): MapActor {
        const actor = new MapActor();
        actor.id = compactActor[i++];
        let baseName = compactActor[i++];
        actor.baseName = baseName === null ? '' : baseName;
        let name = compactActor[i++];
        actor.name = name === null ? '' : name;
        actor.levelX = compactActor[i++];
        actor.levelY = compactActor[i++];
        actor.heading = compactActor[i++];
        return actor;
    }

    @action.bound
    update(compactActor: any[], tiles: Tile[][]): number {
        let i = 2;

        let unset = false;
        for (; i < compactActor.length;) {
            switch (compactActor[i++]) {
                case 1:
                    let baseName = compactActor[i++];
                    this.baseName = baseName === null ? '' : baseName;
                    break;
                case 2:
                    let name = compactActor[i++];
                    this.name = name === null ? '' : name;
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

    addTo(map: Map<string, MapActor>): MapActor {
        map.set(this.id.toString(), this);
        return this;
    }

    @action.bound
    set(tiles: Tile[][]): MapActor {
        return tiles[this.levelY][this.levelX].actor = this;
    }

    @action.bound
    unset(tiles: Tile[][], x: number, y: number): MapActor {
        const tile = tiles[y][x];
        if (tile.actor === this) {
            tile.actor = null;
        }
        return this;
    }
}

export class MapItem {
    id: number = -1;
    @observable type: ItemType = ItemType.None;
    @observable baseName: string = '';
    @observable name: string = '';
    @observable levelX: number = -1;
    @observable levelY: number = -1;

    @action
    static expandToCollection(compactItem: any[], collection: Map<string, MapItem>, tiles: Tile[][] | null, parentState: EntityState) {
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
    static expand(compactItem: any[], i: number): MapItem {
        const item = new MapItem();

        item.id = compactItem[i++];
        item.type = compactItem[i++];
        item.baseName = compactItem[i++];
        item.name = compactItem[i++];
        item.levelX = compactItem[i++];
        item.levelY = compactItem[i++];
        return item;
    }

    @action.bound
    update(compactItem: any[], tiles: Tile[][] | null): number {
        let i = 2;

        let unset = false;
        for (; i < compactItem.length;) {
            switch (compactItem[i++]) {
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

    addTo(map: Map<string, MapItem>): MapItem {
        map.set(this.id.toString(), this);
        return this;
    }

    @action.bound
    set(tiles: Tile[][] | null): MapItem {
        if (tiles == null) {
            return this;
        }
        return tiles[this.levelY][this.levelX].item = this;
    }

    @action.bound
    unset(tiles: Tile[][] | null, x: number, y: number): MapItem {
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

export class Item {
    id: number = -1;
    @observable type: ItemType = ItemType.None;
    @observable baseName: string = '';
    @observable name: string = '';
    @observable equippableSlots: Map<string, EquipableSlot> = new Map<string, EquipableSlot>();
    @observable equippedSlot: EquipableSlot | null = null;

    @action
    static expandToCollection(compactItem: any[], collection: Map<string, Item>, parentState: EntityState) {
        let i = 0;
        if (parentState === EntityState.Added) {
            this.expand(compactItem, i).addTo(collection);
            return;
        }

        const state = compactItem[i++];
        switch (state) {
            case EntityState.Added: {
                this.expand(compactItem, i).addTo(collection);
                break;
            }
            case EntityState.Deleted: {
                const id = compactItem[i++].toString();
                const existingItem = collection.get(id);
                if (existingItem == undefined) {
                    throw 'Item ' + id + ' not deleted';
                }

                collection.delete(id);
                break;
            }
            case EntityState.Modified: {
                const id = compactItem[i++].toString();
                const existingItem = collection.get(id);
                if (existingItem == undefined) {
                    throw 'Item ' + id + ' not found';
                }
                existingItem.update(compactItem);
                break;
            }
        }
    }

    @action
    static expand(compactItem: any[], i: number): Item {
        const item = new Item();

        item.id = compactItem[i++];
        item.type = compactItem[i++];
        item.baseName = compactItem[i++];
        item.name = compactItem[i++];

        const equippableSlots = compactItem[i++];
        if (equippableSlots != null) {
            equippableSlots.map((s: any[]) => EquipableSlot.expandToCollection(s, item.equippableSlots, EntityState.Added));
        }

        const equippedSlot = compactItem[i++];
        if (equippedSlot != null) {
            item.equippedSlot = EquipableSlot.expand(equippedSlot, 0);
        }
        return item;
    }

    updateImplementation(compactItem: any[]): number {
        let i = 2;

        for (; i < compactItem.length;) {
            switch (compactItem[i++]) {
                case 3:
                    this.name = compactItem[i++];
                    break;
                case 4:
                    this.equippableSlots.clear();
                    compactItem[i++].map(
                        (s: any[]) => EquipableSlot.expandToCollection(s, this.equippableSlots, EntityState.Modified));
                    break;
                case 5:
                    const equippedSlot = compactItem[i++];
                    this.equippedSlot = equippedSlot == null
                        ? null
                        : EquipableSlot.expand(equippedSlot, 0);
                    break;
                default:
                    return i - 1;
            }
        }

        return i;
    }

    @action.bound
    update(compactItem: any[]): number {
        return this.updateImplementation(compactItem);
    }

    addTo(map: Map<string, Item>): Item {
        map.set(this.id.toString(), this);
        return this;
    }
}

export class EquipableSlot {
    id: number = -1;
    @observable shortName: string = '';
    @observable name: string = '';

    @action
    static expandToCollection(compactSlot: any[], collection: Map<string, EquipableSlot>, parentState: EntityState) {
        this.expand(compactSlot, 0).addTo(collection);
    }

    @action
    static expand(compactSlot: any[], i: number): EquipableSlot {
        const slot = new EquipableSlot();
        slot.id = compactSlot[i++];
        slot.shortName = compactSlot[i++];
        slot.name = compactSlot[i++];

        return slot;
    }

    @action.bound
    addTo(map: Map<string, EquipableSlot>) {
        map.set(this.id.toString(), this);
    }
}

export class Ability {
    id: number = -1;
    @observable name: string = '';
    @observable activation: ActivationType = ActivationType.Default;
    @observable slot: number | null = null;
    @observable cooldownTick: number | null = null;
    @observable cooldownXpLeft: number | null = null;

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
        ability.activation = compactAbility[i++];
        ability.slot = compactAbility[i++];
        ability.cooldownTick = compactAbility[i++];
        ability.cooldownXpLeft = compactAbility[i++];

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
                    this.activation = compactAbility[i++];
                    break;
                case 3:
                    this.slot = compactAbility[i++];
                    break;
                case 4:
                    this.cooldownTick = compactAbility[i++];
                    break;
                case 5:
                    this.cooldownXpLeft = compactAbility[i++];
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
    id: number = -1;
    @observable message: string = '';

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
    id: number = 0;
    @observable name: string = '';
    @observable shortName: string = '';
    @observable xpLevel: number = 0;

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
        playerRace.shortName = compactPlayerRace[i++];
        playerRace.xpLevel = compactPlayerRace[i++];

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
                    this.shortName = compactPlayerRace[i++];
                    break;
                case 3:
                    this.xpLevel = compactPlayerRace[i++];
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
    id: number = -1;
    @observable levelX: number = -1;
    @observable levelY: number = -1;
    @observable isDown: boolean = true;

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