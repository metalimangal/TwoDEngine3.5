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
open NewtonianObject
open Player

type SysColor = System.Drawing.Color

(*   Asteriods test program by JPK *)

let Random = Random()
let RandomFloat (low:float) (high:float) =
   Random.NextDouble() * (high - low) + low




let Start() =
    // Thes registrations will eventually be automatic runtims plugins
     //Register GraphicsManager
   
    let graphics = ManagerUtils.TryGetManager<GraphicsManager> ()
    let textRenderer = ManagerUtils.TryGetManager<TextManager> ()
    let inputDeviceManager = ManagerUtils.TryGetManager<InputDeviceInterface> ()
    let collision = ManagerUtils.TryGetManager<CollisionManager> ()
    
    let window = graphics.OpenWindow (Windowed (800u,600u)) "Asteroids"
  
    let atlas = 
        File.Open("Assets/asteroids-arcade.png", FileMode.Open)
        |>window.LoadImage
        
    let font =  textRenderer.LoadFont window "Assets/Basic.fnt"
    
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
            x=400.0f;y=300.0f;r=0f;
            vx=0.0f;vy=0.0f;vr=0f;img=shipImage}
        let startBulletList = {lastBulletTime=DateTime(0);bullets=[]}    
        let mutable PlayerObj = Ship {shipObject=startShipObj;bullets=startBulletList}
        let mutable asteroids:NewtonianObject list =
                                    [ for i in 1..10 -> {
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
       
        while window.IsOpen() && not (Key.IsKeyDown Key.ESC) do
            let currentTime = DateTime.Now
            let deltaMS = (currentTime - lastTime).Milliseconds
          
            if deltaMS >10 then
               // Console.WriteLine ("deltaMS: " + deltaMS.ToString()) |> ignore
                lastTime <- currentTime
                // update state
                PlayerObj <- Player.Update PlayerObj deltaMS        
                asteroids <- List.map (fun rock ->
                    NewtonianObject.NewtonianUpdate deltaMS rock |> NewtonianObject.Wrap 800f 600f) asteroids
                
                //display code
                window.Clear (SysColor.Black)  
                match PlayerObj with
                | Ship ship ->
                    //printfn $"{ship.shipObject.x} {ship.shipObject.y} {ship.shipObject.r}"
                    let xform =
                        window.TranslationTransform ship.shipObject.x ship.shipObject.y
                        |> fun x -> x.Multiply
                                        (window.TranslationTransform
                                                (ship.shipObject.img.Size.X /2f)
                                                (ship.shipObject.img.Size.Y /2f)) 
                        |> fun x -> x.Multiply (window.RotationTransform ship.shipObject.r)
                        |> fun x -> x.Multiply
                                        (window.TranslationTransform
                                                (-ship.shipObject.img.Size.X /2f)
                                                (-ship.shipObject.img.Size.Y /2f)) 
                    window.DrawImage xform ship.shipObject.img
                    
                | Explosion expl ->
                     window.TranslationTransform expl.x expl.y
                     |> AnimatedImage.draw expl.img window
                            // for asteroid in asteroids do
                | Dead -> () //Eventually draw game over screen
                
                asteroids
                |>List.iter(fun (asteroid:NewtonianObject) ->
                    let xform =
                        window.TranslationTransform asteroid.x asteroid.y
                        |> fun x -> x.Multiply
                                        (window.TranslationTransform
                                                (asteroid.img.Size.X /2f)
                                                (asteroid.img.Size.Y /2f)) 
                        |> fun x -> x.Multiply (window.RotationTransform asteroid.r)
                        |> fun x -> x.Multiply
                                        (window.TranslationTransform
                                                (-asteroid.img.Size.X /2f)
                                                (-asteroid.img.Size.Y /2f) )
                    window.DrawImage xform asteroid.img )
                
                            
                let fpsStr = "fps: "+ (1000/deltaMS).ToString()
                font.MakeText fpsStr
                |> fun x -> x.Draw window window.IdentityTransform
                
                match PlayerObj with
                | Ship ship ->
                     let shipCollison =  CircleCollider { Center = Vector2 (ship.shipObject.x, ship.shipObject.y);
                                           Radius = float32 ship.shipObject.img.Size.X } 
                //collision detection
                     asteroids
                     |> List.iter (fun asteroid ->
                              let asteroidCollision = CircleCollider {Center= Vector2 (asteroid.x, asteroid.y);
                                                       Radius = float32 asteroid.img.Size.X} 
                              match collision.Collide shipCollison asteroidCollision with
                              | Some result ->  PlayerObj <- Explosion {x=asteroid.x;y=asteroid.y;img=explosion}
                                                printfn "Ship hit" |> ignore
                              | None -> ()
                          ) |> ignore
                        
                 | _ -> ()   
                window.Show()  
               
        window.Close()
        ()
            
    window.Start(logic)
   
    