namespace ParticleSystemLib

open SFML.Window
open SFML.System
open SFML.Graphics
open System.Collections.Generic
open System
open GraphicsManagerSFML
open TDE3ManagerInterfaces
open TDE3ManagerInterfaces.GraphicsManagerInterface

// Define the particle type
type Particle = {
    Position: Vector2f
    Velocity: Vector2f
    Color: Color
    Size: float32
    Lifespan: float32
    StartDelay: float32  // Delay before the particle starts "living"
    InitialLifespan: float32  // To manage effects over time
    Sprite: Option<Sprite>
}



module ParticleSystem = 
    let mutable particles : List<Particle> = List<Particle>()

    // Function to add a new particle
    let addParticle particle = 
        particles.Add(particle)

    //// Update all particles
    //let update (dt: float32) = 
    //    for i = particles.Count - 1 downto 0 do
    //        let p = particles.[i]
    //        if p.StartDelay > 0.0f then
    //            particles.[i] <- { p with StartDelay = p.StartDelay - dt }
    //        else
    //            let updatedParticle = 
    //                { p with 
    //                    Position = Vector2f(p.Position.X + p.Velocity.X * float32(dt),
    //                                    p.Position.Y + p.Velocity.Y * float32(dt));
    //                    Lifespan = p.Lifespan - dt;
    //                    Color = if p.Lifespan > 0.0f then p.Color else Color.Transparent
    //                }
    //            particles.[i] <- updatedParticle
    //            if updatedParticle.Lifespan <= 0.0f then
    //                particles.RemoveAt(i)


    let update (dt: float32) = 
        for i = particles.Count - 1 downto 0 do
            let p = particles.[i]
            if p.StartDelay <= 0.0f then
                let newPosition = Vector2f(p.Position.X + p.Velocity.X * dt,
                                       p.Position.Y + p.Velocity.Y * dt)
                let newLifespan = p.Lifespan - dt
                let newColor = if newLifespan > 0.0f then p.Color else Color.Transparent
                let updatedParticle = { p with Position = newPosition; Lifespan = newLifespan; Color = newColor }
                particles.[i] <- updatedParticle
                if newLifespan <= 0.0f then
                    particles.RemoveAt(i)

    // Draw all particles
    let draw (window: RenderWindow) = 
        particles |> Seq.iter (fun p ->
            
            if p.StartDelay <= 0.0f then
                match p.Sprite with
                | Some sprite -> window.Draw(sprite)
                | None ->
                    let radius = p.Size / 2.0f // Assuming Size is diameter, SFML expects radius
                    let shape = new CircleShape(radius) // SFML expects a float value for the radius
                    shape.Position <- p.Position
                    shape.FillColor <- p.Color
                    // Cast window to RenderWindow if needed, or directly use drawing capabilities if supported
                    //for i = particles.Count - 1 downto 0 do
                    //window.DrawImage transform particles.[i]
                    window.Draw(shape :> Drawable))

    let loadSprite (filePath : string) =
        let texture = new Texture(filePath)
        let sprite = new Sprite(texture)
        sprite


    //let drawParticle (window: TDE3ManagerInterfaces.GraphicsManagerInterface.Window) (particle: Particle) =
    //    let radius = particle.Size / 2.0f  // Assuming Size is diameter
    //    let xform =
    //        window.TranslationTransform particle.Position.X particle.Position.Y
    //        |> fun x -> x.Multiply (window.RotationTransform 0.0f)  
    //        |> fun x -> x.Multiply
    //                        (window.TranslationTransform
    //                                (-radius)  
    //                                (-radius)) 
    //    // Assuming we use a simple circle as a particle representation
    //    //let color = System.Drawing.Color.FromArgb(int particle.Color.A, int particle.Color.R, int particle.Color.G, int particle.Color.B)
    //    //window.DrawImage xform .img |> ignore
        

    //let draw2 (window: TDE3ManagerInterfaces.GraphicsManagerInterface.Window) = 
    //    for i = particles.Count - 1 downto 0 do
    //        let p = particles.[i]
    //        if p.StartDelay <= 0.0f then
    //            drawParticle window p



    // Emits particles from a specified point with varying directions
    let emitFromPoint (origin: Vector2f) count color size lifespan spreadAngle (velocityMagnitude: float) (spriteFilePath : string option) =
        let random = System.Random()

        let spriteOption =
            match spriteFilePath with 
            | Some path -> Some (loadSprite path)
            | None -> None

        for i = 1 to count do
            let angle = (random.NextDouble() * float spreadAngle - (spreadAngle / 2.0)) * Math.PI / 180.0
            let velocity = Vector2f(float32(velocityMagnitude * Math.Cos(angle)), float32(velocityMagnitude * Math.Sin(angle)))
            let particle = {
                Position = origin
                Velocity = velocity
                Color = color
                Size = size
                Lifespan = lifespan
                StartDelay = 0.0f
                InitialLifespan = lifespan
                Sprite = spriteOption
            }
            addParticle particle

    // Emits particles along a line with a specified direction and spread
    let emitFromLine (startPoint: Vector2f) (endPoint: Vector2f) count color size lifespan spreadAngle (velocityMagnitude: float) (spriteFilePath : string option) =
        let random = System.Random()
        let lineVec = Vector2f(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y)
        let lineLength = Math.Sqrt(float (lineVec.X * lineVec.X + lineVec.Y * lineVec.Y))
        for i = 1 to count do
            let positionAlongLine = random.NextDouble() * lineLength
            let position = Vector2f(startPoint.X + lineVec.X * float32 positionAlongLine / float32 lineLength,
                                    startPoint.Y + lineVec.Y * float32 positionAlongLine / float32 lineLength)
            emitFromPoint position 1 color size lifespan spreadAngle velocityMagnitude spriteFilePath
