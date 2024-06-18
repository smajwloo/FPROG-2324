module Model.Candidate

open System
open Thoth.Json.Net
open Validator

// Names are words separated by spaces
// GuardianId must be a valid guardian id (see below)
// Diploma is the highest diploma obtained by the candidate. It can be
// - an empty string meaning no diploma
// - or the strings "A", "B", or "C".
type Candidate =
    { Name: string
      DateOfBirth: DateTime
      GuardianId: string
      Diploma: string }


module Candidate =
    let encode: Encoder<Candidate> =
        fun candidate ->
            Encode.object
                [ "name", Encode.string candidate.Name
                  "date_of_birth", Encode.datetime candidate.DateOfBirth
                  "guardian_id", Encode.string candidate.GuardianId
                  "diploma", Encode.string candidate.Diploma ]

    let decode: Decoder<Candidate> =
        Decode.object (fun get ->
            { Name = get.Required.Field "name" Decode.string
              DateOfBirth = get.Required.Field "date_of_birth" Decode.datetime
              GuardianId = get.Required.Field "guardian_id" Decode.string
              Diploma = get.Required.Field "diploma" Decode.string })
        
    let validateName (name: string) =
        Validator.validateName name