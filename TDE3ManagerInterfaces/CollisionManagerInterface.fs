module TDE3ManagerInterfaces.CollisionManagerInterface

open System.Numerics

type CollisionCircle =
    { Center : Vector2
      Radius : float32 }
type CollisionRectangle =
    { Position : Vector2
      Width : float32
      Height : float32 }
type CollisionResult =
    { Collided : bool;
      CollisionPoint : Vector2;
      CollisionNormal : Vector2
      CollisionPenetration : float32 }
type CollisionGeometry =
    | CircleCollider of CollisionCircle
    | RectangleCollider of CollisionRectangle
    
  
type CollisionManager =
    abstract Collide : CollisionGeometry -> CollisionGeometry -> CollisionResult option