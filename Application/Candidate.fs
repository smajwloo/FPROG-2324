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
    candidates
    |> List.filter (fun candidate -> candidate.GuardianId = guardianId && candidate.Name = name)
    
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
    candidate
    |> Option.map makeCandidate
    |> Option.bind convertResultToOption
    
let addCandidate (candidateStore: ICandidateStore) (candidate: Candidate) =
    let candidate = Candidate.make candidate.Name candidate.DateOfBirth candidate.GuardianId candidate.Diploma
    match candidate with
    | Error errorMessage -> Error (errorMessage.ToString())
    | Ok candidate ->
        let existingCandidates = getCandidates candidateStore
        match existingCandidates with
        | [] -> candidateStore.addCandidate candidate
        | candidates ->
            let filteredCandidates = filterCandidateByGuardianId candidate.GuardianId candidate.Name candidates
            match filteredCandidates with
            | [] -> candidateStore.addCandidate candidate
            | _ -> Error "The guardian already has a candidate with that name."
            
let awardDiploma candidateStore name diploma =
    let diploma = Diploma.make diploma
    let candidate = getCandidate candidateStore name
    match candidate with
    | None -> Error "Candidate not found!"
    | Some candidate ->
        candidateStore.updateCandidate candidate diploma
        Ok ()