module Model.Candidate

open System
open Model.Diploma

// Names are words separated by spaces
// GuardianId must be a valid guardian id (see below)
// Diploma is the highest diploma obtained by the candidate. It can be
// - an empty string meaning no diploma
// - or the strings "A", "B", or "C".
type Candidate =
    { Name: string
      DateOfBirth: DateTime
      GuardianId: string
      Diploma: Option<Diploma> }


module Candidate =
    let make rawName rawDateOfBirth rawGuardianId rawDiploma =
        rawName
        |> Validator.validateName
        |> Result.map (fun _ -> { Name = rawName
                                  DateOfBirth = rawDateOfBirth
                                  GuardianId = rawGuardianId
                                  Diploma = rawDiploma })