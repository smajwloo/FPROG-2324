module Application.Candidate

open System
open Model.Candidate
    
type ICandidateStore =
    abstract getCandidates : unit -> seq<string * DateTime * string * string>
    abstract getCandidate : string -> Option<string * DateTime * string * string>
    abstract addCandidate : Candidate -> Result<unit, string>
    
let filterCandidateByGuardianId (guardianId: string) (name: string) (candidates: List<Candidate>) : List<Candidate> =
    candidates
    |> List.filter (fun candidate -> candidate.GuardianId = guardianId && candidate.Name = name)
    
let getCandidates (candidateStore: ICandidateStore) : List<Candidate> =
    let candidates = candidateStore.getCandidates ()
    candidates
    |> Seq.map (fun (name, dateOfBirth, guardianId, diploma) -> Candidate.make name dateOfBirth guardianId diploma)
    |> List.ofSeq
    
    
let getCandidate (candidateStore: ICandidateStore) (name: string) : Option<Candidate> =
    let candidate = candidateStore.getCandidate name
    candidate
    |> Option.map (fun (name, dateOfBirth, guardianId, diploma) -> Candidate.make name dateOfBirth guardianId diploma)
    
let validateCandidate (candidate: Candidate) =
    Candidate.validateName candidate.Name
    
let addCandidate (candidateStore: ICandidateStore) (candidate: Candidate) : Result<unit, string> =
    let existingCandidates = getCandidates candidateStore
    match existingCandidates with
    | [] -> candidateStore.addCandidate candidate
    | candidates ->
        let filteredCandidates = filterCandidateByGuardianId candidate.GuardianId candidate.Name candidates
        match filteredCandidates with
        | [] -> candidateStore.addCandidate candidate
        | _ -> Error "The guardian already has a candidate with that name."
    
    