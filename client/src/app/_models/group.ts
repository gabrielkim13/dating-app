interface Connection {
  connectionId: string;
  username: string;
}

export interface Group {
  name: string;
  connections: Connection[];
}
