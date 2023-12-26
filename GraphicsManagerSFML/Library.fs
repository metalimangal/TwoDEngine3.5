namespace GraphicsManagerSFML
open SFML.Graphics 
open SFML.System
open SFML.Window
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open System.Numerics


//aliases
type SFImage = SFML.Graphics.Image
type SFTexture = SFML.Graphics.Texture
type SFSprite = SFML.Graphics.Sprite
type SFWindow =  SFML.Graphics.RenderWindow
type SFTransform = SFML.Graphics.Transform
type SFVideoMode = SFML.Window.VideoMode
type SFRenderStates = SFML.Graphics.RenderStates

    

type TransformSFML(sfXform:SFTransform) =
    member val sfXform = sfXform with get
    interface Transform with
      override this.Multiply (otherVec:Vector2) : Vector2 =
          let result =sfXform.TransformPoint(Vector2f(otherVec.X,otherVec.Y))
          Vector2(result.X,result.Y)
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
            Vector2(float32(sprite.TextureRect.Width),
                       float32(sprite.TextureRect.Height))
        member this.SubImage(rect:Rectangle) =
             let pos = rect.Position 
             let sz = rect.Size 
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
        member this.IdentityTransform =
            TransformSFML(SFTransform.Identity)
        member this.LoadImage(stream) = 
            ImageSFML(new SFTexture(stream))
        member this.RotationTransform(degrees) =
            let xform = SFTransform()
            xform.Rotate(degrees)
            TransformSFML(xform)
        member this.ScaleTransform(x) (y) =
            let xform = SFTransform()
            xform.Scale(x,y)
            TransformSFML(xform)
        member this.ScreenSize =
            Vector2(
                float32(SFVideoMode.DesktopMode.Width),
                float32(SFVideoMode.DesktopMode.Height))
        member this.TranslationTransform(x) (y) =
            let xform = SFTransform()
            xform.Translate(x,y)
            TransformSFML(xform)