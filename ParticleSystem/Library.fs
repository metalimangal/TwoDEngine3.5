namespace ParticleSystemLib

open OpenCL.Net
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
    let getPlatforms () =
        let errorCode = ref ErrorCode.Success  // Initialize a reference to an ErrorCode
        let platforms = Cl.GetPlatformIDs(errorCode)  // Pass the reference to GetPlatformIDs
        if errorCode.Value <> ErrorCode.Success then
            failwithf "Failed to get OpenCL platforms. ErrorCode: %A" !errorCode
        platforms

    // Using the getPlatforms function
    let platforms = getPlatforms()

    let platformId = platforms.[0]
    let getDeviceIds () = 
        let errorCode = ref ErrorCode.Success
        let deviceIds = Cl.GetDeviceIDs(platformId, DeviceType.Gpu, errorCode)
        if errorCode.Value <> ErrorCode.Success then
            failwithf "Failed to get OpenCL deviceIds. ErrorCode: %A" errorCode.Value
        deviceIds
    let deviceIds = getDeviceIds()
    let deviceId = deviceIds.[0]
    let getContext () = 
        let errorCode = ref ErrorCode.Success
        let context = Cl.CreateContext(null, 1u, [|deviceId|], null, IntPtr.Zero, errorCode)
        if errorCode.Value <> ErrorCode.Success then
            failwithf "Failed to get OpenCL context. ErrorCode: %A" errorCode.Value
        context
    let context = getContext()
    let queue = Cl.CreateCommandQueue(context, deviceId, CommandQueueProperties.None)
    let programSource = System.IO.File.ReadAllText("E:/github/TwoDEngine3.5/ParticleSystem/particle_update.cl")
    let getProgram () = 
        let errorCode = ref ErrorCode.Success
        let program = Cl.CreateProgramWithSource(context, 1u, [|programSource|], [|programSource.Length|], errorCode)
        if errorCode.Value <> ErrorCode.Success then
            failwithf "Failed to get OpenCL program. ErrorCode: %A" errorCode.Value
        program
    let program = getProgram()
    let _ = Cl.BuildProgram(program, 1u, [|deviceId|], null, null, IntPtr.Zero)
    let getKernel () = 
        let errorCode = ref ErrorCode.Success
        let kernel = Cl.CreateKernel(program, "update_particles", errorCode)
        if errorCode.Value <> ErrorCode.Success then
            failwithf "Failed to get OpenCL kernel. ErrorCode: %A" errorCode.Value
        kernel
    let kernel = getKernel()

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
        if particles.Count>0 then
            // Prepare data
            let positions = particles |> Seq.map (fun p -> p.Position) |> Array.ofSeq
            let velocities = particles |> Seq.map (fun p -> p.Velocity) |> Array.ofSeq
            let lifespans = particles |> Seq.map (fun p -> p.Lifespan) |> Array.ofSeq
            let getQueue () = 
                let errorCode = ref ErrorCode.Success
                let queue = Cl.CreateCommandQueue(context, deviceId, CommandQueueProperties.None, errorCode)
                if errorCode.Value <> ErrorCode.Success then
                    failwithf "Failed to get OpenCL queue. ErrorCode: %A" errorCode.Value
                queue
            let queue = getQueue()
            // Create buffers
            let getPosBuffer () = 
                let errorCode = ref ErrorCode.Success
                let posBuffer = Cl.CreateBuffer(context, MemFlags.CopyHostPtr, positions, errorCode)
                if errorCode.Value <> ErrorCode.Success then
                    failwithf "Failed to get OpenCL posBuffer. ErrorCode: %A" errorCode.Value
                posBuffer
            let posBuffer = getPosBuffer()
            let getVelBuffer () = 
                let errorCode = ref ErrorCode.Success
                let velBuffer = Cl.CreateBuffer(context, MemFlags.CopyHostPtr, velocities, errorCode)
                if errorCode.Value <> ErrorCode.Success then
                    failwithf "Failed to get OpenCL velBuffer. ErrorCode: %A" errorCode.Value
                velBuffer
            let velBuffer = getVelBuffer()
            let getLifeBuffer () = 
                let errorCode = ref ErrorCode.Success
                let lifeBuffer = Cl.CreateBuffer(context, MemFlags.CopyHostPtr, lifespans, errorCode)
                if errorCode.Value <> ErrorCode.Success then
                    failwithf "Failed to get OpenCL Life Buffer. ErrorCode: %A" errorCode.Value
                lifeBuffer
            let lifeBuffer = getLifeBuffer()

            // Set kernel arguments
            Cl.SetKernelArg(kernel, uint32(0), posBuffer) |> ignore
            Cl.SetKernelArg(kernel, uint32(1), velBuffer) |> ignore
            Cl.SetKernelArg(kernel, uint32(2), lifeBuffer) |> ignore
            Cl.SetKernelArg(kernel, uint32(3), dt) |> ignore

            // Execute the kernel
            let globalWorkSize = [| nativeint particles.Count |]
            Cl.EnqueueNDRangeKernel(queue, kernel, 1u, null, globalWorkSize, null, 0u, null) |> ignore
            let readVal: Bool = Bool.True

            // Buffer sizes
            let posBufferSize = nativeint (positions.Length * sizeof<float32> * 2)
            let lifeBufferSize = nativeint (lifespans.Length * sizeof<float32>)
        

            // Reading position buffer
            let posReadStatus = Cl.EnqueueReadBuffer(
                queue,
                posBuffer,
                readVal,
                0,  // Offset as nativeint
                posBufferSize,  // Buffer size as nativeint
                positions,  // Data array
                0u,  // numEventsInWaitList
                null  // eventWaitList
            )

            // Reading lifespan buffer
            let lifeReadStatus = Cl.EnqueueReadBuffer(
                queue,
                lifeBuffer,
                readVal,
                nativeint 0,  // Offset
                lifeBufferSize,
                lifespans,
                0u,  // numEventsInWaitList
                null // eventWaitList
            )

            

            // Update local particle list with new data
            for i in 0 .. particles.Count - 1 do
                particles.[i] <- { particles.[i] with Position = positions.[i]; Lifespan = lifespans.[i] }

            particles <- List<Particle>((particles |>   Seq.filter (fun p -> p.Lifespan > 0.0f)))


            // Release resources
            Cl.ReleaseMemObject(posBuffer) |> ignore
            Cl.ReleaseMemObject(velBuffer) |> ignore
            Cl.ReleaseMemObject(lifeBuffer) |> ignore


    // Draw all particles
    let draw (window: RenderWindow) =
        if particles.Count > 0 then
            particles |> Seq.iter (fun p ->
                if p.StartDelay <= 0.0f then
                    match p.Sprite with
                    | Some sprite ->
                        // Define your desired scale factors here
                        let desiredScaleX = p.Size*0.01f  // Example: Scale down to 50% of the original width
                        let desiredScaleY = p.Size*0.01f  // Example: Scale down to 50% of the original height
                        sprite.Scale <- Vector2f(desiredScaleX, desiredScaleY)
                        sprite.Position <- p.Position
                        sprite.Color <- p.Color
                        window.Draw(sprite :> Drawable)  // Draw the scaled sprite
                    | None ->
                        // Assuming Size is diameter and SFML expects radius
                        let radius = p.Size / 2.0f
                        let shape = new CircleShape(radius)
                        shape.Position <- p.Position
                        shape.FillColor <- p.Color
                        window.Draw(shape :> Drawable))  // Draw the shape if there is no sprite


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
