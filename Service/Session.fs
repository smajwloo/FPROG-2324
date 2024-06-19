module Service.Session

open Application
open Application.Candidate
open Application.Session
open Model.Session
open Giraffe
open Thoth.Json.Net
open Thoth.Json.Giraffe

let addSession (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let! session = ThothSerializer.ReadBody ctx Session.decode

            match session with
            | Error errorMessage -> return! RequestErrors.BAD_REQUEST errorMessage next ctx
            | Ok session ->
                let sessionStore = ctx.GetService<ISessionStore>()
                let candidateStore = ctx.GetService<ICandidateStore>()
                
                let candidate = Candidate.getCandidate candidateStore name
                match candidate with
                | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
                | Ok _ ->                
                    let result = Session.addSession sessionStore name session
                    match result with
                    | Error errorMessage -> return! RequestErrors.BAD_REQUEST errorMessage next ctx
                    | Ok _ -> return! text "Session added successfully" next ctx
        }

let getTotalEligibleMinutes (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let sessionsResult = Session.getSessionsOfCandidate sessionStore name

            match sessionsResult with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok sessions ->
                let eligibleSessionsResult = Session.getEligibleSessions sessions diploma
                
                match eligibleSessionsResult with
                | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
                | Ok eligibleSessions ->
                    let total = Session.getTotalMinutes eligibleSessions
                    return! ThothSerializer.RespondJson total Encode.int next ctx
        }

let getTotalMinutes (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let sessionsResult = Session.getSessionsOfCandidate sessionStore name
            
            match sessionsResult with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok sessions ->
                let total = Session.getTotalMinutes sessions
                return! ThothSerializer.RespondJson total Encode.int next ctx
        }

let getSessions (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let sessionsResult = Session.getSessionsOfCandidate sessionStore name
            
            match sessionsResult with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok sessions -> return! ThothSerializer.RespondJsonSeq sessions Session.encode next ctx
        }

let getEligibleSessions (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let sessionsResult = Session.getSessionsOfCandidate sessionStore name
            
            match sessionsResult with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok sessions ->
                let eligibleSessionsResult = getEligibleSessions sessions diploma
                
                match eligibleSessionsResult with
                | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
                | Ok eligibleSessions -> return! ThothSerializer.RespondJsonSeq eligibleSessions Session.encode next ctx
        }