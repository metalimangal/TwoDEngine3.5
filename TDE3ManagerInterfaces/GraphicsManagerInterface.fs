module TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

open System.IO
open System.Numerics

type Rectangle(position:Vector2,size:Vector2) =
    member val Position  = position with get
    member val Size = size with get
    
type Transform =
    abstract Multiply : Vector2 -> Vector2
    abstract Multiply : Transform -> Transform
type VideoMode =
    | FullScreen
    | FullScreenWindow
    | Window of X:uint32 * Y:uint32
    

 [<AbstractClass>]
 type Window(graphicsManager:GraphicsManager)=
    member val graphics  = graphicsManager with get
    abstract Start : (Window -> unit) -> unit
    abstract Start : unit -> unit 
and GraphicsListener =
    abstract Update : GraphicsManager->uint -> string option
    abstract Render : GraphicsManager -> unit

and GraphicsManager =
    abstract OpenWindow : VideoMode->string->Window
    abstract GraphicsListeners : GraphicsListener list with get, set
    abstract ScreenSize : Vector2
    abstract LoadImage : Stream -> Image
   

    abstract IdentityTransform : Transform with get
    abstract RotationTransform : float32 -> Transform
    abstract TranslationTransform : float32-> float32 -> Transform
    abstract ScaleTransform : float32-> float32 -> Transform

and Image =
    abstract SubImage : Rectangle -> Image
    abstract Size : Vector2 with get
    abstract Draw : Window->Transform->unit  