namespace AngelCodeTextRenderer

open System.IO
open System.Numerics
open FSharp.Collections
open Cyotek.Drawing.BitmapFont
open ManagerRegistry
open TDE3ManagerInterfaces.TextRendererInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface



[<Manager("Text renderer that uses angelcode bitmap fonts",
          supportedSystems.Windows ||| supportedSystems.Mac ||| supportedSystems.Windows)>]
type AngelCodeTextRenderer() =
    interface TextManager with
        member this.FontList = Directory.GetFiles("AngelcodeFonts") |> Array.toList
        member this.LoadFont(fontName) =
            BitmapFontLoader.LoadFontFromFile(fontName) |> AngelCodeFont :> Font

and AngelCodeFont(bmFont) =
    let bitmapFont = bmFont
    let graphics = ManagerRegistry.getManager<GraphicsManager> ()
    let pages =
        bmFont.Pages
        |> Array.fold
            (fun (pageMap: Map<int, Lazy<Image>>) page ->
                let fileStream = File.Open(page.FileName, FileMode.Open)
                let lazyImage = lazy (graphics.Value.LoadImage fileStream)
                Map.add page.Id lazyImage pageMap)
            Map.empty
    member this.GetPage(id) = pages.[id].Force()
    member this.GetCharacter char = bitmapFont.Characters.[char]
    member this.GetKern(last, curr) =
        float32 (bitmapFont.GetKerning(last, curr))
    interface Font with
        member this.MakeText(text) = AngelCodeText(text, this) :> Text
        member val Name = bmFont.FamilyName
        member val Size = bmFont.FontSize

and AngelCodeText(text: string, font: AngelCodeFont) =
    inherit Text(text, font) 
    override this.Draw (window:Window) (xform:Transform) =
        let graphics = window.graphics
        text
        |> Seq.fold
            (fun (state: (Vector2 * char) ) char ->
                let pos = fst state
                let lastChar = snd state
                let acChar: Character = font.GetCharacter char
                let acImage: Image = font.GetPage(acChar.TexturePage)
                let rectPos = Vector2 (float32 acChar.X, float32 acChar.Y)
                let rectSz = Vector2 (float32 acChar.Width, float32 acChar.Height)
                let charImage = acImage.SubImage(Rectangle(rectPos, rectSz))
                let kern = font.GetKern(lastChar, char)
                let newX = pos.X + (float32 acChar.Width) + kern
                let xlateXform = graphics.TranslationTransform newX pos.Y
                let xform = xlateXform.Multiply xform
                charImage.Draw window xform
                (Vector2(newX, pos.Y), char)
            ) (Vector2(0f,0f),'\n')
        |> ignore
