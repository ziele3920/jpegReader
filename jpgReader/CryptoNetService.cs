
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace jpgReader
{
    class CryptoNetService
    {
        const int keyBitsCount = 128;
        private RSAParameters privKey;
        private RSAParameters pubKey;

        internal void Rsa(JpegModel jpegModel) {
            RSA csp = new RSACryptoServiceProvider(768);
            privKey = csp.ExportParameters(true);
            pubKey = csp.ExportParameters(false);

            string file = MakeCipherFile(jpegModel, csp);
        }

        private void DecryptFile(string file) {
            throw new NotImplementedException();
        }

        private string MakeCipherFile(JpegModel jpegModel, RSA netRsa) {
            FileStream oryginalImageStream = File.OpenRead(jpegModel.file);
            BinaryReader oryginalReader = new BinaryReader(oryginalImageStream);
            JpegReader jpgReader = new JpegReader();
            JpegBuilder builder = new JpegBuilder();
            //jpgReader.FillipherData(oryginalReader, jpegModel);
            jpgReader.ReadToQt(oryginalReader, jpegModel);
            int dataCount = jpegModel.sampleData.Count;
            byte[] oryginalData = jpegModel.quantizationTables[0];
           // byte[] data = jpegModel.quantizationTables[0];
           // for (int i = 0; i < 50; ++i)
           //     oryginalData[i] = data[i];
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(pubKey);
            

            jpegModel.cryptedQT1 = csp.Encrypt(jpegModel.QT1, false);
            //int iMax = 27;
           // for(int i =0; i < iMax; ++i)
           //     jpegModel.sampleData.Enqueue(cryptedData[i]);
            builder.BuildQTJpeg(jpegModel, "dupaczaba");
            return "";
        }

        public string GetPublicKey() {
            string pubKeyString;
            var sw = new StringWriter();
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, pubKey);
            pubKeyString = sw.ToString();
            return pubKeyString;
        }

        public RSAParameters GetPublicKey(string key) {
            var sr = new StringReader(key);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            return (RSAParameters)xs.Deserialize(sr);
        }


    }
}
