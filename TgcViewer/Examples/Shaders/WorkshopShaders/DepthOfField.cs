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

namespace Examples.Shaders.WorkshopShaders
{

    public class DepthOfField: TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        TgcMesh mesh;
        Effect effect;
        Surface g_pDepthStencil;     // Depth-stencil buffer 
        Texture g_pRenderTarget, g_pBlurFactor;
        VertexBuffer g_pVBV3D;


        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-DepthOfField";
        }

        public override string getDescription()
        {
            return "Depth of Field Sample";
        }

        public override void init()
        {
            GuiController.Instance.CustomRenderEnabled = true;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Cargamos un escenario

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "\\MeshCreator\\Meshes\\Esqueletos\\EsqueletoHumano3\\Esqueleto3-TgcScene.xml");
            mesh = scene.Meshes[0];

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
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-33.7693f, 26.9606f, -12.1414f), new Vector3(-33.232f,26.739f,-11.3277f));
                   
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
            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            // Blur Factor
            g_pBlurFactor = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight , 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);
            effect.SetValue("g_BlurFactor", g_pBlurFactor);

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

            GuiController.Instance.Modifiers.addBoolean("activar_efecto", "Activar efecto", true);
            GuiController.Instance.Modifiers.addFloat("focus_plane", 1, 300, 10);
            GuiController.Instance.Modifiers.addFloat("blur_factor", 0.1f, 5f, 0.5f);

        }


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;


            bool activar_efecto = (bool)GuiController.Instance.Modifiers["activar_efecto"];
            effect.SetValue("zfoco" , (float)GuiController.Instance.Modifiers["focus_plane"]);
            effect.SetValue("blur_k" , (float)GuiController.Instance.Modifiers["blur_factor"]);

            // dibujo la escena una textura 
            // guardo el Render target anterior y seteo la textura como render target
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            if(activar_efecto)
                device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = device.DepthStencilSurface;
            // Probar de comentar esta linea, para ver como se produce el fallo en el ztest
            // por no soportar usualmente el multisampling en el render to texture.
            if (activar_efecto)
                device.DepthStencilSurface = g_pDepthStencil;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            renderScene("DefaultTechnique");
            device.EndScene();
            pSurf.Dispose();

            if (activar_efecto)
            {
                // Genero el depth map
                Surface pSurf2 = g_pBlurFactor.GetSurfaceLevel(0);
                device.SetRenderTarget(0, pSurf2);
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                device.BeginScene();
                renderScene("RenderBlurFactor");
                device.EndScene();
                pSurf2.Dispose();

                // restuaro el render target y el stencil
                device.DepthStencilSurface = pOldDS;
                device.SetRenderTarget(0, pOldRT);

                // dibujo el quad pp dicho :
                device.BeginScene();
                effect.Technique = "DepthOfField";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("g_RenderTarget", g_pRenderTarget);
                effect.SetValue("g_BlurFactor", g_pBlurFactor);

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                device.EndScene();

            }
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
        }


        public void renderScene(string technique)
        {
            // seteo la tecnica en el efecto 
            effect.Technique = technique;
            mesh.Effect = effect;
            mesh.Technique = technique;
            for (int j = 0; j < 5; ++j)
            {
                for (int i = 0; i < 15; ++i)
                {
                    mesh.Position = new Vector3(j*20, 0, i * 50);
                    mesh.render();
                }
            }
        }


        public override void close()
        {
            mesh.dispose();
            effect.Dispose();
            g_pRenderTarget.Dispose();
            g_pBlurFactor.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
        }
    }

}
