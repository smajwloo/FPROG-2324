module Rommulbad.Web

open Application
open Application.Candidate
open Application.Session
open Model.Candidate
open Model.Session
open Model.Guardian
open Giraffe
open Thoth.Json.Net
open Thoth.Json.Giraffe


let getCandidates: HttpHandler =
    fun next ctx ->
        task {
            let candidateStore = ctx.GetService<ICandidateStore>()
            let candidates = Candidate.getCandidates candidateStore
            return! ThothSerializer.RespondJsonSeq candidates Candidate.encode next ctx
        }

let getCandidate (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let candidateStore = ctx.GetService<ICandidateStore>()
            let candidate = Candidate.getCandidate (candidateStore, name)
            
            match candidate with
            | None -> return! RequestErrors.NOT_FOUND "Candidate not found!" next ctx
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
            let sessionsResult = Session.getSessions sessionStore name

            match sessionsResult with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok sessions ->
                let eligibleSessions = getEligibleSessions sessions diploma
                let total = getTotalMinutes eligibleSessions
                return! ThothSerializer.RespondJson total Encode.int next ctx
        }

let getTotalMinutes (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let sessionsResult = Session.getSessions sessionStore name
            
            match sessionsResult with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok sessions ->
                let total = getTotalMinutes sessions
                return! ThothSerializer.RespondJson total Encode.int next ctx
        }

let getSessions (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let sessionsResult = Session.getSessions sessionStore name
            
            match sessionsResult with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok sessions -> return! ThothSerializer.RespondJsonSeq sessions Session.encode next ctx
        }

let getEligibleSessions (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let sessionsResult = Session.getSessions sessionStore name
            
            match sessionsResult with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok sessions ->
                let eligibleSessions = getEligibleSessions sessions diploma
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
