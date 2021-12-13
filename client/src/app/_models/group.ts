export interface Group {
    name: string;
    connections: Conncetion[];
}

interface Conncetion {
    connectionId: string;
    username: string;
}