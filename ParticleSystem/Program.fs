open SFML.Window
open SFML.Graphics
open SFML.System
open System.Collections.Generic
open ParticleSystemLib
open ParticleSystem

//type Particle() =
//    let mutable position = Vector2f(0.f, 0.f)
//    let mutable velocity = Vector2f(0.f, 0.f)
//    let mutable color = Color.White
//    let mutable lifespan = 0.f // Lifespan in seconds

//    member this.Update (deltaTime: float) =
//        // Update particle position
//        position <- Vector2f(position.X + velocity.X * float32 deltaTime, position.Y + velocity.Y * float32 deltaTime)
//        // Decrease lifespan
//        lifespan <- lifespan - float32 deltaTime

//    member this.IsAlive = lifespan > 0.f

//    member this.Draw (window: RenderWindow) =
//        if this.IsAlive then
//            let shape = new CircleShape(5.f)
//            shape.Position <- position
//            shape.FillColor <- color
//            window.Draw(shape)

//    member this.Position with get() = position and set(value) = position <- value
//    member this.Velocity with get() = velocity and set(value) = velocity <- value
//    member this.Lifespan with get() = lifespan and set(value) = lifespan <- value
//    member this.Color with get() = color and set(value) = color <- value

//[<EntryPoint>]
//let main argv =
//    // Create a window
//    let mode = VideoMode(800u, 600u)
//    let window = new RenderWindow(mode, "Particle System with SFML")
//    window.SetFramerateLimit(60u)

//    let rand = System.Random()
//    let particles = List<Particle>()

//    // Main loop
//    while window.IsOpen do
//        if Keyboard.IsKeyPressed(Keyboard.Key.Escape) then
//            window.Close()
        
//        window.Clear(Color.Black)

//        // Add a new particle at a random position with a random velocity
//        if particles.Count < 100 then
//            let p = Particle()
//            p.Position <- Vector2f(float32 (rand.Next(0, int mode.Width)), float32 (rand.Next(0, int mode.Height)))
//            p.Velocity <- Vector2f((float32 (rand.NextDouble()) - 0.5f) * 100.f, (float32 (rand.NextDouble()) - 0.5f) * 100.f)
//            p.Lifespan <- 5.f // 5 seconds
//            particles.Add(p)

//        // Update and draw particles
//        for particle in particles do
//            particle.Update(float(1.f /  60.0f)) // Update based on frame rate
//            particle.Draw(window)

//        // Remove dead particles
//        particles.RemoveAll(fun p -> not p.IsAlive) |> ignore

//        window.Display()
//    0 

let random = System.Random()  // It's good practice to have a single instance of Random.



[<EntryPoint>]
let main argv =
    // Create a window
    let mode = VideoMode(800u, 600u)
    let window = new RenderWindow(mode, "Particle System with SFML")
    window.SetFramerateLimit(60u)

    // Create a clock to manage time
    let timer = new Clock()

    // Emit particles once, if you only want a single burst
    let count = 1000
    let position = Vector2f(400.0f, 300.f)
    let emissionRate = 10.0f  // particles per second
    let mutable emissionAccumulator = 0.0f
    

    // Start the main loop
    while window.IsOpen do

        // Clear the window
        window.Clear(Color.Black)
        
        let color = Color(byte (random.Next(256)), byte (random.Next(256)), byte (random.Next(256)), 255uy)
        // Calculate elapsed time
        let str = "E:/github/TwoDEngine3.5/TwoDEngine3/Assets/football_small.png"
        //let str = "C:/Users/Asus/Downloads/logo.png"
        //ParticleSystem.emitFromLine(position-(Vector2f(200.0f, 200.0f))) (position+(Vector2f(200.0f, 200.0f))) count color 5.f 3.f 100 20.0 None
        let frameTime = timer.Restart().AsSeconds()

        // Calculate the number of particles to emit this frame
        emissionAccumulator <- emissionAccumulator + (emissionRate * frameTime)

        let particlesToEmit = int emissionAccumulator

        // Emit particles based on the calculated amount
        if particlesToEmit > 0 then
            ParticleSystem.emitFromPoint(position) particlesToEmit color 1.f 3.33f 360 120.0 None
            //ParticleSystem.emitFromLine(position-(Vector2f(200.0f, 200.0f))) (position+(Vector2f(200.0f, 200.0f))) particlesToEmit color 5.f 3.f 100 20.0 (Some str)
            emissionAccumulator <- emissionAccumulator - float32 particlesToEmit
            //ParticleSystem.emitFromLine(position-(Vector2f(200.0f, 200.0f))) (position+(Vector2f(200.0f, 200.0f))) count color 5.f 3.f 100 20.0 None
            // Update particles

            
        ParticleSystem.update(1.f /  60.0f)

            // Draw particles
        ParticleSystem.draw(window)
        // Display what has been drawn
        window.Display()

        // Handle keyboard events for closing the window
        if Keyboard.IsKeyPressed(Keyboard.Key.Escape) then
            window.Close()

    // Return an integer exit code
    0
