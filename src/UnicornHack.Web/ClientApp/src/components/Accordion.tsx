import React from 'react';
import { action } from 'mobx';
import { Provider, observer, inject } from 'mobx-react';
import css from 'dom-helpers/css';
import onEnd from 'dom-helpers/transitionEnd';
import Transition, {
    EXITED,
    ENTERED,
    ENTERING,
    EXITING,
    TransitionStatus,
} from 'react-transition-group/Transition';
import { IKeyContext } from './KeyContext';

export const Accordion = React.forwardRef((props: IAccordionProps, ref: React.Ref<HTMLDivElement>) =>
    <Provider keyContext={props.keyContext}>
        <div ref={ref} className="accordion">
            {props.children}
        </div>
    </Provider>
);

export interface IAccordionProps extends React.ComponentPropsWithRef<any> {
    keyContext: IKeyContext;
}

@inject("keyContext")
@observer
export class AccordionToggle extends React.PureComponent<IAccordionToggleProps, {}> {
    @action.bound
    onSelect(event: React.KeyboardEvent<HTMLButtonElement> | React.MouseEvent<HTMLButtonElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLButtonElement>).key == 'Enter') {
            this.props.keyContext.onSelect(this.props.eventKey, event);
        }
    }

    render() {
        return <button className={this.props.className} onClick={this.onSelect} onKeyPress={this.onSelect}
            aria-controls={this.props.bodyId}
        >
            {this.props.children}
        </button>;
    }
}

export interface IAccordionToggleProps extends React.ComponentPropsWithRef<any> {
    eventKey: string | number;
    keyContext: IKeyContext;
    bodyId: string;
    className?: string;
}

export const AccordionCollapse = inject("keyContext")(observer(({ children, id, eventKey, keyContext }: IAccordionCollapseProps) =>
    <Collapse id={id} in={keyContext.activeKey === eventKey}>
        <div>{React.Children.only(children)}</div>
    </Collapse>
));

export interface IAccordionCollapseProps extends React.ComponentPropsWithRef<any> {
    eventKey: string | number;
    keyContext: IKeyContext;
    id: string;
}

const collapseStyles: { [key: string]: string; } = {
    [EXITED]: 'collapse',
    [EXITING]: 'collapsing',
    [ENTERING]: 'collapsing',
    [ENTERED]: 'collapse show',
};

export interface ICollapseProps extends React.ComponentPropsWithRef<any> {
    in?: boolean,
    mountOnEnter?: boolean,
    unmountOnExit?: boolean,
    appear?: boolean,
    timeout?: number,
    role?: string
}

class Collapse extends React.PureComponent<ICollapseProps> {
    getDimensionValue(elem: HTMLElement) {
        const top = css(elem, 'marginTop') ?? 0;
        const bottom = css(elem, 'marginBottom') ?? 0;
        return (elem.offsetHeight +
            (top == 0 ? 0 : parseInt(top, 10)) +
            (bottom == 0 ? 0 : parseInt(bottom, 10))
        );
    }

    handleEnter = (elem: HTMLElement) => {
        elem.style['height'] = '0';
    };

    handleEntering = (elem: HTMLElement) => {
        elem.style['height'] = `${elem.scrollHeight}px`;
    };

    handleEntered = (elem: HTMLElement) => {
        elem.style['height'] = '';
    };

    handleExit = (elem: HTMLElement) => {
        elem.style['height'] = `${this.getDimensionValue(elem)}px`;
        triggerBrowserReflow(elem);
    };

    handleExiting = (elem: HTMLElement) => {
        elem.style['height'] = '';
    };

    render() {
        const {
            children,
            in: inValue = false,
            timeout = 300,
            role,
            ...props
        } = this.props;

        const child = React.Children.only(children) as React.ComponentElement<any, any>;
        if (!React.isValidElement(child)) {
            return <></>;
        }

        return <Transition
            addEndListener={onEnd}
            {...props}
            in={inValue}
            timeout={timeout}
            role={role}
            aria-expanded={inValue}
            onEnter={this.handleEnter}
            onEntering={this.handleEntering}
            onEntered={this.handleEntered}
            onExit={this.handleExit}
            onExiting={this.handleExiting}
        >
            {(state: TransitionStatus, innerProps: any) =>
                React.cloneElement(child, {
                    ...innerProps,
                    className:
                        child.props.className + ' ' +
                        collapseStyles[state]
                })
            }
        </Transition>;
    }
}

function triggerBrowserReflow(node: any) {
    node.offsetHeight; // eslint-disable-line no-unused-expressions
}