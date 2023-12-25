module TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

open System.IO
open System.Numerics

type Vector = 
   abstract Multiply : float32 -> Vector
     
type Rectangle(pos, sz) =
    member val Position:Vector = pos with get
    member val Size:Vector =sz with get
type Transform =
    abstract Multiply : Vector -> Vector
    abstract Multiply : Transform -> Transform
type VideoMode =
    | FullScreen
    | FullScreenWindow
    | Window of X:uint32 * Y:uint32
    
[<AbstractClass>]
type Window(graphicsManager)=
    member val graphics  = graphicsManager with get
    abstract Start : (Window -> unit) -> unit
    abstract Start : unit -> unit
type Image =
    abstract SubImage : Rectangle -> Image
    abstract Size : Vector with get
    abstract Draw : Window->Transform->unit    
    
type GraphicsListener =
    abstract Update : GraphicsManager->uint -> string option
    abstract Render : GraphicsManager -> unit

and GraphicsManager =
    abstract OpenWindow : VideoMode->string->Window
    abstract GraphicsListeners : GraphicsListener list with get, set
    abstract ScreenSize : Vector
    abstract LoadImage : Stream -> Image
   

    abstract IdentityTransform : Transform with get
    abstract RotationTransform : float32 -> Transform
    abstract TranslationTransform : float32-> float32 -> Transform
    abstract ScaleTransform : float32-> float32 -> Transform

    
