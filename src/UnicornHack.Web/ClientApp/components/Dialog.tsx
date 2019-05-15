import * as React from 'React';
import { action, IComputedValue } from 'mobx';
import { observer } from 'mobx-react';
import { GameQueryType } from '../transport/GameQueryType';
import { IGameContext } from './Game';

@observer
export class Dialog extends React.Component<IDialogProps, {}> {
    private _container: React.RefObject<HTMLDivElement>;

    constructor(props: IDialogProps) {
        super(props);

        this._container = React.createRef();
    }

    componentDidUpdate() {
        if (this._container.current !== null) {
            this._container.current.focus();
        }
    }

    @action.bound
    clear(event: React.MouseEvent<HTMLDivElement>) {
        this.props.context.showDialog(GameQueryType.Clear);
        event.preventDefault();
    }

    @action.bound
    back() {
        this.props.context.showDialog(GameQueryType.Back);
    }

    stopPropagation(event: React.SyntheticEvent<{}>) {
        event.stopPropagation();
        event.preventDefault();
    }

    render() {
        if (!this.props.show.get()) {
            return <></>
        }

        const { title, className, children } = this.props;
        return <div className="dialog__overlay" ref={this._container} tabIndex={100} onClick={this.clear} onContextMenu={this.clear}>
            <div className="dialog__overlayContent" onClick={this.stopPropagation} onContextMenu={this.stopPropagation}>
                <div className={className} role="dialog" aria-label={title != undefined ? title.get() : undefined}>
                    {title != undefined ? <div className="dialog__title">{title.get()}</div> : <></>}
                    <button type="button" className="dialog__backButton" onClick={this.back}>{"<"}</button>
                    {children}
                </div>
            </div>
        </div>;
    }
}

interface IDialogProps {
    context: IGameContext;
    show: IComputedValue<boolean>;
    className?: string;
    title?: IComputedValue<string>;
}