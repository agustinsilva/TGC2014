using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.PiratasEspaciales
{
    public class Nave
    {
        public float VelocidadMovimiento { get; set; }
        public float Flotacion { get; set; }
        public float DireccionFlotacion { get; set; }
        public float VelocidadRotacion { get; set; }
        public float TiempoParado { get; set; }
        public TgcMesh Modelo { get; set; }
        public List<Disparo> Disparos { get; set; }
        public float TiempoRecarga { get; set; }
        public bool Saltando { get; set; }
        public float RendAcumuladoS { get; set; }
        public float RendAcumuladoW { get; set; }

        public Nave()
        {
            Flotacion = 5f;
            VelocidadMovimiento = 200f;
            DireccionFlotacion = 1f;
            VelocidadRotacion = 30f;
            TiempoParado = 0F;
            TiempoRecarga = 1f;
            Disparos = new List<Disparo>();
            Saltando = false;
            RendAcumuladoS = 0f;
            RendAcumuladoW = 0f;
        }

        public void Iniciar(TgcScene naves)
        {
            this.Modelo = naves.Meshes[0];

            this.Modelo.move(0,0,1500);
        }

        public void Movimiento(float tiempoRenderizado, List<TgcMesh> obstaculos)
        {
            bool rotando = false;
            bool seMovio = false;
            float mover = 0f;
            float rotar = 0f;
            TgcD3dInput input = GuiController.Instance.D3dInput;
            Vector3 movimiento = new Vector3(0, 0, 0);

            //tiempos de renderizado para calcular aceleracion con limite
            if (RendAcumuladoS < 10) RendAcumuladoS += tiempoRenderizado;    //tiempo que se estuvo yendo hacia atras
            if (RendAcumuladoW < 10) RendAcumuladoW += tiempoRenderizado;    //tiempo que se estuvo yendo hacia adelante

            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                rotando = true;
                rotar = -VelocidadRotacion;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                rotando = true;
                rotar = VelocidadRotacion;
            }
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                seMovio = true;
                mover = -VelocidadMovimiento - 12 * (float)Math.Pow(RendAcumuladoW, 2);
                Modelo.moveOrientedY(mover * tiempoRenderizado);
            }
            else
            {
                RendAcumuladoW = 0;
            }
            if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                seMovio = true;
                mover = VelocidadMovimiento + 12 * (float)Math.Pow(RendAcumuladoS, 2);
                Modelo.moveOrientedY(mover * tiempoRenderizado);
            }
            else
            {
                RendAcumuladoS = 0;
            }
            if ( input.keyDown(Key.R))
            {
                seMovio = true;
                mover = -VelocidadMovimiento;
                Modelo.move(0,mover*tiempoRenderizado,0);
            }
            else if (input.keyDown(Key.T))
            {
                seMovio = true;
                mover = VelocidadMovimiento;
                Modelo.move(0, mover * tiempoRenderizado, 0);
            }

            if (seMovio)
            {
                Vector3 ultimaPosicion = Modelo.Position;
                bool colisiono = false;
                foreach (TgcMesh obstaculo in obstaculos)
                {
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(Modelo.BoundingBox, obstaculo.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        colisiono = true;
                        break;
                    }
                }

                if (colisiono)
                {

                    Modelo.Position = ultimaPosicion;

                }                              
            }

            if (rotando)
            {
                Modelo.rotateY(Geometry.DegreeToRadian(rotar * tiempoRenderizado));
                GuiController.Instance.ThirdPersonCamera.rotateY(Geometry.DegreeToRadian(rotar * tiempoRenderizado));
;
            }

            
        }

        public void FlotacionEspacial(float tiempoRenderizado)
        {
            Modelo.move(0, Flotacion * DireccionFlotacion * tiempoRenderizado * 2, 0);
            if (FastMath.Abs(Modelo.Position.Y) > 7f)
            {
                DireccionFlotacion *= -1;
            }
        }

        public void Disparar(float tiempoRenderizado)
        {
            if (TiempoParado == 0 || TiempoParado >= TiempoRecarga)
            {
                
            
            TgcD3dInput input = GuiController.Instance.D3dInput;
            if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                Disparo disparo = new Disparo(Modelo,new Matrix());
                Disparos.Add(disparo);

            }
                TiempoParado = 0f;
            }
            TiempoParado = TiempoParado + tiempoRenderizado*4;
        }

        public void SaltaHiperEspacio()
        {
             TgcD3dInput input = GuiController.Instance.D3dInput;

            if (Saltando == false) { 
            if (input.keyDown(Key.Space))
            {
                VelocidadMovimiento += 400f;
                Saltando = true;
            }
            }
            if (Saltando) { 
            if (input.keyUp(Key.Space))
            {
                VelocidadMovimiento -= 400f;
                Saltando = false;
            }
           }
        }

        public void Renderizar(float tiempoRenderizado,List<TgcMesh> obstaculos)
        {
            SaltaHiperEspacio();
            this.Movimiento(tiempoRenderizado,obstaculos);
            if (!Saltando) 
            { 
            this.Disparar(tiempoRenderizado);
            }
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
            //la flotacion requiere mejoras. Agustin S.
            //this.FlotacionEspacial(tiempoRenderizado);
            Modelo.render();
        }

    }
}
