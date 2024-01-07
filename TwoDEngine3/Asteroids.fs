module Asteroids
open System
open System.Numerics
open System.IO
open AngelCodeTextRenderer
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

type NewtonianObject = {x:float32; y:float32;r:float32;
                        vx:float32;vy:float32;vr:float32;
                        img:Image}
let NewtonianUpdate  deltaMS (obj:NewtonianObject)  =
    let x = obj.x + obj.vx * (float32 deltaMS)
    let y = obj.y + obj.vy * (float32 deltaMS)
    let r = obj.r + obj.vr * (float32 deltaMS)
    {obj with x=x;y=y;r=r}
    
let Wrap (w:float32) (h:float32) (obj:NewtonianObject) =
    let x = if obj.x+obj.img.Size.X< 0.0f then w
                else if obj.x > w then 0.0f-obj.img.Size.X else obj.x
    let y = if obj.y+obj.img.Size.Y < 0.0f then h
                else if obj.y > h then 0.0f-obj.img.Size.Y else obj.y
    {obj with x=x;y=y}

let TryGetManager<'a> () =
    let manager = ManagerRegistry.getManager<'a>()
    match manager with
    | Some m -> m
    | None -> failwith "Manager not found"
    
let DegToRad (deg:float32) = deg * (float32 Math.PI) / 180.0f

let IsKeyDown(asciicode:int) =
    let inputDeviceManager = TryGetManager<InputDeviceInterface> ()
    inputDeviceManager.PollState()
    |> Map.values
    |> Seq.fold (fun acc axisState ->
        if acc = true then
            acc
        else
            match axisState with
            | KeyboardState kbdState ->
                // debug code
                //printfn "Keystate: %A\n" kbdState
                //kbdState |> Seq.map(fun onechar ->
                //                        int32 onechar)
                //|> printfn "Keycodes: %A\n" 
                kbdState
                |> List.tryFind (fun keyState -> int keyState = asciicode)
                |> function
                    | Some x -> true
                    | None -> false
             | _ -> false
        ) false
let Start() =
    // Thes registrations will eventually be automatic runtims plugins
     //Register GraphicsManager
    typedefof<GraphicsManagerSFML>
    |> ManagerRegistry.addManager
    //register textRenderer
    typedefof<AngelCodeTextRenderer>
    |> ManagerRegistry.addManager
     //register InputManager
    typedefof<InputManagerWinRawInput>
    |> ManagerRegistry.addManager
    //register CollisionManager
    typedefof<SimpleCollisionManager>
    |> ManagerRegistry.addManager
    
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
                Point(75, 142),
                Size(180, 36)
            )
         )    
    let bigAsteroid =
        atlas.SubImage (
            Rectangle(
                Point(62, 186),
                Size(60, 62)
            )
         )
    let logic (window:Window) =
        let mutable ship = {
            x=400.0f;y=300.0f;r=0f
            vx=0.0f;vy=0.0f;vr=0f;img=shipImage
        }
        let mutable asteroids = [for i in 1..10 -> {
                                    x=float32(Random.Next(0,800))
                                    y=float32(Random.Next(0,600))
                                    r=float32(Random.Next(0,359))
                                    vx=float32(RandomFloat -0.05 0.05)
                                    vy=float32(RandomFloat -0.05 0.05)
                                    vr=float32(RandomFloat -0.1 0.1)
                                    img=bigAsteroid } ]
        let mutable bullets = []
        let mutable lastTime = DateTime.Now
        let mutable lastBulletTime = DateTime.Now
        let font =  textRenderer.LoadFont window "Assets/Basic.fnt"
        

        while window.IsOpen() && not (IsKeyDown Key.ESC) do
            let currentTime = DateTime.Now
            let deltaMS = (currentTime - lastTime).Milliseconds
            if deltaMS >0 then
                let shipRV = if IsKeyDown Key.Left then -0.1f
                             elif IsKeyDown Key.Right then 0.1f
                             else 0.0f
                let shipXV = if IsKeyDown Key.SPACE then
                                ship.r
                                |> DegToRad |> sin |> fun xv -> xv*0.1f |>float32
                             else 0.0f
                             
                let shipYV = if IsKeyDown Key.SPACE then
                                ship.r
                                |> DegToRad |> cos |> fun yv -> yv* -0.1f |>float32
                             else 0.0f
            
                ship <- {ship with vr=shipRV;vx=shipXV;vy=shipYV}
                NewtonianUpdate deltaMS ship 
                |> Wrap 800.0f 600.0f |> fun x -> ship <- x
                asteroids <- List.map (fun rock ->
                    NewtonianUpdate deltaMS rock |> Wrap 800f 600f) asteroids
                let shipXform = (window.TranslationTransform ship.x ship.y).Multiply
                                 (window.RotationTransform ship.r)
                window.Clear (SysColor.Black)
                window.DrawImage shipXform  ship.img           // for asteroid in asteroids do
                asteroids
                |>Seq.iter(fun asteroid ->
                    let xform = (window.TranslationTransform asteroid.x asteroid.y)
                                |> fun x -> x.Multiply
                                                (window.TranslationTransform
                                                        (asteroid.img.Size.X /2f)
                                                        (asteroid.img.Size.Y /2f)) 
                                |> fun x -> x.Multiply (window.RotationTransform asteroid.r)
                                |> fun x -> x.Multiply
                                                (window.TranslationTransform
                                                        (-asteroid.img.Size.X /2f)
                                                        (-asteroid.img.Size.Y /2f)) 
                    window.DrawImage xform asteroid.img )
                
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
                    | Some x -> printfn "Collision detected"
                    | None -> ()   
                
                let fpsStr = "fps: "+ (1000.0f/float32 deltaMS).ToString()
                font.MakeText fpsStr
                |> fun x -> x.Draw window window.IdentityTransform
                window.Show()
                lastTime <- currentTime 
        window.Close()
        ()
            
    window.Start(logic)
   
    