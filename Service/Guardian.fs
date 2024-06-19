module Service.Guardian

open Application
open Application.Candidate
open Application.Guardian
open Model.Guardian
open Giraffe
open Thoth.Json.Giraffe
        
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
                | Ok _ -> return! text "Guardian added successfully" next ctx
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