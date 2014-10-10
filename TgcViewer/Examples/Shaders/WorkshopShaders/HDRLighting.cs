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
using TgcViewer.Utils;
using TgcViewer.Utils.TgcSkeletalAnimation;

namespace Examples.Shaders.WorkshopShaders
{

    public class HDRLighting: TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        List<TgcMesh> meshes;
        TgcSkyBox skyBox;
        TgcSimpleTerrain terrain;
        TgcMesh pasto, arbol, arbusto;
        Effect effect;
        Surface g_pDepthStencil;     // Depth-stencil buffer 
        Texture g_pRenderTarget, g_pGlowMap, g_pRenderTarget4, g_pRenderTarget4Aux;
        
        const int NUM_REDUCE_TX = 5;
        Texture [] g_pLuminance = new Texture[NUM_REDUCE_TX];
        Texture g_pLuminance_ant;

        VertexBuffer g_pVBV3D;
        int cant_pasadas = 5;

        float pupila_time = 0;
        float MAX_PUPILA_TIME = 3;

        public enum ToneMapping : int
        {
            Nada = 0,
            Reinhard = 1,
            Modified_Reinhard = 2,
            Logaritmico = 3,
            MiddleGray = 4 
        };


        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-HdrLighting";
        }

        public override string getDescription()
        {
            return "HDR lighting";
        }


        public override void init()
        {
            GuiController.Instance.CustomRenderEnabled = true;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Cargamos un escenario
            TgcSceneLoader loader = new TgcSceneLoader();

            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Selva\\Selva-TgcScene.xml");
            meshes = scene.Meshes;
            TgcScene scene2 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pasto\\Pasto-TgcScene.xml");
            pasto = scene2.Meshes[0];
            TgcScene scene3 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\ArbolSelvatico\\ArbolSelvatico-TgcScene.xml");
            arbol = scene3.Meshes[0];
            arbol.Scale = new Vector3(1, 3, 1);
            TgcScene scene4 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Arbusto2\\Arbusto2-TgcScene.xml");
            arbusto = scene4.Meshes[0];

            //Cargar terreno: cargar heightmap y textura de color
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(MyMediaDir + "Heighmaps\\" + "TerrainTexture2.jpg",
                    20, 0.3f, new Vector3(0, -115, 0));
            terrain.loadTexture(MyMediaDir + "Heighmaps\\" + "grass.jpg");

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 500, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox2\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lun4_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lun4_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lun4_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lun4_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lun4_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lun4_ft.jpg");
            skyBox.updateValues();



            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(GuiController.Instance.D3dDevice,
                GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\GaussianBlur.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";

            //Camara en primera personas
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-944.1269f, 50, -1033.307f), new Vector3(-943.6573f, 50.8481f, -1033.533f));
            GuiController.Instance.FpsCamera.MovementSpeed *= 2;
            GuiController.Instance.FpsCamera.JumpSpeed = 600f;
            GuiController.Instance.FpsCamera.RotationSpeed *= 4;
                   
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                                                                         d3dDevice.PresentationParameters.BackBufferHeight,
                                                                         DepthFormat.D24S8,
                                                                         MultiSampleType.None,
                                                                         0,
                                                                         true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.A16B16G16R16F, Pool.Default);

            g_pGlowMap = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.A16B16G16R16F, Pool.Default);

            g_pRenderTarget4 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth/4
                    , d3dDevice.PresentationParameters.BackBufferHeight/4, 1, Usage.RenderTarget,
                        Format.A16B16G16R16F, Pool.Default);

            g_pRenderTarget4Aux = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4
                    , d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget,
                        Format.A16B16G16R16F, Pool.Default);


            // Para computar el promedio de Luminance
            int tx_size = 1;
            for (int i = 0; i < NUM_REDUCE_TX; ++i)
            {
                g_pLuminance[i] = new Texture(d3dDevice, tx_size, tx_size, 1,
                    Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);
                tx_size *= 4;
            }

            g_pLuminance_ant = new Texture(d3dDevice, 1, 1, 1,
                Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);


            effect.SetValue("g_RenderTarget", g_pRenderTarget);

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

            GuiController.Instance.Modifiers.addBoolean("activar_glow", "Activar Glow", true);
            GuiController.Instance.Modifiers.addBoolean("pantalla_completa", "Pant.completa", true);
            GuiController.Instance.Modifiers.addEnum("tm_izq", typeof(ToneMapping), ToneMapping.MiddleGray);
            GuiController.Instance.Modifiers.addEnum("tm_der", typeof(ToneMapping), ToneMapping.Nada);
            GuiController.Instance.Modifiers.addInterval("adaptacion_pupila", new object[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f }, 2);

        }



        public void update(float elapsedTime)
        {
            Vector3 pos = GuiController.Instance.CurrentCamera.getPosition();
        }


        public override void render(float elapsedTime)
        {
            update(elapsedTime);
            renderConEfectos(elapsedTime);
        }

        public void renderSinEfectos(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;

            // dibujo la escena una textura 
            effect.Technique = "DefaultTechnique";
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            //Dibujamos todos los meshes del escenario
            renderScene(elapsedTime, "DefaultTechnique");
            //Render skybox 
            skyBox.render();
            device.EndScene();
        }

        public void renderConEfectos(float elapsedTime)
        {
            update(elapsedTime);
            
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;

            // Resolucion de pantalla
            float screen_dx = device.PresentationParameters.BackBufferWidth;
            float screen_dy = device.PresentationParameters.BackBufferHeight;
            effect.SetValue("screen_dx", screen_dx);
            effect.SetValue("screen_dy", screen_dy);

            // dibujo la escena una textura 
            effect.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.SetValue("KLum", 0.05f);
            device.BeginScene();
            //Dibujamos todos los meshes del escenario
            renderScene(elapsedTime, "DefaultTechnique");
            // y el skybox (el skybox no tiene efectos, va por fixed OJOOO)
            skyBox.render();

            device.EndScene();
            pSurf.Dispose();

            MAX_PUPILA_TIME = (float)GuiController.Instance.Modifiers["adaptacion_pupila"];
            bool glow = (bool)GuiController.Instance.Modifiers["activar_glow"];
            effect.SetValue("glow", glow);
            if (glow)
            {
                // dibujo el glow map
                effect.SetValue("KLum", 1.0f);
                effect.Technique = "DefaultTechnique";
                pSurf = g_pGlowMap.GetSurfaceLevel(0);
                device.SetRenderTarget(0, pSurf);
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                device.BeginScene();

                // dibujo el skybox que es brillante, con la tecnica estandard
                skyBox.render();

                // El resto opacos
                renderScene(elapsedTime, "DibujarObjetosOscuros");

                device.EndScene();
                pSurf.Dispose();

                // Hago un blur sobre el glow map
                // 1er pasada: downfilter x 4
                // -----------------------------------------------------
                pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                device.SetRenderTarget(0, pSurf);
                device.BeginScene();
                effect.Technique = "DownFilter4";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("g_RenderTarget", g_pGlowMap);


                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                pSurf.Dispose();
                device.EndScene();
                device.DepthStencilSurface = pOldDS;

                // Pasadas de blur
                for (int P = 0; P < cant_pasadas; ++P)
                {
                    // Gaussian blur Horizontal
                    // -----------------------------------------------------
                    pSurf = g_pRenderTarget4Aux.GetSurfaceLevel(0);
                    device.SetRenderTarget(0, pSurf);
                    // dibujo el quad pp dicho :
                    device.BeginScene();
                    effect.Technique = "GaussianBlurSeparable";
                    device.VertexFormat = CustomVertex.PositionTextured.Format;
                    device.SetStreamSource(0, g_pVBV3D, 0);
                    effect.SetValue("g_RenderTarget", g_pRenderTarget4);

                    device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                    effect.Begin(FX.None);
                    effect.BeginPass(0);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    effect.EndPass();
                    effect.End();
                    pSurf.Dispose();
                    device.EndScene();

                    pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                    device.SetRenderTarget(0, pSurf);
                    pSurf.Dispose();

                    //  Gaussian blur Vertical
                    // -----------------------------------------------------
                    device.BeginScene();
                    effect.Technique = "GaussianBlurSeparable";
                    device.VertexFormat = CustomVertex.PositionTextured.Format;
                    device.SetStreamSource(0, g_pVBV3D, 0);
                    effect.SetValue("g_RenderTarget", g_pRenderTarget4Aux);

                    device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                    effect.Begin(FX.None);
                    effect.BeginPass(1);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    effect.EndPass();
                    effect.End();
                    device.EndScene();

                }
                //TextureLoader.Save("glowmap", ImageFileFormat.Bmp, g_pRenderTarget4Aux);
            }


            // computo el promedio
            pSurf = g_pLuminance[NUM_REDUCE_TX-1].GetSurfaceLevel(0);
            screen_dx = pSurf.Description.Width;
            screen_dy = pSurf.Description.Height;
            device.SetRenderTarget(0, pSurf);
            device.BeginScene();
            effect.Technique = "DownFilter4";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            pSurf.Dispose();
            device.EndScene();
            device.DepthStencilSurface = pOldDS;
            string fname2 = string.Format("Pass{0:D}.bmp", NUM_REDUCE_TX);
            //SurfaceLoader.Save(fname2, ImageFileFormat.Bmp, pSurf);

            // Reduce
            for( int i = NUM_REDUCE_TX- 1; i > 0; i-- )
            {

                pSurf = g_pLuminance[i-1].GetSurfaceLevel(0);
                effect.SetValue("screen_dx", screen_dx);
                effect.SetValue("screen_dy", screen_dy);

                device.SetRenderTarget(0, pSurf);
                effect.SetValue("g_RenderTarget", g_pLuminance[i]);
                device.BeginScene();
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                pSurf.Dispose();
                device.EndScene();

                string fname = string.Format("Pass{0:D}.bmp", i);
                //SurfaceLoader.Save(fname, ImageFileFormat.Bmp, pSurf);

                screen_dx /= 4.0f;
                screen_dy /= 4.0f;
            }


            //  Tone mapping
            // -----------------------------------------------------
            effect.SetValue("tone_mapping_izq", (int)GuiController.Instance.Modifiers["tm_izq"]);
            effect.SetValue("tone_mapping_der", (int)GuiController.Instance.Modifiers["tm_der"]);
            effect.SetValue("pantalla_completa", (bool)GuiController.Instance.Modifiers["pantalla_completa"]);
            effect.SetValue("screen_dx", device.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", device.PresentationParameters.BackBufferHeight);
            device.SetRenderTarget(0, pOldRT);
            device.BeginScene();
            effect.Technique = "ToneMapping";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            effect.SetValue("g_GlowMap", g_pRenderTarget4Aux);
            pupila_time += elapsedTime;
            if (pupila_time >= MAX_PUPILA_TIME)
            {
                pupila_time = 0;
                effect.SetValue("g_Luminance_ant", g_pLuminance[0]);
                Texture aux = g_pLuminance[0];
                g_pLuminance[0] = g_pLuminance_ant;
                g_pLuminance_ant = aux;
            }
            else
            {
                effect.SetValue("g_Luminance", g_pLuminance[0]);
            }

            effect.SetValue("pupila_time", pupila_time / MAX_PUPILA_TIME);      // 0..1
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
            device.EndScene();
        }


        public void renderScene(float elapsedTime,String Technique)
        {
            //Dibujamos todos los meshes del escenario
            Random rnd = new Random(1);
            pasto.Effect = effect;
            pasto.Technique = Technique;
            for (int i = 0; i < 10; ++i)
                for (int j = 0; j < 10; ++j)
                {
                    pasto.Position = new Vector3(-i * 200 + rnd.Next(0, 50), 0, -j * 200 + rnd.Next(0, 50));
                    pasto.Scale = new Vector3(3, 4 + rnd.Next(0, 4), 5);
                    pasto.render();
                }

            arbusto.Effect = effect;
            arbusto.Technique = Technique;
            for (int i = 0; i < 5; ++i)
                for (int j = 0; j < 5; ++j)
                {
                    arbusto.Position = new Vector3(-i * 400 + rnd.Next(0, 50), 0, -j * 400 + rnd.Next(0, 50));
                    arbusto.render();
                }


            arbol.Effect = effect;
            arbol.Technique = Technique;
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                {
                    arbol.Position = new Vector3(-i * 700 + rnd.Next(0, 50), 0, -j * 700 + rnd.Next(0, 50));
                    arbol.render();
                }


            // -------------------------------------
            //Renderizar terreno
            terrain.Effect = effect;
            terrain.Technique = Technique;
            terrain.render();


        }




        public override void close()
        {
            foreach (TgcMesh m in meshes)
            {
                m.dispose();
            }
            effect.Dispose();
            skyBox.dispose();
            terrain.dispose();
            pasto.dispose();
            arbol.dispose();
            arbusto.dispose();
            g_pRenderTarget.Dispose();
            g_pGlowMap.Dispose();
            g_pRenderTarget4Aux.Dispose();
            g_pRenderTarget4.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
            for (int i = 0; i < NUM_REDUCE_TX; i++)
            {
                g_pLuminance[i].Dispose();
            }
            g_pLuminance_ant.Dispose();
        }
    }

}
