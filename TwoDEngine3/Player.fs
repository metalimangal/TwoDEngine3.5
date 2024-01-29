module TwoDEngine3.Player

open System
open TDE3ManagerInterfaces.GraphicsManagerInterface
open TDE3ManagerInterfaces.InputDevices
open TwoDEngine3.NewtonianObject
open ImageExtensions
open ManagerUtils
open TwoDEngine3.ManagerInterfaces

type BulletList = {lastBulletTime:DateTime;bullets:NewtonianObject list}
type ShipType = {shipObject:NewtonianObject; bullets:BulletList}
    
type Player =
    |Ship of ShipType
    |Explosion of AnimatedImage
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
     let CheckFireBullet (ship:ShipType) (bulletImg:Image):BulletList =
        let bullets = ship.bullets
        let shipObject = ship.shipObject
        if IsKeyDown Key.SPACE then
            let currentTime = DateTime.Now
            let deltaMS = (currentTime - bullets.lastBulletTime).Milliseconds
            if deltaMS > 100 then
                let bullet = {
                    x=shipObject.x;y=shipObject.y;r=shipObject.r
                    vx=shipObject.vx;vy=shipObject.vy;vr=shipObject.vr;img=bulletImg}
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
                NewtonianObject.NewtonianUpdate deltaMS bullet)
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
            let bullets =  Player.CheckFireBullet ship shipObject.img
                           |> Player.UpdateBullets deltaMS                        
            let newShipObj:NewtonianObject = {ship.shipObject with vr=shipRV;vx=shipXV;vy=shipYV}
            NewtonianObject.NewtonianUpdate deltaMS newShipObj 
            |> NewtonianObject.Wrap 800.0f 600.0f |> fun x -> {shipObject=newShipObj;bullets=bullets}
    



