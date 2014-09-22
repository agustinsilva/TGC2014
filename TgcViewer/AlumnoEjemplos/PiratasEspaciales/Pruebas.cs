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

        #region Declaración

        const float NAVE_MOVEMENT_SPEED = 200f;
        const float NAVE_ROTATION_SPEED = 30f;
        readonly Vector3 NAVE_SCALE = new Vector3(0.2f, 0.2f, 0.2f);
        
        TgcMesh nave;
        TgcMesh[] planetas = new TgcMesh[10];
        //TgcScene universo;

        #endregion

        #region Categoria, Nombre y Descripción.
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
        #endregion

        #region Init



        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            TgcSceneLoader loader = new TgcSceneLoader();

            //universo = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Universo\\Universo-TgcScene.xml");
            
            TgcScene scene_nave = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");

            for (int i = 0; i < 10; i++)
            {
                planetas[i] = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml").Meshes[0];
                //planetas[i].AutoTransformEnable = false;

                Random rnd = new Random();

                planetas[i].Scale *= rnd.Next(10);

                float x = ((float)rnd.NextDouble()) * 1000;
                float z = ((float)rnd.NextDouble()) * 1000;

                planetas[i].Position = new Vector3(x, 0, z);
            }

            nave = scene_nave.Meshes[0];
            nave.Scale = NAVE_SCALE;
                        
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(nave.Position, 10, 30);
            //GuiController.Instance.ThirdPersonCamera.setCamera(planetas[1].Position, 300, 300);

            GuiController.Instance.BackgroundColor = Color.Black;
        }



        #endregion

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
                rotate = -NAVE_ROTATION_SPEED;
                rotating = true;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                rotate = NAVE_ROTATION_SPEED;
                rotating = true;
            }
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                moveForward = -NAVE_MOVEMENT_SPEED;
                moving = true;
            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                moveForward = NAVE_MOVEMENT_SPEED;
                moving = true;
            }

            Vector3 Position_prev = nave.Position;

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

            bool Collision = false;

            foreach (TgcMesh planeta in planetas)
            {
                TgcBoundingBox nave_BBox = nave.BoundingBox;
                TgcBoundingBox planeta_BBox = planeta.BoundingBox;

                TgcCollisionUtils.BoxBoxResult Collision_Type = TgcCollisionUtils.classifyBoxBox(nave_BBox, planeta_BBox);

                if (Collision_Type != TgcCollisionUtils.BoxBoxResult.Afuera)
                {
                    Collision = true;
                    break;
                }
            }

            if (Collision)
            {
                ;
            }

            #endregion

            #region Camara

            GuiController.Instance.ThirdPersonCamera.Target = nave.Position;

            #endregion

            #region Render

            nave.render();
            for (int i = 0; i < 10; i++)
            {
                planetas[i].render();
            }
            //universo.renderAll();

            nave.BoundingBox.render();
            foreach (TgcMesh planeta in planetas)
            {
                planeta.BoundingBox.render();
            }

            #endregion

        }

        #endregion

        #region Close / Dispose

        public override void close()
        {
            //universo.disposeAll();
            nave.dispose();
            for (int i = 0; i < 10; i++)
            {
                planetas[i].dispose();
            }
        }

        #endregion
    }
}
