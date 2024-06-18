module Application.Guardian

open Model.Guardian
open Model.Candidate

type IGuardianStore =
    abstract member getGuardians: unit -> seq<string * string>
    abstract member getGuardian: string -> Option<string * string>
    abstract member addGuardian: Guardian -> Result<unit, string>

let guardiansIsEmpty guardians =
    match Seq.isEmpty guardians with
    | true -> Error "No guardians found!"
    | false -> Ok guardians
    
let mapGuardians id name =
    { Guardian.Id = id;
          Name = name;
          Candidates = list.Empty }
    
let guardianExists (guardian: Option<Guardian>) : Result<unit, string> =
    match guardian with
    | None -> Error "Guardian not found!"
    | Some _ -> Ok ()
    
let getGuardians (store: IGuardianStore) : Result<seq<Guardian>, string> =
    let guardians = store.getGuardians ()
    let mappedGuardians = guardians
                         |> Seq.map (fun (id, name) -> mapGuardians id name)
    guardiansIsEmpty mappedGuardians
    
let getGuardian (store: IGuardianStore) (id: string) : Option<Guardian> =
    let guardian = store.getGuardian id
    guardian
    |> Option.map (fun (id, name) -> mapGuardians id name)
    
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