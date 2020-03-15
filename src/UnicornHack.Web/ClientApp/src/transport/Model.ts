import { observable, computed, action } from "mobx";
import { ActivationType } from "./ActivationType";
import { Direction } from "./Direction";
import { EntityState } from "./EntityState";
import { ItemType } from "./ItemType";
import { MapFeature } from "./MapFeature";
import { DirectionFlags } from "./DirectionFlags";
import { TargetingShape } from "./TargetingShape";
import { TargetingType } from "./TargetingType";
import { ActorAction } from "./ActorAction";

export class Player {
    readonly name: string = '';
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
    @observable abilities: Map<number, Ability> = new Map<number, Ability>();
    @observable log: Map<number, LogEntry> = new Map<number, LogEntry>();
    @observable races: Map<number, PlayerRace> = new Map<number, PlayerRace>();

    constructor(name: string) {
        this.name = name;
    }

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
    static expand(compactPlayer: readonly any[], currentPlayer: Player): (Player | null) {
        let i = 0;

        const state = compactPlayer[i++];
        if (state === EntityState.Added) {
            return Player.expandImplementation(compactPlayer, currentPlayer, i);
        }

        const previousTick = compactPlayer[i++];
        const currentTick = compactPlayer[i++];
        if (currentPlayer.currentTick < previousTick) {
            return null;
        }

        if (currentPlayer.currentTick > currentTick) {
            return currentPlayer;
        }

        currentPlayer.currentTick = currentTick;
        currentPlayer.update(compactPlayer, i);
        return currentPlayer;
    }

