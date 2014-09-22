using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.PiratasEspaciales
{
    public class NaveEnemiga
    {
        public float VelocidadMovimiento { get; set; }
        public TgcMesh Modelo { get; set; }
        public Disparo Disparo { get; set; }

         public NaveEnemiga()
        {
            VelocidadMovimiento = 200f;

        }

         public void Iniciar(TgcScene naves)
        {
            this.Modelo = naves.Meshes[0];
             Modelo.move(500,500,500);
        }


    }
}
