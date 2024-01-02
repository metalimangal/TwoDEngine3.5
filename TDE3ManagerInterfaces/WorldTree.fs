module TDE3ManagerInterfaces.WorldTree

open System
open TDE3ManagerInterfaces.GraphicsManagerInterface


////// WORLD TREE CODE
/// This is based on sample code written by Justin Wick and used
/// with his permission. It is licensed under the MIT license

/// Big picture:
///
/// Our goal is to provide a common structure for rendering a scene graph, and the tools to construct that renderer.
/// We use a familiar pattern of a discriminated union to represent the scene graph, and a recursive function to render
/// it. That recursive function, however, is provided via dependency injection, so that the engine can be used in
/// different ways. For example, we might want to render the scene graph to a screen, or to a file, or to a network
/// stream. We might want to render it with debug information, or without. We might want to render it with a different
/// graphics manager. We might want to render it with a different app data type. Furthermore this context object also
/// contains all of the other state information that must be carried down through the renderer, such as the current
/// transform.
///
/// This scenegraph DU is provided by the application itself, allowing the application to create arbitrary node types,
/// some of which will be provided by the engine.
///
/// The purpose of providing the AppData is to keep the engine generic. The engine doesn't care what the app data is,
/// but the callbacks that the app provides to the engine do.
///
/// Overall, the app is "in charge" of driving the big picture stuff, the engine just provides the pieces and a coherent
/// convention.

