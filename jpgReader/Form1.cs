using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using AForge.Math;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

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
            CryptoNetService cryptoService = new CryptoNetService();

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
                            jpegModel.file = (myStream as FileStream).Name;
                           // FFTService.FFTImageDataModel fftImageDataModel = FFTService.FFT(jpegModel);
                            SetImageDetails(jpegModel);
                            ClearGraph();
                           // DrawGraph(fftImageDataModel);
                            cryptoService.Rsa(jpegModel);
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void ClearGraph() {
            chart1.Series["SeriesR"].Points.Clear();
            chart1.Series["SeriesG"].Points.Clear();
            chart1.Series["SeriesB"].Points.Clear();
        }

        private void DrawGraph(FFTService.FFTImageDataModel data) {
            for(int i = 0; i < data.R.frequency.Count; ++i) {
                chart1.Series["SeriesR"].Points.AddXY
                                (data.R.frequency[i], data.G.magnitude[i]);
                chart1.Series["SeriesG"].Points.AddXY
                                (data.G.frequency[i], data.G.magnitude[i]);
                chart1.Series["SeriesB"].Points.AddXY
                                (data.B.frequency[i], data.B.magnitude[i]);
            }

            chart1.Series["SeriesR"].ChartType = SeriesChartType.Line;
            chart1.Series["SeriesR"].Color = Color.Red;
            chart1.Series["SeriesR"].LegendText = "Channel R";
            chart1.ChartAreas["R"].AxisX.Title = "Frequency [ju]";
            chart1.ChartAreas["R"].AxisY.Title = "Magnitude [ju]";


            chart1.Series["SeriesG"].ChartType = SeriesChartType.Line;
            chart1.Series["SeriesG"].Color = Color.Green;
            chart1.Series["SeriesG"].LegendText = "Channel G";
            chart1.ChartAreas["G"].AxisX.Title = "Frequency [ju]";
            chart1.ChartAreas["G"].AxisY.Title = "Magnitude [ju]";

            chart1.Series["SeriesB"].ChartType = SeriesChartType.Line;
            chart1.Series["SeriesB"].Color = Color.Blue;
            chart1.Series["SeriesB"].LegendText = "Channel B";
            chart1.ChartAreas["B"].AxisX.Title = "Frequency [ju]";
            chart1.ChartAreas["B"].AxisY.Title = "Magnitude [ju]";
        }

        private void SetImageDetails(JpegModel imageModel) {
            horizontalResLabel.Text = imageModel.sof1.X + " px";
            verticalResLabel.Text = imageModel.sof1.Y + " px";
            colorDepthLabel.Text = imageModel.sof1.P + " bit";
            versionLabel.Text = imageModel.app0.majorVersion + ".0" + imageModel.app0.minorVersion;
            idLabel.Text = imageModel.app0.identifier;
            xDensityLabel.Text = imageModel.app0.xDensity + " (" + imageModel.app0.unitsOfPixelDensity + ")";
            yDensityLabel.Text = imageModel.app0.yDensity + " (" + imageModel.app0.unitsOfPixelDensity + ")";
        }

        private void label2_Click(object sender, EventArgs e) {

        }

        private void chart1_Click(object sender, EventArgs e) {

        }
    }
}
