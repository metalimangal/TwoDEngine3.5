module TDE3ManagerInterfaces.RenderTree

open System
open TwoDEngine3.ManagerInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

[<AbstractClass>]
type RenderNode(childrenArray:RenderNode list,
                window:Window) =
    abstract member Render: Transform-> unit
    default this.Render xform =
        this.RenderChildren xform
    abstract member Update: int32-> RenderNode
    member this.RenderChildren xform  =    
        childrenArray
        |> List.iter (fun child -> child.Render xform)
        
    member this.UpdateChildren deltaMS  =    
        childrenArray
        |> List.map (fun child -> child.Update deltaMS)

    
(************ Tree builder ***************
 Each node definition is followesd by the function to build it.
 See file header for more information on use
 ****************************************)
 
let ProcessFuncs gm (funcs:(GraphicsManager->RenderNode) list) =
    funcs
    |> List.map (fun func -> func gm )
    
type RootNode(childrenArray:RenderNode list,
                window:Window) =
   inherit RenderNode(childrenArray,window)

   override this.Update(deltaMS) =
       new RootNode((this.UpdateChildren deltaMS),window)
let  RENDERTREE gm childrenArray =
    new RootNode(
        ProcessFuncs gm childrenArray,
        gm)
    
type SpriteNode(img:Image,
                childrenArray:RenderNode list,
                window:Window) =
    inherit RenderNode(childrenArray, window)
    override this.Render xform  =
       img.Draw window xform
       this.RenderChildren xform

    override this.Update(deltaMS) =
        new SpriteNode(img, (this.UpdateChildren deltaMS),window)
let SPRITE img childrenArray window =
    let childnodes = ProcessFuncs window childrenArray
    new SpriteNode(img, childnodes, window) :> RenderNode
  
[<AbstractClass>]       
type GenericTransformNode(transform:Transform,
                          childrenArray:RenderNode list,
                          window:Window) =
    inherit RenderNode(childrenArray,window)

    override this.Render xform  =
        this.RenderChildren (xform.Multiply transform)   
        
type RotateNode(degrees:float32,  childrenArray:RenderNode list,
            window:Window) =
    inherit GenericTransformNode(window.graphics.RotationTransform degrees,
                             childrenArray,window)

    override this.Update(deltaMS) =
        new RotateNode(degrees,(this.UpdateChildren deltaMS),window)
let ROTATE degrees childrenArray window =
     let childnodes = ProcessFuncs window childrenArray
     new RotateNode(degrees, childnodes, window)
     
type TranslateNode(x:float32, y:float32,
                    childrenArray:RenderNode list,
                    window:Window) =
    inherit GenericTransformNode(window.graphics.TranslationTransform x y,
                             childrenArray,window)

    override this.Update(deltaMS) =
        new TranslateNode(x,y,(this.UpdateChildren deltaMS),window)
let TRANSLATE x y childrenArray window =
     let childnodes = ProcessFuncs window childrenArray
     new TranslateNode(x,y, childnodes, window)    
    
 