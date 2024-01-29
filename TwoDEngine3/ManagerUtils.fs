module ManagerUtils

let TryGetManager<'a> () =
    let manager = ManagerRegistry.getManager<'a>()
    match manager with
    | Some m -> m
    | None -> failwith "Manager not found"