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

type ImageSFML(tex:SFTexture,rect) =
    let sprite = new SFSprite(tex,rect)
    
    new(tex) =
        ImageSFML(tex,IntRect(
            Vector2i(0,0),
            Vector2i(int(tex.Size.X),int(tex.Size.Y))))
    interface Image with
        member this.Size =
            let size = sprite.TextureRect
            new Vector(float32(sprite.TextureRect.Width),
                       float32(sprite.TextureRect.Height))
        member this.SubImage(rect) =
             new ImageSFML(tex,new IntRect(
                       int32(rect.Position.X),
                       int32(rect.Position.Y),
                       int32(rect.Size.X),
                       int32(rect.Size.Y)))
    member this.Draw(xform,gm) =
        
type GraphicsManagerSFML =
    interface GraphicsManager with
        member val window =
            new Window(new VideoMode(800,600),)
        member this.DrawImage(xform, image) =
            image.Draw(xform,this)
        member this.GraphicsListeners = failwith "todo"
        member this.GraphicsListeners with set value = failwith "todo"
        member this.IdentityTransform = failwith "todo"
        member this.LoadImage(stream) = 
            new ImageSFML(new SFImage(stream))
        member this.RotationTransform(var0) = failwith "todo"
        member this.ScaleTransform(var0) (var1) = failwith "todo"
        member this.ScreenSize = failwith "todo"
        member this.Start() =
            
        member this.Start(var0) = failwith "todo"
        member this.TranslationTransform(var0) (var1) = failwith "todo"