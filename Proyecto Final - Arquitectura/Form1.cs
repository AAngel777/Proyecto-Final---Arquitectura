using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome.Sharp;
using System.Runtime.InteropServices;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Diagnostics;

namespace Proyecto_Final___Arquitectura
{
    public partial class Form1 : Form
    {
        private bool Encender = true; // Estado inicial
        System.IO.Ports.SerialPort Arduino;
        string Puerto = "COM7";
        int n = 0;

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        public Form1()
        {
            InitializeComponent();
    
        }

        private void btnAutomático_Click(object sender, EventArgs e)
        {
            Arduino = new System.IO.Ports.SerialPort();
            Arduino.PortName = Puerto;
            Arduino.BaudRate = 9600;
            Arduino.Open();
            if (Arduino.IsOpen) { pnlInformacion.Enabled = true; tmrHumedad.Start(); btnAutomático.Enabled = false; }
        }
        private void tmrHumedad_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("Tick del temporizador");
            string letras = "HTP";

            char letraActual = letras[n % letras.Length];
            n++;

            Arduino.Write(letraActual.ToString());

            string Lectura = Arduino.ReadLine();
            // Verificar si la cadena comienza con la letra "h"
            if (Lectura.StartsWith("h"))

            {

                string Resultante = Lectura.Substring(1);
                float porcentajeSeco = (int.Parse(Resultante)/1023.0f)*(100);
                float porcentajeHumedo = 100 - porcentajeSeco;

                ActualizarLabel(lblHumedad, porcentajeHumedo.ToString("F2"));

                /*if (porcentajeHumedo >= 70)
                {
                    MessageBox.Show("La planta está lo suficientemente húmeda.");
                }
                else if (porcentajeHumedo <= 30)
                {
                    MessageBox.Show("La planta está seca y necesita ser regada.");
                }*/

            }
            else if (Lectura.StartsWith("t"))
            {

                string Resultante = Lectura.Substring(1);
                ActualizarLabel(lblTemperatura, Resultante);
            }
            else if (Lectura.StartsWith("p"))
            {
                string Resultante = Lectura.Substring(1);
                picBomba.Text = Resultante;
            }
            else
            {

            }

