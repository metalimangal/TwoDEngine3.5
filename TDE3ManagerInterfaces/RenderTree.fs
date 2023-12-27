module TDE3ManagerInterfaces.RenderTree

open System
open TwoDEngine3.ManagerInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

[<AbstractClass>]
type RenderNode(childrenArray:RenderNode list,
                graphicsManager:GraphicsManager) =
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
                graphicsManager:GraphicsManager) =
   inherit RenderNode(childrenArray,graphicsManager)

   override this.Update(deltaMS) =
       new RootNode((this.UpdateChildren deltaMS),graphicsManager)
let  RENDERTREE gm childrenArray =
    new RootNode(
        ProcessFuncs gm childrenArray,
        gm)
    
type SpriteNode(img:Image,
                childrenArray:RenderNode list,
                graphicsManager:GraphicsManager) =
    inherit RenderNode(childrenArray, graphicsManager)
    override this.Render xform  =
       graphicsManager.DrawImage img
       this.RenderChildren xform

    override this.Update(deltaMS) =
        new SpriteNode(img, (this.UpdateChildren deltaMS),graphicsManager)
let SPRITE img childrenArray graphicsManager =
    let childnodes = ProcessFuncs graphicsManager childrenArray
    new SpriteNode(img, childnodes, graphicsManager) :> RenderNode
  
[<AbstractClass>]       
type GenericTransformNode(transform:Transform,
                          childrenArray:RenderNode list,
                          gm:GraphicsManager) =
    inherit RenderNode(childrenArray,gm)

    override this.Render xform  =
        this.RenderChildren (xform.Multiply transform)   
        
type RotateNode(degrees:float32,  childrenArray:RenderNode list,
            gm:GraphicsManager) =
    inherit GenericTransformNode(gm.RotationTransform degrees,
                             childrenArray,gm)

    override this.Update(deltaMS) =
        new RotateNode(degrees,(this.UpdateChildren deltaMS),gm)
let ROTATE degrees childrenArray graphicsManager =
     let childnodes = ProcessFuncs graphicsManager childrenArray
     new RotateNode(degrees, childnodes, graphicsManager)
     
type TranslateNode(x:float32, y:float32,
                    childrenArray:RenderNode list,
                    gm:GraphicsManager) =
    inherit GenericTransformNode(gm.TranslationTransform x y,
                             childrenArray,gm)

    override this.Update(deltaMS) =
        new TranslateNode(x,y,(this.UpdateChildren deltaMS),gm)
let TRANSLATE x y childrenArray graphicsManager =
     let childnodes = ProcessFuncs graphicsManager childrenArray
     new TranslateNode(x,y, childnodes, graphicsManager)    
    
 