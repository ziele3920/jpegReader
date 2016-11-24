using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace jpgReader
{
    public partial class Form1 : Form
    {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            AllocConsole();
            Console.WriteLine("dupaczaba");
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();


        private void button1_Click(object sender, EventArgs e) {

            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            JpegModel jpegModel;
            JpegReader jpegReader = new JpegReader();

            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);
            openFileDialog1.Filter = "images (*.jpg)|*.jpg";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                try {
                    if ((myStream = openFileDialog1.OpenFile()) != null) {
                        using (myStream) {
                            // Insert code to read the stream here.
                            jpegModel = jpegReader.ReadImage(myStream);
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
    }
}
