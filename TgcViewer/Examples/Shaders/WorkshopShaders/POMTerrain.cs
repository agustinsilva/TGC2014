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

namespace Examples.Shaders.WorkshopShaders
{
    public class POMTerrain
    {
        VertexBuffer vbTerrain;
        public Vector3 center;
        public Texture terrainTexture;
        public int totalVertices;
        public int[,] heightmapData;
        public float scaleXZ;
        public float scaleY;
        public float ki;
        public float kj;
        public float ftex;      // factor para la textura

        public POMTerrain()
        {
            ftex = 1f;
            ki = 1;
            kj = 1;
        }

        public void loadHeightmap(string heightmapPath, float pscaleXZ, float pscaleY, Vector3 center)
        {
            scaleXZ = pscaleXZ;
            scaleY = pscaleY;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            this.center = center;

            //Dispose de VertexBuffer anterior, si habia
            if (vbTerrain != null && !vbTerrain.Disposed)
            {
                vbTerrain.Dispose();
            }

            //cargar heightmap
            heightmapData = loadHeightMap(d3dDevice, heightmapPath);
            float width = (float)heightmapData.GetLength(0);
            float length = (float)heightmapData.GetLength(1);


            //Crear vertexBuffer
            totalVertices = 2 * 3 * (heightmapData.GetLength(0) + 1) * (heightmapData.GetLength(1) + 1);
            totalVertices *= (int)ki * (int)kj;
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            int dataIdx = 0;
            CustomVertex.PositionNormalTextured[] data = new CustomVertex.PositionNormalTextured[totalVertices];

            center.X = center.X * scaleXZ - (width / 2) * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - (length / 2) * scaleXZ;

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < length - 1; j++)
                {
                    //Vertices
                    Vector3 v1 = new Vector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j] * scaleY, center.Z + j * scaleXZ);
                    Vector3 v2 = new Vector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j + 1] * scaleY, center.Z + (j + 1) * scaleXZ);
                    Vector3 v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j] * scaleY, center.Z + j * scaleXZ);
                    Vector3 v4 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j + 1] * scaleY, center.Z + (j + 1) * scaleXZ);

                    //Coordendas de textura
                    Vector2 t1 = new Vector2(ftex * i / width, ftex * j / length);
                    Vector2 t2 = new Vector2(ftex * i / width, ftex * (j + 1) / length);
                    Vector2 t3 = new Vector2(ftex * (i + 1) / width, ftex * j / length);
                    Vector2 t4 = new Vector2(ftex * (i + 1) / width, ftex * (j + 1) / length);

                    //Cargar triangulo 1
                    Vector3 n1 = Vector3.Cross(v2 - v1, v3 - v1);
                    n1.Normalize();
                    data[dataIdx] = new CustomVertex.PositionNormalTextured(v1, n1 , t1.X, t1.Y);
                    data[dataIdx + 1] = new CustomVertex.PositionNormalTextured(v2,n1, t2.X, t2.Y);
                    data[dataIdx + 2] = new CustomVertex.PositionNormalTextured(v4,n1, t4.X, t4.Y);


                    //Cargar triangulo 2
                    Vector3 n2 = Vector3.Cross(v4 - v1, v3 - v1);
                    n2.Normalize();
                    data[dataIdx + 3] = new CustomVertex.PositionNormalTextured(v1, n2, t1.X, t1.Y);
                    data[dataIdx + 4] = new CustomVertex.PositionNormalTextured(v4,n2, t4.X, t4.Y);
                    data[dataIdx + 5] = new CustomVertex.PositionNormalTextured(v3,n2, t3.X, t3.Y);

                    dataIdx += 6;
                }
            }
            vbTerrain.SetData(data, 0, LockFlags.None);
        }

        /// <summary>
        /// Carga la textura del terreno
        /// </summary>
        public void loadTexture(string path)
        {
            //Dispose textura anterior, si habia
            if (terrainTexture != null && !terrainTexture.Disposed)
            {
                terrainTexture.Dispose();
            }

            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Rotar e invertir textura
            Bitmap b = (Bitmap)Bitmap.FromFile(path);
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            terrainTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
        }




        /// <summary>
        /// Carga los valores del Heightmap en una matriz
        /// </summary>
        private int[,] loadHeightMap(Device d3dDevice, string path)
        {
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(path);
            int width = bitmap.Size.Width;
            int height = bitmap.Size.Height;
            int[,] heightmap = new int[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //(j, i) invertido para primero barrer filas y despues columnas
                    Color pixel = bitmap.GetPixel(j, i);
                    float intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    heightmap[i, j] = (int)intensity;
                }

            }

            bitmap.Dispose();
            return heightmap;
        }


        public void executeRender(Effect effect)
        {
            Device device = GuiController.Instance.D3dDevice;
            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);

            //Render terrain 
            effect.SetValue("texDiffuseMap", terrainTexture);

            device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            device.SetStreamSource(0, vbTerrain, 0);

            int numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
                effect.EndPass();
            }
            effect.End();
        }

        public float CalcularAltura(float x, float z)
        {
            float largo = scaleXZ * 64;
            float pos_i = 64f * (0.5f + x / largo);
            float pos_j = 64f * (0.5f + z / largo);

            int pi = (int)pos_i;
            float fracc_i = pos_i - pi;
            int pj = (int)pos_j;
            float fracc_j = pos_j - pj;

            if (pi < 0)
                pi = 0;
            else
                if (pi > 63)
                    pi = 63;

            if (pj < 0)
                pj = 0;
            else
                if (pj > 63)
                    pj = 63;

            int pi1 = pi + 1;
            int pj1 = pj + 1;
            if (pi1 > 63)
                pi1 = 63;
            if (pj1 > 63)
                pj1 = 63;

            // 2x2 percent closest filtering usual: 
            float H0 = heightmapData[pi, pj] * scaleY;
            float H1 = heightmapData[pi1, pj] * scaleY;
            float H2 = heightmapData[pi, pj1] * scaleY;
            float H3 = heightmapData[pi1, pj1] * scaleY;
            float H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) +
                      (H2 * (1 - fracc_i) + H3 * fracc_i) * fracc_j;
            return H;
        }



        public void dispose()
        {
            if (vbTerrain != null)
            {
                vbTerrain.Dispose();
            }
            if (terrainTexture != null)
            {
                terrainTexture.Dispose();
            }
        }
    }


    public class POMTerrainSample : TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        Effect effect;
        Texture g_pBaseTexture;
        Texture g_pHeightmap;
        POMTerrain terrain;
        Vector2 pos = new Vector2(0, 0);
        float dir_an = 0;
        float kvel = 1.0f;


        float time;
        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-POMTerrain";
        }

        public override string getDescription()
        {
            return "POM Terrain";
        }

        public override void init()
        {
            time = 0f;
            Device d3dDevice = GuiController.Instance.D3dDevice;
            GuiController.Instance.CustomRenderEnabled = true;
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            g_pBaseTexture = TextureLoader.FromFile(d3dDevice, MyMediaDir+ "Piso\\Textures\\rocks.jpg");
            g_pHeightmap = TextureLoader.FromFile(d3dDevice, MyMediaDir+ "Piso\\Textures\\rocks_NM_height.tga");

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "Parallax.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            effect.Technique = "ParallaxOcclusion";
            effect.SetValue("aux_Tex", g_pBaseTexture);
            effect.SetValue("height_map", g_pHeightmap);
            effect.SetValue("phong_lighting", true);
            effect.SetValue("k_alpha", 0.75f);


            GuiController.Instance.Modifiers.addVertex3f("LightDir", new Vector3(-1, -1, -1), new Vector3(1, 1, 1), new Vector3(0, -1, 0));
            GuiController.Instance.Modifiers.addFloat("minSample", 1f, 10f, 10f);
            GuiController.Instance.Modifiers.addFloat("maxSample", 11f, 50f, 50f);
            GuiController.Instance.Modifiers.addFloat("HeightMapScale", 0.001f, 0.5f, 0.1f);

            // ------------------------------------------------------------
            // Creo el Heightmap para el terreno:
            terrain = new POMTerrain();
            terrain.ftex = 250f;
            terrain.loadHeightmap(GuiController.Instance.ExamplesDir
                    + "Shaders\\WorkshopShaders\\Media\\Heighmaps\\" + "Heightmap3.jpg", 100f, 2.25f, new Vector3(0, 0, 0));
            terrain.loadTexture(GuiController.Instance.ExamplesDir
                    + "Shaders\\WorkshopShaders\\Media\\Heighmaps\\" + "TerrainTexture3.jpg");


            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-350,1000,-1100),new Vector3(0,0,0));
            GuiController.Instance.RotCamera.Enable = false;

      
        }

        public void update(float elapsedTime)
        {
            GuiController.Instance.FpsCamera.Enable = false;
            // Actualizo la direccion
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.A))
            {
                dir_an += 1f * elapsedTime;
            }
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.D))
            {
                dir_an -= 1f * elapsedTime;
            }

               // calculo la velocidad
            Vector2 vel = new Vector2((float)Math.Sin(dir_an) , (float)Math.Cos(dir_an));
            // actualizo la posicion
            pos += vel * kvel * elapsedTime;

            // actualizo los parametros de la camara
            float dH = 1.0f;       // altura del personaje
            float H = terrain.CalcularAltura(pos.X, pos.Y);
            Vector2 pos_s = pos + vel * 2;
            Vector3 lookFrom = new Vector3(pos.X, H + dH, pos.Y);
            Vector3 lookAt = new Vector3(pos_s.X, H, pos_s.Y);
            GuiController.Instance.D3dDevice.Transform.View = Matrix.LookAtLH(lookFrom, lookAt, new Vector3(0, 1, 0));
            effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(lookFrom));

        }


        public override void render(float elapsedTime)
        {
            update(elapsedTime);

            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;
            time += elapsedTime;

            
            Vector3 lightDir = (Vector3)GuiController.Instance.Modifiers["LightDir"];
            effect.SetValue("g_LightDir", TgcParserUtils.vector3ToFloat3Array(lightDir));
            effect.SetValue("min_cant_samples", (float)GuiController.Instance.Modifiers["minSample"]);
            effect.SetValue("max_cant_samples", (float)GuiController.Instance.Modifiers["maxSample"]);
            effect.SetValue("fHeightMapScale", (float)GuiController.Instance.Modifiers["HeightMapScale"]);
            //effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.FpsCamera.getPosition()));
            effect.SetValue("time", time);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            //Renderizar terreno con POM
            effect.Technique = "ParallaxOcclusion";
            terrain.executeRender(effect);
            device.EndScene();

        }



        public override void close()
        {
            effect.Dispose();
            g_pBaseTexture.Dispose();
            g_pHeightmap.Dispose();
            terrain.dispose();
        }

    }

}
