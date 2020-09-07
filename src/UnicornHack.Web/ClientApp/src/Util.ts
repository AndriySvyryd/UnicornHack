export function capitalize(string: string): string {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

export function formatTime(time: number, showZeroes: boolean = false): string {
    let ticks = time % 100;
    let seconds = Math.round(time / 100);
    let minutes = Math.round(seconds / 60);
    let hours = Math.round(minutes / 60);

    if (hours != 0) {
        showZeroes = true;
    }
    var result = !showZeroes
        ? ""
        : hours + ':';
    result += minutes == 0 && !showZeroes
        ? ""
        : formatInteger(minutes, 2) + ':';
    return result + formatInteger(seconds, 2) + '.' + formatInteger(ticks, 2);
}

export function formatInteger(number: number, minimumDigits: number): string {
    return number.toLocaleString(undefined, { minimumIntegerDigits: minimumDigits });
}

export function unshift<T>(value: T, array: T[]): T[] {
    array.unshift(value);
    return array;
}