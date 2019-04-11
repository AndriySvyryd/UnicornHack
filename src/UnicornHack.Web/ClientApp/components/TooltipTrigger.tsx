import * as React from 'React';
import { observable, action, IObservableValue } from 'mobx';
import { observer } from 'mobx-react';
import * as PopperJS from 'popper.js';
import * as contains from 'dom-helpers/query/contains';
import * as Overlays from 'react-overlays';

@observer
export class TooltipTrigger extends React.Component<TooltipTriggerProps> {
    _trigger: React.RefObject<any>;
    _show: IObservableValue<boolean> = observable.box(false);
    _timeout: any;
    _ariaModifier: PopperJS.BaseModifier;

    constructor(props: TooltipTriggerProps) {
        super(props);

        this._trigger = React.createRef();
        this._show.set(!!props.initialShow);
        this._timeout = setTimeout(() => { }, 0);

        this._ariaModifier = {
            enabled: true,
            order: 900,
            fn: data => {
                const { popper } = data.instance;
                const target = this.getTarget() as Element;
                if (!this._show || !target) return data;

                if (popper.id) {
                    target.setAttribute('aria-describedby', popper.id);
                }
                return data;
            },
        };

        const child = React.Children.only(this.props.children);
        if (!React.isValidElement(child)) {
            throw "TooltipTrigger must have a child element";
        }

        const { onFocus, onBlur, onClick, onMouseOver, onMouseOut } = (child as React.ReactElement).props;

        this.handleFocus = (e: React.FocusEvent<React.ReactElement>) => {
            this.handleShow();
            if (onFocus) onFocus(e);
        };

        this.handleBlur = (e: React.FocusEvent<React.ReactElement>) => {
            this.handleHide();
            if (onBlur) onBlur(e);
        };

        this.handleClick = (e: React.MouseEvent<React.ReactElement>) => {
            if (this._show.get()) this.hide();
            else this.show();

            if (onClick) onClick(e);
        };

        this.handleMouseOver = (e: React.MouseEvent<React.ReactElement>) => {
            this.handleMouseOverOut(this.handleShow, e);
            if (onMouseOver) onMouseOver(e);
        };

        this.handleMouseOut = (e: React.MouseEvent<React.ReactElement>) => {
            this.handleMouseOverOut(this.handleHide, e);
            if (onMouseOut) onMouseOut(e);
        };
    }

    componentWillUnmount() {
        clearTimeout(this._timeout);
    }

    getTarget = () => this._trigger.current as React.ReactInstance;

    normalizeDelay(delay: undefined | number | { show: number; hide: number }) {
        return delay && typeof delay === 'object' ? delay : { show: delay, hide: delay };
    }

    handleShow = () => {
        clearTimeout(this._timeout);

        const delay = this.normalizeDelay(this.props.delay);

        if (!delay.show) {
            this.show();
            return;
        }

        this._timeout = setTimeout(() => { if (!this._show.get()) this.show(); }, delay.show);
    };

    handleHide = () => {
        clearTimeout(this._timeout);

        const delay = this.normalizeDelay(this.props.delay);

        if (!delay.hide) {
            this.hide();
            return;
        }

        this._timeout = setTimeout(() => { this.hide(); }, delay.hide);
    };

    handleFocus: (e: React.FocusEvent<React.ReactElement>) => void;

    handleBlur: (e: React.FocusEvent<React.ReactElement>) => void;

    handleClick: (e: React.MouseEvent<React.ReactElement>) => void;

    handleMouseOver: (e: React.MouseEvent<React.ReactElement>) => void;

    handleMouseOut: (e: React.MouseEvent<React.ReactElement>) => void;

    handleMouseOverOut(handler: (e: React.MouseEvent<React.ReactElement>) => void, e: React.MouseEvent<React.ReactElement>) {
        const target = e.currentTarget as EventTarget;
        const related = e.relatedTarget;

        if ((!related || related !== target) && !contains(target as Node, related as Node)) {
            handler(e);
        }
    }

    @action
    hide() {
        this._show.set(false);
    }

    @action
    show() {
        this._show.set(true);
    }

