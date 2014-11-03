using System;
using System.Drawing;
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
        public TgcBox lightMesh { get; set; }
        public TgcBox lightMesh2 { get; set; }

         public NaveEnemiga()
        {
            VelocidadMovimiento = 100f;
            flag = false;
            Disparos = new List<Disparo>();
        }

         public void Iniciar(TgcScene naves, Vector3 posicionObjetivo) 
             
        {
            this.Modelo = naves.Meshes[0];
            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(5, 5, 5), Color.Blue);
            lightMesh2 = TgcBox.fromSize(new Vector3(5, 5, 5), Color.Blue);

            this.Modelo.Position = new Vector3(499, 100, 499);
            posicionInicial = this.Modelo.Position;
            RotacionOriginal = new Vector3(0, 0, -1);
            MatrizRotacion = Matrix.Identity;

            Vector3 corrector = new Vector3(25, 0, 85);
            Vector3 corrector2 = new Vector3(-25, 0, 85);
            lightMesh.Position = this.Modelo.Position + corrector;
            lightMesh2.Position = this.Modelo.Position + corrector2;

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
                    lightMesh.rotateY(Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado));
                    lightMesh2.rotateY(Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado));

                    float nuevaPosX = (float)(Modelo.Position.X + Math.Cos((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh.Position.X - Modelo.Position.X) - Math.Sin((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh.Position.Z - Modelo.Position.Z));
                    float nuevaPosZ = (float)(Modelo.Position.Z + Math.Sin((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh.Position.X - Modelo.Position.X) + Math.Cos((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh.Position.Z - Modelo.Position.Z));
                    Vector3 nuevaPos = new Vector3(nuevaPosX, lightMesh.Position.Y, nuevaPosZ);
                    lightMesh.move(lightMesh.Position - nuevaPos);

                    nuevaPosX = (float)(Modelo.Position.X + Math.Cos((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh2.Position.X - Modelo.Position.X) - Math.Sin((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh2.Position.Z - Modelo.Position.Z));
                    nuevaPosZ = (float)(Modelo.Position.Z + Math.Sin((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh2.Position.X - Modelo.Position.X) + Math.Cos((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh2.Position.Z - Modelo.Position.Z));
                    nuevaPos = new Vector3(nuevaPosX, lightMesh.Position.Y, nuevaPosZ);
                    lightMesh2.move(lightMesh2.Position - nuevaPos);

                    return;
                }
                else if (giro > 0.1)
                {
                    this.Modelo.rotateY(Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado));
                    lightMesh.rotateY(Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado));
                    lightMesh2.rotateY(Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado));

                    float nuevaPosX = (float)(Modelo.Position.X + Math.Cos((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh.Position.X - Modelo.Position.X) - Math.Sin((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh.Position.Z - Modelo.Position.Z));
                    float nuevaPosZ = (float)(Modelo.Position.Z + Math.Sin((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh.Position.X - Modelo.Position.X) + Math.Cos((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh.Position.Z - Modelo.Position.Z));
                    Vector3 nuevaPos = new Vector3(nuevaPosX, lightMesh.Position.Y, nuevaPosZ);
                    lightMesh.move(lightMesh.Position - nuevaPos);

                    nuevaPosX = (float)(Modelo.Position.X + Math.Cos((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh2.Position.X - Modelo.Position.X) - Math.Sin((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh2.Position.Z - Modelo.Position.Z));
                    nuevaPosZ = (float)(Modelo.Position.Z + Math.Sin((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh2.Position.X - Modelo.Position.X) + Math.Cos((double)Geometry.DegreeToRadian(-giro * 100 * tiempoRenderizado)) * (lightMesh2.Position.Z - Modelo.Position.Z));
                    nuevaPos = new Vector3(nuevaPosX, lightMesh.Position.Y, nuevaPosZ);
                    lightMesh2.move(lightMesh2.Position - nuevaPos);
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
                                lightMesh.move(cantidadDeMovimiento, 0, 0);
                                lightMesh2.move(cantidadDeMovimiento, 0, 0);
                            }
                            else
                            {
                                this.Modelo.move(cantidadDeMovimiento * -1, 0, 0);
                                lightMesh.move(cantidadDeMovimiento * -1, 0, 0);
                                lightMesh2.move(cantidadDeMovimiento * -1, 0, 0);
                            }

                        }
                        else
                        {
                            // MOVER EN Z
                            if (Distancia.Z > 0)
                            {
                                this.Modelo.move(0, 0, cantidadDeMovimiento);
                                lightMesh.move(0, 0, cantidadDeMovimiento);
                                lightMesh2.move(0, 0, cantidadDeMovimiento);
                            }
                            else
                            {
                                this.Modelo.move(0, 0, cantidadDeMovimiento * -1);
                                lightMesh.move(0, 0, cantidadDeMovimiento * -1);
                                lightMesh2.move(0, 0, cantidadDeMovimiento * -1);
                            }
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
                                lightMesh.move(0, cantidadDeMovimiento, 0);
                                lightMesh2.move(0, cantidadDeMovimiento, 0);
                            }
                            else
                            {
                                this.Modelo.move(0, cantidadDeMovimiento * -1, 0);
                                lightMesh.move(0, cantidadDeMovimiento * -1, 0);
                                lightMesh2.move(0, cantidadDeMovimiento * -1, 0);
                            }
                        }
                        else
                        {
                            // MOVER EN Z
                            if (Distancia.Z > 0)
                            {
                                this.Modelo.move(0, 0, cantidadDeMovimiento);
                                lightMesh.move(0, 0, cantidadDeMovimiento);
                                lightMesh2.move(0, 0, cantidadDeMovimiento);
                            }
                            else
                            {
                                this.Modelo.move(0, 0, cantidadDeMovimiento * -1);
                                lightMesh.move(0, 0, cantidadDeMovimiento * -1);
                                lightMesh2.move(0, 0, cantidadDeMovimiento * -1);
                            }
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
            lightMesh.render();
            lightMesh2.render();
            Modelo.render();
        }
    }
}
