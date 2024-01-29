module TwoDEngine3
open System
open AngelCodeTextRenderer
open GraphicsManagerSFML
open InputManagerWinRawInput
open SimpleCollisionManager
open TDE3ManagerInterfaces.CollisionManagerInterface
open TDE3ManagerInterfaces.GraphicsManagerInterface
open TDE3ManagerInterfaces.MouseAndKeyboardManager
open TDE3ManagerInterfaces.TextRendererInterfaces
open TDE3ManagerInterfaces.InputDevices
open System.Drawing

type SysColor = System.Drawing.Color

(*   Asteriods test program by JPK *)


    


[<EntryPoint>]
[<STAThread>]
let main argv =
    //tempoary: set up the managers
    // To be replaced with runtime dynmaic loading
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
    
    Asteroids.Start()
    0 // return an integer exit code
    
   
