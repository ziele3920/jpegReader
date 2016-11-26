using AForge.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jpgReader
{
    class FFTService
    {
        public class FFTDataModel
        {
            public List<int> frequency;
            public List<double> magnitude;
            public Complex[] data;

            public FFTDataModel() {
                frequency = new List<int>();
                magnitude = new List<double>();
            }
        }

        public class FFTImageDataModel
        {
            public FFTDataModel R;
            public FFTDataModel G;
            public FFTDataModel B;
        }

        public static FFTImageDataModel FFT(JpegModel imageModel) {
            Image image = Image.FromFile(imageModel.file);
            Bitmap bitmap = new Bitmap(image);
            FFTImageDataModel fftImageModel = new FFTImageDataModel();

            List<Color> colorList = new List<Color>();
            for (int y = 0; y < bitmap.Height; ++y)
                for (int x = 0; x < bitmap.Width; ++x)
                    colorList.Add(bitmap.GetPixel(x, y));
            
            return DoFFTOverImage(colorList);
        }

        private static FFTDataModel GetFftDataModel(Complex[] data) {
            FFTDataModel model = new FFTDataModel();
            model.data = data;
            for(int i = 0; i < data.Length/2; ++i) {
                model.frequency.Add(i);
                model.magnitude.Add(data[i].Magnitude);
            }
            return model;
        }

        private static FFTImageDataModel DoFFTOverImage(List<Color> colorsData) {
            int dataCount = 1024;
            FFTImageDataModel fftImageDataModel = new FFTImageDataModel();
            Complex[] dataR = new Complex[dataCount];
            Complex[] dataG = new Complex[dataCount];
            Complex[] dataB = new Complex[dataCount];
            for(int i = 0; i < dataCount; ++i) {
                dataR[i] = new Complex(colorsData[i].R, 0);
                dataG[i] = new Complex(colorsData[i].G, 0);
                dataB[i] = new Complex(colorsData[i].B, 0);
            }
            FourierTransform.FFT(dataR, FourierTransform.Direction.Forward);
            FourierTransform.FFT(dataG, FourierTransform.Direction.Forward);
            FourierTransform.FFT(dataB, FourierTransform.Direction.Forward);
            fftImageDataModel.R = GetFftDataModel(dataR);
            fftImageDataModel.G = GetFftDataModel(dataG);
            fftImageDataModel.B = GetFftDataModel(dataB);
            return fftImageDataModel;
        }

    }
}
