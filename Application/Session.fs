module Application.Session

open Model.Session

type ISessionStore =
    abstract getSessions : string -> seq<Session>
    abstract addSession : string -> Session -> Result<unit, string>
    
let getSessions (sessionStore: ISessionStore) (name: string) : seq<Session> =
    sessionStore.getSessions name
    
let getEligibleSessions (sessionStore: ISessionStore) (name: string) (diploma: string) : seq<Session> =
    let sessions = getSessions sessionStore name
    let diploma = Diploma.make diploma
    Session.eligibleSessions sessions diploma
    
let getTotalMinutes (sessions: seq<Session>) : int =
    sessions
    |> Seq.map (_.Minutes)
    |> Seq.sum
    
let addSession (sessionStore: ISessionStore) (name: string) (session: Session) : Result<unit, string> =
    sessionStore.addSession name session