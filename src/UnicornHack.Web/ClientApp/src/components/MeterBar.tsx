import React from 'react';
import { observer } from 'mobx-react';

export const MeterBar = observer((props: IProgressBarProps) => {
    if (React.Children.count(props.children) !== 0) {
        return <div className="progress">
            {props.children}
            {renderLabel(props)}
        </div>
    }

    return <div
        role="meter"
        className={`progress-bar ${props.className || ''}`}
        style={{ width: `${getPercentage(props.now || 0, props.min || 0, props.max || 100)}%` }}
        aria-valuenow={props.now}
        aria-valuemin={props.min}
        aria-valuemax={props.max}
    >
        {renderLabel(props)}
    </div>;
});

function getPercentage(now: number, min: number, max: number): number {
    const percentage = ((now - min) / (max - min)) * 100;
    return Math.round(percentage * 1000) / 1000;
}

function renderLabel(props: IProgressBarProps) {
    return props.label == undefined
        ? ''
        : props.srOnly
            ? <span className="sr-only">{props.label}</span>
            : <span className="progress-bar-label">{props.label}</span>;
}

interface IProgressBarProps {
    min?: number;
    now?: number;
    max?: number;
    label?: string;
    srOnly?: boolean;
    className?: string;
    children?: React.ReactNode;
};