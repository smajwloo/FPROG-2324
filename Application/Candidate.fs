module Application.Candidate

open Model.Candidate
    
type ICandidateStore =
    abstract GetCandidates : unit -> seq<Candidate>
    
let getCandidates (candidateStore: ICandidateStore) : seq<Candidate> =
    candidateStore.GetCandidates ()