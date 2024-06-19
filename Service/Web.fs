module Rommulbad.Web

open Service.Session
open Service.Candidate
open Service.Guardian
open Giraffe

let routes: HttpHandler =
    choose
        [ GET >=> route "/candidate" >=> getCandidates
          GET >=> routef "/candidate/%s" getCandidate
          POST >=> route "/candidate" >=> addCandidate
          GET >=> routef "/candidate/%s/session" getSessions
          POST >=> routef "/candidate/%s/session" addSession
          GET >=> routef "/candidate/%s/session/total" getTotalMinutes
          GET >=> routef "/candidate/%s/session/%s" getEligibleSessions
          GET >=> routef "/candidate/%s/session/%s/total" getTotalEligibleMinutes
          POST >=> routef "/candidate/%s/diploma/%s" awardDiploma
          GET >=> routef "/candidate/diploma/%s" getCandidatesQualifyingForDiploma
          GET >=> route "/guardian" >=> getGuardians
          POST >=> route "/guardian" >=> addGuardian ]
