export function coalesce(value: any, fallback: any): any {
    return value === null || value === undefined ? fallback : value;
}