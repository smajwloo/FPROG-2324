module Model.Diploma

open Thoth.Json.Net

type Diploma = 
    private | A
    | B
    | C
    
let (|Diploma|) = function
    | A -> "A"
    | B -> "B"
    | C -> "C"

module Diploma =
    let make (rawDiploma: string) =
        match rawDiploma.ToUpper() with
        | "A" -> Some A
        | "B" -> Some B
        | "C" -> Some C
        | _ -> None
        
    let encode: Encoder<Diploma> =
        (fun (Diploma diploma) -> Encode.string diploma)
       
    let decode: Decoder<Diploma> =
        Decode.string
        |> Decode.andThen (fun diploma ->
            match diploma with
            | "A" -> Decode.succeed A
            | "B" -> Decode.succeed B
            | "C" -> Decode.succeed C
            | _ -> Decode.fail "Invalid diploma"
        )
        
    let shallowOk (diploma: Diploma) =
        match diploma with
        | A -> true
        | _ -> false
        
    let minMinutes (diploma: Diploma) =
        match diploma with
        | A -> 1
        | B -> 10
        | C -> 15
        
    let totalRequired (diploma: Diploma) =
        match diploma with
        | A -> 120
        | B -> 150
        | C -> 180