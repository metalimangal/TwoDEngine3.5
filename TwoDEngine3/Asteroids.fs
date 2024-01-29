module Asteroids
open System
open System.Numerics
open System.IO
open AngelCodeTextRenderer
open ImageExtensions
open InputManagerWinRawInput
open GraphicsManagerSFML
open SimpleCollisionManager
open TDE3ManagerInterfaces.CollisionManagerInterface
open TDE3ManagerInterfaces.InputDevices
open TDE3ManagerInterfaces.GraphicsManagerInterface
open TDE3ManagerInterfaces.TextRendererInterfaces
open System.Drawing

type SysColor = System.Drawing.Color

(*   Asteriods test program by JPK *)

let Random = Random()
let RandomFloat (low:float) (high:float) =
   Random.NextDouble() * (high - low) + low

module Key =
     let Left = 37
     let Up = 38
     let Right = 39
     let Down = 40
     let ESC = 27
     let SPACE = 32


let Wrap (w:float32) (h:float32) (obj:NewtonianObject) =
    let x = if obj.x+obj.img.Size.X< 0.0f then w
                else if obj.x > w then 0.0f-obj.img.Size.X else obj.x
    let y = if obj.y+obj.img.Size.Y < 0.0f then h
                else if obj.y > h then 0.0f-obj.img.Size.Y else obj.y
    {obj with x=x;y=y}



let Start() =
    // Thes registrations will eventually be automatic runtims plugins
     //Register GraphicsManager
   
    let graphics = TryGetManager<GraphicsManager> ()
    let textRenderer = TryGetManager<TextManager> ()
    let inputDeviceManager = TryGetManager<InputDeviceInterface> ()
    let window = graphics.OpenWindow (Windowed (800u,600u)) "Asteroids"
    let collision = TryGetManager<CollisionManager> ()
    
    let atlas = 
        File.Open("Assets/asteroids-arcade.png", FileMode.Open)
        |>window.LoadImage 
    let shipImage =
        atlas.SubImage (
            Rectangle(
                Point(3, 2),
                Size(25, 30)
            )
         )
    let explosionSheet = atlas.SubImage (
            Rectangle(
                Point(51, 142),
                Size(204, 36)
            )
         )    
    let bigAsteroid =
        atlas.SubImage (
            Rectangle(
                Point(66, 195),
                Size(60, 64)
            )
         )
    
    
    
   
            
                
    let logic (window:Window) =
        let startShipObj = {
            x=400.0f;y=300.0f;r=0f
            vx=0.0f;vy=0.0f;vr=0f;img=shipImage}
        let startBulletList = {lastBulletTime=DateTime(0);bullets=[]}    
        let mutable Player = Ship {shipObject=startShipObj;bullets=startBulletList}
        let mutable asteroids = [for i in 1..10 -> {
                                    x=float32(Random.Next(0,800))
                                    y=float32(Random.Next(0,600))
                                    r=float32(Random.Next(0,359))
                                    vx=float32(RandomFloat -0.05 0.05)
                                    vy=float32(RandomFloat -0.05 0.05)
                                    vr=float32(RandomFloat -0.1 0.1)
                                    img=bigAsteroid } ]
        let mutable explosion = AnimatedImage.createFromFrameCounts explosionSheet 3 1 (float 500) false
        let mutable lastTime = DateTime.Now
        let mutable lastBulletTime = DateTime.Now
        let font =  textRenderer.LoadFont window "Assets/Basic.fnt"
        

        while window.IsOpen() && not (IsKeyDown Key.ESC) do
            let currentTime = DateTime.Now
            let deltaMS = (currentTime - lastTime).Milliseconds
            if deltaMS >0 then
                match Player with
                | Ship ship ->
                    Player <- ShipUpdate ship deltaMS
                | Explosion anim ->
                    Player <- anim |> AnimatedImage.update (uint32 deltaMS)
                    |> fun anim ->
                        if anim.Finished then
                            Dead
                        else
                            anim |> Explosion
                | Dead -> () // game over        
                
                asteroids <- List.map (fun rock ->
                    NewtonianUpdate deltaMS rock |> Wrap 800f 600f) asteroids
                
                //display code
                match Player with
                | Ship ship -> 
                    let shipXform =
                        (window.TranslationTransform
                            ship.shipObject.x ship.shipObject.y).Multiply
                            (window.RotationTransform ship.shipObject.r)
                    window.Clear (SysColor.Black)
                    window.DrawImage shipXform ship.shipObject.img
                | Explosion anim ->
                    window.TranslationTransform anim.x anim.y
                    |> AnimatedImage.draw anim window
                            // for asteroid in asteroids do
                | Dead -> () //Eventually draw game over screen
                asteroids
                |>Seq.iter(fun asteroid ->
                    let xform =
                        (window.TranslationTransform asteroid.x asteroid.y)
                        |> fun x -> x.Multiply
                                        (window.TranslationTransform
                                                (asteroid.img.Size.X /2f)
                                                (asteroid.img.Size.Y /2f)) 
                        |> fun x -> x.Multiply (window.RotationTransform asteroid.r)
                        |> fun x -> x.Multiply
                                        (window.TranslationTransform
                                                (-asteroid.img.Size.X /2f)
                                                (-asteroid.img.Size.Y /2f)) 
                    window.DrawImage xform asteroid.img 
                
                let shipCicle:CollisionGeometry =
                    CircleCollider{Center=Vector2(ship.x,ship.y);Radius=ship.img.Size.Y/2f}
                asteroids
                |> List.tryFind (fun asteroid ->
                    CircleCollider{Center=Vector2(asteroid.x,asteroid.y);Radius=asteroid.img.Size.Y/2f}
                    |> fun x -> collision.Collide shipCicle x
                    |> function
                        | Some result -> true
                        | _ -> false
                    )
                |> function
                    | Some x -> () //printfn "Collision detected"
                    | None -> ()
                //window.DrawImage (window.TranslationTransform 100.0f 100.0f) explosionSheet
                AnimatedImage.draw explosion window (window.TranslationTransform 100.0f 100.0f)
                explosion <- AnimatedImage.update (uint32 deltaMS) explosion //TODO minor problm causing hang
                let fpsStr = "fps: "+ (1000.0f/float32 deltaMS).ToString()
                font.MakeText fpsStr
                |> fun x -> x.Draw window window.IdentityTransform
                window.Show()
                lastTime <- currentTime
        window.Close()
        ()
            
    window.Start(logic)
   
    