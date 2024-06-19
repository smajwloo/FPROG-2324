module Model.Diploma

open Thoth.Json.Net

type Diploma = 
    | A
    | B
    | C
    | NoDiploma
    
let (|Diploma|) = function
    | A -> "A"
    | B -> "B"
    | C -> "C"
    | NoDiploma -> ""

module Diploma =
    let make (rawDiploma: string) =
        match rawDiploma.ToUpper() with
        | "A" -> A
        | "B" -> B
        | "C" -> C
        | _ -> NoDiploma
        
    let encode: Encoder<Diploma> =
        (fun (Diploma diploma) -> Encode.string diploma)
       
    let decode: Decoder<Diploma> =
        Decode.string
        |> Decode.andThen (fun diploma -> Decode.succeed (make diploma))
        
    let shallowOk (diploma: Diploma) =
        match diploma with
        | A -> true
        | _ -> false
        
    let minMinutes (diploma: Diploma) =
        match diploma with
        | A -> 1
        | B -> 10
        | _ -> 15