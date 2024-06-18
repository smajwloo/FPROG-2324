module Application.Session

open Model.Diploma
open Model.Session

type ISessionStore =
    abstract getSessions : unit -> seq<string * bool * System.DateTime * int>
    abstract addSession : string -> Session -> Result<unit, string>
    

let sessionsIsEmpty sessions =
    match Seq.isEmpty sessions with
    | true -> Error "No sessions found!"
    | false -> Ok sessions
    
let filterSessionsByName name sessions =
    sessions
    |> Seq.filter (fun (n, _, _, _) -> n = name)

let mapSessions sessions name =
    sessions
    |> filterSessionsByName name
    |> Seq.map (fun (_, deep, date, minutes) -> Session.make deep date minutes)
    |> Seq.choose (function
        | Ok session -> Some session
        | Error _ -> None
    )

let getSessionsOfCandidate (sessionStore: ISessionStore) (name: string) : Result<seq<Session>, string> =
    let sessions = sessionStore.getSessions ()
    let mappedSessions = mapSessions sessions name
    sessionsIsEmpty mappedSessions
    

let eligibleSessions sessions diploma =
    sessions
    |> Seq.filter (fun session -> session.Deep || Session.shallowOk diploma)
    |> Seq.filter (fun session -> session.Minutes >= Session.minMinutes diploma)
    |> Seq.toList
    
let getEligibleSessions sessions diploma =
    let diploma = Diploma.make diploma
    eligibleSessions sessions diploma
    
let getTotalMinutes (sessions: seq<Session>) : int =
    sessions
    |> Seq.map (_.Minutes)
    |> Seq.sum
    
let addSession (sessionStore: ISessionStore) (name: string) (session: Session) : Result<unit, string> =
    sessionStore.addSession name session