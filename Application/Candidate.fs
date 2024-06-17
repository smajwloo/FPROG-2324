module Application.Candidate

open System
open Model.Candidate
    
type ICandidateStore =
    abstract GetCandidates : unit -> seq<string * DateTime * string * string>
    abstract GetCandidate : string -> Option<string * DateTime * string * string>
    
let mapCandidate name gId dpl : Candidate =
    { Candidate.Name = name;
          GuardianId = gId;
          Diploma = dpl }

    
let getCandidates (candidateStore: ICandidateStore) : List<Candidate> =
    let candidates = candidateStore.GetCandidates ()
    candidates
    |> Seq.map (fun (name, _, gId, dpl) -> mapCandidate name gId dpl)
    |> List.ofSeq
    
    
let getCandidate (candidateStore: ICandidateStore, name: string) : Option<Candidate> =
    let candidate = candidateStore.GetCandidate name
    candidate
    |> Option.map (fun (name, _, gId, dpl) -> mapCandidate name gId dpl)
    
    
    