module Application.Session

open Model.Session

type ISessionStore =
    abstract getSessions : string -> seq<string * bool * System.DateTime * int>
    abstract addSession : string -> Session -> Result<unit, string>
    

let sessionsIsEmpty sessions =
    match Seq.isEmpty sessions with
    | true -> Error "No sessions found!"
    | false -> Ok sessions

let getSessions (sessionStore: ISessionStore) (name: string) : Result<seq<Session>, string> =
    let sessions = sessionStore.getSessions name
    let mappedSessions = sessions
                        |> Seq.map (fun (_, deep, date, minutes) ->
                            { Session.Deep = deep
                              Date = date
                              Minutes = minutes })
    sessionsIsEmpty mappedSessions
    
let getEligibleSessions (sessions: seq<Session>) (diploma: string) : seq<Session> =
    let diploma = Diploma.make diploma
    Session.eligibleSessions sessions diploma
    
let getTotalMinutes (sessions: seq<Session>) : int =
    sessions
    |> Seq.map (_.Minutes)
    |> Seq.sum
    
let addSession (sessionStore: ISessionStore) (name: string) (session: Session) : Result<unit, string> =
    sessionStore.addSession name session