export function coalesce(value: any, fallback: any): any {
    return value === null || value === undefined ? fallback : value;
}

export function capitalize(string: string): string {
    return string.charAt(0).toUpperCase() + string.slice(1);
}