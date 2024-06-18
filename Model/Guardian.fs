module Model.Guardian

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
    let make (rawId: string) (rawName: string) =
        rawId
        |> Validator.validateId
        |> Result.bind (fun _ -> Validator.validateName rawName)
        |> Result.map (fun _ -> { Id = rawId; Name = rawName; Candidates = List.empty }) //TODO: GetCandidates from store
    
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