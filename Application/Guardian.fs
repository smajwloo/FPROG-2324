module Application.Guardian

open Model.Guardian
open Application.Common

type IGuardianStore =
    abstract member getGuardians: unit -> seq<string * string>
    abstract member getGuardian: string -> Option<string * string>
    abstract member addGuardian: Guardian -> Result<unit, string>
    
let makeGuardian (id, name) =
    Guardian.make id name
    
let getGuardians (store: IGuardianStore) =
    let guardians = store.getGuardians ()
    guardians
    |> Seq.map makeGuardian
    |> Seq.choose convertResultToOption
    |> sequenceIsEmpty "No guardians found."
    
let getGuardian (store: IGuardianStore) (id: string) =
    let guardian = store.getGuardian id
    guardian
    |> Option.map makeGuardian
    |> Option.bind convertResultToOption
     
let addGuardian (store: IGuardianStore) (guardian: Guardian) =
    let result = makeGuardian (guardian.Id, guardian.Name)
    match result with
    | Error errorMessage -> Error (errorMessage.ToString ())
    | Ok guardian -> store.addGuardian guardian