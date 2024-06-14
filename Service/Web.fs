module Rommulbad.Web

open Application.Candidate
open Application.Session
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
            let candidateStore = ctx.GetService<ICandidateStore>()
            let candidate = getCandidate (candidateStore, name)
            
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
            | Ok session ->
                let sessionStore = ctx.GetService<ISessionStore>()

                let result = addSession sessionStore name session
                match result with
                | Error errorMessage -> return! RequestErrors.BAD_REQUEST errorMessage next ctx
                | Ok _ -> return! text "OK" next ctx
        }

let getTotalEligibleMinutes (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let eligibleSessions = getEligibleSessions sessionStore name diploma
            let total = getTotalMinutes eligibleSessions
            return! ThothSerializer.RespondJson total Encode.int next ctx
        }

let getTotalMinutes (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let sessions = getSessions sessionStore name
            let total = getTotalMinutes sessions
            return! ThothSerializer.RespondJson total Encode.int next ctx
        }

let getSessions (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let sessions = getSessions sessionStore name
            return! ThothSerializer.RespondJsonSeq sessions Session.encode next ctx
        }

let getEligibleSessions (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let eligibleSessions = getEligibleSessions sessionStore name diploma
            return! ThothSerializer.RespondJsonSeq eligibleSessions Session.encode next ctx
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
