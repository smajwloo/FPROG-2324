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
    let make rawDiploma =
        match rawDiploma with
        | "A" -> A
        | "B" -> B
        | "C" -> C
        | _ -> NoDiploma
        
    let decode: Decoder<Diploma> =
        Decode.string
        |> Decode.andThen (fun diploma -> Decode.succeed (make diploma))