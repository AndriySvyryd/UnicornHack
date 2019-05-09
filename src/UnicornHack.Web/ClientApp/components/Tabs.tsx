import * as React from 'React';
import { observer, Provider } from 'mobx-react';
import { action } from 'mobx';
import { IKeyContext } from './KeyContext';

@observer
export class Tabs extends React.Component<ITabsProps, {}> {
    @action.bound
    renderTab(child: any, index: number) {
        if (!React.isValidElement(child)) {
            return child;
        }

        const childProps = (child as unknown as Tab).props;
        return <NavTab id={this.props.id} eventKey={childProps.eventKey} title={childProps.title} keyContext={this.props.keyContext}
            tabIndex={this.props.baseTabIndex === undefined ? -1 : this.props.baseTabIndex + index} />;
    }

    @action.bound
    renderContent(child: any) {
        if (!React.isValidElement(child)) {
            return child;
        }

        const childProps = (child as unknown as Tab).props;
        return <TabPane id={this.props.id} eventKey={childProps.eventKey} keyContext={this.props.keyContext}>
            {childProps.children}
        </TabPane>;
    }

    render() {
        return <Provider keyContext={this.props.keyContext}>
            <div>
                <nav className="nav nav-tabs nav-fill" role="tablist">
                    {React.Children.map(this.props.children, this.renderTab)}
                </nav>
                <div className="tab-content">
                    {React.Children.map(this.props.children, this.renderContent)}
                </div>
            </div>
        </Provider>;
    }
}

interface ITabsProps {
    id: string;
    keyContext: IKeyContext;
    baseTabIndex?: number;
}

export class Tab extends React.Component<ITabProps, {}> {
    render(): React.ReactNode {
        throw new Error(
            "The `Tab` component is not meant to be rendered! " +
            "It's an abstract component that is only valid as a direct Child of the `Tabs` Component. "
        );
    }
}

interface ITabProps {
    eventKey: string;
    title: string;
    disabled?: boolean;
}

@observer
class NavTab extends React.Component<INavTab, {}> {
    @action.bound
    setAbilitySlot(event: React.KeyboardEvent<HTMLAnchorElement> | React.MouseEvent<HTMLAnchorElement>) {
        if (event.type == 'click' || (event as React.KeyboardEvent<HTMLAnchorElement>).key == 'Enter') {
            this.props.keyContext.onSelect(this.props.eventKey, event);
        }
    }

    render() {
        const {
            id,
            keyContext,
            title,
            disabled,
            tabIndex,
            eventKey
        } = this.props;

        let isActive = keyContext.activeKey === eventKey;

        return <a
            id={`${id}-tab-${eventKey}`}
            aria-controls={`${id}-tabpane-${eventKey}`}
            aria-selected={isActive}
            tabIndex={tabIndex}
            className={`nav-item nav-link${isActive ? ' active' : ''}${disabled ? ' disabled' : ''}`}
            role="tab"
            onClick={this.setAbilitySlot}
            onKeyPress={this.setAbilitySlot}
        >
            {title}
        </a>;
    }
};

interface INavTab {
    id: string;
    eventKey: string;
    title: string;
    keyContext: IKeyContext;
    disabled?: boolean;
    tabIndex: number;
}

const TabPane = observer((props: ITabPaneProps) => {
    const {
        id,
        keyContext,
        eventKey
    } = props;

    let isActive = keyContext.activeKey === eventKey;

    return <div
        id={`${id}-tabpane-${eventKey}`}
        aria-labelledby={`${id}-tab-${eventKey}`}
        aria-hidden={!isActive}
        className={`tab-pane${isActive ? ' active' : ''}`}
        role="tabpanel"
    >
        {props.children}
    </div>;
});

interface ITabPaneProps {
    id: string;
    eventKey: string;
    keyContext: IKeyContext;
    children?: React.ReactNode;
}