    @action
    private static expandImplementation(compactPlayer: readonly any[], currentPlayer: Player, i: number): Player {
        const name = compactPlayer[i++];
        if (currentPlayer.name.localeCompare(name, undefined, { sensitivity: 'base' })) {
            throw `Expected ${currentPlayer.name}, but got ${name}`;
        }

        if (currentPlayer.name != name) {
            currentPlayer = new Player(name);
        }
        currentPlayer.currentTick = compactPlayer[i++];
        currentPlayer.level = Level.expand(compactPlayer[i++], currentPlayer.level, EntityState.Added);
        currentPlayer.races.clear();
        compactPlayer[i++].forEach((a: any[]) => PlayerRace.expandToCollection(a, currentPlayer.races, EntityState.Added));
        currentPlayer.abilities.clear();
        compactPlayer[i++].forEach((a: any[]) => Ability.expandToCollection(a, currentPlayer.abilities, EntityState.Added));
        currentPlayer.log.clear();
        compactPlayer[i++].forEach((a: any[]) => LogEntry.expandToCollection(a, currentPlayer.log, EntityState.Added));
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
    update(compactPlayer: readonly any[], i: number): number {
        for (; i < compactPlayer.length;) {
            switch (compactPlayer[i++]) {
                case 1:
                    this.level = Level.expand(compactPlayer[i++], this.level, EntityState.Modified);
                    break;
                case 2:
                    compactPlayer[i++].forEach((a: any[]) => PlayerRace.expandToCollection(a, this.races, EntityState.Modified));
                    break;
                case 3:
                    compactPlayer[i++].forEach(
                        (a: any[]) => Ability.expandToCollection(a, this.abilities, EntityState.Modified));
                    break;
                case 4:
                    compactPlayer[i++].forEach((a: any[]) => LogEntry.expandToCollection(a, this.log, EntityState.Modified));
                    break;
                case 5:
                    this.nextActionTick = compactPlayer[i++];
                    break;
                case 6:
                    this.nextLevelXP = compactPlayer[i++];
                    break;
                case 7:
                    this.xP = compactPlayer[i++];
                    break;
                case 8:
                    this.hp = compactPlayer[i++];
                    break;
                case 9:
                    this.maxHp = compactPlayer[i++];
                    break;
                case 10:
                    this.ep = compactPlayer[i++];
                    break;
                case 11:
                    this.maxEp = compactPlayer[i++];
                    break;
                case 12:
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
    @observable actors: Map<number, MapActor> = new Map<number, MapActor>();
    @observable items: Map<number, MapItem> = new Map<number, MapItem>();
    @observable connections: Map<number, Connection> = new Map<number, Connection>();

    indexToPoint: number[][] = [[0]];

    @action
    static expand(compactLevel: readonly any[], currentLevel: Level, parentState: EntityState): Level {
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
    private static expandImplementation(compactLevel: readonly any[], currentLevel: Level, i: number): Level {
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
        actors.forEach((a: any[]) => MapActor.expandToCollection(a, currentLevel.actors, currentLevel.tiles, EntityState.Added));
        currentLevel.items.clear();
        items.forEach((a: any[]) => MapItem.expandToCollection(a, currentLevel.items, currentLevel.tiles, EntityState.Added));
        currentLevel.connections.clear();
        connections.forEach((a: any[]) => Connection.expandToCollection(a, currentLevel.connections, currentLevel.tiles, EntityState.Added));

        return currentLevel;
    }

    @action.bound
    update(compactLevel: readonly any[], i: number): number {
        for (; i < compactLevel.length;) {
            switch (compactLevel[i++]) {
                case 2:
                    compactLevel[i++].forEach(
                        (a: any[]) => MapActor.expandToCollection(a, this.actors, this.tiles, EntityState.Modified));
                    break;
                case 3:
                    compactLevel[i++].forEach(
                        (a: any[]) => MapItem.expandToCollection(a, this.items, this.tiles, EntityState.Modified));
                    break;
                case 4:
                    compactLevel[i++].forEach(
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
    readonly id: number = 0;
    @observable baseName: string = '';
    @observable name: string = '';
    @observable levelX: number = 0;
    @observable levelY: number = 0;
    @observable heading: Direction = 0;
    @observable isCurrentlyPerceived: boolean = false;
    @observable hp: number = 0;
    @observable maxHp: number = 0;
    @observable ep: number = 0;
    @observable maxEp: number = 0;
    @observable nextActionTick: number = 0;
    @observable nextAction: ActorAction | null = null;
    @observable nextActionName: string | null = null;
    @observable nextActionTarget: number = 0;
    @observable meleeAttack: AttackSummary | null = null;
    @observable rangeAttack: AttackSummary | null = null;
    @observable meleeDefense: AttackSummary | null = null;
    @observable rangeDefense: AttackSummary | null = null;

    constructor(id: number) {
        this.id = id;
    }

    @action
    static expandToCollection(compactActor: readonly any[], collection: Map<number, MapActor>, tiles: Tile[][], parentState: EntityState) {
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
                const id = compactActor[i++];
                const existingActor = collection.get(id);
                if (existingActor == undefined) {
                    throw 'Actor ' + id + ' not deleted';
                }

                existingActor.unset(tiles, existingActor.levelX, existingActor.levelY);
                collection.delete(id);
                break;
            case EntityState.Modified:
                {
                    const id = compactActor[i++];
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
    static expand(compactActor: readonly any[], i: number, tiles: Tile[][]): MapActor {
        const actor = new MapActor(compactActor[i++]);
        let baseName = compactActor[i++];
        actor.baseName = baseName ?? '';
        let name = compactActor[i++];
        actor.name = name ?? '';
        actor.levelX = compactActor[i++];
        actor.levelY = compactActor[i++];
        actor.heading = compactActor[i++];

        if (compactActor.length > i) {
            actor.isCurrentlyPerceived = compactActor[i++];
            actor.hp = compactActor[i++];
            actor.maxHp = compactActor[i++];
            actor.ep = compactActor[i++];
            actor.maxEp = compactActor[i++];
            actor.nextActionTick = compactActor[i++];
            MapActor.expandAction(compactActor[i++], actor);
            actor.meleeAttack = new AttackSummary().update(compactActor[i++]);
            actor.rangeAttack = new AttackSummary().update(compactActor[i++]);
            actor.meleeDefense = new AttackSummary().update(compactActor[i++]);
            actor.rangeDefense = new AttackSummary().update(compactActor[i++]);
        }

        return actor;
    }

    @action.bound
    update(compactActor: readonly any[], tiles: Tile[][]): number {
        let i = 2;

        let unset = false;
        for (; i < compactActor.length;) {
            var index = compactActor[i++];
            switch (index) {
                case 1:
                    let baseName = compactActor[i++];
                    this.baseName = baseName ?? '';
                    break;
                case 2:
                    let name = compactActor[i++];
                    this.name = name ?? '';
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
                case 6:
                    this.isCurrentlyPerceived = compactActor[i++];
                    break;
                case 7:
                    this.hp = compactActor[i++];
                    break;
                case 8:
                    this.maxHp = compactActor[i++];
                    break;
                case 9:
                    this.ep = compactActor[i++];
                    break;
                case 10:
                    this.maxEp = compactActor[i++];
                    break;
                case 11:
                    this.nextActionTick = compactActor[i++];
                    break;
                case 12:
                    MapActor.expandAction(compactActor[i++], this);
                    break;
                case 13:
                    if (this.meleeAttack == null) {
                        this.meleeAttack = new AttackSummary();
                    }

                    this.meleeAttack = this.meleeAttack.update(compactActor[i++]);
                    break;
                case 14:
                    if (this.rangeAttack == null) {
                        this.rangeAttack = new AttackSummary();
                    }

                    this.rangeAttack = this.rangeAttack.update(compactActor[i++]);
                    break;
                case 15:
                    if (this.meleeDefense == null) {
                        this.meleeDefense = new AttackSummary();
                    }

                    this.meleeDefense = this.meleeDefense.update(compactActor[i++]);
                    break;
                case 16:
                    if (this.rangeDefense == null) {
                        this.rangeDefense = new AttackSummary();
                    }

                    this.rangeDefense = this.rangeDefense.update(compactActor[i++]);
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

    @action
    static expandAction(compactAction: any[] | null, actor: MapActor) {
        var i = 0;
        const nextAction = compactAction == null
            ? null
            : compactAction[i++];
        if (nextAction == null
            || compactAction == null) {
            actor.nextAction = null;
            actor.nextActionName = null;
            actor.nextActionTarget = 0;
            return;
        }

        actor.nextAction = nextAction;
        switch (nextAction) {
            default:
                actor.nextActionName = compactAction[i++];
                actor.nextActionTarget = compactAction[i++];
                break;
        }
    }

    addTo(map: Map<number, MapActor>): MapActor {
        map.set(this.id, this);
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

export class AttackSummary {
    @observable delay: number = 0;
    @observable hitProbability: string = '';
    @observable damage: string = '';
    @observable ticksToKill: number = 0;

    @action.bound
    update(compactAttack: readonly any[]): AttackSummary | null {
        var i = 0;
        const delay = compactAttack == null
            ? null
            : compactAttack[i++];
        if (delay == null) {
            return null;
        }

        this.delay = delay;
        this.hitProbability = compactAttack[i++];
        this.damage = compactAttack[i++];
        this.ticksToKill = compactAttack[i++];
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
    static expandToCollection(compactItem: readonly any[], collection: Map<number, MapItem>, tiles: Tile[][] | null, parentState: EntityState) {
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
                const id = compactItem[i++];
                const existingItem = collection.get(id);
                if (existingItem == undefined) {
                    throw 'Item ' + id + ' not deleted';
                }

                existingItem.unset(tiles, existingItem.levelX, existingItem.levelY);
                collection.delete(id);
                break;
            case EntityState.Modified:
                {
                    const id = compactItem[i++];
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
    static expand(compactItem: readonly any[], i: number): MapItem {
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
    update(compactItem: readonly any[], tiles: Tile[][] | null): number {
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

    addTo(map: Map<number, MapItem>): MapItem {
        map.set(this.id, this);
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

export class Ability {
    id: number = -1;
    @observable name: string = '';
    @observable activation: ActivationType = ActivationType.Default;
    @observable slot: number | null = null;
    @observable cooldownTick: number | null = null;
    @observable cooldownXpLeft: number | null = null;
    @observable targetingType: TargetingType = TargetingType.Single;
    @observable targetingShape: TargetingShape = TargetingShape.Line;

    @action
    static expandToCollection(compactAbility: readonly any[], collection: Map<number, Ability>, parentState: EntityState) {
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
                const id = compactAbility[i++];
                if (!collection.delete(id)) {
                    throw 'Ability ' + id + ' not deleted';
                }
                break;
            case EntityState.Modified:
                {
                    const id = compactAbility[i++];
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
    static expand(compactAbility: readonly any[], i: number): Ability {
        const ability = new Ability();
        ability.id = compactAbility[i++];
        ability.name = compactAbility[i++];
        ability.activation = compactAbility[i++];
        ability.slot = compactAbility[i++];
        ability.cooldownTick = compactAbility[i++];
        ability.cooldownXpLeft = compactAbility[i++];
        ability.targetingType = compactAbility[i++];
        ability.targetingShape = compactAbility[i++];

        return ability;
    }

    @action.bound
    update(compactAbility: readonly any[]): number {
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
                case 6:
                    this.targetingType = compactAbility[i++];
                    break;
                case 7:
                    this.targetingShape = compactAbility[i++];
                    break;
            }
        }

        return i;
    }

    @action.bound
    addTo(map: Map<number, Ability>) {
        map.set(this.id, this);
    }
}

export class LogEntry {
    id: number = -1;
    @observable message: string = '';
    @observable ticks: string = '';

    @action
    static expandToCollection(compactLogEntry: any[], collection: Map<number, LogEntry>, parentState: EntityState) {
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
                const id = compactLogEntry[i++];
                if (!collection.delete(id)) {
                    throw 'LogEntry ' + id + ' not deleted';
                }
                break;
            case EntityState.Modified:
                {
                    const id = compactLogEntry[i++];
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
    static expand(compactLogEntry: readonly any[], i: number): LogEntry {
        const logEntry = new LogEntry();
        logEntry.id = compactLogEntry[i++];
        logEntry.message = compactLogEntry[i++];
        logEntry.ticks = compactLogEntry[i++];

        return logEntry;
    }

    @action.bound
    update(compactLogEntry: readonly any[]): number {
        let i = 2;

        for (; i < compactLogEntry.length;) {
            switch (compactLogEntry[i++]) {
                case 1:
                    this.message = compactLogEntry[i++];
                    break;
                case 2:
                    this.ticks = compactLogEntry[i++];
                    break;
                default:
                    return i - 1;
            }
        }

        return i;
    }

    @action.bound
    addTo(map: Map<number, LogEntry>) {
        map.set(this.id, this);
    }
}

export class PlayerRace {
    id: number = 0;
    @observable name: string = '';
    @observable shortName: string = '';
    @observable xpLevel: number = 0;

    @action
    static expandToCollection(compactPlayerRace: readonly any[], collection: Map<number, PlayerRace>, parentState: EntityState) {
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
                const id = compactPlayerRace[i++];
                if (!collection.delete(id)) {
                    throw 'PlayerRace ' + id + ' not deleted';
                }
                break;
            case EntityState.Modified:
                {
                    const id = compactPlayerRace[i++];
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
    static expand(compactPlayerRace: readonly any[], i: number): PlayerRace {
        const playerRace = new PlayerRace();
        playerRace.id = compactPlayerRace[i++];
        playerRace.name = compactPlayerRace[i++];
        playerRace.shortName = compactPlayerRace[i++];
        playerRace.xpLevel = compactPlayerRace[i++];

        return playerRace;
    }

    @action.bound
    update(compactPlayerRace: readonly any[]): number {
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
    addTo(map: Map<number, PlayerRace>) {
        map.set(this.id, this);
    }
}

export class Connection {
    id: number = -1;
    @observable levelX: number = -1;
    @observable levelY: number = -1;
    @observable isDown: boolean = true;

    @action
    static expandToCollection(compactConnection: readonly any[], collection: Map<number, Connection>, tiles: Tile[][], parentState: EntityState) {
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
                const id = compactConnection[i++];
                const existingConnection = collection.get(id);
                if (existingConnection == undefined) {
                    throw 'Connection ' + id + ' not deleted';
                }
                collection.delete(id);
                existingConnection.unset(tiles, existingConnection.levelX, existingConnection.levelY);
                break;
            case EntityState.Modified:
                {
                    const id = compactConnection[i++];
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
    static expand(compactConnection: readonly any[], i: number): Connection {
        const connection = new Connection();
        connection.id = compactConnection[i++];
        connection.levelX = compactConnection[i++];
        connection.levelY = compactConnection[i++];
        connection.isDown = compactConnection[i++];

        return connection;
    }

    @action.bound
    update(compactConnection: readonly any[], tiles: Tile[][]): number {
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

    addTo(map: Map<number, Connection>): Connection {
        map.set(this.id, this);
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