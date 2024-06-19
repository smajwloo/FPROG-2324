module Database.CandidateStore

open Model.Diploma
open Model.Candidate
open Rommulbad.Database
open Rommulbad.Store
open Application.Candidate

type CandidateStore (store: Store) =
    interface ICandidateStore with
        member this.getCandidates () = 
            InMemoryDatabase.all store.candidates
                    
        member this.getCandidate (name: string) =
            InMemoryDatabase.lookup name store.candidates
            
        member this.addCandidate (candidate: Candidate) =
            let result = InMemoryDatabase.insert candidate.Name (candidate.Name, candidate.DateOfBirth, candidate.GuardianId, "") store.candidates
            match result with
            | Ok _ -> Ok ()
            | Error error -> Error (error.ToString ())
                
        member this.updateCandidate (candidate: Candidate) (diploma: Diploma) =
            match diploma with
            | Diploma diploma ->
                InMemoryDatabase.update candidate.Name (candidate.Name, candidate.DateOfBirth, candidate.GuardianId, diploma) store.candidates