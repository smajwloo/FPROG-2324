module Model.Session

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

module Session =
    let make rawDeep rawDate rawMinutes =
        rawMinutes
        |> Validator.validateSessionLength
        |> Result.map (fun _ -> { Deep = rawDeep
                                  Date = rawDate
                                  Minutes = rawMinutes })