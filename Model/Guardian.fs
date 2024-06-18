module Model.Guardian

open System.Text.RegularExpressions
open Thoth.Json.Net
open Candidate

/// A guardian has an Id (3 digits followed by a dash and 4 letters),
/// a Name (only letters and spaces, but cannot contain two or more consecutive spaces),
/// and a list of Candidates (which may be empty)
type Guardian =
    { Id: string
      Name: string
      Candidates: List<Candidate> }

module Guardian =
    let encode: Encoder<Guardian> =
        fun guardian ->
            Encode.object
                [ "Id", Encode.string guardian.Id
                  "Name", Encode.string guardian.Name ]

    let decode: Decoder<Guardian> =
        Decode.object (fun get ->
            { Id = get.Required.Field "Id" Decode.string
              Name = get.Required.Field "Name" Decode.string
              Candidates = list.Empty }) //TODO: GetCandidates from store
        
    let validateGuardianId (id: string) =
        let isValid = Regex.IsMatch(id, "^\d{3}-[A-Z]{4}$")
        match isValid with
        | false -> Error "Invalid Guardian ID"
        | true -> Ok ()
        
    let validateGuardianName (name: string) =
        let isValid = Regex.IsMatch(name, "^(?:[A-Za-z0-9]+ ?)+[A-Za-z0-9]+$")
        match isValid with
        | false -> Error "Invalid Guardian Name"
        | true -> Ok ()
        
    let validateGuardian (guardian: Guardian) =
        let idValidation = validateGuardianId guardian.Id
        let nameValidation = validateGuardianName guardian.Name
        match idValidation, nameValidation with
        | Ok (), Ok () -> Ok ()
        | Error idError, Error nameError -> Error (idError + " and " + nameError)
        | Error error, _ -> Error error
        | _, Error error -> Error error