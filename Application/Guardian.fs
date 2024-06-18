module Application.Guardian

open Model.Guardian

type IGuardianStore =
    abstract member getGuardians: unit -> seq<string * string>
    abstract member addGuardian: Guardian -> Result<unit, string>

let guardiansIsEmpty guardians =
    match Seq.isEmpty guardians with
    | true -> Error "No guardians found!"
    | false -> Ok guardians
    
let mapGuardians guardians =
    guardians
    |> Seq.map (fun (id, name) ->
        { Guardian.Id = id
          Name = name
          Candidates = list.Empty })
    
let getGuardians (store: IGuardianStore) : Result<seq<Guardian>, string> =
    let guardians = store.getGuardians ()
    let mappedGuardians = guardians
                         |> mapGuardians
    guardiansIsEmpty mappedGuardians
    
let validateGuardian (guardian: Guardian) =
    let idValidation = Guardian.validateGuardianId guardian.Id
    let nameValidation = Guardian.validateGuardianName guardian.Name
    match idValidation, nameValidation with
    | Ok (), Ok () -> Ok ()
    | Error idError, Error nameError -> Error (idError + " and " + nameError)
    | Error error, _ -> Error error
    | _, Error error -> Error error

let addGuardian (store: IGuardianStore) (guardian: Guardian) =
    let validatedGuardian = validateGuardian guardian
    match validatedGuardian with
    | Error error -> Error error
    | Ok _ -> store.addGuardian guardian