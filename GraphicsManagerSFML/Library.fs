namespace GraphicsManagerSFML
open SFML.Graphics 
open SFML.System
open SFML.Window
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open System.IO


//aliases
type SFImage = SFML.Graphics.Image
type SFTexture = SFML.Graphics.Texture
type SFSprite = SFML.Graphics.Sprite
type SFWindow =  SFML.Graphics.RenderWindow
type SFTransform = SFML.Graphics.Transform
type SFVideoMode = SFML.Window.VideoMode
type SFRenderStates = SFML.Graphics.RenderStates

    
type VectorSFML(sfmlVector:Vector2f)=
    member val sfmlVector = sfmlVector with get
    interface Vector with
        override this.Multiply other=
            VectorSFML(sfmlVector * other)
type TransformSFML(sfXform:SFTransform) =
    member val sfXform = sfXform with get
    interface Transform with
      override this.Multiply (otherVec:Vector) : Vector =
          new VectorSFML(sfXform.TransformPoint((otherVec:?>VectorSFML).sfmlVector))
      override this.Multiply (otherXform:Transform) : Transform =
          TransformSFML(sfXform * (otherXform:?>TransformSFML).sfXform)

type ImageSFML(tex:SFTexture,rect) =
    let sprite = new SFSprite(tex,rect)
    
    new(tex) =
        ImageSFML(tex,IntRect(
            Vector2i(0,0),
            Vector2i(int(tex.Size.X),int(tex.Size.Y))))
    interface Image with
        member this.Size =
            let size = sprite.TextureRect
            VectorSFML(Vector2f(float32(sprite.TextureRect.Width),
                       float32(sprite.TextureRect.Height)))
        member this.SubImage(rect:Rectangle) =
             let pos = (rect.Position :?> VectorSFML).sfmlVector
             let sz = (rect.Size :?> VectorSFML).sfmlVector
             ImageSFML(tex,new IntRect(
                       int32(pos.X),
                       int32(pos.Y),
                       int32(sz.X),
                       int32(sz.Y)))

       
        member this.Draw(window:Window) (xform:Transform) =
            (window:?>WindowSFML).DrawSprite (xform:?>TransformSFML).sfXform sprite
and WindowSFML(sfmlWindow:SFWindow,gm:GraphicsManager) =
    inherit Window(gm) with
        member val sfmlWindow = sfmlWindow with get
        override this.Start(startFunc) =
            startFunc this
        override this.Start() =
            ()
        member this.DrawSprite xform sprite =
            let state = SFRenderStates(xform)
            sfmlWindow.Draw(sprite,state)
            ()
            
type GraphicsManagerSFML =
    interface GraphicsManager with
        override this.OpenWindow videoMode name  =
            match videoMode with
            | FullScreen ->
                WindowSFML(new SFWindow(SFVideoMode.FullscreenModes[0],
                           name),this)
            | FullScreenWindow ->
                WindowSFML(new SFWindow(SFVideoMode.DesktopMode,
                           name),this)
            | Window (x, y) ->
                WindowSFML(new SFWindow(SFVideoMode(x,y),
                           name),this)
        
        member this.GraphicsListeners = failwith "todo"
        member this.GraphicsListeners with set value = failwith "todo"
        member this.IdentityTransform = failwith "todo"
        member this.LoadImage(stream) = 
            new ImageSFML(new SFTexture(stream))
        member this.RotationTransform(var0) = failwith "todo"
        member this.ScaleTransform(var0) (var1) = failwith "todo"
        member this.ScreenSize = failwith "todo"
        member this.TranslationTransform(var0) (var1) = failwith "todo"