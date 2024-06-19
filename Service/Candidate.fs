module Service.Candidate

open Application
open Application.Candidate
open Application.Guardian
open Application.Session
open Model.Candidate
open Giraffe
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
            
                let existingCandidates = Candidate.getCandidates candidateStore
                let guardian = Guardian.getGuardian guardianStore candidate.GuardianId existingCandidates
                
                match guardian with
                | None -> return! RequestErrors.BAD_REQUEST "Guardian does not exist." next ctx
                | Some _ ->
                    let filteredCandidates = Candidate.filterCandidateByGuardianId candidate.GuardianId candidate.Name existingCandidates
                    let result = Candidate.addCandidate candidateStore candidate filteredCandidates
                    match result with
                    | Error errorMessage -> return! RequestErrors.BAD_REQUEST errorMessage next ctx
                    | Ok _ -> return! text "Candidate added successfully" next ctx
        }
        
let getEligibleSessionsOfCandidate (sessionStore: ISessionStore) (candidate: Candidate) (diploma: string) =
    let sessionsResult = Session.getSessionsOfCandidate sessionStore candidate.Name
    
    match sessionsResult with
    | Error _ -> None
    | Ok sessions ->
        let eligibleSessions = Session.getEligibleSessions sessions diploma
        
        match eligibleSessions with
        | Error _ -> None
        | Ok _ -> Some candidate
                
let getQualifyingCandidates (sessionStore: ISessionStore) (candidates: List<Candidate>) (diploma: string) =
    candidates
    |> List.choose (fun candidate -> getEligibleSessionsOfCandidate sessionStore candidate diploma)
        
let awardDiploma (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let candidateStore = ctx.GetService<ICandidateStore>()
            let candidate = Candidate.getCandidate candidateStore name
            
            match candidate with
            | Error errorMessage -> return! RequestErrors.NOT_FOUND errorMessage next ctx
            | Ok candidate ->
                let eligibleSessionsOfCandidate = getEligibleSessionsOfCandidate sessionStore candidate diploma
                
                match eligibleSessionsOfCandidate with
                | None -> return! RequestErrors.BAD_REQUEST "The candidate is not eligible for that diploma." next ctx
                | _ ->
                    Candidate.awardDiploma candidateStore candidate diploma
                    return! text "Diploma awarded successfully" next ctx
        }
        
let getCandidatesQualifyingForDiploma (diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionStore = ctx.GetService<ISessionStore>()
            let candidateStore = ctx.GetService<ICandidateStore>()
            let candidates = Candidate.getCandidates candidateStore
            
            match candidates with
            | [] -> return! RequestErrors.NOT_FOUND "No candidates found." next ctx
            | candidates ->
                let qualifyingCandidates = getQualifyingCandidates sessionStore candidates diploma
                match qualifyingCandidates with
                | [] -> return! RequestErrors.NOT_FOUND "No candidates qualify for that diploma." next ctx
                | qualifyingCandidates ->
                    return! ThothSerializer.RespondJsonSeq qualifyingCandidates Candidate.encode next ctx
        }