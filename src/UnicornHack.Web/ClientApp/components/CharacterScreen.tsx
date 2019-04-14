import * as React from 'React';
import { action, computed } from 'mobx';
import { observer } from 'mobx-react';
import { GameQueryType } from '../transport/GameQueryType';
import { Player } from '../transport/Model';
import { PlayerAction } from '../transport/PlayerAction';
import { ActorAttributes, PlayerAdaptations, PlayerSkills } from '../transport/DialogData';
import { Tabs, Tab } from './Tabs';
import { AttributesScreen } from './CreatureProperties';

@observer
export class CharacterScreen extends React.Component<ICharacterScreenProps, {}> {
    container: React.RefObject<HTMLDivElement>;

    constructor(props: ICharacterScreenProps) {
        super(props);

        this.container = React.createRef();
    }

    componentDidUpdate(prevProps: any) {
        if ((this.props.data.playerAttributes !== null
            || this.props.data.playerAdaptations !== null
            || this.props.data.playerSkills !== null)
                && this.container.current !== null) {
            this.container.current.focus();
        }
    }

    @action.bound
    handleTabSelection(key: string, event: React.SyntheticEvent<{}>) {
        switch (key) {
            case 'attributes':
                this.props.queryGame(GameQueryType.PlayerAttributes);
                break;
            case 'adaptations':
                this.props.queryGame(GameQueryType.PlayerAdaptations);
                break;
            case 'skills':
                this.props.queryGame(GameQueryType.PlayerSkills);
                break;
        }

        event.stopPropagation();
    }

    @computed get hidden(): boolean {
        return this.props.data.playerAttributes === null
            && this.props.data.playerAdaptations === null
            && this.props.data.playerSkills === null;
    }

    @computed get activeKey(): string {
        return this.props.data.playerAttributes !== null
            ? 'attributes'
            : this.props.data.playerAdaptations !== null
                ? 'adaptations'
                : 'skills';
    }

    @action.bound
    clear(event: React.MouseEvent<HTMLDivElement>) {
        this.props.queryGame(GameQueryType.Clear);
        event.preventDefault();
    }

    stopPropagation(e: React.SyntheticEvent<{}>) {
        e.stopPropagation;
    }

    render() {
        if (this.hidden) {
            return <></>
        }

        return <div className="dialog__overlay" ref={this.container} tabIndex={100} onClick={this.clear} onContextMenu={this.clear}>
            <div className="characterScreen" onClick={this.stopPropagation} role="dialog" aria-label="Character screen">
                <Tabs
                    id="tabs"
                    activeKey={this.activeKey}
                    onSelect={this.handleTabSelection}
                >
                    <Tab eventKey="attributes" title="Attributes">
                        <AttributesScreen actorAttributes={this.props.data.playerAttributes} />
                    </Tab>
                    <Tab eventKey="adaptations" title="Adaptations">
                        <AdaptationsScreen data={this.props.data} />
                    </Tab>
                    <Tab eventKey="skills" title="Skills">
                        <SkillsScreen data={this.props.data} />
                    </Tab>
                </Tabs>
            </div>
        </div>;
    }
}

interface ICharacterScreenProps {
    data: ICharacterScreenData;
    player: Player;
    performAction: (action: PlayerAction, target: (number | null), target2: (number | null)) => void;
    queryGame: (queryType: GameQueryType, ...args: Array<number>) => void;
}

const AdaptationsScreen = observer((props: ICharacterSubScreenProps) => {
    const playerAdaptations = props.data.playerAdaptations;
    if (playerAdaptations == null) {
        return <></>;
    }
    
    return <div className="characterScreen__content">
        <div className="characterScreen__row">
            <div className="characterScreen__label">Mutation points:</div>
            <div className="characterScreen__value">{playerAdaptations.MutationPoints}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Mutations:</div>
            <div className="characterScreen__value">{playerAdaptations.Mutations}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Trait points:</div>
            <div className="characterScreen__value">{playerAdaptations.TraitPoints}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Traits:</div>
            <div className="characterScreen__value">{playerAdaptations.Traits}</div>
        </div>
    </div>;
});

const SkillsScreen = observer((props: ICharacterSubScreenProps) => {
    const playerSkills = props.data.playerSkills;
    if (playerSkills == null) {
        return <></>;
    }

    return <div className="characterScreen__content">
        <div className="characterScreen__row">
            <div className="characterScreen__label">Skill points:</div>
            <div className="characterScreen__value">{playerSkills.SkillPoints}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Hand weapons:</div>
            <div className="characterScreen__value">{playerSkills.HandWeapons}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Short weapons:</div>
            <div className="characterScreen__value">{playerSkills.ShortWeapons}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Medium weapons:</div>
            <div className="characterScreen__value">{playerSkills.MediumWeapons}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Long weapons:</div>
            <div className="characterScreen__value">{playerSkills.LongWeapons}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Close range weapons:</div>
            <div className="characterScreen__value">{playerSkills.CloseRangeWeapons}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Short range weapons:</div>
            <div className="characterScreen__value">{playerSkills.ShortRangeWeapons}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Medium range weapons:</div>
            <div className="characterScreen__value">{playerSkills.MediumRangeWeapons}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Long range weapons:</div>
            <div className="characterScreen__value">{playerSkills.LongRangeWeapons}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">One-handed:</div>
            <div className="characterScreen__value">{playerSkills.OneHanded}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Two-handed:</div>
            <div className="characterScreen__value">{playerSkills.TwoHanded}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Dual-wielding:</div>
            <div className="characterScreen__value">{playerSkills.DualWielding}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Acrobatics:</div>
            <div className="characterScreen__value">{playerSkills.Acrobatics}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Light armor:</div>
            <div className="characterScreen__value">{playerSkills.LightArmor}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Heavy armor:</div>
            <div className="characterScreen__value">{playerSkills.HeavyArmor}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Air sourcery:</div>
            <div className="characterScreen__value">{playerSkills.AirSourcery}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Blood sourcery:</div>
            <div className="characterScreen__value">{playerSkills.BloodSourcery}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Earth sourcery:</div>
            <div className="characterScreen__value">{playerSkills.EarthSourcery}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Fire sourcery:</div>
            <div className="characterScreen__value">{playerSkills.FireSourcery}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Spirit sourcery:</div>
            <div className="characterScreen__value">{playerSkills.SpiritSourcery}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Water sourcery:</div>
            <div className="characterScreen__value">{playerSkills.WaterSourcery}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Conjuration:</div>
            <div className="characterScreen__value">{playerSkills.Conjuration}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Enchantment:</div>
            <div className="characterScreen__value">{playerSkills.Enchantment}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Evocation:</div>
            <div className="characterScreen__value">{playerSkills.Evocation}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Malediction:</div>
            <div className="characterScreen__value">{playerSkills.Malediction}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Illusion:</div>
            <div className="characterScreen__value">{playerSkills.Illusion}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Transmutation:</div>
            <div className="characterScreen__value">{playerSkills.Transmutation}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Assassination:</div>
            <div className="characterScreen__value">{playerSkills.Assassination}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Stealth:</div>
            <div className="characterScreen__value">{playerSkills.Stealth}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Artifice:</div>
            <div className="characterScreen__value">{playerSkills.Artifice}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Leadership:</div>
            <div className="characterScreen__value">{playerSkills.Leadership}</div>
        </div>
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