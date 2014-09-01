using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;

namespace AlumnoEjemplos.MiGrupo
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class Pruebas : TgcExample
    {
        TgcMesh nave;
        TgcScene isla;

        const float MOVEMENT_SPEED = 200f;
        readonly Vector3 NAVE_SCALE = new Vector3(1, 1, 1);

        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "Pruebas";
        }

        public override string getDescription()
        {
            return "Clase para probar!";
        }

        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            TgcSceneLoader loader = new TgcSceneLoader();

            //isla = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Isla\\Isla-TgcScene.xml");
            isla = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");
            
            TgcScene scene_nave = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");

            nave = scene_nave.Meshes[0];
            nave.move(0, 700, 0);

            //nave.AutoTransformEnable = false;
            
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(nave.Position, 1000, 1000);
        }

        public override void render(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //nave.Transform = Matrix.Scaling(NAVE_SCALE) * Matrix.Translation(0, 9000, 0);
            //nave.Transform = Matrix.Translation(0, 500, 0);

            TgcD3dInput input = GuiController.Instance.D3dInput;
            Vector3 movement = new Vector3(0, 0, 0);
            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                movement.X = 1;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                movement.X = -1;
            }
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                movement.Z = -1;
            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                movement.Z = 1;
            }

            //Aplicar movimiento
            movement *= MOVEMENT_SPEED * elapsedTime;
            nave.move(movement);

            GuiController.Instance.ThirdPersonCamera.Target = nave.Position;
          
            nave.render();
            isla.renderAll();
            
            //Limpiamos todas las transformaciones con la Matrix identidad
            //d3dDevice.Transform.World = Matrix.Identity;
        }

       public override void close()
        {
            isla.disposeAll();
            nave.dispose();
        }

    }
}
