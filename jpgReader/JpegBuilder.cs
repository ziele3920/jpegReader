using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace jpgReader
{
    class JpegBuilder
    {
        public void BuildJpeg(JpegModel imageModel, string fileAppend) {
            string path = imageModel.file;
            path = path.Replace(".jpg", fileAppend + ".jpg");
            if (File.Exists(path))
                File.Delete(path);
            FileStream creatingFile = File.Create(path);
            BinaryWriter writter = new BinaryWriter(creatingFile);
            int iMax = imageModel.headerData.Count;
            for (int i = 0; i < iMax; ++i)
                writter.Write((byte)imageModel.headerData.Dequeue());
            iMax = imageModel.sampleData.Count;
            for (int i = 0; i < iMax; ++i)
                writter.Write((byte)imageModel.sampleData.Dequeue());
            writter.Write((byte)0xff);
            writter.Write((byte)0xd9);
            creatingFile.Close();
        }

        public void BuildQTJpeg(JpegModel imageModel, string fileAppend) {
            string path = imageModel.file;
            path = path.Replace(".jpg", fileAppend + ".jpg");
            if (File.Exists(path))
                File.Delete(path);
            FileStream creatingFile = File.Create(path);
            BinaryWriter writter = new BinaryWriter(creatingFile);
            writter.Write(imageModel.beforeQT1.ToArray());
            writter.Write(imageModel.QT1);
            for (int i = 551; i > 545; --i)
                imageModel.afterQT1[i] = (byte)0x01;
            writter.Write(imageModel.afterQT1.ToArray());
        }

        internal void BuilQTCryptedJpeg(JpegModel imageModel, string fileAppend) {
            string path = imageModel.file;
            path = path.Replace(".jpg", fileAppend + ".jpg");
            if (File.Exists(path))
                File.Delete(path);
            FileStream creatingFile = File.Create(path);
            BinaryWriter writter = new BinaryWriter(creatingFile);
            writter.Write(imageModel.beforeQT1.ToArray());
            WriteCrypedQT(imageModel, writter);
            writter.Write(imageModel.afterQT1.ToArray());
            //WriteLastPartOfQt(imageModel, writter);
        }

        internal string BuildCryptedImage(JpegModel imageModel, string fileAppend) {
            string path = imageModel.file;
            path = path.Replace(".jpg", fileAppend + ".jpg");
            if (File.Exists(path))
                File.Delete(path);
            FileStream creatingFile = File.Create(path);
            BinaryWriter writter = new BinaryWriter(creatingFile);
            writter.Write(imageModel.beforeCurrentSamples.ToArray());
            foreach (List<byte> list in imageModel.cryptedSamples) 
                writter.Write(list.ToArray());          
            writter.Write((byte)0xff);
            writter.Write((byte)0xd9);
            writter.Close();
            creatingFile.Close();
            return path;
        }

        private void WriteLastPartOfQt(JpegModel imageModel, BinaryWriter writter) {
            byte[] data = new byte[96 - 64];
            for (int i = 0; i < (96 - 64); ++i)
                data[i] = imageModel.cryptedQT1[i + (96 - 64)];
            writter.Write(data);
        }

        private void WriteCrypedQT(JpegModel imageModel, BinaryWriter writter) {
            byte[] data = new byte[64];
            for (int i = 0; i < 64; ++i)
                data[i] = imageModel.cryptedQT1[i];
            writter.Write(data);
        }


        private void AddSomething(BinaryWriter writter, int tableCounter) {
            if (tableCounter == 0)
                writter.Write((byte)0x01);
            else if (tableCounter == 2)
                writter.Write((byte)0x03);
            else 
                writter.Write((byte)0x02);
        }

        private void AddIndex(BinaryWriter writter, int key) {
            byte[] index = BitConverter.GetBytes(key);
            writter.Write(index[0]);
            writter.Write(index[1]); 
        }

        private void Addlength(BinaryWriter writter, JpegModel imageModel, int tableCounter) {
            writter.Write((byte)0x00);
            if (tableCounter == 0 || tableCounter == 2)
                writter.Write((byte)0x1f);
            else
                writter.Write((byte)0xB5);
        }

        private void AddDhtMarker(BinaryWriter writter) {
            writter.Write((byte)0xff);
            writter.Write((byte)0xc4);
        }
    }
}
