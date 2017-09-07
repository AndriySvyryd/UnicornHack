interface MousetrapStatic {
    record(callback: (e: string[]) => void): void;
    addKeycodes(map: any): void;
}