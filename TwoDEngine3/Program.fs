module TwoDEngine3
open System
open Asteroids

type SysColor = System.Drawing.Color

(*   Asteriods test program by JPK *)


[<EntryPoint>]
[<STAThread>]
let main argv =
    Asteroids.Start()
    0 // return an integer exit code
    
   
