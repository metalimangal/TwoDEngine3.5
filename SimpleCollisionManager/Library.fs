namespace SimpleCollisionManager

open System.Numerics
open ManagerRegistry
open TDE3ManagerInterfaces.CollisionManagerInterface

[<Manager("A simple spherical collision detector",
          supportedSystems.Windows ||| supportedSystems.Linux ||| supportedSystems.Mac |||
          supportedSystems.Wasm)>]
type SimpleCollisionManager() =
    interface CollisionManager with
        override this.Collide  (collider1:CollisionGeometry) (collider2:CollisionGeometry) : CollisionResult option =
            match collider1 with
            | CircleCollider c ->
                match collider2 with
                | CircleCollider c2 ->
                    let collisionVector = Vector2.Subtract( c2.Center, c.Center)
                    if collisionVector.LengthSquared() < (c.Radius + c2.Radius) * (c.Radius + c2.Radius) then
                        let penetration = (c.Radius + c2.Radius) - collisionVector.Length()
                        let collisionNormal = Vector2.Normalize(collisionVector)
                        let collisionPoint = Vector2.Multiply(collisionNormal, c.Radius)
                        Some {Collided=true; CollisionPoint=collisionPoint;CollisionNormal=collisionNormal
                              CollisionPenetration = penetration }
                    else
                        None
                | _ -> failwith "Collision not implemented"
            | _ -> failwith "Collision not implemented"
    