module TwoDEngine3
open System
open System.Numerics
open System.IO
open AngelCodeTextRenderer
open InputManagerWinRawInput
open GraphicsManagerSFML
open TDE3ManagerInterfaces.InputDevices
open TDE3ManagerInterfaces.GraphicsManagerInterface
open TDE3ManagerInterfaces
open TDE3ManagerInterfaces.TextRendererInterfaces
open System.Drawing
open TDE3ManagerInterfaces.WorldTree
type SysColor = System.Drawing.Color

(*   Asteriods test program by JPK *)

type AppRenderContext = { MyAppData: string }
let TryGetManager<'a> () =
    match ManagerRegistry.getManager<'a> () with
    | Some manager -> manager
    | None -> failwith "Manager not found"

type RenderNode =
        // Engine nodes
        | Collection of CollectionNode<RenderNode>
        | Sprite of SpriteNode<RenderNode>
        | GenericTransform of GenericTransformNode<RenderNode>
        | Rotate of RotateNode<RenderNode>
        | Translate of TranslateNode<RenderNode>
        // App nodes
        
let renderDispatch  (rc: WorldContext<RenderNode, AppRenderContext>)(node:RenderNode) :
    WorldContext<RenderNode, AppRenderContext> =
        match node with
        | Collection c -> c |> CollectionNode.render rc
        | Sprite s -> s |> SpriteNode.render rc
        | GenericTransform gt -> gt |> GenericTransformNode.render rc
        | Rotate r -> r |> RotateNode.render rc
        | Translate t -> t |> TranslateNode.render rc
        


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
    let window = graphics.OpenWindow (Windowed (800u,600u)) "Asteroids"
    
    use filestream = File.Open("Assets/asteroids-arcade.png", FileMode.Open)
    let atlas = window.LoadImage filestream
    let shipImage =
        atlas.SubImage (
            Rectangle(
                Point(3, 2),
                Size(25, 30)
            )
         )
    
   
   
    window.Start(fun window ->
        let worldTree = 
                 Translate(TranslateNode.create 400.0f 300.0f [
                     Sprite(SpriteNode.create shipImage [])
                 ])
             
            
        let worldContext:WorldContext<'N, 'A> =
            { Window=window; Transform=window.IdentityTransform;
              RenderDispatch = renderDispatch; AppData={MyAppData="Hello World"}}
           
              
               
       
        window.Clear Color.Black
        renderDispatch worldContext worldTree |> ignore
        window.Show()
    )
    while(true) do  System.Threading.Thread.Sleep 10
    0
