module Database.CandidateStore

open Model.Candidate
open Rommulbad.Database
open Rommulbad.Store
open Application.Candidate

type CandidateStore (store: Store) =
    interface ICandidateStore with
        member this.GetCandidates () = 
            InMemoryDatabase.all store.candidates
                    |> Seq.map (fun (name, _, gId, dpl) ->
                        { Candidate.Name = name
                          GuardianId = gId
                          Diploma = dpl })
                    
        member this.GetCandidate (name: string) =
            InMemoryDatabase.lookup name store.candidates
                    |> Option.map (fun (name, _, gId, dpl) ->
                        { Candidate.Name = name
                          GuardianId = gId
                          Diploma = dpl })
