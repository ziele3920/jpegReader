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
            for(int i = 0; i < iMax; ++i)
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
            writter.Write(imageModel.afterQT1.ToArray());
        }
    }
}
