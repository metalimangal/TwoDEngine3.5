module TDE3ManagerInterfaces.TextRendererInterfaces
open TDE3ManagerInterfaces.GraphicsManagerInterface
type Font =
    abstract member Name : string
    abstract member Size : int
    abstract member MakeText : string -> Text

and [<AbstractClass>] Text(text:string,font:Font) =
    abstract member Draw : Window->Transform-> unit

and TextManager =
    abstract member FontList : string list
    abstract member LoadFont : Window -> string -> Font
   
