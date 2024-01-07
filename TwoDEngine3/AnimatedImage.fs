module ImageExtensions
open System.Drawing
open System.Numerics
open TDE3ManagerInterfaces.GraphicsManagerInterface


type AnimatedImage = { 
    ImageStrip : Image
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
    let createFromFrameWidth (imageStrip:Image) frameWidth frameHeight frameRate loop =
        let width= imageStrip.Size.X/float32 frameWidth
        let height = imageStrip.Size.Y/float32 frameHeight
        {       
            ImageStrip = imageStrip
            FrameWidth = frameWidth
            FrameHeight = frameHeight
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
            ImageStrip = imageStrip
            FrameWidth = int width
            FrameHeight = int height
            FrameCount = frameCountX * frameCountY
            FrameRate = frameRate
            FrameIndexAndTimer = (0, float 0.0f)
            Loop = loop
            Playing = false
            Finished = false
        }
        
    let draw (image:AnimatedImage) (window:Window) (xform:Transform)=
        let frameX = (fst image.FrameIndexAndTimer % (int image.ImageStrip.Size.X/image.FrameWidth)) * image.FrameWidth
        let frameY = (fst image.FrameIndexAndTimer / (int image.ImageStrip.Size.Y/image.FrameHeight)) * image.FrameHeight
        let frameRect = Rectangle(frameX, frameY, image.FrameWidth, image.FrameHeight)
        let subimage = image.ImageStrip.SubImage frameRect
        window.DrawImage xform subimage
    
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