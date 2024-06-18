module Model.Guardian

open Thoth.Json.Net
open Candidate
open Validator

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
        Validator.validateId id
        
    let validateGuardianName (name: string) =
        Validator.validateName name