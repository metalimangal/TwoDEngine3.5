module ImageExtensions
open System.Drawing
open System.Numerics
open TDE3ManagerInterfaces.GraphicsManagerInterface


type AnimatedImage = { 
    ImageList : List<Image>
    FrameWidth : int
    FrameHeight : int
    FrameCount : int
    FrameRate : float
    FrameIndexAndTimer : int*float
    Loop : bool
    Playing : bool
    Finished : bool
}

module AnimatedImage =
    let creatSubImageList (image:Image) (frameWidth:int) (frameHeight:int) =
        let cols= int (image.Size.X/float32 frameWidth)
        let rows = int (image.Size.Y/float32 frameHeight)
        
        [|0..rows-1|]
        |> Array.fold (fun (acc:Image list) row ->
            [|0..cols-1|]
            |> Array.fold (fun (acc:Image list) col ->
                let frameX = (col * frameWidth) 
                let frameY = row * frameHeight
                let frameRect = Rectangle(frameX, frameY, frameWidth, frameHeight)
                let subimage = image.SubImage frameRect
                subimage::acc
            ) acc
        ) List.empty
        |> List.rev
    let createFromFrameWidth (imageStrip:Image) frameWidth frameHeight frameRate loop =
        let width= imageStrip.Size.X/float32 frameWidth
        let height = imageStrip.Size.Y/float32 frameHeight
        {       
            FrameWidth = frameWidth
            FrameHeight = frameHeight
            ImageList = creatSubImageList imageStrip (int frameWidth) (int frameHeight)
            FrameCount = int(width*height)
            FrameRate = frameRate
            FrameIndexAndTimer = (0,float 0.0f)
            Loop = loop
            Playing = false
            Finished = false
        }
    let createFromFrameCounts (imageStrip:Image) frameCountX frameCountY frameRate loop =
        let width= imageStrip.Size.X/float32 frameCountX
        let height = imageStrip.Size.Y/float32 frameCountY
        {      
            FrameWidth = int width
            FrameHeight = int height
            ImageList = creatSubImageList imageStrip (int width) (int height)
            FrameCount = frameCountX * frameCountY
            FrameRate = frameRate
            FrameIndexAndTimer = (0, float 0.0f)
            Loop = loop
            Playing = true
            Finished = false
        }
        
    let draw (image:AnimatedImage) (window:Window) (xform:Transform)=
        window.DrawImage xform image.ImageList.[fst image.FrameIndexAndTimer]
    
    let update (deltaTime:uint32) (image:AnimatedImage) : AnimatedImage =
        if image.Playing then
                {image with 
                    FrameIndexAndTimer =
                        if snd(image.FrameIndexAndTimer) + float deltaTime > image.FrameRate then
                            ( if fst image.FrameIndexAndTimer + 1>= image.FrameCount then
                                    if image.Loop then
                                        0
                                    else
                                        image.FrameCount - 1
                                else
                                    fst image.FrameIndexAndTimer + 1
                            , (snd image.FrameIndexAndTimer + float deltaTime - image.FrameRate) )
                        else 
                           (fst image.FrameIndexAndTimer, snd  image.FrameIndexAndTimer + float deltaTime)                
                }
        else
            image