type RenderContext<'N, 'A> =
    { Window:Window ; Transform: Transform ; RenderDispatch: RenderDispatch<'N, 'A> ; AppData: 'A }   
and RenderDispatch<'N, 'A> = RenderContext<'N, 'A> -> 'N -> RenderContext<'N, 'A>

type UpdateContext<'N, 'A> =
    { UpdateDispatch: UpdateDispatch<'N, 'A>
      LastTime:DateTime
      DeltaMS: uint32
      NewTree:'N option
      AppData: 'A }
and UpdateDispatch<'N, 'A> = UpdateContext<'N, 'A> -> 'N -> UpdateContext<'N, 'A>

// 'SN is used to constrain the type of the children of the node

module RenderNode =
    let inline renderChildren<'N, 'A, 'SN when 'SN : (member Children : 'N list)>
        (renderContext: RenderContext<'N, 'A>) (specificNode: 'SN) =
            specificNode.Children |> List.fold renderContext.RenderDispatch renderContext
    let inline updateChildren<'N, 'A, 'SN when 'SN : (member Children : 'N list)>
        (updateContext: UpdateContext<'N, 'A>) (specificNode: 'SN) =
            specificNode.Children |> List.fold updateContext.UpdateDispatch updateContext
type CollectionNode<'N> = { Children: 'N list }
module CollectionNode =
    let create (children:'N list) = { Children = children }
    let render (renderContext: RenderContext<'N, 'A>) (collectionNode:CollectionNode<'N>) =
        RenderNode.renderChildren renderContext collectionNode
    let update (updateContext: UpdateContext<'N, 'A>) (collectionNode:CollectionNode<'N>) =
        RenderNode.updateChildren updateContext collectionNode

type SpriteNode<'N> = { Image:Image ; Children: 'N list }
module SpriteNode =
    let create (img:Image) (children:'N list) = { Image = img ; Children = children }
    let render  (renderContext: RenderContext<'N, 'A>) (node:SpriteNode<'N>)=
        renderContext.Window.DrawImage renderContext.Transform  node.Image
        renderContext //unchanged
    let update (updateContext: UpdateContext<'N, 'A>) (node:SpriteNode<'N>) =
        updateContext // unchanged
type GenericTransformNode<'N> = { Transform:Transform ; Children: 'N list }
module GenericTransformNode =
    let create (transform:Transform) (children:'N list) = { Transform = transform ; Children = children }
    let render (renderContext: RenderContext<'N, 'A>) (node:GenericTransformNode<'N>) =
        let newContext = { renderContext with Transform = renderContext.Transform.Multiply node.Transform }
        let afterContext = RenderNode.renderChildren newContext node
        { renderContext with AppData = afterContext.AppData }
    let update (updateContext: UpdateContext<'N, 'A>) (node:GenericTransformNode<'N>) =
        updateContext // unchanged

type RotateNode<'N> = { Degrees:float32 ; Children: 'N list }
module RotateNode =
    let create (degrees:float32) (children:'N list) = { Degrees = degrees ; Children = children } 
    let render (renderContext: RenderContext<'N, 'A>) (node:RotateNode<'N>) =
        let newContext = { renderContext with Transform = renderContext.Transform.Multiply (renderContext.Window.RotationTransform node.Degrees) }
        let afterContext = RenderNode.renderChildren newContext node
        { renderContext with AppData = afterContext.AppData }
    let update (updateContext: UpdateContext<'N, 'A>) (node:RotateNode<'N>) =
        updateContext // unchanged
type TranslateNode<'N> = { X:float32 ; Y:float32 ; Children: 'N list }
module TranslateNode =
    let create (x:float32) (y:float32) (children:'N list) = { X = x ; Y = y ; Children = children } 
    let render (renderContext: RenderContext<'N, 'A>) (node:TranslateNode<'N>) =
        let newContext = { renderContext
                           with Transform = renderContext.Transform.Multiply (renderContext.Window.TranslationTransform node.X node.Y)}
        let afterContext = RenderNode.renderChildren newContext node
        { renderContext with AppData = afterContext.AppData }
    let update (updateContext: UpdateContext<'N, 'A>) (node:TranslateNode<'N>) =
        updateContext // unchanged

////// APPLICATION CODE
///
/// This is a sample application that uses the engine. It defines its own app data type, and its own node types.
///
/// We have two rendering functions, one which is the "real" one, and one which is a debug one. The debug one
/// illustrates how we can add new functionality to the recursive rendering function without changing the engine
/// 'N is the node type, 'A is the app data type

module Application =
    type AppRenderContext = { MyAppData: string }
    type PlayerNode<'N> = { Name: string ; HP: int; Children: 'N list }
    module PlayerNode =
        let create (name: string) (hp: int) (children:'N list) = { Name = name ; HP = hp ; Children = children }  
        let render (renderContext: RenderContext<'N, 'A>) (node:PlayerNode<'N>) =
            // TBD stuff here
            RenderNode.renderChildren renderContext node    
        let update (updateContext: UpdateContext<'N, 'A>) (node:PlayerNode<'N>) =
             // TBD stuff here
             updateContext // unchanged
    type EnemyNode<'N> = { Name: string ;  HP: int; Children: 'N list }
    module EnemyNode =
        let create (name: string) (hp: int) (children:'N list) = { Name = name ; HP = hp ; Children = children }       
        let render (renderContext: RenderContext<'N, 'A>) (node:EnemyNode<'N>) =
            // TBD stuff here
            RenderNode.renderChildren renderContext node
        let update (updateContext: UpdateContext<'N, 'A>) (node:EnemyNode<'N>) =
             // TBD stuff here
             updateContext // unchanged   
    type RenderNode =
        // Engine nodes
        | Collection of CollectionNode<RenderNode>
        | Sprite of SpriteNode<RenderNode>
        | GenericTransform of GenericTransformNode<RenderNode>
        | Rotate of RotateNode<RenderNode>
        | Translate of TranslateNode<RenderNode>
        // App nodes
        | Player of PlayerNode<RenderNode>
        | Enemy of EnemyNode<RenderNode>    
    let render (node:RenderNode) (rc: RenderContext<RenderNode, AppRenderContext>) =
        match node with
        | Collection c -> c |> CollectionNode.render rc
        | Sprite s -> s |> SpriteNode.render rc
        | GenericTransform gt -> gt |> GenericTransformNode.render rc
        | Rotate r -> r |> RotateNode.render rc
        | Translate t -> t |> TranslateNode.render rc
        | Player p -> p |> PlayerNode.render rc
        | Enemy e -> e |> EnemyNode.render rc
    
    
    
    
    //
    // Same as above, but with debug info, and potential integration of other debug features.
    // 
    
    type DebugEvent = { EventTime: DateTime ; EventData: string }  
    module DebugEvent =
        let create (eventData: string) = { EventTime = DateTime.Now ; EventData = eventData }
        let createRendering (what: string) = create $"Rendering: %s{what}"
        let update (updateContext: UpdateContext<'N, 'A>) (node:SpriteNode<'N>) =
             // TBD stuff here
             updateContext // unchanged  
    type DebugAppRenderContext = { AppRenderContext: AppRenderContext ; DebugData: string ; DebugEvents: string list }
    module DebugAppRenderContext =
        let create (appRenderContext: AppRenderContext) (debugData: string) (debugEvents: string list) =
            { AppRenderContext = appRenderContext ; DebugData = debugData ; DebugEvents = debugEvents }            
        let addEvent (event: DebugEvent) (debugAppRenderContext: DebugAppRenderContext) =
            { debugAppRenderContext with DebugEvents = event.EventData :: debugAppRenderContext.DebugEvents }       
        let update (updateContext: UpdateContext<'N, 'A>) (node:SpriteNode<'N>) =
             // TBD stuff here
             updateContext // unchanged
    
    module RenderContext =
        let addDebugEvent (event: DebugEvent) (renderContext: RenderContext<RenderNode, DebugAppRenderContext>) =
            { renderContext with AppData = renderContext.AppData |> DebugAppRenderContext.addEvent event }    
    let renderDebug (node:RenderNode) (rc: RenderContext<RenderNode, DebugAppRenderContext>) =
        match node with
        | Collection c ->
            let newContext = ("Collection" |> DebugEvent.createRendering , rc) ||> RenderContext.addDebugEvent
            c |> CollectionNode.render newContext
        | Sprite s ->
            let newContext = ("Sprite" |> DebugEvent.createRendering , rc) ||> RenderContext.addDebugEvent
            s |> SpriteNode.render newContext
        | GenericTransform gt ->
            let newContext = ("GenericTransform" |> DebugEvent.createRendering , rc) ||> RenderContext.addDebugEvent
            gt |> GenericTransformNode.render newContext
        | Rotate r ->
            let newContext = ("Rotate" |> DebugEvent.createRendering , rc) ||> RenderContext.addDebugEvent
            r |> RotateNode.render newContext
        | Translate t ->
            let newContext = ("Translate" |> DebugEvent.createRendering , rc) ||> RenderContext.addDebugEvent
            t |> TranslateNode.render newContext
        | Player p ->
            let newContext = ("Player" |> DebugEvent.createRendering , rc) ||> RenderContext.addDebugEvent
            p |> PlayerNode.render newContext
        | Enemy e ->
            let newContext = ("Enemy" |> DebugEvent.createRendering , rc) ||> RenderContext.addDebugEvent
            e |> EnemyNode.render newContext
            
            

            
