
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
            RSA csp = new RSACryptoServiceProvider(512);
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
            jpgReader.FillipherData(oryginalReader, jpegModel);
            int dataCount = jpegModel.sampleData.Count;
            byte[] oryginalData = new byte[dataCount];
            //for (int i = 0; i < dataCount; ++i)
           //     oryginalData[i] = jpegModel.sampleData.Dequeue();
           // var csp = new RSACryptoServiceProvider();
           // csp.ImportParameters(pubKey);

            //decrypt and strip pkcs#1.5 padding
            //byte[] cryptedData = csp.Encrypt(oryginalData, false);
            //int iMax = 27;
           // for(int i =0; i < iMax; ++i)
           //     jpegModel.sampleData.Enqueue(cryptedData[i]);
            builder.BuildJpeg(jpegModel, "dupaczaba");
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
