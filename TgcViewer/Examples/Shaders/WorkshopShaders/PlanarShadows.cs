using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Shaders;

namespace Examples.Shaders.WorkshopShaders
{
    public class PlanarShadows: TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        TgcScene scene, scene2;
        TgcBox box;
        Effect effect;
        TgcMesh avion;

        // Shadow map
        Vector3 g_LightPos;						// posicion de la luz actual (la que estoy analizando)
        float near_plane = 2f;
        float far_plane = 1500f;

        Vector3 dir_avion;
        float time;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-PlanarShadows";
        }

        public override string getDescription()
        {
            return "Planar Shadows";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";
            GuiController.Instance.CustomRenderEnabled = true;
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            // ------------------------------------------------------------
            //Cargar la escena
            scene = loader.loadSceneFromFile(MyMediaDir
                    + "shadowTest\\ShadowTest-TgcScene.xml");

            scene2 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir
                    + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            avion = scene2.Meshes[0];

            avion.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            avion.Position = new Vector3(100f, 100f, 0f);
            avion.AutoTransformEnable = false;
            dir_avion = new Vector3(0, 0, 1);

            GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 0, 0);
            GuiController.Instance.RotCamera.CameraDistance = 50;
            GuiController.Instance.RotCamera.RotationSpeed = 50f;
            GuiController.Instance.RotCamera.updateCamera();

            //Cargar Shader personalizado
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\PlanarShadows.fx");

            // le asigno el efecto a las mallas 
            foreach (TgcMesh T in scene.Meshes)
            {
                T.Scale = new Vector3(1f, 1f, 1f);
                T.Effect = effect;
            }
            avion.Effect = effect;


            box = new TgcBox();
            box.Color = Color.Yellow;
            
            GuiController.Instance.RotCamera.targetObject(scene.Meshes[0].BoundingBox);
            float K = 300;
            GuiController.Instance.Modifiers.addVertex3f("LightLookFrom", new Vector3(-K, -K, -K), new Vector3(K, K, K), new Vector3(80, 120, 0));

        }


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;
            time += elapsedTime;
            // animo la pos del avion
            float alfa = -time * Geometry.DegreeToRadian(115.0f);
            avion.Position = new Vector3(80f * (float)Math.Cos(alfa), 20-20*(float)Math.Sin(alfa), 80f * (float)Math.Sin(alfa));
            dir_avion = new Vector3(-(float)Math.Sin(alfa), 0, (float)Math.Cos(alfa));
            avion.Transform = CalcularMatriz(avion.Position, avion.Scale, dir_avion);
            g_LightPos = (Vector3)GuiController.Instance.Modifiers["LightLookFrom"];

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            // dibujo la escena pp dicha
            device.BeginScene();
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            // piso
            scene.Meshes[0].Technique = "RenderScene";
            scene.Meshes[0].render();

            // dibujo las sombra del avion sobre el piso
            effect.SetValue("matViewProj", device.Transform.View * device.Transform.Projection);
            effect.SetValue("g_vLightPos", new Vector4(g_LightPos.X,g_LightPos.Y,g_LightPos.Z,1));
            device.RenderState.ZBufferEnable = false;
            avion.Technique = "RenderShadows";
            avion.render();
            device.RenderState.ZBufferEnable = true;

            // avion
            avion.Technique = "RenderScene";
            avion.render();

            // dibujo la luz
            box.setPositionSize(g_LightPos, new Vector3(5, 5, 5));
            box.updateValues();
            box.render();

            device.EndScene();


        }


        // helper
        public Matrix CalcularMatriz(Vector3 Pos, Vector3 Scale, Vector3 Dir)
        {
            Vector3 VUP = new Vector3(0, 1, 0);

            Matrix matWorld = Matrix.Scaling(Scale);
            // determino la orientacion
            Vector3 U = Vector3.Cross(VUP, Dir);
            U.Normalize();
            Vector3 V = Vector3.Cross(Dir, U);
            Matrix Orientacion;
            Orientacion.M11 = U.X;
            Orientacion.M12 = U.Y;
            Orientacion.M13 = U.Z;
            Orientacion.M14 = 0;

            Orientacion.M21 = V.X;
            Orientacion.M22 = V.Y;
            Orientacion.M23 = V.Z;
            Orientacion.M24 = 0;

            Orientacion.M31 = Dir.X;
            Orientacion.M32 = Dir.Y;
            Orientacion.M33 = Dir.Z;
            Orientacion.M34 = 0;

            Orientacion.M41 = 0;
            Orientacion.M42 = 0;
            Orientacion.M43 = 0;
            Orientacion.M44 = 1;
            matWorld = matWorld * Orientacion;

            // traslado
            matWorld = matWorld * Matrix.Translation(Pos);
            return matWorld;
        }


        public override void close()
        {
            effect.Dispose();
            scene.disposeAll();
            scene2.disposeAll();
        }            
    }

}
