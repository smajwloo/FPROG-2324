module Model.Validator

open System.Text.RegularExpressions

let validate (s: string) (expression: string) =
    Regex.IsMatch(s, expression)
    
let validateId (name: string) =
        let isValid = validate name "^\d{3}-[A-Z]{4}$"
        match isValid with
        | false -> Error "Invalid Id"
        | true -> Ok ()

let validateName (name: string) =
        let isValid = validate name "^(?:[A-Za-z0-9]+ ?)+[A-Za-z0-9]+$"
        match isValid with
        | false -> Error "Invalid Name"
        | true -> Ok ()