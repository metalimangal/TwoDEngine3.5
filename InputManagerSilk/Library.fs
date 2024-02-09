module InputManagerSilk

open TDE3ManagerInterfaces.InputDevices
open TwoDEngine3.ManagerInterfaces.InputManager
open Silk.NET.Windowing
open Silk.NET.Input

type InputManagerSilk() as self =
    let mutable primaryKeyboard  = None
    // Create a window.
    let window  = Window.Create(WindowOptions.Default)
    do
            
            window.add_Load  (System.Action self.OnLoad)
            window.add_Update  (System.Action<float> self.OnUpdate)
            window.add_Closing (System.Action self.OnClosing)
            
            window.Run();
        
    member this.MouseDown = 
        printfn "MouseDown"
    member this.MouseUp =
        printfn "MuoseUp"
    member this.OnMouseMove mouse pos = 
        printfn "OnMouseMove"
    member this.OnMouseWheel mouse wheel =
        printfn "OnMouseWheel"
        
    member this.OnKeyDown kb key num=
         printfn "KeyDown"
    member this.OnKeyUp kb key num =
        printfn "KeyUp"
    member this.OnLoad() = 
        let input = window.CreateInput()
        primaryKeyboard <- Option.ofObj input.Keyboards.[0]
        match primaryKeyboard with
        | Some primaryKeyboard ->
            primaryKeyboard.add_KeyDown this.OnKeyDown
            primaryKeyboard.add_KeyUp this.OnKeyUp
        | None -> ()

        [|0..input.Mice.Count-1|]
        |> Array.iter (fun i ->
                        input.Mice[i].Cursor.CursorMode = CursorMode.Raw 
                        input.Mice[i].add_MouseMove this.OnMouseMove 
                        input.Mice[i].add_Scroll this.OnMouseWheel 
            )
        
    member this.OnUpdate deltaTime =
        printfn "OnUpdate"
    member this.OnClosing() = 
        printfn "OnClosing"
        window.Close()
        
    interface InputDeviceInterface with
        member this.Controllers() = 
            ()
        member this.PollState() =     
    end