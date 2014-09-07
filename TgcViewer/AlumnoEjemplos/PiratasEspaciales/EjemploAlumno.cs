using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Example;
using TgcViewer;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Sound;
using System.IO;
using Device = Microsoft.DirectX.Direct3D.Device;

namespace AlumnoEjemplos.MiGrupo
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        string currentFile;


        const float VelocidadMovimiento = 200f;
        readonly Vector3 SUN_SCALE = new Vector3(12, 12, 12);
        const float AXIS_ROTATION_SPEED = 0.5f;
        float axisRotation = 0f;
        TgcMesh sol;
        TgcMesh nave;

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
            return "Spaceship - Descripcion de la idea";
        }


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
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\NaveEspacial\\NaveEspacial-TgcScene.xml");
            nave = scene.Meshes[0];
            
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



            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(nave.Position, 800, 1500);


            /*
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            GuiController.Instance.FpsCamera.Enable = true;
            //Configurar posicion y hacia donde se mira
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 0, -20), new Vector3(0, 0, 0));
            */

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
            sol.Transform = TransformarSol(elapsedTime);
            TgcD3dInput input = GuiController.Instance.D3dInput;
            Vector3 movimiento = new Vector3(0, 0, 0);

            axisRotation += AXIS_ROTATION_SPEED * elapsedTime;
            //Obtener valor de UserVar (hay que castear)
            int valor = (int)GuiController.Instance.UserVars.getValue("variablePrueba");

            string filePath = (string)GuiController.Instance.Modifiers["MP3-File"];
            //LoadMp3(filePath);

            //TgcMp3Player player = GuiController.Instance.Mp3Player;
            //TgcMp3Player.States currentState = player.getStatus();
            //player.play(true);
            //Obtener valores de Modifiers



            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                movimiento.X = 1;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                movimiento.X = -1;
            }
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                movimiento.Z = -1;
            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                movimiento.Z = 1;
            }
            movimiento *= VelocidadMovimiento * elapsedTime;
            nave.move(movimiento);
            GuiController.Instance.ThirdPersonCamera.Target = nave.Position;
            //Limpiamos todas las transformaciones con la Matrix identidad
            sol.render();
            nave.render();
            d3dDevice.Transform.World = Matrix.Identity;
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            sol.dispose();
            nave.dispose();
        }


        private Matrix TransformarSol(float elapsedTime)
        {
            Matrix scale = Matrix.Scaling(SUN_SCALE);
            Matrix yRot = Matrix.RotationY(axisRotation);

            return scale * yRot;
        }
    }
}
