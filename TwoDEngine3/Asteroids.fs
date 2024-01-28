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

type NewtonianObject = {x:float32; y:float32;r:float32;
                        vx:float32;vy:float32;vr:float32;
                        img:Image}
let NewtonianUpdate  deltaMS (obj:NewtonianObject)  =
    let x = obj.x + obj.vx * (float32 deltaMS)
    let y = obj.y + obj.vy * (float32 deltaMS)
    let r = obj.r + obj.vr * (float32 deltaMS)
    {obj with x=x;y=y;r=r}

type BulletList = {lastBulletTime:DateTime;bullets:NewtonianObject list}
type ShipType = {shipObject:NewtonianObject; bullets:BulletList}
    
type Player =
    |Ship of ShipType
    |Explosion of AnimatedImage
    |Dead
       
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
    
    let CheckFireBullet (ship:ShipType):BulletList =
        let bullets = ship.bullets
        let shipObject = ship.shipObject
        if IsKeyDown Key.SPACE then
            let currentTime = DateTime.Now
            let deltaMS = (currentTime - bullets.lastBulletTime).Milliseconds
            if deltaMS > 100 then
                let bullet = {
                    x=shipObject.x;y=shipObject.y;r=shipObject.r
                    vx=shipObject.vx;vy=shipObject.vy;vr=shipObject.vr;img=shipImage
                }
                let newBulletList = bullet::bullets.bullets
                {lastBulletTime=currentTime; bullets =newBulletList}
            else
                bullets
        else
            bullets
    
    let UpdateBullets deltaMS (bl:BulletList) =
        let newBulletList =
            bl.bullets
            |> List.map (fun bullet ->
                NewtonianUpdate deltaMS bullet)
            |> List.filter (fun bullet ->
                bullet.x > 0.0f && bullet.x < 800.0f
                && bullet.y > 0.0f && bullet.y < 600.0f)
        {lastBulletTime=bl.lastBulletTime+(TimeSpan.FromMilliseconds deltaMS)
         bullets= newBulletList}
        
    let ShipUpdate (ship:ShipType) deltaMS : ShipType =
            let shipObject = ship.shipObject
            let shipRV = if IsKeyDown Key.Left then -0.1f
                             elif IsKeyDown Key.Right then 0.1f
                             else 0.0f
            let shipXV = if IsKeyDown Key.Down then
                            shipObject.r
                            |> DegToRad |> sin |> fun xv -> xv*0.1f |>float32
                         else 0.0f
                         
            let shipYV = if IsKeyDown Key.Down then
                            shipObject.r
                            |> DegToRad |> cos |> fun yv -> yv* -0.1f |>float32
                         else 0.0f
            let bullets =
                           CheckFireBullet ship
                           |> UpdateBullets deltaMS                        
            let newShipObj:NewtonianObject = {ship.shipObject with vr=shipRV;vx=shipXV;vy=shipYV}
            NewtonianUpdate deltaMS newShipObj 
            |> Wrap 800.0f 600.0f |> fun x -> {shipObject=newShipObj;bullets=bullets}
            
                
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
                    bullets <- CheckFireBullet Player bullets
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
                let shipXform = (window.TranslationTransform Player.x Player.y).Multiply
                                 (window.RotationTransform Player.r)
                window.Clear (SysColor.Black)
                window.DrawImage shipXform  Player.img           // for asteroid in asteroids do
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
   
    