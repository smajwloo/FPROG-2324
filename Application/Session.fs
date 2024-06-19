module Application.Session

open Model.Diploma
open Model.Session
open Application.Common

type ISessionStore =
    abstract getSessions : unit -> seq<string * bool * System.DateTime * int>
    abstract addSession : string -> Session -> Result<unit, string>
    
let filterSessionsByName name sessions =
    sessions
    |> Seq.filter (fun (n, _, _, _) -> n = name)
    
let filterSessionsByEligibility sessions diploma =
    sessions
    |> Seq.filter (fun session -> session.Deep || Session.shallowOk diploma)
    |> Seq.filter (fun session -> session.Minutes >= Session.minMinutes diploma)
    |> Seq.toList
    
let makeSession (_, deep, date, minutes) =
    Session.make deep date minutes
    
let getSessions (sessionStore: ISessionStore) (name: string) =
    let sessions = sessionStore.getSessions ()
    sessions
    |> filterSessionsByName name
    |> Seq.map makeSession
    |> Seq.choose convertResultToOption
    |> sequenceIsEmpty

let getEligibleSessions sessions diploma =
    let diploma = Diploma.make diploma
    filterSessionsByEligibility sessions diploma
    
let getTotalMinutes sessions : int =
    sessions
    |> Seq.map _.Minutes
    |> Seq.sum
    
let addSession (sessionStore: ISessionStore) (name: string) (session: Session) =
    sessionStore.addSession name session