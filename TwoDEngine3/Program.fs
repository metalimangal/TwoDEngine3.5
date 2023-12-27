module TwoDEngine3
open System
open AngelCodeTextRenderer
open InputManagerWinRawInput
open GraphicsManagerSFML
open TwoDEngine3.AsteroidsLevel
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface




let mutable currentLevelController: AbstractLevelController option = None

let SetLevelManager newLevelManger : unit =
    match currentLevelController with
    | Some oldLevelMgr ->
        oldLevelMgr.Close()
        ()
    | None -> ()

    currentLevelController <- newLevelManger

    match currentLevelController with
    | Some mgr -> mgr.Open()
    | None -> ()

let Update deltaMS =
    printfn $"Update deltams=%d{deltaMS}"
    None // no errors or other reason to quit

let Render unit =
    printfn "Render"
    ()

// test func

[<EntryPoint>]
[<STAThread>]
let main argv =

    //Register GraphicsManager
    typedefof<GraphicsManagerSFML>
    |> ManagerRegistry.addManager
    //register textRenderer
    typedefof<AngelCodeTextRenderer>
    |> ManagerRegistry.addManager
     //register InputManager
    typedefof<InputManagerWinRawInput>
    |> ManagerRegistry.addManager
    

    match ManagerRegistry.getManager<GraphicsManager> () with
    | Some graphics ->
        let window = graphics.OpenWindow (VideoMode.Window (800,600)) "Asteroids"
        window.Start(fun gmgr -> SetLevelManager(Some(AsteroidsLevel() :> AbstractLevelController)))
    | None -> printfn "No Graphics Manager registered, check your project references"

    0 // return an integer exit code
