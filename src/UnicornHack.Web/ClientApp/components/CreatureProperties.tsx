import * as React from 'React';
import { action } from 'mobx';
import { observer } from 'mobx-react';
import { GameQueryType } from '../transport/GameQueryType';
import { ActorAttributes } from '../transport/DialogData';
import { capitalize } from '../Util';

@observer
export class CreatureProperties extends React.Component<ICreaturePropertiesProps, {}> {
    container: React.RefObject<HTMLDivElement>;

    constructor(props: ICreaturePropertiesProps) {
        super(props);

        this.container = React.createRef();
    }

    componentDidUpdate(prevProps: any) {
        if (this.props.data.actorAttributes !== null
            && this.container.current !== null) {
            this.container.current.focus();
        }
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
        if (this.props.data.actorAttributes == null) {
            return <></>
        }

        const title = capitalize(this.props.data.actorAttributes.Name || "Unknown creature");
        return <div className="dialog__overlay" ref={this.container} tabIndex={100} onClick={this.clear} onContextMenu={this.clear}>
            <div className="creatureProperties" onClick={this.stopPropagation} role="dialog" aria-label="Creature properties">
                <h2>{title}</h2>
                <div>{this.props.data.actorAttributes.Description}</div>
                {this.props.data.actorAttributes.Name == null ? <></> : <AttributesScreen actorAttributes={this.props.data.actorAttributes} />}
            </div>
        </div>;
    }
}

interface ICreaturePropertiesProps {
    data: IActorPropertiesData;
    queryGame: (queryType: GameQueryType, ...args: Array<number>) => void;
}

export const AttributesScreen = observer((props: IActorPropertiesData) => {
    const actorAttributes = props.actorAttributes;
    if (actorAttributes == null) {
        return <></>;
    }

    return <div className="characterScreen__content">
        <div className="characterScreen__row">
            <div className="characterScreen__label">Hit points:</div>
            <div className="characterScreen__value">{actorAttributes.HitPoints}/{actorAttributes.HitPointMaximum}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Energy points:</div>
            <div className="characterScreen__value">{actorAttributes.EnergyPoints}/{actorAttributes.EnergyPointMaximum}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Might:</div>
            <div className="characterScreen__value">{actorAttributes.Might}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Focus:</div>
            <div className="characterScreen__value">{actorAttributes.Focus}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Speed:</div>
            <div className="characterScreen__value">{actorAttributes.Speed}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Perception:</div>
            <div className="characterScreen__value">{actorAttributes.Perception}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Primary FOV:</div>
            <div className="characterScreen__value">{actorAttributes.PrimaryFOVQuadrants * 90}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Primary vision range:</div>
            <div className="characterScreen__value">{actorAttributes.PrimaryVisionRange}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Total FOV:</div>
            <div className="characterScreen__value">{actorAttributes.TotalFOVQuadrants * 90}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Secondary vision range:</div>
            <div className="characterScreen__value">{actorAttributes.SecondaryVisionRange}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Infravision:</div>
            <div className="characterScreen__value">{actorAttributes.Infravision}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Invisibility detection:</div>
            <div className="characterScreen__value">{actorAttributes.InvisibilityDetection}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Visibility:</div>
            <div className="characterScreen__value">{actorAttributes.Visibility}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Movement delay:</div>
            <div className="characterScreen__value">{actorAttributes.MovementDelay}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Regeneration:</div>
            <div className="characterScreen__value">{actorAttributes.Regeneration}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Energy regeneration:</div>
            <div className="characterScreen__value">{actorAttributes.EnergyRegeneration}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Armor:</div>
            <div className="characterScreen__value">{actorAttributes.Armor}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Deflection:</div>
            <div className="characterScreen__value">{actorAttributes.Deflection}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Evasion:</div>
            <div className="characterScreen__value">{actorAttributes.Evasion}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Physical resistance:</div>
            <div className="characterScreen__value">{actorAttributes.PhysicalResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Magic resistance:</div>
            <div className="characterScreen__value">{actorAttributes.MagicResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Bleeding resistance:</div>
            <div className="characterScreen__value">{actorAttributes.BleedingResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Acid resistance:</div>
            <div className="characterScreen__value">{actorAttributes.AcidResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Cold resistance:</div>
            <div className="characterScreen__value">{actorAttributes.ColdResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Electricity resistance:</div>
            <div className="characterScreen__value">{actorAttributes.ElectricityResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Fire resistance:</div>
            <div className="characterScreen__value">{actorAttributes.FireResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Psychic resistance:</div>
            <div className="characterScreen__value">{actorAttributes.PsychicResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Toxin resistance:</div>
            <div className="characterScreen__value">{actorAttributes.ToxinResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Void resistance:</div>
            <div className="characterScreen__value">{actorAttributes.VoidResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Sonic resistance:</div>
            <div className="characterScreen__value">{actorAttributes.SonicResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Stun resistance:</div>
            <div className="characterScreen__value">{actorAttributes.StunResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Light resistance:</div>
            <div className="characterScreen__value">{actorAttributes.LightResistance}</div>
        </div>
        <div className="characterScreen__row">
            <div className="characterScreen__label">Water resistance:</div>
            <div className="characterScreen__value">{actorAttributes.WaterResistance}</div>
        </div>
    </div>;
});

export interface IActorPropertiesData {
    actorAttributes: ActorAttributes | null;
}