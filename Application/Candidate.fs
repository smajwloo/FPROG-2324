module Application.Candidate

open System
open Model.Diploma
open Model.Candidate
    
type ICandidateStore =
    abstract getCandidates : unit -> seq<string * DateTime * string * string>
    abstract getCandidate : string -> Option<string * DateTime * string * string>
    abstract addCandidate : Candidate -> Result<unit, string>
    abstract updateCandidate : Candidate -> Diploma -> unit
    
let filterCandidateByGuardianId (guardianId: string) (name: string) (candidates: List<Candidate>) : List<Candidate> =
    candidates
    |> List.filter (fun candidate -> candidate.GuardianId = guardianId && candidate.Name = name)
    
let mapCandidates candidates =
    candidates
    |> Seq.map (fun (name, dateOfBirth, guardianId, diploma) -> Candidate.make name dateOfBirth guardianId (Diploma.make diploma))
    |> Seq.choose (function
        | Ok session -> Some session
        | Error _ -> None
    )
    |> Seq.toList
    
let getCandidates (candidateStore: ICandidateStore) : List<Candidate> =
    let candidates = candidateStore.getCandidates ()
    let mappedCandidates = mapCandidates candidates
    mappedCandidates
    
    
let getCandidate (candidateStore: ICandidateStore) (name: string) : Option<Candidate> =
    let candidate = candidateStore.getCandidate name
    candidate
    |> Option.map (fun (name, dateOfBirth, guardianId, diploma) -> Candidate.make name dateOfBirth guardianId (Diploma.make diploma))
    |> Option.bind (fun candidate ->
                     match candidate with
                     | Ok candidate -> Some candidate
                     | Error _ -> None
                    )
    
let addCandidate (candidateStore: ICandidateStore) (candidate: Candidate) : Result<unit, string> =
    let candidate = Candidate.make candidate.Name candidate.DateOfBirth candidate.GuardianId candidate.Diploma
    match candidate with
    | Error errorMessage -> Error errorMessage
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