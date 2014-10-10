using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.Input;
using TgcViewer.Utils;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Shaders.WorkshopShaders
{
    public class MotionBlur : TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        Effect effect;
        const int CANT_MODELOS = 8;
        TgcBox[] edicios = new TgcBox[CANT_MODELOS];
        TgcMesh arbol;
        float time = 0;

        VertexBuffer g_pVBV3D;
        Surface g_pDepthStencil;     // Depth-stencil buffer 
        Texture g_pRenderTarget;
        Texture g_pVel1, g_pVel2;   // velocidad
        Matrix antMatView;
        float R = 2000;
        int cant_edificios = 600;
        float an_actual = 0;
        float vel_angular = (float)Math.PI / 64.0f * 1.5f;
        float M_2PI = (float)2.0 * (float)Math.PI;

        // ruta
        VertexBuffer vb;
        Texture textura_piso;
        int cant_ptos_ruta = 400;

        // skybox
        TgcSkyBox skyBox;


        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-MotionBlur";
        }

        public override string getDescription()
        {
            return "Motion Effect";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            GuiController.Instance.CustomRenderEnabled = true;

            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(GuiController.Instance.D3dDevice,MyShaderDir + "MotionBlur.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";

            // Cargo los modelos de edificios
            for (int i = 0; i < CANT_MODELOS; ++i)
            {
                edicios[i] = new TgcBox();
                edicios[i].setPositionSize(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                edicios[i].setTexture(TgcTexture.createTexture(MyMediaDir + "fachadas\\pared" + i + ".jpg"));
                edicios[i].AutoTransformEnable = false;
                edicios[i].Effect = effect;
                edicios[i].updateValues();
            }


            //Camara
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.BackgroundColor = Color.Black;


            // stencil
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                                                                         d3dDevice.PresentationParameters.BackBufferHeight,
                                                                         DepthFormat.D24S8,
                                                                         MultiSampleType.None,
                                                                         0,
                                                                         true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);


            // velocidad del pixel
            g_pVel1 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.A16B16G16R16F, Pool.Default);
            g_pVel2 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.A16B16G16R16F, Pool.Default);

            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[]
		    {
    			new CustomVertex.PositionTextured( -1, 1, 1, 0,0), 
			    new CustomVertex.PositionTextured(1,  1, 1, 1,0),
			    new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
			    new CustomVertex.PositionTextured(1,-1, 1, 1,1)
    		};
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

            // Creo la ruta circular
            CrearRuta();

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, -200, 0);
            skyBox.Size = new Vector3(5000, 5000, 5000);
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox4\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "city_top.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "city_down.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "city_left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "city_right.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "city_front.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "city_back.jpg");
            skyBox.updateValues();

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene3 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\ArbolSelvatico\\ArbolSelvatico-TgcScene.xml");
            arbol = scene3.Meshes[0];
            arbol.Scale = new Vector3(0.02f,0.05f,0.02f);


            GuiController.Instance.Modifiers.addBoolean("motion_blur", "Activar MB", true);
            GuiController.Instance.Modifiers.addBoolean("vel_map", "Mapa Velocidad", false);
            GuiController.Instance.Modifiers.addFloat("factor_blur", 0.01f, 0.5f, 0.05f);
            GuiController.Instance.Modifiers.addFloat("velocidad", 0.1f, 5f, 1.5f);
            GuiController.Instance.Modifiers.addBoolean("paused", "Pausar", false);

        }


        public void CrearRuta()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            // Cargo la ruta
            int totalVertices = cant_ptos_ruta * 3;
            //Crear vertexBuffer
            vb = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            int dataIdx = 0;
            CustomVertex.PositionTextured[] data = new CustomVertex.PositionTextured[totalVertices];

            float r0 = R + 16;
            float r1 = R - 16;
            float Kr = 0.5f;

            for (int i = 0; i < cant_ptos_ruta; ++i)
            {
                float an = 2.0f * (float)Math.PI * (float)i / (float)cant_ptos_ruta;
                Vector3 p0 = new Vector3(r0 * (float)Math.Sin(an), 0, r0 * (float)Math.Cos(an));
                Vector3 p1 = new Vector3(r1 * (float)Math.Sin(an), 0, r1 * (float)Math.Cos(an));
                data[dataIdx++] = new CustomVertex.PositionTextured(p0, 1, i * Kr);
                data[dataIdx++] = new CustomVertex.PositionTextured(p1, 0, i * Kr);
            }
            vb.SetData(data, 0, LockFlags.None);

            // Cargo la textura del piso 
            textura_piso = Texture.FromBitmap(d3dDevice, (Bitmap)Bitmap.FromFile(MyMediaDir + "f1\\piso3.png"), Usage.None, Pool.Managed);

        }


        public void update(float elapsedTime)
        {
            time += elapsedTime;
            vel_angular = (float)Math.PI / 64.0f * (float)GuiController.Instance.Modifiers["velocidad"];
            Device device = GuiController.Instance.D3dDevice;
            antMatView = device.Transform.View;

            an_actual += vel_angular * elapsedTime;
            if (an_actual > M_2PI)
                an_actual -= M_2PI;

            Vector3 LF = new Vector3(R * (float)Math.Sin(an_actual), 4, R * (float)Math.Cos(an_actual));
            Vector3 LA = new Vector3(R * (float)Math.Sin(an_actual + 0.1f), 2, R * (float)Math.Cos(an_actual + 0.1f));
            GuiController.Instance.FpsCamera.setCamera(LF, LA);
            GuiController.Instance.FpsCamera.updateCamera();

        }

        public void renderScene(String technique)
        {
            Device device = GuiController.Instance.D3dDevice;

            // Render Skybox
            skyBox.render();

            // Render Edificios
            // -----------------------------------------------------------------------
            Random rnd = new Random(1);
            float r0 = R + 22;
            float r1 = R - 22;
            for (int i = 0; i < cant_edificios; ++i)
            {
                float an = 2.0f * (float)Math.PI * (float)i / (float)cant_edificios;
                float h0 = rnd.Next(10, 40);
                float h1 = rnd.Next(10, 40);

                if (an >= an_actual && an <= an_actual + 0.5f)
                {
                    int t = (2 * i) % CANT_MODELOS;
                    edicios[t].Transform = Matrix.Scaling(15, h0, 15) *
                                Matrix.RotationYawPitchRoll(an, 0, 0) *
                                Matrix.Translation(r0 * (float)Math.Sin(an), h0/2, r0 * (float)Math.Cos(an));
                    edicios[t].Technique = technique;
                    edicios[t].render();

                    t = (2 * i + 1) % CANT_MODELOS;
                    edicios[t].Transform = Matrix.Scaling(15, h1, 15) *
                                Matrix.RotationYawPitchRoll(an, 0, 0) *
                                Matrix.Translation(r1 * (float)Math.Sin(an), h1 / 2, r1 * (float)Math.Cos(an));
                    edicios[t].Technique = technique;
                    edicios[t].render();
                }

            }

            // Render ruta
            // -----------------------------------------------------------------------
            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, vb, 0);
            effect.SetValue("texDiffuseMap", textura_piso);
            int numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2 * (cant_ptos_ruta - 1));
                effect.EndPass();
            }
            effect.End();

            arbol.Effect = effect;
            arbol.Technique = technique;
            int cant_arboles = 200;
            r0 = R + 10;
            r1 = R - 10;
            for (int i = 0; i < cant_arboles; ++i)
            {
                float an = 2.0f * (float)Math.PI * (float)i / (float)cant_arboles;
                if (an >= an_actual && an <= an_actual + 0.5f)
                {
                    arbol.Position = new Vector3(r1 * (float)Math.Sin(an), -2, r1 * (float)Math.Cos(an));
                    arbol.render();
                    arbol.Position = new Vector3(r0 * (float)Math.Sin(an), -2, r0 * (float)Math.Cos(an));
                    arbol.render();
                }
            }
        }


        public override void render(float elapsedTime)
        {

            if (!(bool)GuiController.Instance.Modifiers["paused"])
            {
                update(elapsedTime);
            }

            Device device = GuiController.Instance.D3dDevice;

            if (!(bool)GuiController.Instance.Modifiers["motion_blur"])
            {
                // dibujar sin motion blur
                effect.Technique = "DefaultTechnique";
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                device.BeginScene();
                renderScene("DefaultTechnique");
                device.EndScene();
                return;
            }

            effect.SetValue("PixelBlurConst", (float)GuiController.Instance.Modifiers["factor_blur"]);

            // guardo el Render target anterior y seteo la textura como render target
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pVel1.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;

            // 1 - Genero un mapa de velocidad 
            effect.Technique = "VelocityMap";
            // necesito mandarle la matrix de view actual y la anterior
            effect.SetValue("matView", device.Transform.View);
            effect.SetValue("matViewAnt", antMatView);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            renderScene("VelocityMap");
            device.EndScene();
            pSurf.Dispose();


            // 2- Genero la imagen pp dicha 
            effect.Technique = "DefaultTechnique";
            pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            if((bool)GuiController.Instance.Modifiers["vel_map"])
            {
                effect.Technique = "DrawGrid";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, g_pVBV3D, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
            }
            else
            {
                device.BeginScene();
                renderScene("DefaultTechnique");
                device.EndScene();
                pSurf.Dispose();
            }

            // Ultima pasada vertical va sobre la pantalla pp dicha
            device.SetRenderTarget(0, pOldRT);
            device.DepthStencilSurface = pOldDS;
            device.BeginScene();
            effect.Technique = "PostProcessMotionBlur";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            effect.SetValue("texVelocityMap", g_pVel1);
            effect.SetValue("texVelocityMapAnt", g_pVel2);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            device.EndScene();

            // actualizo los valores para el proximo frame
            //antMatWorldView = mesh.Transform* device.Transform.View;
            Texture aux = g_pVel2;
            g_pVel2 = g_pVel1;
            g_pVel1 = aux;

            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);

        }




        public override void close()
        {
            g_pRenderTarget.Dispose();
            g_pDepthStencil.Dispose();
            g_pVBV3D.Dispose();
            g_pVel1.Dispose();
            g_pVel2.Dispose();
            arbol.dispose();
            skyBox.dispose();

        }
    }



}
