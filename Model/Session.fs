module Model.Session

open Thoth.Json.Net
open System

/// Swimming session registered on a specific date
///
/// A Swimming session can be in the deep or shallow pool
/// Minutes cannot be negative or larger than 30
/// Only the year, month and date of Date are used.
type Session =
    { Deep: bool
      Date: DateTime
      Minutes: int }
    
type Diploma = 
    private | A
    | B
    | C


module Diploma =
    let make rawDiploma =
        match rawDiploma with
        | "A" -> A
        | "B" -> B
        | _ -> C

module Session =
    let encode: Encoder<Session> =
        fun session ->
            Encode.object
                [ "deep", Encode.bool session.Deep
                  "date", Encode.datetime session.Date
                  "amount", Encode.int session.Minutes ]

    let decode: Decoder<Session> =
        Decode.object (fun get ->
            { Deep = get.Required.Field "deep" Decode.bool
              Date = get.Required.Field "date" Decode.datetime
              Minutes = get.Required.Field "amount" Decode.int })
        
    let shallowOk (diploma: Diploma) =
        match diploma with
        | A -> true
        | _ -> false
        
    let minMinutes (diploma: Diploma) =
        match diploma with
        | A -> 1
        | B -> 10
        | _ -> 15
    
    let eligibleSessions (sessions: seq<Session>) (diploma: Diploma) =
        sessions
        |> Seq.filter (fun session -> session.Deep || shallowOk diploma)
        |> Seq.filter (fun session -> session.Minutes >= minMinutes diploma)