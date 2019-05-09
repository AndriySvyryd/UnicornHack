import * as React from 'React';

export const PropertyRow = ({ label, value, show }: IPropertyRowProps) => {
    if (show === false) {
        return <></>;
    }

    return <div className="property__row">
        <div className="property__label">{label}:</div><div className="property__value">{value}</div>
    </div>;
};

interface IPropertyRowProps {
    label: string;
    value: string | number | boolean | string[] | null;
    show?: boolean;
}