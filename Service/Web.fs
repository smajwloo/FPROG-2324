module Rommulbad.Web

open Application.Candidate
open Model.Candidate
open Model.Session
open Model.Guardian
open Rommulbad.Database
open Rommulbad.Store
open Giraffe
open Thoth.Json.Net
open Thoth.Json.Giraffe


let getCandidates: HttpHandler =
    fun next ctx ->
        task {
            let candidateStore = ctx.GetService<ICandidateStore>()
            let candidates = getCandidates candidateStore
            return! ThothSerializer.RespondJsonSeq candidates Candidate.encode next ctx
        }

let getCandidate (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let store = ctx.GetService<ICandidateStore>()
            let candidate = getCandidate (store, name)
            
            match candidate with
            | None -> return! RequestErrors.NOT_FOUND "Employee not found!" next ctx
            | Some candidate -> return! ThothSerializer.RespondJson candidate Candidate.encode next ctx
        }

let addSession (name: string) : HttpHandler =
    fun next ctx ->
        task {

            let! session = ThothSerializer.ReadBody ctx Session.decode

            match session with
            | Error errorMessage -> return! RequestErrors.BAD_REQUEST errorMessage next ctx
            | Ok { Deep = deep
                   Date = date
                   Minutes = minutes } ->
                let store = ctx.GetService<Store>()

                InMemoryDatabase.insert (name, date) (name, deep, date, minutes) store.sessions
                |> ignore


                return! text "OK" next ctx
        }

let encodeSession (_, deep, date, minutes) =
    Encode.object
        [ "date", Encode.datetime date
          "deep", Encode.bool deep
          "minutes", Encode.int minutes ]


let getSessions (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let store = ctx.GetService<Store>()

            let sessions = InMemoryDatabase.filter (fun (n, _, _, _) -> n = name) store.sessions

            return! ThothSerializer.RespondJsonSeq sessions encodeSession next ctx
        }

let getTotalMinutes (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let store = ctx.GetService<Store>()

            let total =
                InMemoryDatabase.filter (fun (n, _, _, _) -> n = name) store.sessions
                |> Seq.map (fun (_, _, _, a) -> a)
                |> Seq.sum

            return! ThothSerializer.RespondJson total Encode.int next ctx
        }


let getEligibleSessions (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {

            let store = ctx.GetService<Store>()

            let shallowOk =
                match diploma with
                | "A" -> true
                | _ -> false

            let minMinutes =
                match diploma with
                | "A" -> 1
                | "B" -> 10
                | _ -> 15

            let filter (n, d, _, a) = (d || shallowOk) && (a >= minMinutes)


            let sessions = InMemoryDatabase.filter filter store.sessions

            return! ThothSerializer.RespondJsonSeq sessions encodeSession next ctx

        }

let getTotalEligibleMinutes (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let store = ctx.GetService<Store>()

            let shallowOk =
                match diploma with
                | "A" -> true
                | _ -> false

            let minMinutes =
                match diploma with
                | "A" -> 1
                | "B" -> 10
                | _ -> 15

            let filter (n, d, _, a) = (d || shallowOk) && (a >= minMinutes)


            let total =
                InMemoryDatabase.filter filter store.sessions
                |> Seq.map (fun (_, _, _, a) -> a)
                |> Seq.sum

            return! ThothSerializer.RespondJson total Encode.int next ctx
        }



let routes: HttpHandler =
    choose
        [ GET >=> route "/candidate" >=> getCandidates
          GET >=> routef "/candidate/%s" getCandidate
          POST >=> routef "/candidate/%s/session" addSession
          GET >=> routef "/candidate/%s/session" getSessions
          GET >=> routef "/candidate/%s/session/total" getTotalMinutes
          GET >=> routef "/candidate/%s/session/%s" getEligibleSessions
          GET >=> routef "/candidate/%s/session/%s/total" getTotalEligibleMinutes ]
