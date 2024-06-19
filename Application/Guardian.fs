module Application.Guardian

open Model.Guardian
open Model.Candidate
open Application.Common

type IGuardianStore =
    abstract member getGuardians: unit -> seq<string * string>
    abstract member getGuardian: string -> Option<string * string>
    abstract member addGuardian: Guardian -> Result<unit, string>
    
let filterGuardiansCandidates id candidates =
    candidates
    |> List.filter (fun candidate -> candidate.GuardianId = id)
    
let makeGuardian (id, name, candidates) =
    Guardian.make id name candidates
    
let getGuardians (store: IGuardianStore) (candidates: List<Candidate>) =
    let guardians = store.getGuardians ()
    guardians
    |> Seq.map (fun (id, name) -> makeGuardian (id, name, (filterGuardiansCandidates id candidates)))
    |> Seq.choose convertResultToOption
    |> sequenceIsEmpty "No guardians found."
    
let getGuardian (store: IGuardianStore) (id: string) (candidates: List<Candidate>) =
    let guardian = store.getGuardian id
    guardian
    |> Option.map (fun (id, name) -> makeGuardian (id, name, (filterGuardiansCandidates id candidates)))
    |> Option.bind convertResultToOption
     
let addGuardian (store: IGuardianStore) (guardian: Guardian) = //TODO: Check
    let result = makeGuardian (guardian.Id, guardian.Name, list.Empty)
    match result with
    | Error errorMessage -> Error (errorMessage.ToString ())
    | Ok guardian -> store.addGuardian guardian