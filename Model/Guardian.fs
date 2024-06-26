module Model.Guardian

open Candidate

/// A guardian has an Id (3 digits followed by a dash and 4 letters),
/// a Name (only letters and spaces, but cannot contain two or more consecutive spaces),
/// and a list of Candidates (which may be empty)
type Guardian =
    { Id: string
      Name: string
      Candidates: List<Candidate> }

module Guardian =
    let make rawId rawName candidates =
        rawId
        |> Validator.validateId
        |> Result.bind (fun _ -> Validator.validateName rawName)
        |> Result.map (fun _ -> { Id = rawId
                                  Name = rawName
                                  Candidates = candidates })