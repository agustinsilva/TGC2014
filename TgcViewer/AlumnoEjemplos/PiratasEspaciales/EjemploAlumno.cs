using System;
using System.Collections.Generic;
using System.Text;
using AlumnoEjemplos.PiratasEspaciales;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Example;
using TgcViewer;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Sound;
using System.IO;
using TgcViewer.Utils._2D;
using TgcViewer.Utils;
using TgcViewer.Utils.Shaders;


namespace AlumnoEjemplos.MiGrupo
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        string currentFile;
        VertexBuffer screenQuadVB;
        Texture renderTarget2D;
        Surface pOldRT;
        Effect effect;

        public Nave nave = new Nave();
        public NaveEnemiga NaveEnemiga1 = new NaveEnemiga();

        public float Time = 0;
        public int CantidadRenderizadas = 0;
        List<TgcMesh> obstaculos = new List<TgcMesh>();
        readonly Vector3 SUN_SCALE = new Vector3(12, 12, 12);
        const float AXIS_ROTATION_SPEED = 0.5f;
        float axisRotation = 0f;
        TgcMesh sol;
        
        #region Descripcion del Plugin
        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
        public override string getCategory()
        {   
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "Piratas Espaciales";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return "Spaceship - Prender radio con Y, Apagar con O";
        }

        #endregion

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;
            //Creacion de una esfera
            string sphere = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml";
            TgcSceneLoader loader = new TgcSceneLoader();

            //Cargado texturas para nave

            //TgcScene modeloNaveEnemiga = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\StarWars-Speeder\\StarWars-Speeder-TgcScene.xml");
            TgcScene modeloNaveEnemiga = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            TgcScene modelosDeNaves = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            
            nave.Iniciar(modelosDeNaves);

            NaveEnemiga1.Iniciar(modeloNaveEnemiga, nave.Modelo.Position);
                //Cargado de textura para el sol
            sol = loader.loadSceneFromFile(sphere).Meshes[0];
            sol.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesDir + "Transformations\\SistemaSolar\\SunTexture.jpg") });

            
            //Deshabilitamos el manejo automático de Transformaciones de TgcMesh, para poder manipularlas en forma customizada
            sol.AutoTransformEnable = false;


            GuiController.Instance.UserVars.addVar("variablePrueba");

            GuiController.Instance.UserVars.setValue("variablePrueba", 5451);



            ///////////////MODIFIERS//////////////////

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("valorFloat", -50f, 200f, 0f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[]{"opcion1", "opcion2", "opcion3"};
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));

            Camara.Iniciar(nave.Modelo.Position);

            List<string> lista = new List<string>();

            lista.Add("elemento1");
            lista.Add("elemento2");

            string elemento1 = lista[0];

            foreach (string elemento in lista)
            {
                //Loggear por consola del Framework
                GuiController.Instance.Logger.log(elemento);
            }


            for (int i = 0; i < lista.Count; i++)
            {
                string element = lista[i];
            }

            GuiController.Instance.BackgroundColor = Color.Black;
            currentFile = null;
            GuiController.Instance.Modifiers.addFile("MP3-File", GuiController.Instance.ExamplesMediaDir + "Music\\I am The Money.mp3", "MP3s|*.mp3");           
            obstaculos.Add(sol);

            GuiController.Instance.CustomRenderEnabled = true;
            CustomVertex.PositionTextured[] screenQuadVertices = new CustomVertex.PositionTextured[]
		    {
    			new CustomVertex.PositionTextured( -1, 1, 1, 0,0), 
			    new CustomVertex.PositionTextured(1,  1, 1, 1,0),
			    new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
			    new CustomVertex.PositionTextured(1,-1, 1, 1,1)
    		};
            screenQuadVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            screenQuadVB.SetData(screenQuadVertices, 0, LockFlags.None);

            renderTarget2D = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);
            //Cargar shader con efectos de Post-Procesado
            effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesMediaDir + "Shaders\\PostProcess.fx");

            //Configurar Technique dentro del shader
            effect.Technique = "BlurTechnique";


        }

        private void LoadMp3(string filePath)
        {
            if (currentFile == null || currentFile != filePath)
            {
                currentFile = filePath;

                //Cargar archivo
                GuiController.Instance.Mp3Player.closeFile();
                GuiController.Instance.Mp3Player.FileName = currentFile;               
            }
        }
        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {
                //Device de DirectX para renderizar
            
                Device d3dDevice = GuiController.Instance.D3dDevice;

                axisRotation += AXIS_ROTATION_SPEED*elapsedTime;
                //Obtener valor de UserVar (hay que castear)
                int valor = (int) GuiController.Instance.UserVars.getValue("variablePrueba");

                #region

                //Radio de la nave
                string filePath = (string) GuiController.Instance.Modifiers["MP3-File"];
                LoadMp3(filePath);

                TgcMp3Player player = GuiController.Instance.Mp3Player;
                TgcMp3Player.States currentState = player.getStatus();

                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Y))
                {
                    if (currentState == TgcMp3Player.States.Open)
                    {
                        //Reproducir MP3
                        player.play(true);
                    }
                    if (currentState == TgcMp3Player.States.Stopped)
                    {
                        //Parar y reproducir MP3
                        player.closeFile();
                        player.play(true);
                    }
                }
                else if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.O))
                {
                    if (currentState == TgcMp3Player.States.Playing)
                    {
                        //Parar el MP3
                        player.stop();
                    }
                }

                #endregion

                //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
                //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.
                pOldRT = d3dDevice.GetRenderTarget(0);
                Surface pSurf = renderTarget2D.GetSurfaceLevel(0);
                d3dDevice.SetRenderTarget(0, pSurf);
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);


                //Dibujamos la escena comun, pero en vez de a la pantalla al Render Target
                drawSceneToRenderTarget(d3dDevice,elapsedTime);

                //Liberar memoria de surface de Render Target
                pSurf.Dispose();

                //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
                //TextureLoader.Save(GuiController.Instance.ExamplesMediaDir + "Shaders\\render_target.bmp", ImageFileFormat.Bmp, renderTarget2D);


                //Ahora volvemos a restaurar el Render Target original (osea dibujar a la pantalla)
                d3dDevice.SetRenderTarget(0, pOldRT);


                //Luego tomamos lo dibujado antes y lo combinamos con una textura con efecto de alarma
                drawPostProcess(d3dDevice);
                
                nave.Renderizar(elapsedTime, obstaculos);
                NaveEnemiga1.MoverHaciaObjetivo(elapsedTime, nave.Modelo.Position);
                NaveEnemiga1.Renderizar(elapsedTime, obstaculos);
                
                sol.BoundingBox.transform(sol.Transform);
                sol.Transform = TransformarSol(elapsedTime);
                GuiController.Instance.ThirdPersonCamera.Target = nave.Modelo.Position;
                //Limpiamos todas las transformaciones con la Matrix identidad
                sol.render();
                d3dDevice.Transform.World = Matrix.Identity;
            
           
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            sol.dispose();
            nave.Modelo.dispose();
            effect.Dispose();
            screenQuadVB.Dispose();
            renderTarget2D.Dispose();
            NaveEnemiga1.Modelo.dispose();
        }

        private void drawSceneToRenderTarget(Device d3dDevice,float elapsedTime)
        {
            //Arrancamos el renderizado. Esto lo tenemos que hacer nosotros a mano porque estamos en modo CustomRenderEnabled = true
            d3dDevice.BeginScene();


            //Como estamos en modo CustomRenderEnabled, tenemos que dibujar todo nosotros, incluso el contador de FPS
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);

            //Tambien hay que dibujar el indicador de los ejes cartesianos
            GuiController.Instance.AxisLines.render();

            //Dibujamos todos los meshes del escenario
             nave.Renderizar(elapsedTime, obstaculos);
                sol.BoundingBox.transform(sol.Transform);
                sol.Transform = TransformarSol(elapsedTime);
                GuiController.Instance.ThirdPersonCamera.Target = nave.Modelo.Position;
                //Limpiamos todas las transformaciones con la Matrix identidad
                sol.render();

                sol.BoundingBox.render();
                NaveEnemiga1.Renderizar(elapsedTime, obstaculos);

            d3dDevice.Transform.World = Matrix.Identity;


            //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
            d3dDevice.EndScene();
        }

        private void drawPostProcess(Device d3dDevice)
        {
            //Arrancamos la escena
            d3dDevice.BeginScene();

            //Cargamos para renderizar el unico modelo que tenemos, un Quad que ocupa toda la pantalla, con la textura de todo lo dibujado antes
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);

            //Ver si el efecto de oscurecer esta activado, configurar Technique del shader segun corresponda
            if(nave.Saltando)
            {
                effect.Technique = "BlurTechnique";
            }
            else
            {
                effect.Technique = "DefaultTechnique";
            }

            //Cargamos parametros en el shader de Post-Procesado
            effect.SetValue("render_target2D", renderTarget2D);
            effect.SetValue("blur_intensity", 0.025f);


            //Limiamos la pantalla y ejecutamos el render del shader
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            //Terminamos el renderizado de la escena
            d3dDevice.EndScene();
        }

        private Matrix TransformarSol(float elapsedTime)
        {
            Matrix scale = Matrix.Scaling(SUN_SCALE);
            Matrix yRot = Matrix.RotationY(axisRotation);
            return scale * yRot;
        }
    }
}
