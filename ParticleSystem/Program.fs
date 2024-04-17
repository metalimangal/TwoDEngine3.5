open SFML.Window
open SFML.Graphics
open SFML.System
open System.Collections.Generic
open ParticleSystem


[<EntryPoint>]
let main argv =
    // Create a window
    let mode = VideoMode(800u, 600u)
    let window = new RenderWindow(mode, "Particle System with SFML")
    window.SetFramerateLimit(60u)

    let rand = System.Random()
    let mutable lastFrameTime = Clock()

    // Main loop
    let rec mainLoop particles=

        printfn "Entered loop"
        let currentTime = Clock().ElapsedTime.AsSeconds()
        let deltaTime = currentTime - lastFrameTime.ElapsedTime.AsSeconds()
        lastFrameTime.Restart()
        
        let updatedParticles = ParticleSystem.update particles deltaTime

        window.Clear()

        ParticleSystem.draw window updatedParticles

        window.Display()

        mainLoop updatedParticles


    let initialParticles = initializeParticles 100 None (Some Color.Red)
    try
        mainLoop initialParticles
    with
    | ex -> printfn "An exception occurred: %s" ex.Message
    0
