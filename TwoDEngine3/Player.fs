module Player 

open System
open TDE3ManagerInterfaces.GraphicsManagerInterface
open TDE3ManagerInterfaces.InputDevices
open NewtonianObject
open ImageExtensions
open ManagerUtils
open TwoDEngine3.ManagerInterfaces


type ShipType = {shipObject:NewtonianObject; lastFired:DateTime}
   
type ExplosionType = {img:AnimatedImage;x:float32;y:float32;}    
type Player =
    |Ship of ShipType
    |Explosion of ExplosionType
    |Dead
    
let DegToRad (deg:float32) = deg * (float32 Math.PI) / 180.0f
    
module Key =
     let Left = 37
     let Up = 38
     let Right = 39
     let Down = 40
     let ESC = 27
     let SPACE = 32
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

module Player =
     let CheckFireBullet (ship:ShipType)  =
        let shipObject = ship.shipObject
        if Key.IsKeyDown Key.SPACE then 
            let currentTime = DateTime.Now
            let deltaMS = (currentTime - ship.lastFired).Milliseconds
            if deltaMS > 500 then
                true
            else
                false
        else
            false
     
        
     let ShipUpdate (ship:ShipType) deltaMS  : ShipType =
        let shipObject = ship.shipObject
        let shipRV = if Key.IsKeyDown Key.Left then -0.1f
                         elif Key.IsKeyDown Key.Right then 0.1f
                         else 0.0f
        let shipXV = if Key.IsKeyDown Key.Down then
                        shipObject.r
                        |> DegToRad |> sin |> fun xv -> xv*0.1f |>float32
                     else 0.0f
                     
        let shipYV = if Key.IsKeyDown Key.Down then
                        shipObject.r
                        |> DegToRad |> cos |> fun yv -> yv* -0.1f |>float32
                     else 0.0f
                          
        {ship.shipObject with vr=shipRV;vx=shipXV;vy=shipYV}
        |> NewtonianObject.NewtonianUpdate deltaMS  
        |> NewtonianObject.Wrap 800.0f 600.0f |> fun shipObj -> {ship with shipObject = shipObj }
        
    
     let Update (player:Player) deltaMS bulletImg : Player =
        match player with
        | Ship ship ->  ShipUpdate ship deltaMS  |> Ship
                         
        | Explosion expl -> AnimatedImage.update (uint32 deltaMS) expl.img
                            |> fun newImg ->
                                if newImg.Playing then
                                    Explosion {expl with img=newImg}
                                else
                                    Dead                   
        | Dead -> Dead



