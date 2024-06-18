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

let addGuardian (store: IGuardianStore) (guardian: Guardian) =
    let validatedGuardian = Guardian.validateGuardian guardian
    match validatedGuardian with
    | Error error -> Error error
    | Ok _ -> store.addGuardian guardian