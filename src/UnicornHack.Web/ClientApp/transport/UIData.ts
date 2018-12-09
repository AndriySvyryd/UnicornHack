import { action, observable } from 'mobx';
import { Ability, EntityState } from './Model';
import { GameQueryType } from './GameQueryType';

export class UIData {
    @observable abilitySlot: number | null = null;
    @observable slottableAbilities: Map<string, Ability> = new Map<string, Ability>();

    @action.bound
    update(compactData: any[]): UIData {
        let i = 0;

        var queryType = compactData[i++];
        switch (queryType) {
            case GameQueryType.Clear:
                this.clear();
                break;
            case GameQueryType.SlottableAbilities:
                this.abilitySlot = compactData[i++];
                this.slottableAbilities.clear();
                compactData[i++].map(
                    (s: any[]) => Ability.expandToCollection(s, this.slottableAbilities, EntityState.Added));
                break;
            default:
                console.error("Unhandled GameQueryType %d", queryType);
                break;
        }

        return this;
    }

    @action.bound
    clear() {
        this.abilitySlot = null;
        this.slottableAbilities.clear();
    }
}