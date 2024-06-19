module Application.Session

open Model.Diploma
open Model.Session
open Application.Common

type ISessionStore =
    abstract getSessions : unit -> seq<string * bool * System.DateTime * int>
    abstract addSession : string -> Session -> Result<unit, string>
    
let filterSessionsByName name sessions =
    sessions |> Seq.filter (fun (n, _, _, _) -> n = name)
    
let filterSessionsByEligibility sessions diploma =
    sessions
    |> Seq.filter (fun session -> session.Deep || Diploma.shallowOk diploma)
    |> Seq.filter (fun session -> session.Minutes >= Diploma.minMinutes diploma)
    
let makeSession (_, deep, date, minutes) =
    Session.make deep date minutes
  
let makeDiploma diploma =
    let diploma = Diploma.make diploma
    match diploma with
    | None -> Error "Invalid diploma."
    | Some diploma -> Ok diploma
    
let handleSessionSequence sessions =
    sessions
    |> Seq.map makeSession
    |> Seq.choose convertResultToOption
    |> sequenceIsEmpty "No sessions found."
    
let getSessions (sessionStore: ISessionStore) =
    sessionStore.getSessions ()
    
let getAllCandidatesSessions (sessionStore: ISessionStore) =
    getSessions sessionStore
    |> handleSessionSequence
    
let getSessionsOfCandidate (sessionStore: ISessionStore) (name: string) =
    getSessions sessionStore
    |> filterSessionsByName name
    |> handleSessionSequence

let getEligibleSessions sessions diploma =
    let diploma = makeDiploma diploma
    match diploma with
    | Error errorMessage -> Error errorMessage
    | Ok diploma ->
        filterSessionsByEligibility sessions diploma
        |> sequenceIsEmpty "No eligible sessions found."
    
let getTotalMinutes sessions : int =
    sessions
    |> Seq.map _.Minutes
    |> Seq.sum
    
let candidateHasSwumEnough totalMinutes diploma =
    let diploma = makeDiploma diploma
    match diploma with
    | Error errorMessage -> Error errorMessage
    | Ok diploma -> Ok (totalMinutes >= Diploma.totalRequired diploma)
    
let addSession (sessionStore: ISessionStore) (name: string) (session: Session) =
    let session = makeSession (name, session.Deep, session.Date, session.Minutes)
    match session with
    | Error errorMessage -> Error (errorMessage.ToString ())
    | Ok session -> sessionStore.addSession name session