    render() {
        const {
            trigger = TriggerType.Focus | TriggerType.Hover,
            tooltip,
            children,
            popperConfig = {},
            delay: _,
            initialShow: __,
            ...props
        } = this.props;

        const triggerProps: React.DOMAttributes<React.ReactElement> = {};
        let rootClose: boolean | undefined;

        if (trigger & TriggerType.Click) {
            triggerProps.onClick = this.handleClick;
            rootClose = true;
        }

        if (trigger & TriggerType.Focus) {
            triggerProps.onFocus = this.handleFocus;
            triggerProps.onBlur = this.handleBlur;
        }

        if (trigger & TriggerType.Hover) {
            triggerProps.onMouseOver = this.handleMouseOver;
            triggerProps.onMouseOut = this.handleMouseOut;
        }

        const child = React.Children.only(children) as React.ReactElement;
        const triggerElement = React.cloneElement(child, { ref: this._trigger, ...triggerProps });

        return <>
            {triggerElement}
            <OverlayWrapper
                {...props}
                popperConfig={{
                    ...popperConfig,
                    modifiers: {
                        ...popperConfig.modifiers,
                        ariaModifier: this._ariaModifier,
                    },
                }}
                observableShow={this._show}
                rootClose={rootClose}
                onHide={this.handleHide}
                target={this.getTarget}
            >
                {tooltip}
            </OverlayWrapper>
        </>;
    }
}

enum TriggerType {
    None = 0,
    Hover = 1 << 0,
    Click = 1 << 1,
    Focus = 1 << 2
}

interface TooltipTriggerProps {
    id: string;
    tooltip: React.ReactNode | (() => React.ReactNode);
    trigger?: TriggerType;
    delay?: number | { show: number; hide: number };
    placement?: PopperJS.Placement;
    initialShow?: boolean;
    popperConfig?: PopperJS.PopperOptions;
}

const OverlayWrapper = observer(({ id, placement, observableShow, children,...outerProps }: OverlayWrapperProps) => {
    return (
        <Overlays.Overlay placement={placement as any} show={observableShow.get()} {...outerProps}>
            {({ props: overlayProps, arrowProps, show, ...props }: OverlayChildProps) => {
                return <Tooltip
                    {...props}
                    {...overlayProps}
                    arrowProps={arrowProps}
                    id={id}
                    className={(show ? 'show' : '')}
                >
                    {children}
                </Tooltip>;
            }}
        </Overlays.Overlay>
    );
});

interface OverlayWrapperProps extends React.ComponentPropsWithoutRef<any>{
    id: string;
    target: React.ReactInstance | (() => React.ReactInstance);
    observableShow: IObservableValue<boolean>;
    popperConfig: PopperJS.PopperOptions;
    onHide: () => void;
    placement?: PopperJS.Placement;
    rootClose?: boolean;
    rootCloseEvent?: 'click' | 'mousedown';
}

interface OverlayChildProps extends React.ComponentPropsWithRef<any> {
    props: React.ComponentPropsWithRef<any>;
    arrowProps: React.ComponentPropsWithRef<any>;
    show: boolean;
    placement: PopperJS.Placement;
}

const Tooltip = React.forwardRef(({
    placement,
    className,
    style,
    children,
    arrowProps,
    outOfBoundaries,
    scheduleUpdate,
    ...props
}: TooltipProps, ref: React.Ref<HTMLDivElement>) => {
    return (
        <div
            ref={ref}
            className={`${className == undefined ? '' : className + ' '}tooltip bs-tooltip-${placement}`}
            style={style}
            role="tooltip"
            x-placement={placement}
            {...props}
        >
            <div className="arrow" {...arrowProps} />
            <div className="tooltip-inner">{children}</div>
        </div>
    );
});

interface TooltipProps extends PopperChildrenProps {
    id: string;
}

interface PopperChildrenProps extends React.ComponentPropsWithoutRef<any> {
    arrowProps?: React.ComponentPropsWithRef<any>;
    outOfBoundaries?: boolean | null;
    placement?: PopperJS.Placement;
    scheduleUpdate?: () => void;
}
