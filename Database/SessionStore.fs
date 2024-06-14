module Database.SessionStore

open Application.Session
open Model.Session
open Rommulbad.Database
open Rommulbad.Store

type SessionStore (store: Store) =
    interface ISessionStore with
        member this.getSessions (name: string) = 
            InMemoryDatabase.all store.sessions
                    |> Seq.filter (fun (n, _, _, _) -> n = name)
                    |> Seq.map (fun (_, deep, date, minutes) ->
                        { Session.Deep = deep
                          Date = date
                          Minutes = minutes })
                    
        member this.addSession (name: string) (session: Session) =
            let result = InMemoryDatabase.insert (name, session.Date) (name, session.Deep, session.Date, session.Minutes) store.sessions
            match result with
            | Ok _ -> Ok ()
            | Error _ -> Error "Could not add session to store"