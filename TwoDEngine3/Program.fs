module TwoDEngine3
open System
open System.Numerics
open System.IO
open System.Runtime.InteropServices.JavaScript
open AngelCodeTextRenderer
open InputManagerWinRawInput
open GraphicsManagerSFML
open TDE3ManagerInterfaces.InputDevices
open TDE3ManagerInterfaces.GraphicsManagerInterface
open TDE3ManagerInterfaces
open TDE3ManagerInterfaces.MouseAndKeyboardManager
open TDE3ManagerInterfaces.TextRendererInterfaces
open System.Drawing
open TDE3ManagerInterfaces.WorldTree
type SysColor = System.Drawing.Color

(*   Asteriods test program by JPK *)

module Key =
     let Left = 2190
     let Right = 2192

type KBRotateNode<'N> = { Degrees:float32 ; Children: 'N list }

let TryGetManager<'a> () =
    match ManagerRegistry.getManager<'a> () with
    | Some manager -> manager
    | None -> failwith "Manager not found"
module KBRotateNode =
    let create (degrees:float32) (children:'N list) = { Degrees = degrees ; Children = children }
    
    let render (renderContext: RenderContext<'N, 'A>) (node:RotateNode<'N>) =
        let newContext = { renderContext with Transform = renderContext.Transform.Multiply (renderContext.Window.RotationTransform node.Degrees) }
        let afterContext = RenderNode.renderChildren newContext node
        { renderContext with AppData = afterContext.AppData }
    let update (updateContext: UpdateContext<'N, 'A>) (node:RotateNode<'N>) =
        let mgr = TryGetManager<MouseAndKeyboardManager>()
        deviceStates |> Seq.iter (fun state -> //TODO needs to be a fols to collect all key actions
            match state with
            | KeyboardState down  ->
                down |> Seq.iter (fun key ->
                    match key with
                    | Key.Left -> { updateContext with NewTree = Some { node with Degrees = node.Degrees - 1.0f } }
                    | Key.Right -> { updateContext with NewTree = Some { node with Degrees = node.Degrees + 1.0f } }
                    | _ -> updateContext // unchanged
            | _ -> updateContext // unchanged
        ) 

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
        
let renderDispatch  (rc: RenderContext<RenderNode, AppRenderContext>)(node:RenderNode) :
    RenderContext<RenderNode, AppRenderContext> =
        match node with
        | Collection c -> c |> CollectionNode.render rc
        | Sprite s -> s |> SpriteNode.render rc
        | GenericTransform gt -> gt |> GenericTransformNode.render rc
        | Rotate r -> r |> RotateNode.render rc
        | Translate t -> t |> TranslateNode.render rc
let updateDispatch  (rc: UpdateContext<RenderNode, AppRenderContext>)(node:RenderNode) :
    UpdateContext<RenderNode, AppRenderContext> =
        match node with
        | Collection c -> c |> CollectionNode.update rc
        | Sprite s -> s |> SpriteNode.update rc
        | GenericTransform gt -> gt |> GenericTransformNode.update rc
        | Rotate r -> r |> RotateNode.update rc
        | Translate t -> t |> TranslateNode.update rc        


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
        let mutable worldTree = 
                 Translate(TranslateNode.create 400.0f 300.0f [
                     Sprite(SpriteNode.create shipImage [])
                 ])
             
            
        let renderContext:RenderContext<'N, 'A> =
            { Window=window; Transform=window.IdentityTransform;
              RenderDispatch = renderDispatch; AppData={MyAppData="Hello World"}}
           
        let updateContext:UpdateContext<'N, 'A> =
            { UpdateDispatch = updateDispatch; AppData={MyAppData="Hello World"};
              DeltaMS=0u; LastTime = DateTime.Now;NewTree = None}     
               
        while window.IsOpen() do
            window.Clear Color.Black
            renderDispatch renderContext worldTree |> ignore
            window.Show()
            let now = DateTime.Now
            updateDispatch
                {updateContext with
                    DeltaMS = (now-updateContext.LastTime).Milliseconds |> uint32;
                    LastTime = now}
                worldTree
            |> function
                | {NewTree = Some newTree} -> worldTree <- newTree
                | _ -> ()
       
    )
    while(true) do  System.Threading.Thread.Sleep 10
    0
