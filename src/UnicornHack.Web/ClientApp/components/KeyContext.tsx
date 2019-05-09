import { observable, action } from "mobx";

export interface IKeyContext {
    activeKey: string | number;
    onSelect: (eventKey: any, e: React.SyntheticEvent<{}>) => void;
}

export class KeyContext implements IKeyContext {
    @observable activeKey: string | number = '';

    @action.bound
    onSelect(key: string) {
        if (this.activeKey == key) {
            this.activeKey = '';
        } else {
            this.activeKey = key;
        }
    }
}