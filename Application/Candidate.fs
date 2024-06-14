module Application.Candidate

open Model.Candidate
    
type ICandidateStore =
    abstract GetCandidates : unit -> seq<Candidate>
    abstract GetCandidate : string -> Option<Candidate>
    
let getCandidates (candidateStore: ICandidateStore) : seq<Candidate> =
    candidateStore.GetCandidates ()
    
let getCandidate (candidateStore: ICandidateStore, name: string) : Option<Candidate> =
    candidateStore.GetCandidate name