module Application.Common

let sequenceIsEmpty errorMessage sequence =
    match Seq.isEmpty sequence with
    | true -> Error errorMessage
    | false -> Ok sequence

// Ignore invalid objects, including errors, as they should not be able to exist, and so they are not relevant to the user.
let convertResultToOption object =
    match object with
    | Ok object -> Some object
    | Error _ -> None