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

namespace Examples.Shaders
{
   
    public class EjemploGPUAttack : TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        Effect effect;
        Texture g_pRenderTarget , g_pTempData;
        Surface g_pDepthStencil;     // Depth-stencil buffer 
        VertexBuffer g_pVB;
        static int MAX_DS = 512;
        public int a = 33;
        public int c = 213;
        public int m = 251;
        public int[] hash = new int[4];
        public bool found = false;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-GPUAttack";
        }

        public override string getDescription()
        {
            return "GPUAttack";
        }


        public unsafe override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            GuiController.Instance.CustomRenderEnabled = true;

            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "GPUAttack.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            // temporaria para recuperar los valores 
            g_pTempData = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, 0, Format.A8R8G8B8, Pool.SystemMemory);
            // stencil
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(MAX_DS,MAX_DS,DepthFormat.D24S8,MultiSampleType.None,0,true);

            //Se crean 2 triangulos con las dimensiones de la pantalla con sus posiciones ya transformadas
            // x = -1 es el extremo izquiedo de la pantalla, x=1 es el extremo derecho
            // Lo mismo para la Y con arriba y abajo
            // la Z en 1 simpre
            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[]
		    {
    			new CustomVertex.PositionTextured( -1, 1, 1, 0,0), 
			    new CustomVertex.PositionTextured(1,  1, 1, 1,0),
			    new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
			    new CustomVertex.PositionTextured(1,-1, 1, 1,1)
    		};
            //vertex buffer de los triangulos
            g_pVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVB.SetData(vertices, 0, LockFlags.None);


            string input = InputBox("Ingrese la Clave de 4 letras (A..Z)", "Clave", "CASA");

            int []clave = new int[4];
            for (int i = 0; i < 4;++i )
                clave[i] = input[i];
            Hash(clave, hash);
            effect.SetValue("hash_buscado", hash);

            char[] buffer = new char[5];
            buffer[0] = (char)hash[0];
            buffer[1] = (char)hash[1];
            buffer[2] = (char)hash[2];
            buffer[3] = (char)hash[3];
            buffer[4] = (char)0;
            string msg = new string(buffer);
            msg = "El hash es " + msg + "\n";
            MessageBox.Show(msg);

        }


        public unsafe override void render(float elapsedTime)
        {
            if(found)
                return;

            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;

            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;

            device.RenderState.ZBufferEnable = false;
            device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
            device.BeginScene();
            effect.Technique = "ComputeHash";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVB, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            device.EndScene();
            device.Present();
            device.SetRenderTarget(0, pOldRT);
            device.DepthStencilSurface = pOldDS;


            // leo los datos de la textura 
            // ----------------------------------------------------------------------
            Surface pDestSurf = g_pTempData.GetSurfaceLevel(0);
            device.GetRenderTargetData(pSurf, pDestSurf);
            Byte* pData = (Byte*)pDestSurf.LockRectangle(LockFlags.None).InternalData.ToPointer();

            string msg = "";

            for (int i = 0; i < MAX_DS; i++)
            {
                for (int j = 0; j < MAX_DS; j++)
                {
                    Byte A = *pData++;
                    Byte R = *pData++;
                    Byte G = *pData++;
                    Byte B = *pData++;

                    if (R == 255 && G == 255 && B == 255)
                    {
                        int group_x = j / 32;
                        int x = j % 32;
                        int group_y = i / 32;
                        int y = i % 32;

                        int[] clave = new int[4];
                        clave[0] = 'A' + group_x;
                        clave[1] = 'A' + group_y;
                        clave[2] = 'A' + x;
                        clave[3] = 'A' + y;
                        Hash(clave, hash);
                        char []buffer = new char[5];
                        buffer[0] = (char)clave[0]; 
                        buffer[1] = (char)clave[1]; 
                        buffer[2] = (char)clave[2]; 
                        buffer[3] = (char)clave[3]; 
                        buffer[4] = (char)0;
                        msg = new string(buffer);
                        msg = "La clave que elegiste es " + msg + "\n";
                        found = true;
                    }


                    /*
                    int group_x = j / 32;
                    int x = j % 32;
                    int group_y = i/ 32;
                    int y = i % 32;

                    int[] clave = new int[4];
                    clave[0] = 'A' + group_x;
                    clave[1] = 'A' + group_y;
                    clave[2] = 'A' + x;
                    clave[3] = 'A' + y;

                    Hash(clave, hash);

                    if (hash[0] != G || hash[1] != R || hash[2] != A || hash[3] != B)
                    {
                        int a = 0;
                    }
                     */
                }
            }
            pDestSurf.UnlockRectangle();
            pSurf.Dispose();

            if (found)
                MessageBox.Show(msg);

        }


        public void Hash(int []clave,int[]buffer)
        {
	     /*   for(int i=0;i<4;++i)
                buffer[i] = (a * clave[i] + c) % m;
          */

            int k = (clave[0] + clave[1] + clave[2] + clave[3]) % 256;
	        for(int i=0;i<4;++i)
	        {
		        k = (a*k + c) % m;
                buffer[i] = k;
                k += clave[i];
            }
        }



        public override void close()
        {
            effect.Dispose();
            g_pRenderTarget.Dispose();
            g_pDepthStencil.Dispose();
            g_pVB.Dispose();
            g_pTempData.Dispose();
        }



        public static String InputBox(String caption, String prompt, String defaultText)
        {
            String localInputText = defaultText;
            if (InputQuery(caption, prompt, ref localInputText))
            {
                return localInputText;
            }
            else
            {
                return "";
            }
        }

        public static int MulDiv(int a, float b, int c)
        {
            return (int)((float)a * b / (float)c);
        }

        public static Boolean InputQuery(String caption, String prompt, ref String value)
        {
            Form form;
            form = new Form();
            form.AutoScaleMode = AutoScaleMode.Font;
            form.Font = SystemFonts.IconTitleFont;

            SizeF dialogUnits;
            dialogUnits = form.AutoScaleDimensions;

            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.Text = caption;

            form.ClientSize = new Size(
                        MulDiv(180, dialogUnits.Width, 4),
                        MulDiv(63, dialogUnits.Height, 8));

            form.StartPosition = FormStartPosition.CenterScreen;

            System.Windows.Forms.Label lblPrompt;
            lblPrompt = new System.Windows.Forms.Label();
            lblPrompt.Parent = form;
            lblPrompt.AutoSize = true;
            lblPrompt.Left = MulDiv(8, dialogUnits.Width, 4);
            lblPrompt.Top =  MulDiv(8, dialogUnits.Height, 8);
            lblPrompt.Text = prompt;

            System.Windows.Forms.TextBox edInput;
            edInput = new System.Windows.Forms.TextBox();
            edInput.Parent = form;
            edInput.Left = lblPrompt.Left;
            edInput.Top = MulDiv(19, dialogUnits.Height, 8);
            edInput.Width = MulDiv(164, dialogUnits.Width, 4);
            edInput.Text = value;
            edInput.SelectAll();


            int buttonTop = MulDiv(41, dialogUnits.Height, 8);
            //Command buttons should be 50x14 dlus
            //Size buttonSize = ScaleSize(new Size(50, 14), dialogUnits.Width / 4, dialogUnits.Height / 8);
            Size buttonSize = new Size(MulDiv(50, dialogUnits.Width, 4),
                                                 MulDiv(14, dialogUnits.Height, 8));
            System.Windows.Forms.Button bbOk = new System.Windows.Forms.Button();
            bbOk.Parent = form;
            bbOk.Text = "OK";
            bbOk.DialogResult = DialogResult.OK;
            form.AcceptButton = bbOk;
            bbOk.Location = new Point(MulDiv(38, dialogUnits.Width, 4), buttonTop);
            bbOk.Size = buttonSize;

            System.Windows.Forms.Button bbCancel = new System.Windows.Forms.Button();
            bbCancel.Parent = form;
            bbCancel.Text = "Cancel";
            bbCancel.DialogResult = DialogResult.Cancel;
            form.CancelButton = bbCancel;
            bbCancel.Location = new Point(MulDiv(92, dialogUnits.Width, 4), buttonTop);
            bbCancel.Size = buttonSize;

            if (form.ShowDialog() == DialogResult.OK)
            {
                value = edInput.Text;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}


