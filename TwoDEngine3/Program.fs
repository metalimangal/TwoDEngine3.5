module TwoDEngine3
open System
open System.Numerics
open System.IO
open AngelCodeTextRenderer
open InputManagerWinRawInput
open GraphicsManagerSFML
open TDE3ManagerInterfaces.InputDevices
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TDE3ManagerInterfaces.RenderTree
open TDE3ManagerInterfaces.TextRendererInterfaces
open System.Drawing
type SysColor = System.Drawing.Color

(*   Asteriods text program by JPK *)
let TryGetManager<'a> () =
    match ManagerRegistry.getManager<'a> () with
    | Some manager -> manager
    | None -> failwith "Manager not found"




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
    
    let graphics = TryGetManager<GraphicsManager> ()
    let textRenderer = TryGetManager<TextManager> ()
    let inputDeviceManager = TryGetManager<InputDeviceInterface> ()
    
    use filestream = File.Open("Assets/asteroids-arcade.png", FileMode.Open)
    let atlas = graphics.LoadImage filestream
    let shipImage = atlas.SubImage (
                            Rectangle(
                                Point(3, 2),
                                Size(25, 30)
                            )
                         )
    
    let window = graphics.OpenWindow (Windowed (800u,600u)) "Asteroids"
    //let font = textRenderer.LoadFont TODO
    window.Start(fun window ->
        let renderTree=RENDERTREE window [
            TRANSLATE 400f 300f [
                SPRITE shipImage []
            ] 
        ]
        window.Clear Color.Black
        renderTree.Render window.graphics.IdentityTransform
        window.Show()
    )
    while(true) do  System.Threading.Thread.Sleep 10
    0
