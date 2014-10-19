using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using System.Collections.Generic;
using TgcViewer;

namespace AlumnoEjemplos.PiratasEspaciales
{
    public class NaveEnemiga
    {
        public float VelocidadMovimiento { get; set; }
        private bool flag;
        public Vector3 posicionInicial;
        public TgcMesh Modelo { get; set; }
        public List<Disparo> Disparos { get; set; }
        Matrix MatrizRotacion;
        Vector3 RotacionOriginal;
        public float TiempoParado { get; set; }
        public float TiempoRecarga { get; set; }

         public NaveEnemiga()
        {
            VelocidadMovimiento = 100f;
            flag = false;
            Disparos = new List<Disparo>();
        }

         public void Iniciar(TgcScene naves, Vector3 posicionObjetivo) 
             
        {
            this.Modelo = naves.Meshes[0];

            this.Modelo.Position = new Vector3(499, 100, 499);
            posicionInicial = this.Modelo.Position;
            RotacionOriginal = new Vector3(0, 0, -1);
            MatrizRotacion = Matrix.Identity;

            TiempoParado = 0F;
            TiempoRecarga = 1f;

        }

        public void MoverHaciaObjetivo(float tiempoRenderizado, Vector3 posicionObjetivo)
        {
            if (this.Modelo.Enabled)
            {
                //Resto los dos vectores para hallar el vector distancia
                Vector3 Distancia = Vector3.Subtract(posicionObjetivo, this.Modelo.Position);

                //Otro vector, con valores absolutos para hallar la componente maxima
                Vector3 DistanciaAbs = TgcVectorUtils.abs(Distancia);

                //Calculo matriz de rotacion
                Vector3 DireccionObjetivo = Vector3.Normalize(posicionObjetivo - this.Modelo.Position);
                float angulo = FastMath.Acos(Vector3.Dot(RotacionOriginal, DireccionObjetivo));
                Vector3 axisRotation = Vector3.Cross(this.Modelo.Rotation, DireccionObjetivo);
                MatrizRotacion = Matrix.RotationAxis(axisRotation, angulo);

                float cantidadDeMovimiento = this.VelocidadMovimiento * tiempoRenderizado;
                float giro = this.Modelo.Rotation.Y - angulo;
                if (giro < -0.1)
                {
                    this.Modelo.rotateY(Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado));
                    return;
                }
                else if (giro > 0.1)
                {
                    this.Modelo.rotateY(Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado));
                    return;
                }
                if (DistanciaAbs.X + DistanciaAbs.Y + DistanciaAbs.Z > 700f)
                {

                    //Hallo la componente de mayor valor y me muevo en esa direccion. VER SENTIDO.
                    if (DistanciaAbs.X >= DistanciaAbs.Y)
                    {
                        if (DistanciaAbs.X >= DistanciaAbs.Z)
                        {
                            // MOVER EN X
                            if (Distancia.X > cantidadDeMovimiento)
                            {
                                this.Modelo.move(cantidadDeMovimiento, 0, 0);
                            }
                            else this.Modelo.move(cantidadDeMovimiento * -1, 0, 0);

                        }
                        else
                        {
                            // MOVER EN Z
                            if (Distancia.Z > 0)
                            {
                                this.Modelo.move(0, 0, cantidadDeMovimiento);
                            }
                            else this.Modelo.move(0, 0, cantidadDeMovimiento * -1);

                        }
                    }
                    else
                    {
                        if (DistanciaAbs.Y >= DistanciaAbs.Z)
                        {
                            // MOVER EN Y
                            if (Distancia.Y > 0)
                            {
                                this.Modelo.move(0, cantidadDeMovimiento, 0);
                            }
                            else this.Modelo.move(0, cantidadDeMovimiento * -1, 0);
                        }
                        else
                        {
                            // MOVER EN Z
                            if (Distancia.Z > 0)
                            {
                                this.Modelo.move(0, 0, cantidadDeMovimiento);
                            }
                            else this.Modelo.move(0, 0, cantidadDeMovimiento * -1);
                        }
                    }
                }
                else
                {
                    //Disparar. Tambien deberia rotar para que el disparo vaya bien

                    if (TiempoParado == 0 || TiempoParado >= TiempoRecarga)
                    {
                        Disparo disparo = new Disparo(this.Modelo, MatrizRotacion);
                        Disparos.Add(disparo);
                        TiempoParado = 0f;
                    }
                    TiempoParado = TiempoParado + tiempoRenderizado * 4;
                } 
            }
        }

        public void Renderizar(float tiempoRenderizado, List<TgcMesh> obstaculos)
        {
            if (Disparos != null)
            {
                foreach (Disparo disparo in Disparos)
                {

                    disparo.Actualizar(tiempoRenderizado, obstaculos);
                    if (disparo.TiempoDeVida - tiempoRenderizado <= 0)
                    {
                        disparo.EnJuego = false;
                        disparo.TestDisparo.dispose();
                    }
                }

                Disparos.RemoveAll(x => x.EnJuego == false);
            }
            Modelo.render();
        }
    }
}
