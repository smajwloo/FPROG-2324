module Application.Candidate

open System
open Model.Diploma
open Model.Candidate
open Application.Common
    
type ICandidateStore =
    abstract getCandidates : unit -> seq<string * DateTime * string * string>
    abstract getCandidate : string -> Option<string * DateTime * string * string>
    abstract addCandidate : Candidate -> Result<unit, string>
    abstract updateCandidate : Candidate -> Diploma -> unit
    
let filterCandidateByGuardianId guardianId name candidates =
    candidates |> List.filter (fun candidate -> candidate.GuardianId = guardianId && candidate.Name = name)
    
let makeCandidate (name, dateOfBirth, guardianId, diploma) =
    Candidate.make name dateOfBirth guardianId (Diploma.make diploma)
    
let getCandidates (candidateStore: ICandidateStore) =
    let candidates = candidateStore.getCandidates ()
    candidates
    |> Seq.map makeCandidate
    |> Seq.choose convertResultToOption
    |> Seq.toList
    
let getCandidate (candidateStore: ICandidateStore) (name: string) =
    let candidate = candidateStore.getCandidate name
    match candidate with
    | None -> Error "Candidate not found."
    | Some candidate ->
        match makeCandidate candidate with
        | Error errorMessage -> Error (errorMessage.ToString ())
        | Ok candidate -> Ok candidate
   
let addCandidate (candidateStore: ICandidateStore) (candidate: Candidate) (existingCandidates: List<Candidate>) =
    let candidate = Candidate.make candidate.Name candidate.DateOfBirth candidate.GuardianId candidate.Diploma
    match candidate with
    | Error errorMessage -> Error (errorMessage.ToString())
    | Ok candidate ->
        match existingCandidates with
        | [] -> candidateStore.addCandidate candidate
        | _ -> Error "The guardian already has a candidate with that name."
            
let awardDiploma (candidateStore: ICandidateStore) candidate diploma =
    let diploma = Diploma.make diploma
    match diploma with
    | None -> Error "Invalid diploma."
    | Some diploma -> Ok (candidateStore.updateCandidate candidate diploma)