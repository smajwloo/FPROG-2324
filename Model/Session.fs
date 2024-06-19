module Model.Session

open Thoth.Json.Net
open System
open Model.Diploma

/// Swimming session registered on a specific date
///
/// A Swimming session can be in the deep or shallow pool
/// Minutes cannot be negative or larger than 30
/// Only the year, month and date of Date are used.
type Session =
    { Deep: bool
      Date: DateTime
      Minutes: int }

module Session =
    let make rawDeep rawDate rawMinutes =
        rawMinutes
        |> Validator.validateSessionLength
        |> Result.map (fun _ -> { Deep = rawDeep; Date = rawDate; Minutes = rawMinutes })
    
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