using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.DirectX;
using TgcViewer;

namespace AlumnoEjemplos.PiratasEspaciales
{
    public class Camara
    {
        public Vector3 TargetCamara { get; set; }
        /*
           ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
           //Camara en primera persona, tipo videojuego FPS
           //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
           //Por default la camara FPS viene desactivada
           GuiController.Instance.FpsCamera.Enable = true;
           //Configurar posicion y hacia donde se mira
           GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 0, -20), new Vector3(0, 0, 0));
           */

        public void Iniciar(Vector3 posicion,float posicionY,float posicionZ)
        {
            TargetCamara = posicion;
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            //GuiController.Instance.ThirdPersonCamera.setCamera(posicion, 100, 200);
            GuiController.Instance.ThirdPersonCamera.setCamera(posicion, posicionY, posicionZ);
        }

        public void CambiarCamara(float posicionY, float posicionZ)
        {
            GuiController.Instance.ThirdPersonCamera.setCamera(TargetCamara, posicionY, posicionZ);
        }
    }
}
