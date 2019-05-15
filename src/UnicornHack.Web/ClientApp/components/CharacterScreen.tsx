import * as React from 'React';
import { action, computed } from 'mobx';
import { observer } from 'mobx-react';
import { GameQueryType } from '../transport/GameQueryType';
import { ActorAttributes, PlayerAdaptations, PlayerSkills } from '../transport/DialogData';
import { ActorAttributesScreen } from './CreatureProperties';
import { Dialog } from './Dialog';
import { IGameContext } from './Game';
import { IKeyContext } from './KeyContext';
import { PropertyRow } from './PropertyRow';
import { Tabs, Tab } from './Tabs';

export const CharacterScreenDialog = observer((props: ICharacterScreenProps) => {
    const { data, context } = props;
    const show = computed(() => data.playerAttributes !== null || data.playerAdaptations !== null || data.playerSkills !== null);
    return <Dialog context={context} show={show} className="characterScreen">
        <CharacterScreen {...props} />
    </Dialog>;
});

const CharacterScreen = observer((props: ICharacterScreenProps) => {
    return <Tabs id="tabs" keyContext={new CharacterKeyContext(props)}>
        <Tab eventKey="attributes" title="Attributes">
            <AttributesScreen data={props.data} />
        </Tab>
        <Tab eventKey="adaptations" title="Adaptations">
            <AdaptationsScreen data={props.data} />
        </Tab>
        <Tab eventKey="skills" title="Skills">
            <SkillsScreen data={props.data} />
        </Tab>
    </Tabs>;
});

interface ICharacterScreenProps {
    data: ICharacterScreenData;
    context: IGameContext;
}

class CharacterKeyContext implements IKeyContext {
    private _props: ICharacterScreenProps;

    constructor(props: ICharacterScreenProps) {
        this._props = props;
    }

    @action.bound
    onSelect(key: string, event: React.SyntheticEvent<{}>) {
        switch (key) {
            case 'attributes':
                this._props.context.showDialog(GameQueryType.PlayerAttributes);
                break;
            case 'adaptations':
                this._props.context.showDialog(GameQueryType.PlayerAdaptations);
                break;
            case 'skills':
                this._props.context.showDialog(GameQueryType.PlayerSkills);
                break;
        }

        event.stopPropagation();
    }

    @computed get activeKey(): string {
        return this._props.data.playerAttributes !== null
            ? 'attributes'
            : this._props.data.playerAdaptations !== null
                ? 'adaptations'
                : 'skills';
    }
}

const AttributesScreen = observer((props: ICharacterSubScreenProps) => {
    const playerAttributes = props.data.playerAttributes;
    if (playerAttributes == null) {
        return <></>;
    }

    return <ActorAttributesScreen actorAttributes={playerAttributes} />;
});

const AdaptationsScreen = observer((props: ICharacterSubScreenProps) => {
    const playerAdaptations = props.data.playerAdaptations;
    if (playerAdaptations == null) {
        return <></>;
    }
    
    return <div className="characterScreen__content">
        <PropertyRow label="Mutation points" value={playerAdaptations.mutationPoints} />
        <PropertyRow label="Mutations" value={playerAdaptations.mutations.size} />
        <PropertyRow label="Trait points" value={playerAdaptations.traitPoints} />
        <PropertyRow label="Traits" value={playerAdaptations.traits.size} />
    </div>;
});

const SkillsScreen = observer((props: ICharacterSubScreenProps) => {
    const playerSkills = props.data.playerSkills;
    if (playerSkills == null) {
        return <></>;
    }

    return <div className="characterScreen__content">
        <PropertyRow label="Skill points" value={playerSkills.skillPoints} />
        <PropertyRow label="Hand weapons" value={playerSkills.handWeapons} />
        <PropertyRow label="Short weapons" value={playerSkills.shortWeapons} />
        <PropertyRow label="Medium weapons" value={playerSkills.mediumWeapons} />
        <PropertyRow label="Long weapons" value={playerSkills.longWeapons} />
        <PropertyRow label="Close range weapons" value={playerSkills.closeRangeWeapons} />
        <PropertyRow label="Short range weapons" value={playerSkills.shortRangeWeapons} />
        <PropertyRow label="Medium range weapons" value={playerSkills.mediumRangeWeapons} />
        <PropertyRow label="Long range weapon" value={playerSkills.longRangeWeapons} />
        <PropertyRow label="One-handed" value={playerSkills.oneHanded} />
        <PropertyRow label="Two-handed" value={playerSkills.twoHanded} />
        <PropertyRow label="Dual-wielding" value={playerSkills.dualWielding} />
        <PropertyRow label="Acrobatics" value={playerSkills.acrobatics} />
        <PropertyRow label="Light armor" value={playerSkills.lightArmor} />
        <PropertyRow label="Heavy armor" value={playerSkills.heavyArmor} />
        <PropertyRow label="Air sourcery" value={playerSkills.airSourcery} />
        <PropertyRow label="Blood sourcery" value={playerSkills.bloodSourcery} />
        <PropertyRow label="Earth sourcery" value={playerSkills.earthSourcery} />
        <PropertyRow label="Fire sourcery" value={playerSkills.fireSourcery} />
        <PropertyRow label="Spirit sourcery" value={playerSkills.spiritSourcery} />
        <PropertyRow label="Water sourcery" value={playerSkills.waterSourcery} />
        <PropertyRow label="Conjuration" value={playerSkills.conjuration} />
        <PropertyRow label="Enchantment" value={playerSkills.enchantment} />
        <PropertyRow label="Evocation" value={playerSkills.evocation} />
        <PropertyRow label="Malediction" value={playerSkills.malediction} />
        <PropertyRow label="Illusion" value={playerSkills.illusion} />
        <PropertyRow label="Transmutation" value={playerSkills.transmutation} />
        <PropertyRow label="Assassination" value={playerSkills.assassination} />
        <PropertyRow label="Stealth" value={playerSkills.stealth} />
        <PropertyRow label="Artifice" value={playerSkills.artifice} />
        <PropertyRow label="Leadership" value={playerSkills.leadership} />
    </div>;
});

interface ICharacterSubScreenProps {
    data: ICharacterScreenData;
}

interface ICharacterScreenData {
    playerAttributes: ActorAttributes | null;
    playerAdaptations: PlayerAdaptations | null;
    playerSkills: PlayerSkills | null;
}