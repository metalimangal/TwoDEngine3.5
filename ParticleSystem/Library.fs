module ParticleSystem
open SFML.System
open System.Collections.Generic
open SFML.Graphics
open SFML.Window

type Particle() =
    let mutable position = Vector2f(0.f, 0.f)
    let mutable velocity = Vector2f(0.f, 0.f)
    let mutable color = Color.White
    let mutable lifespan = 0.f // Lifespan in seconds

    member this.Update (deltaTime: float) =
        // Update particle position
        position <- Vector2f(position.X + velocity.X * float32 deltaTime, position.Y + velocity.Y * float32 deltaTime)
        // Decrease lifespan
        lifespan <- lifespan - float32 deltaTime

    member this.IsAlive = lifespan > 0.f

    member this.Draw (window: RenderWindow) =
        if this.IsAlive then
            let shape = new CircleShape(5.f)
            shape.Position <- position
            shape.FillColor <- color
            window.Draw(shape)

    member this.Position with get() = position and set(value) = position <- value
    member this.Velocity with get() = velocity and set(value) = velocity <- value
    member this.Lifespan with get() = lifespan and set(value) = lifespan <- value
    member this.Color with get() = color and set(value) = color <- value

//Emitter

//Movement
//Rendering
//Collision

//Use Sprite from filesystem -Kireet

let mode = VideoMode(800u, 600u)
let rand = System.Random()

let acceleration = Vector2f(float32 1, float32 1)

type Particle2 = {
    Position : Vector2f
    Velocity : Vector2f
    Sprite : Option<Sprite>
    Color : Option<Color>
}

let loadSprite (filePath : string) =
    let texture = new Texture(filePath)
    let sprite = new Sprite(texture)
    sprite

let initializeParticles (count: int) (spriteFilePath : string option) (color : Color option) = 
    let spriteOption =
        match spriteFilePath with 
        | Some path -> Some (loadSprite path)
        | None -> None

    Array.init count (fun _ ->
        {
            Position = Vector2f(float32 (rand.Next(0, int mode.Width)), float32 (rand.Next(0, int mode.Height)))
            Velocity = Vector2f((float32 (rand.NextDouble()) - 0.5f) * 100.f, (float32 (rand.NextDouble()) - 0.5f) * 100.f)
            Sprite = spriteOption
            Color = color
        })

let update (particles : Particle2[]) (deltaTime : float32) = 
    Array.map (fun p -> 
        let newVelocity = p.Velocity + acceleration * deltaTime
        let newPosition = p.Position + newVelocity * deltaTime
        { p with Position = newPosition; Velocity = newVelocity }) particles

let draw (window : RenderWindow) (particles: Particle2[]) =
    Array.iter (fun p ->
        match p.Sprite with
        | Some sprite -> window.Draw(sprite)
        | None -> 
            match p.Color with
                | Some color ->
                    let shape = new CircleShape()
                    shape.Radius <- 5f
                    shape.FillColor <- color
                    shape.Position <- p.Position
                    window.Draw(shape)
                | None -> ()
    ) particles