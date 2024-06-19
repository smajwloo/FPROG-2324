module Rommulbad.Web

open Application
open Application.Candidate
open Application.Session
open Application.Guardian
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
            let candidate = Candidate.getCandidate candidateStore name
            
            match candidate with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok candidate -> return! ThothSerializer.RespondJson candidate Candidate.encode next ctx
        }
        
let addCandidate: HttpHandler =
    fun next ctx ->
        task {
            let! candidate = ThothSerializer.ReadBody ctx Candidate.decode
            
            match candidate with
            | Error errorMessage -> return! RequestErrors.BAD_REQUEST errorMessage next ctx
            | Ok candidate ->
                let candidateStore = ctx.GetService<ICandidateStore>()
                let guardianStore = ctx.GetService<IGuardianStore>()
            
                let candidates = Candidate.getCandidates candidateStore
                let guardian = Guardian.getGuardian guardianStore candidate.GuardianId candidates
                
                match guardian with
                | None -> return! RequestErrors.BAD_REQUEST "Guardian does not exist." next ctx
                | Some _ ->
                    let result = Candidate.addCandidate candidateStore candidate
                    match result with
                    | Error errorMessage -> return! RequestErrors.BAD_REQUEST errorMessage next ctx
                    | Ok _ -> return! text "OK" next ctx
        }

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
            let sessionsResult = Session.getSessions sessionStore name
            
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
                let eligibleSessionsResult = getEligibleSessions sessions diploma
                
                match eligibleSessionsResult with
                | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
                | Ok eligibleSessions -> return! ThothSerializer.RespondJsonSeq eligibleSessions Session.encode next ctx
        }
        
let addGuardian: HttpHandler =
    fun next ctx ->
        task {
            let! guardian = ThothSerializer.ReadBody ctx Guardian.decode

            match guardian with
            | Error errorMessage -> return! RequestErrors.BAD_REQUEST errorMessage next ctx
            | Ok guardian ->
                let guardianStore = ctx.GetService<IGuardianStore>()

                let result = Guardian.addGuardian guardianStore guardian
                match result with
                | Error errorMessage -> return! RequestErrors.BAD_REQUEST errorMessage next ctx
                | Ok _ -> return! text "OK" next ctx
        }
        
let getGuardians: HttpHandler =
    fun next ctx ->
        task {
            let guardianStore = ctx.GetService<IGuardianStore>()
            let candidateStore = ctx.GetService<ICandidateStore>()
            
            let candidates = Candidate.getCandidates candidateStore
            let guardiansResult = Guardian.getGuardians guardianStore candidates
            
            match guardiansResult with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok guardians -> return! ThothSerializer.RespondJsonSeq guardians Guardian.encode next ctx
        }
        
let awardDiploma (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let candidateStore = ctx.GetService<ICandidateStore>()
            let candidate = Candidate.getCandidate candidateStore name
            
            match candidate with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok candidate ->
                let sessionsResult = Session.getSessions sessionStore name
                
                match sessionsResult with
                | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
                | Ok sessions ->
                    let eligibleSessions = Session.getEligibleSessions sessions diploma
                    
                    match eligibleSessions with
                    | Error _ -> return! RequestErrors.BAD_REQUEST "The candidate is not eligible for that diploma." next ctx
                    | Ok _ ->
                        Candidate.awardDiploma candidateStore candidate diploma
                        return! text "OK" next ctx
        }


let routes: HttpHandler =
    choose
        [ GET >=> route "/candidate" >=> getCandidates
          GET >=> routef "/candidate/%s" getCandidate
          POST >=> route "/candidate" >=> addCandidate
          POST >=> routef "/candidate/%s/session" addSession
          GET >=> routef "/candidate/%s/session" getSessions
          GET >=> routef "/candidate/%s/session/total" getTotalMinutes
          GET >=> routef "/candidate/%s/session/%s" getEligibleSessions
          GET >=> routef "/candidate/%s/session/%s/total" getTotalEligibleMinutes
          POST >=> routef "/candidate/%s/diploma/%s" awardDiploma
          POST >=> route "/guardian" >=> addGuardian
          GET >=> route "/guardian" >=> getGuardians ]