            if (n > 2) { n = 0; }

        }
        private void cmdCamara_Click(object sender, EventArgs e)
        {
            // Obtener la lista de dispositivos de video disponibles
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No se encontraron cámaras conectadas.");
            }
            else
            {
                // Seleccionar el primer dispositivo (cámara)
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += VideoSource_NewFrame;

                // Iniciar la captura de video
                videoSource.Start();
            }
        }
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Mostrar el fotograma de video en un control PictureBox
            //pictureBox4.Image = (Bitmap)eventArgs.Frame.Clone();
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom; // Esto ajustará automáticamente la imagen al tamaño del PictureBox
                                                            // Supongamos que eventArgs.Frame es tu imagen de la cámara
            Bitmap imagenCamara = (Bitmap)eventArgs.Frame.Clone();

            // Ajustar la imagen al tamaño del pictureBox4
            pictureBox4.Image = ResizeImage(imagenCamara, pictureBox4.Width, pictureBox4.Height);
        }
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.DrawImage(image, 0, 0, width, height);
            }

            return resizedImage;
        }
        private void cmdSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }


        private void cmdApagarBomba_Click(object sender, EventArgs e)
        {

            if (Encender == false)
            {
                cmdApagarBomba.IconChar = IconChar.DropletSlash;
                Arduino.Write("E");
                Encender = true;
            }
            else
            {
                cmdApagarBomba.IconChar = IconChar.Droplet;
                Arduino.Write("A");
                cmdApagarBomba.Text = "Encender Bomba";
                Encender = false;

            }

            
            //Encender = !Encender; no se que es):
        }
        private void cmdApagarArduino_Click(object sender, EventArgs e)
        {
            tmrHumedad.Enabled = false;
            if (Arduino.IsOpen)
            {
                Arduino.Close();
                btnAutomático.Enabled = true;
                
            }
            pnlInformacion.Enabled = false;
        }
        private void Cerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Maximizar_Click(object sender, EventArgs e)
        {
            //No es posible maximizar ya que es de un tamaño predeterminado
        }

        private void Minimizar_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        //Mover formulario desde la barra superior
        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void Barra_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
        private void btnModo_Click(object sender, EventArgs e)
        {
            ModoOscuro(!IsModoOscuro());
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrHumedad.Enabled = false;

            if (Arduino != null)
                if (Arduino.IsOpen)
                {
                    Arduino.Close();
                }

            // Detener la captura de video cuando se cierra la aplicación
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }

        private void ModoOscuro(bool activarModoOscuro)
        {
            
            if (activarModoOscuro)
            {
                // Modo oscuro

                //Fondo
                BackColor = Color.FromArgb(42, 42, 42);

                //Barra Superior
                Barra.BackColor = Color.FromArgb(34, 34, 34);
                Cerrar.BackColor = Color.FromArgb(34, 34, 34);
                Minimizar.BackColor = Color.FromArgb(34, 34, 34);
                Maximizar.BackColor = Color.FromArgb(34, 34, 34);
                btnModo.BackColor = Color.FromArgb(34, 34, 34);

                //Panel de busqueda
                pnlBusqueda.BackColor = Color.FromArgb(34, 34, 34);
                pnlBuscar.BackColor = Color.FromArgb(61, 61, 61);
                lblPuerto.BackColor = Color.FromArgb(61, 61, 61);
                btnAutomático.BackColor = Color.FromArgb(34, 34, 34);

                //Panel Información
                pnlInformacion.BackColor = Color.FromArgb(34, 34, 34);
                lblINFORMACION.ForeColor = Color.White;
                lblINFORMACION.BackColor = Color.FromArgb(34, 34, 34);
                
                lblTerrario.BackColor = Color.FromArgb(34, 34, 34);
                lblTerrario.ForeColor = Color.Silver;

                pnlPlanta.BackColor = Color.FromArgb(43, 42, 42);
                pnlTemperatura.BackColor = Color.FromArgb(38, 38, 38);
                pnlHumedad.BackColor = Color.FromArgb(38, 38, 38);
                pnlAgua.BackColor = Color.FromArgb(38, 38, 38);

                picAgua.IconColor = Color.Silver;
                picHumedad.IconColor = Color.Silver;
                picTemperatura.IconColor = Color.Silver;

                //Panel Botones
                pnlBotones.BackColor = Color.FromArgb(34, 34, 34);
                cmdApagarArduino.IconColor = Color.White;
                cmdApagarArduino.BackColor = Color.FromArgb(43, 42, 42);
                cmdApagarBomba.BackColor = Color.FromArgb(43, 42, 42);
                cmdApagarBomba.IconColor = Color.White;


                //Panel Botón Camara
                pnlbtnCamara.BackColor = Color.FromArgb(34, 34, 34);
                cmdCamara.IconColor = Color.White;
                cmdCamara.BackColor = Color.FromArgb(43, 42, 42);

                //Panel Grabación
                pnlGrabacion.BackColor = Color.FromArgb(34, 34, 34);
                pnlPlanta2.BackColor = Color.FromArgb(43, 42, 42);
                lblGrabacion.ForeColor = Color.White;
                lblGrabacion.BackColor = Color.FromArgb(34, 34, 34);

            }
            else
            {
                // Modo claro 
                //Fondo
                BackColor = Color.FromArgb(231, 232, 235);

                //Barra Superior
                Barra.BackColor = Color.White;
                Cerrar.BackColor = Color.White;
                Minimizar.BackColor = Color.White;
                Maximizar.BackColor = Color.White;
                btnModo.BackColor = Color.White;

                //Panel de busqueda
                pnlBusqueda.BackColor = Color.White;
                pnlBuscar.BackColor = Color.FromArgb(224, 224, 224);
                lblPuerto.BackColor = Color.FromArgb(224, 224, 224);
                btnAutomático.BackColor = Color.White;

                //Panel Información
                pnlInformacion.BackColor = Color.White;
                lblINFORMACION.ForeColor = Color.Black;
                lblINFORMACION.BackColor = Color.White;

                lblTerrario.BackColor = Color.White;
                lblTerrario.ForeColor = Color.FromArgb(64, 64, 64);

                pnlPlanta.BackColor = Color.FromArgb(236, 242, 239);
                pnlTemperatura.BackColor = Color.FromArgb(234, 241, 235);
                pnlHumedad.BackColor = Color.FromArgb(234, 241, 235);
                pnlAgua.BackColor = Color.FromArgb(234, 241, 235);

                picAgua.IconColor = Color.Gray;
                picHumedad.IconColor = Color.Gray;
                picTemperatura.IconColor = Color.Gray;

                //Panel Botones
                pnlBotones.BackColor = Color.White;
                cmdApagarArduino.IconColor = Color.Black;
                cmdApagarArduino.BackColor = Color.FromArgb(224, 224, 224);
                cmdApagarBomba.BackColor = Color.FromArgb(224, 224, 224);
                cmdApagarBomba.IconColor = Color.Black;


                //Panel Botón Camara
                pnlbtnCamara.BackColor = Color.White;
                cmdCamara.IconColor = Color.Black;
                cmdCamara.BackColor = Color.FromArgb(224, 224, 224);

                //Panel Grabación
                pnlGrabacion.BackColor = Color.White;
                pnlPlanta2.BackColor = Color.FromArgb(236, 242, 239);
                lblGrabacion.ForeColor = Color.Black;
                lblGrabacion.BackColor = Color.White;

            }

            
        }

        private bool IsModoOscuro()
        {
            // Determinar si el formulario está en modo oscuro
            return BackColor == Color.FromArgb(42, 42, 42);
        }
        private void ActualizarLabel(Label label, string texto)
        {
            if (label.InvokeRequired)
            {
                label.BeginInvoke((MethodInvoker)delegate
                {
                    label.Text = texto;
                });
            }
            else
            {
                label.Text = texto;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pnlInformacion_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}
