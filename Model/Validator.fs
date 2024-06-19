module Model.Validator

open System.Text.RegularExpressions

type ValidationError =
    | InvalidId
    | InvalidName
    | InvalidSessionLength

let validate (s: string) (expression: string) =
    Regex.IsMatch(s, expression)
    
let validateId id =
    let isValid = validate id "^\d{3}-[A-Z]{4}$"
    match isValid with
    | false -> Error InvalidId
    | true -> Ok id

let validateName name =
    let isValid = validate name "^(?:[A-Za-z0-9]+ ?)+[A-Za-z0-9]+$"
    match isValid with
    | false -> Error InvalidName
    | true -> Ok name
        
let validateSessionLength minutes =
    if minutes < 0 || minutes > 30 then Error InvalidSessionLength else Ok minutes