module Model.Validator

open System.Text.RegularExpressions

let validate (s: string) (expression: string) =
    Regex.IsMatch(s, expression)
    
let validateId (name: string) =
        let isValid = validate name "^\d{3}-[A-Z]{4}$"
        match isValid with
        | false -> Error "The Id has to start with 3 digits, followed by a dash, and end with 4 capital letters."
        | true -> Ok ()

let validateName (name: string) =
        let isValid = validate name "^(?:[A-Za-z0-9]+ ?)+[A-Za-z0-9]+$"
        match isValid with
        | false -> Error "The Name can only contain words separated by a single space, while not ending with a space."
        | true -> Ok ()