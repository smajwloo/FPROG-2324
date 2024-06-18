module Model.Validator

open System
open System.Text.RegularExpressions

let validate (s: string) (expression: string) =
    Regex.IsMatch(s, expression)
    
let validateId name =
    let isValid = validate name "^\d{3}-[A-Z]{4}$"
    match isValid with
    | false -> Error "The Id has to start with 3 digits, followed by a dash, and end with 4 capital letters."
    | true -> Ok ()

let validateName name =
    let isValid = validate name "^(?:[A-Za-z0-9]+ ?)+[A-Za-z0-9]+$"
    match isValid with
    | false -> Error "The Name can only contain words separated by a single space, while not ending with a space."
    | true -> Ok ()
        
let validateSessionLength minutes =
    if minutes < 0 || minutes > 30 then Error "The session length has to be between 0 and 30 minutes." else Ok ()
    
let validateDate date =
    if date < DateTime.Now then Error "The date has to be in the future." else Ok ()