module TwoDEngine3.AsteroidsLevel

open System.IO
open System.Numerics
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TDE3ManagerInterfaces.RenderTree



type AsteroidsLevel() =
     inherit AbstractLevelController()
     let mutable ship = None
     override this.Open() =
        base.Open()
        use filestream = File.Open("Assets/asteroids-arcade.png", FileMode.Open)
        let atlas = this.graphics.Value.LoadImage filestream
                        
        let shipImage = atlas.SubImage (
                            Rectangle(
                                Vector2(3f, 2f),
                                Vector2(25f, 30f)
                            )
                         )
        let rendertree =RENDERTREE this.graphics.Value [
            SPRITE shipImage []
        ]
        renderTree.Draw
       
        ()
        