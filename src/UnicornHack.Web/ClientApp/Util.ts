export function coalesce<T>(value: T | null | undefined, fallback: T): T {
    return value === null || value === undefined ? fallback : value;
}

export function capitalize(string: string): string {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

export function unshift<T>(value: T, array: T[]): T[] {
    array.unshift(value);
    return array;
}