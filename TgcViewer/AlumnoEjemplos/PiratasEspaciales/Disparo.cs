using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.PiratasEspaciales
{
    public class Disparo
    {
        public float Duracion { get; set; }
        public int Intensidad { get; set; }
        public TgcBox TestDisparo { get; set; }
        public Vector3 Posicion { get; set; }
        public float Velocidad { get; set; }
        public Vector3 Direccion { get; set; }
        public float TiempoDeVida { get; set; }
        public bool EnJuego { get; set; }
        public Disparo(TgcMesh unModelo, Matrix matrixRot)
        {
            TestDisparo = new TgcBox();
            TestDisparo.setPositionSize(unModelo.Position, unModelo.Scale);
            if (unModelo.AutoTransformEnable)
            {
                    TestDisparo.rotateX(unModelo.Rotation.X);
                    TestDisparo.rotateY(unModelo.Rotation.Y);
                    TestDisparo.rotateZ(unModelo.Rotation.Z);

            } else
            {
                TestDisparo.AutoTransformEnable = false;
                TestDisparo.Transform = matrixRot;
            }

            EnJuego = true;
            TiempoDeVida = 4f;
            Duracion = 1f;
            Intensidad = 1;
            Velocidad = 300f;


        }
        public void Actualizar(float tiempoRenderizado,List<TgcMesh> obstaculos)
        {

            
            if (TestDisparo.AutoTransformEnable)
            {
                TestDisparo.moveOrientedY(-Intensidad * tiempoRenderizado * Velocidad);
            }
            else
            {
                TestDisparo.moveOrientedY(-Intensidad * tiempoRenderizado * Velocidad);                
            }
            TiempoDeVida = TiempoDeVida - tiempoRenderizado;
            TestDisparo.BoundingBox.render();
            
        }
    }
}
