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
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Gui;

namespace Examples.Shaders.WorkshopShaders
{
    public class ComboRata : TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        TgcScene scene;
        Effect effect;
        Texture g_pBaseTexture;
        Texture g_pHeightmap;
        Texture g_pBaseTexture2;
        Texture g_pHeightmap2;
        Texture g_pBaseTexture3;
        Texture g_pHeightmap3;
        Texture g_pBaseTexture4;
        Texture g_pHeightmap4;
        float time;
        List<TgcBoundingBox> rooms = new List<TgcBoundingBox>();

        List<TgcSkeletalMesh> enemigos = new List<TgcSkeletalMesh>();
        float []enemigo_an = new float[50];
        int cant_enemigos = 0;

        // gui
        DXGui gui = new DXGui();

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-ComboRata";
        }

        public override string getDescription()
        {
            return "ComboRata";
        }

        public override void init()
        {
            time = 0f;
            Device d3dDevice = GuiController.Instance.D3dDevice;
            GuiController.Instance.CustomRenderEnabled = true;
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MyMediaDir + "Piso\\comborata-TgcScene.xml");

            g_pBaseTexture = TextureLoader.FromFile(d3dDevice, MyMediaDir+ "Piso\\Textures\\rocks.jpg");
            g_pHeightmap = TextureLoader.FromFile(d3dDevice, MyMediaDir+ "Piso\\Textures\\rocks_NM_height.tga");

            g_pBaseTexture2 = TextureLoader.FromFile(d3dDevice, MyMediaDir + "Piso\\Textures\\stones.bmp");
            g_pHeightmap2 = TextureLoader.FromFile(d3dDevice, MyMediaDir + "Piso\\Textures\\stones_NM_height.tga");

            g_pBaseTexture3 = TextureLoader.FromFile(d3dDevice, MyMediaDir + "Piso\\Textures\\granito.jpg");
            g_pHeightmap3 = TextureLoader.FromFile(d3dDevice, MyMediaDir + "Piso\\Textures\\saint_NM_height.tga");

            g_pBaseTexture4 = TextureLoader.FromFile(d3dDevice, MyMediaDir + "Piso\\Textures\\granito.jpg");
            g_pHeightmap4 = TextureLoader.FromFile(d3dDevice, MyMediaDir + "Piso\\Textures\\four_NM_height.tga");

            foreach (TgcMesh mesh in scene.Meshes)
            {
                if (mesh.Name.Contains("Floor"))
                {
                    rooms.Add(mesh.BoundingBox);
                }
            }



            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "Parallax.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }

            GuiController.Instance.Modifiers.addVertex3f("LightDir", new Vector3(-1, -1, -1), new Vector3(1, 1, 1), new Vector3(0, -1, 0));
            GuiController.Instance.Modifiers.addFloat("minSample", 1f, 10f, 10f);
            GuiController.Instance.Modifiers.addFloat("maxSample", 11f, 50f, 50f);
            GuiController.Instance.Modifiers.addFloat("HeightMapScale", 0.001f, 0.5f, 0.1f);

            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(147.2558f,8.0536f,262.2509f), new Vector3(148.0797f,7.7869f,262.7511f));

            //Cargar personaje con animaciones
            TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
            Random rnd = new Random();

            // meto un enemigo por cada cuarto
            cant_enemigos = 0;
            foreach (TgcMesh mesh in scene.Meshes)
            {
                if (mesh.Name.Contains("Floor"))
                {
                    float kx = rnd.Next(25, 75) / 100.0f;
                    float kz = rnd.Next(25, 75) / 100.0f;
                    float pos_x = mesh.BoundingBox.PMin.X * kx + mesh.BoundingBox.PMax.X * (1 - kx);
                    float pos_z = mesh.BoundingBox.PMin.Z * kz + mesh.BoundingBox.PMax.Z * (1 - kz);

                    enemigos.Add(skeletalLoader.loadMeshAndAnimationsFromFile(
                        GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\" + "CombineSoldier-TgcSkeletalMesh.xml",
                        GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\",
                        new string[] { 
                        GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Walk-TgcSkeletalAnim.xml",
                    }));

                    //Configurar animacion inicial
                    enemigos[cant_enemigos].playAnimation("Walk", true);
                    enemigos[cant_enemigos].Position = new Vector3(pos_x, 1f, pos_z);
                    enemigos[cant_enemigos].Scale = new Vector3(0.3f, 0.3f, 0.3f);
                    enemigo_an[cant_enemigos] = 0;
                    cant_enemigos++;

                }
            }

            // levanto el GUI
            float W = GuiController.Instance.Panel3d.Width;
            float H = GuiController.Instance.Panel3d.Height;
            gui.Create();
            gui.InitDialog(false);
            gui.InsertFrame("Combo Rata", 10, 10, 200, 200, Color.FromArgb(32,120,255,132),frameBorder.sin_borde);
            gui.InsertFrame("", 10, (int)H-150, 200, 140, Color.FromArgb(62, 120, 132, 255), frameBorder.sin_borde);
            gui.cursor_izq = gui.cursor_der = tipoCursor.sin_cursor;

            // le cambio el font
            gui.font.Dispose();
            // Fonts
            gui.font = new Microsoft.DirectX.Direct3D.Font(d3dDevice, 12, 0, FontWeight.Bold, 0, false, CharacterSet.Default,
                    Precision.Default, FontQuality.Default, PitchAndFamily.DefaultPitch, "Lucida Console");
            gui.font.PreloadGlyphs('0', '9');
            gui.font.PreloadGlyphs('a', 'z');
            gui.font.PreloadGlyphs('A', 'Z');

            gui.RTQ = gui.rectToQuad(0, 0, W, H, 0, 0, W - 150, 160, W - 200, H - 150, 0, H);

        }

        public void update(float elapsedTime)
        {
            Random rnd = new Random();
            float speed = 20f * elapsedTime;
            for (int t = 0; t < cant_enemigos; ++t)
            {

                float an = enemigo_an[t];
                Vector3 vel = new Vector3((float)Math.Sin(an), 0, (float)Math.Cos(an));
                //Mover personaje
                Vector3 lastPos = enemigos[t].Position;
                enemigos[t].move(vel * speed);
                enemigos[t].Rotation = new Vector3(0, (float)Math.PI + an, 0);           // +(float)Math.PI/2

                //Detectar colisiones de BoundingBox utilizando herramienta TgcCollisionUtils
                bool collide = false;
                foreach (TgcMesh obstaculo in scene.Meshes)
                {
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(enemigos[t].BoundingBox, obstaculo.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        collide = true;
                        break;
                    }
                }


                //Si hubo colision, restaurar la posicion anterior
                if (collide)
                {
                    enemigos[t].Position = lastPos;
                    enemigo_an[t] += (float)rnd.Next(0, 100) / 100.0f;
                }

                enemigos[t].updateAnimation();

            }
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
            effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.FpsCamera.getPosition()));

            effect.SetValue("time", time);
            effect.SetValue("aux_Tex", g_pBaseTexture);
            effect.SetValue("height_map", g_pHeightmap);
            effect.SetValue("phong_lighting", true);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            foreach (TgcMesh mesh in scene.Meshes)
            {
                bool va = true;
                int nro_textura = 0;
                mesh.Effect = effect;
                if (mesh.Name.Contains("Floor"))
                {
                    effect.SetValue("g_normal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, -1, 0)));
                    effect.SetValue("g_tangent", TgcParserUtils.vector3ToFloat3Array(new Vector3(1, 0, 0)));
                    effect.SetValue("g_binormal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 0, 1)));
                    nro_textura = 0;

                }
                else
                if (mesh.Name.Contains("Roof"))
                {
                    effect.SetValue("g_normal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 1, 0)));
                    effect.SetValue("g_tangent", TgcParserUtils.vector3ToFloat3Array(new Vector3(1, 0, 0)));
                    effect.SetValue("g_binormal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 0, 1)));
                    nro_textura = 0;

                    va = false;

                }
                else
                if (mesh.Name.Contains("East"))
                {
                    effect.SetValue("g_normal", TgcParserUtils.vector3ToFloat3Array(new Vector3(1, 0, 0)));
                    effect.SetValue("g_tangent", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 0, 1)));
                    effect.SetValue("g_binormal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 1, 0)));
                    nro_textura = 1;
                }
                else
                if (mesh.Name.Contains("West"))
                {
                    effect.SetValue("g_normal", TgcParserUtils.vector3ToFloat3Array(new Vector3(-1, 0, 0)));
                    effect.SetValue("g_tangent", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 0, 1)));
                    effect.SetValue("g_binormal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 1, 0)));
                    nro_textura = 1;
                }
                else
                if (mesh.Name.Contains("North"))
                {
                    effect.SetValue("g_normal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 0, -1)));
                    effect.SetValue("g_tangent", TgcParserUtils.vector3ToFloat3Array(new Vector3(1, 0, 0)));
                    effect.SetValue("g_binormal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 1, 0)));
                    nro_textura = 1;
                }
                else
                if (mesh.Name.Contains("South"))
                {
                    effect.SetValue("g_normal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 0, 1)));
                    effect.SetValue("g_tangent", TgcParserUtils.vector3ToFloat3Array(new Vector3(1, 0, 0)));
                    effect.SetValue("g_binormal", TgcParserUtils.vector3ToFloat3Array(new Vector3(0, 1, 0)));
                    nro_textura = 1;
                }


                switch(nro_textura)
                {
                    case 0:
                    default:
                        effect.SetValue("aux_Tex", g_pBaseTexture);
                        effect.SetValue("height_map", g_pHeightmap);
                        break;
                    case 1:
                        effect.SetValue("aux_Tex", g_pBaseTexture2);
                        effect.SetValue("height_map", g_pHeightmap2);
                        break;
                    case 2:
                        effect.SetValue("aux_Tex", g_pBaseTexture3);
                        effect.SetValue("height_map", g_pHeightmap3);
                        break;
                    case 3:
                        effect.SetValue("aux_Tex", g_pBaseTexture4);
                        effect.SetValue("height_map", g_pHeightmap4);
                        break;
                }

                if (va)
                {
                    mesh.Technique = "ParallaxOcclusion2";
                    mesh.render();
                }
            }

            //Render personames enemigos
            foreach (TgcSkeletalMesh m in enemigos)
                m.render();

            // Render hud
            renderHUD();


            gui.trapezoidal_style = false;
            //radar de proximidad
            float max_dist = 80;
            foreach (TgcSkeletalMesh m in enemigos)
            {
                Vector3 pos_personaje = GuiController.Instance.FpsCamera.getPosition();
                Vector3 pos_enemigo = m.Position*1;
                float dist = (pos_personaje - pos_enemigo).Length();

                if (dist < max_dist)
                {
                    pos_enemigo.Y = m.BoundingBox.PMax.Y*0.75f + m.BoundingBox.PMin.Y*0.25f;
                    pos_enemigo.Project(device.Viewport, device.Transform.Projection, device.Transform.View, device.Transform.World);
                    if (pos_enemigo.Z > 0 && pos_enemigo.Z < 1)
                    {
                        float an = (max_dist - dist) / max_dist * 3.1415f * 2.0f;
                        int d = (int)dist;
                        gui.DrawArc(new Vector2(pos_enemigo.X + 20, pos_enemigo.Y), 40, 0, an, 10, dist<30 ? Color.Tomato : Color.WhiteSmoke);
                        gui.DrawLine(pos_enemigo.X, pos_enemigo.Y, pos_enemigo.X + 20, pos_enemigo.Y, 3, Color.PowderBlue);
                        gui.DrawLine(pos_enemigo.X + 20, pos_enemigo.Y, pos_enemigo.X + 40, pos_enemigo.Y - 20, 3, Color.PowderBlue);
                        gui.TextOut((int)pos_enemigo.X + 50, (int)pos_enemigo.Y - 20, "Proximidad " + d, Color.PowderBlue);
                    }
                }
            }
            gui.trapezoidal_style = true;


            device.EndScene();


        }


        public void renderHUD()
        {
            Device device = GuiController.Instance.D3dDevice;
            device.RenderState.ZBufferEnable = false;
            int W = GuiController.Instance.Panel3d.Width;
            int H = GuiController.Instance.Panel3d.Height;

            // Elapsed time
            int an = (int)(time * 10) % 360;
            float hasta = an /180.0f * (float)Math.PI;
            gui.DrawArc(new Vector2(40, H-100), 25, 0, hasta, 8, Color.Yellow);
            gui.TextOut(20, H-140, "Elapsed Time:" + Math.Round(time),Color.LightSteelBlue);

            // dibujo los enemigos
            Vector3 pos_personaje = GuiController.Instance.FpsCamera.getPosition();
            Vector3 dir_view = GuiController.Instance.FpsCamera.getLookAt() - pos_personaje;
            Vector2 dir_v = new Vector2(dir_view.X, dir_view.Z);
            dir_v.Normalize();
            Vector2 dir_w = new Vector2(dir_v.Y, -dir_v.X);


            int dx = 1000;
            int dy = 1000;
            int dW = 200;
            int dH = 200;
            float ex = (float)dW / (float)dx;
            float ey = (float)dH / (float)dy;
            int ox = 10 + dW/2;
            int oy = 10 + dH/2;

            for (int t = 0; t < cant_enemigos; ++t)
            {
                Vector3 pos = enemigos[t].Position - pos_personaje;
                Vector2 p = new Vector2(pos.X , pos.Z );
                float x = Vector2.Dot(dir_w, p);
                float y = Vector2.Dot(dir_v, p);
                int xm = (int)(ox + x * ex);
                int ym = (int)(oy + y * ey);
                
                if (Math.Abs(xm-ox) < dW / 2 - 10 && Math.Abs(ym-oy) < dH / 2 - 10)
                    gui.DrawRect(xm-2,ym-2,xm+2,ym+2,1,Color.WhiteSmoke);


            }


            Vector2 []P = new Vector2 [20];
            P[0] = new Vector2(ox - 5, oy + 5);
            P[1] = new Vector2(ox + 5, oy + 5);
            P[2] = new Vector2(ox , oy - 10);
            P[3] = P[0];
            gui.DrawSolidPoly(P, 4, Color.Tomato,false);
            gui.DrawCircle(new Vector2(ox, oy), 14,3, Color.Yellow);


            foreach (TgcBoundingBox room in rooms)
            {
                Vector2[] Q = new Vector2[4];
                Vector2[] Qp = new Vector2[5];


                float xm = 0;
                float ym = 0;
                Q[0] = new Vector2(room.PMin.X - pos_personaje.X, room.PMin.Z - pos_personaje.Z);
                Q[1] = new Vector2(room.PMin.X - pos_personaje.X, room.PMax.Z - pos_personaje.Z);
                Q[2] = new Vector2(room.PMax.X - pos_personaje.X, room.PMax.Z - pos_personaje.Z);
                Q[3] = new Vector2(room.PMax.X - pos_personaje.X, room.PMin.Z - pos_personaje.Z);
                for (int t = 0; t < 4; ++t)
                {
                    float x = Vector2.Dot(dir_w, Q[t]);
                    float y = Vector2.Dot(dir_v, Q[t]);
                    Qp[t] = new Vector2(ox + x * ex, oy + y * ey);
                    xm += x * ex;
                    ym += y * ey;
                }
                Qp[4] = Qp[0];
                xm /= 4;
                ym /= 4;

                if (Math.Abs(xm) < dW / 2-10 && Math.Abs(ym) < dH / 2-10)
                    gui.DrawPoly(Qp, 5, 1, Color.Tomato);
            }

            // posicion X,Z
            float kx = pos_personaje.X * ex;
            P[0] = new Vector2(10, H-10);
            P[1] = new Vector2(15, H-30);
            P[2] = new Vector2(5 + kx ,H-30);
            P[3] = new Vector2(25 + kx ,H-10);
            P[4] = P[0];
            gui.DrawSolidPoly(P, 5, Color.Tomato);
            gui.DrawPoly(P, 5, 2,Color.HotPink);

            float kz = pos_personaje.Z * ey;
            P[0] = new Vector2(10, H - 40);
            P[1] = new Vector2(15, H - 60);
            P[2] = new Vector2(5 + kz, H - 60);
            P[3] = new Vector2(25 + kz, H - 40);
            P[4] = P[0];
            gui.DrawSolidPoly(P, 5, Color.Green);
            gui.DrawPoly(P, 5, 2, Color.YellowGreen);

            device.RenderState.ZBufferEnable = true;
            gui.Render();
        }

        public override void close()
        {
            scene.disposeAll();
            effect.Dispose();
            g_pBaseTexture.Dispose();
            g_pHeightmap.Dispose();
            g_pBaseTexture2.Dispose();
            g_pHeightmap2.Dispose();
            g_pBaseTexture3.Dispose();
            g_pHeightmap3.Dispose();
            g_pBaseTexture4.Dispose();
            g_pHeightmap4.Dispose();
            gui.Dispose();
            foreach (TgcSkeletalMesh m in enemigos)
                m.dispose();

        }
    }

}
