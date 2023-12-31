module Asteroids
open System
open System.Numerics
open System.IO
open System.Runtime.InteropServices.JavaScript
open AngelCodeTextRenderer
open InputManagerWinRawInput
open GraphicsManagerSFML
open TDE3ManagerInterfaces.InputDevices
open TDE3ManagerInterfaces.GraphicsManagerInterface
open TDE3ManagerInterfaces
open TDE3ManagerInterfaces.MouseAndKeyboardManager
open TDE3ManagerInterfaces.TextRendererInterfaces
open System.Drawing
open TDE3ManagerInterfaces.WorldTree
type SysColor = System.Drawing.Color

(*   Asteriods test program by JPK *)

let Random = Random()
let RandomFloat (low:float) (high:float) =
   Random.NextDouble() * (high - low) + low

module Key =
     let Left = 2190
     let Up = 2191
     let Right = 2192
     let Down = 2193

type NewtonianObject = {x:float32; y:float32;r:float32;
                        vx:float32;vy:float32;vr:float32;
                        img:Image}
let NewtonianUpdate  deltaMS (obj:NewtonianObject)  =
    let x = obj.x + obj.vx * (float32 deltaMS)
    let y = obj.y + obj.vy * (float32 deltaMS)
    let r = obj.r + obj.vr * (float32 deltaMS)
    {obj with x=x;y=y;r=r}

let TryGetManager<'a> () =
    let manager = ManagerRegistry.getManager<'a>()
    match manager with
    | Some m -> m
    | None -> failwith "Manager not found"


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
    
    let graphics = TryGetManager<GraphicsManager> ()
    let textRenderer = TryGetManager<TextManager> ()
    let inputDeviceManager = TryGetManager<InputDeviceInterface> ()
    let window = graphics.OpenWindow (Windowed (800u,600u)) "Asteroids"
    
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
        

        while window.IsOpen() do
            let currentTime = DateTime.Now
            let deltaMS = (currentTime - lastTime).Milliseconds
            if deltaMS >0 then
                NewtonianUpdate deltaMS ship |> fun x -> ship <- x
                asteroids <- List.map (NewtonianUpdate deltaMS) asteroids
                window.Clear (SysColor.Black)
                window.DrawImage (window.TranslationTransform ship.x ship.y) ship.img           // for asteroid in asteroids do
                asteroids
                |>Seq.iter(fun asteroid ->
                    let xform = (window.TranslationTransform asteroid.x asteroid.y).Multiply
                                 (window.RotationTransform asteroid.r)
                    window.DrawImage xform asteroid.img )
                let fpsStr = "fps: "+ (1000.0f/float32 deltaMS).ToString()
                font.MakeText fpsStr
                |> fun x -> x.Draw window window.IdentityTransform
                window.Show()
                lastTime <- currentTime
        ()
            
    window.Start(logic)
   
    