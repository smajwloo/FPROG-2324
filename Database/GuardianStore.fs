module Database.GuardianStore

open Rommulbad.Store
open Application.Guardian
open Rommulbad.Database

type GuardianStore (store: Store) =
    interface IGuardianStore with
        member this.getGuardians () =
            InMemoryDatabase.all store.guardians
        
        member this.addGuardian guardian =
            let result = InMemoryDatabase.insert guardian.Id (guardian.Id, guardian.Name) store.guardians
            match result with
            | Ok _ -> Ok ()
            | Error error -> Error (error.ToString ())