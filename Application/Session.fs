module Application.Session

open Model.Session

type ISessionStore =
    abstract getSessions : string -> seq<Session>
    
let getSessions (sessionStore: ISessionStore, name: string) : seq<Session> =
    sessionStore.getSessions name