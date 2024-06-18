module Database.SessionStore

open Application.Session
open Model.Session
open Rommulbad.Database
open Rommulbad.Store

type SessionStore (store: Store) =
    interface ISessionStore with
        member this.getSessions () =
            InMemoryDatabase.all store.sessions
                    
        member this.addSession (name: string) (session: Session) =
            let result = InMemoryDatabase.insert (name, session.Date) (name, session.Deep, session.Date, session.Minutes) store.sessions
            match result with
            | Ok _ -> Ok ()
            | Error error -> Error (error.ToString ())