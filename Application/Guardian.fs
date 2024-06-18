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
    
let guardianExists (guardian: Option<Guardian>) : Result<unit, string> =
    match guardian with
    | None -> Error "Guardian not found!"
    | Some _ -> Ok ()
    
let mapGuardians guardians =
    guardians
    |> Seq.map (fun (id, name) -> Guardian.make id name)
    |> Seq.choose (function
        | Ok guardian -> Some guardian
        | Error _ -> None
    )
    
let getGuardians (store: IGuardianStore) : Result<seq<Guardian>, string> =
    let guardians = store.getGuardians ()
    let mappedGuardians = mapGuardians guardians
    guardiansIsEmpty mappedGuardians
    
let getGuardian (store: IGuardianStore) (id: string) : Option<Guardian> =
    let guardian = store.getGuardian id
    guardian
     |> Option.map (fun (id, name) -> Guardian.make id name)
     |> Option.bind (fun guardian ->
                     match guardian with
                     | Ok guardian -> Some guardian
                     | Error _ -> None
                    )
     
let addGuardian (store: IGuardianStore) (guardian: Guardian) =
    let result = Guardian.make guardian.Id guardian.Name
    match result with
    | Error errorMessage -> Error errorMessage
    | Ok guardian -> store.addGuardian guardian