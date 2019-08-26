import * as React from 'React';
import { IObservableValue } from 'mobx';
import { observer } from 'mobx-react';

export const Banner = observer((props: IBannerProps ) => {
    const message = props.message.get();
    if (!message) {
        return <></>
    }

    return <div className="banner__overlay">
        <div className="banner__overlayContent" aria-label={message}>
            {message}
        </div>
    </div>;
});

export interface IBannerProps {
    message: IObservableValue<string | null>
}