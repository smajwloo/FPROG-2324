module Application.Session

open Model.Session

type ISessionStore =
    abstract getSessions : string -> seq<Session>
    
let getSessions (sessionStore: ISessionStore) (name: string) : seq<Session> =
    sessionStore.getSessions name
    
let getEligibleSessions (sessionStore: ISessionStore) (name: string) (diploma: string) : seq<Session> =
    let sessions = getSessions sessionStore name
    let diploma = Diploma.make diploma
    
    Session.eligibleSessions sessions diploma
    
let getTotalEligibleMinutes (sessionStore: ISessionStore) (name: string) (diploma: string) : int =
    getEligibleSessions sessionStore name diploma
    |> Seq.map (_.Minutes)
    |> Seq.sum