open SFML.Window
open SFML.Graphics
open SFML.System
open System.Collections.Generic
open ParticleSystemLib
open ParticleSystem
open System
open System.IO


let random = System.Random() 



[<EntryPoint>]
let main argv =
    // Create a window
    let mode = VideoMode(800u, 600u)
    let window = new RenderWindow(mode, "Particle System with SFML")
    window.SetFramerateLimit(60u)


    let timer = new Clock()
    let fpsClock = new Clock()
    let mutable frameCount = 0.0f
    let mutable timeElapsed = 0.0f
    let currentDirectory = Directory.GetCurrentDirectory()
    printfn "Current working directory: %s" currentDirectory
    let font = new Font("../../../data-control/data-latin.ttf")
    let fpsText = new Text("", font, uint32(24))
    fpsText.Position <- Vector2f(10.f, 10.f)
    fpsText.FillColor <- Color.White


    let position = Vector2f(400.0f, 300.f)
    let emissionRate = 10000.0f  // particles per second
    let mutable emissionAccumulator = 0.0f
    

    // Start the main loop
    while window.IsOpen do

        // Clear the window
        window.Clear(Color.Black)
        
        let color = Color(byte (random.Next(256)), byte (random.Next(256)), byte (random.Next(256)), 255uy)
        // Calculate elapsed time
        let str = "../../../Assets/football_small.png"

        let frameTime = timer.Restart().AsSeconds()


        frameCount <- frameCount + 1.0f
        timeElapsed <- timeElapsed + frameTime
        if timeElapsed >= 1.0f then
            let fps = frameCount / timeElapsed
            fpsText.DisplayedString <- sprintf "FPS: %.2f" fps
            frameCount <- 0.0f
            timeElapsed <- timeElapsed - 1.0f


        // Calculate the number of particles to emit this frame
        emissionAccumulator <- emissionAccumulator + (emissionRate * frameTime)

        let particlesToEmit = int emissionAccumulator

        // Emit particles based on the calculated amount
        if particlesToEmit > 0 then
            ParticleSystem.emitFromPoint(position) particlesToEmit color 1.f 3.33f 360 120.0 (Some str)
            //ParticleSystem.emitFromLine(position-(Vector2f(200.0f, 200.0f))) (position+(Vector2f(200.0f, 200.0f))) particlesToEmit color 5.f 3.f 100 20.0 (Some str)
            emissionAccumulator <- emissionAccumulator - float32 particlesToEmit
            //ParticleSystem.emitFromLine(position-(Vector2f(200.0f, 200.0f))) (position+(Vector2f(200.0f, 200.0f))) count color 5.f 3.f 100 20.0 None
            // Update particles

            
        ParticleSystem.update(1.f /  60.0f)

            // Draw particles
        ParticleSystem.draw(window)
        // Display what has been drawn
        window.Draw(fpsText)
        window.Display()

        // Handle keyboard events for closing the window
        if Keyboard.IsKeyPressed(Keyboard.Key.Escape) then
            window.Close()

    // Return an integer exit code
    0
