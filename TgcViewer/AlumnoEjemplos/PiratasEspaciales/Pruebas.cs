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
    public class Pruebas : TgcExample
    {
        #region Inicializacion

        const float MOVEMENT_SPEED = 200f;
        const float ROTATION_SPEED = 30f;
        readonly Vector3 NAVE_SCALE = new Vector3(0.2f, 0.2f, 0.2f);
        
        TgcMesh nave;
        TgcScene universo;

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

            universo = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Universo\\Universo-TgcScene.xml");
            
            TgcScene scene_nave = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");

            nave = scene_nave.Meshes[0];
            nave.Scale = NAVE_SCALE;
                        
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(nave.Position, 300, 300);
        }

        #endregion

        #region Renderizado

        public override void render(float elapsedTime)
        {
            #region Controller

            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            #endregion

            #region Transformaciones

            //nave.Transform = Matrix.Scaling(NAVE_SCALE) * Matrix.Translation(0, 9000, 0);
            //nave.Transform = Matrix.Translation(0, 500, 0);
            //nave.Transform = Matrix.Scaling(NAVE_SCALE);

            #endregion

            #region Movimiento y Rotacion

            bool moving = false;
            bool rotating = false;
            float moveForward = 0f;
            float rotate = 0;
            TgcD3dInput input = GuiController.Instance.D3dInput;

            //Vector3 movement = new Vector3(0, 0, 0);
            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                rotate = -ROTATION_SPEED;
                rotating = true;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                rotate = ROTATION_SPEED;
                rotating = true;
            }
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                moveForward = -MOVEMENT_SPEED;
                moving = true;
            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                moveForward = MOVEMENT_SPEED;
                moving = true;
            }


            if (rotating)
            {
                nave.rotateY(Geometry.DegreeToRadian(rotate * elapsedTime));
                GuiController.Instance.ThirdPersonCamera.rotateY(Geometry.DegreeToRadian(rotate * elapsedTime));
            }
            
            if (moving)
            {
                Vector3 lastPos = nave.Position;
                nave.moveOrientedY(moveForward * elapsedTime);
            }
                        
            //Aplicar movimiento (VIEJO)
            //movement *= MOVEMENT_SPEED * elapsedTime;
            //nave.move(movement);

            #endregion

            #region Camara

            GuiController.Instance.ThirdPersonCamera.Target = nave.Position;

            #endregion

            #region Render

            nave.render();
            universo.renderAll();

            #endregion

        }

        #endregion

        #region Close / Dispose

        public override void close()
        {
            universo.disposeAll();
            nave.dispose();
        }

        #endregion
    }
}
