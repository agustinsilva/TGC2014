using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.PiratasEspaciales
{
    public class NaveEnemiga
    {
        public float VelocidadMovimiento { get; set; }
        public TgcMesh Modelo { get; set; }
        public Disparo Disparo { get; set; }

         public NaveEnemiga()
        {
            VelocidadMovimiento = 100f;
    
        }

         public void Iniciar(TgcScene naves) 
             //Podriamos modificarla: Agregar otro parametro que sea posicion de Nave, para iniciar las enemigas lejos de ella. 
        {
            this.Modelo = naves.Meshes[0];
            this.Modelo.move(499, 100, 499);
            //this.Modelo.move(0, 0, 750);
        }

        public void MoverHaciaObjetivo(float tiempoRenderizado, Vector3 posicionObjetivo)
        {
            //Resto los dos vectores para hallar el vector distancia
            Vector3 Distancia = Vector3.Subtract(this.Modelo.Position, posicionObjetivo);

            //Otro vector, con valores absolutos para hallar la componente maxima
            Vector3 DistanciaAbs = TgcVectorUtils.abs(Distancia);

            if (DistanciaAbs.X + DistanciaAbs.Y + DistanciaAbs.Z > 300f)
            {
                //Hallo la componente de mayor valor y me muevo en esa direccion. VER SENTIDO.
                if (DistanciaAbs.X >= DistanciaAbs.Y)
                {
                    if (DistanciaAbs.X >= DistanciaAbs.Z)
                    {
                        // MOVER EN X
                        if (Distancia.X < 0)
                        {
                            this.Modelo.move(this.VelocidadMovimiento * tiempoRenderizado, 0, 0);
                        }
                        else this.Modelo.move(this.VelocidadMovimiento * tiempoRenderizado * -1, 0, 0);
                        
                    }
                    else
                    {
                        // MOVER EN Z
                        if (Distancia.Z < 0)
                        {
                            this.Modelo.move(0, 0, this.VelocidadMovimiento * tiempoRenderizado);
                        }
                        else this.Modelo.move(0, 0, this.VelocidadMovimiento * tiempoRenderizado * -1);
                    }
                }
                else
                {
                    if (DistanciaAbs.Y >= DistanciaAbs.Z)
                    {
                        // MOVER EN Y
                        if (Distancia.Y < 0)
                        {
                            this.Modelo.move(0, this.VelocidadMovimiento * tiempoRenderizado, 0);
                        }
                        else this.Modelo.move(0, this.VelocidadMovimiento * tiempoRenderizado * -1, 0);
                    }
                    else
                    {
                        // MOVER EN Z
                        if (Distancia.Z < 0)
                        {
                            this.Modelo.move(0, 0, this.VelocidadMovimiento * tiempoRenderizado);
                        }
                        else this.Modelo.move(0, 0, this.VelocidadMovimiento * tiempoRenderizado * -1);
                    }
                } 
            }
        }

        public void Renderizar()
        {
            Modelo.render();
        }
    }
}
