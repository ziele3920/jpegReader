
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace jpgReader
{
    class CryptoNetService
    {
        const int keyBitsCount = 2048;
        const int blockSize = 214;
        private RSAParameters privKey;
        private RSAParameters pubKey;

        internal void Rsa(JpegModel jpegModel) {
            RSA csp = new RSACryptoServiceProvider(2048);
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

            jpgReader.ReadToCurrentSamples(oryginalReader, jpegModel);
            EncryptImage(jpegModel);

            return builder.BuildCryptedImage(jpegModel, "Crypted");

        }

        private void EncryptImage(JpegModel jpegModel) {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(pubKey);
            byte[] dataToEncrypt;
            for (int i = 0; i < jpegModel.currentSamples.Count; ++i) {
                jpegModel.cryptedSamples.Add(new List<byte>());
                List<byte> samples = new List<byte>(jpegModel.currentSamples[i]);
                if (i % 2 == 0)
                    do {
                        int currentBlockSize = samples.Count > blockSize ? blockSize : samples.Count;
                        dataToEncrypt = samples.GetRange(0, currentBlockSize).ToArray();
                        samples.RemoveRange(0, currentBlockSize);
                        jpegModel.cryptedSamples[i].AddRange(csp.Encrypt(dataToEncrypt, false));
                    } while (samples.Count > 0);
                else
                    jpegModel.cryptedSamples[i] = samples;
            }
            RemoveEOIMarkers(jpegModel);
        }

        private void RemoveEOIMarkers(JpegModel jpegModel) {
            for (int i = 0; i < jpegModel.cryptedSamples.Count; i+=2) {
                for(int j = 0; j < jpegModel.cryptedSamples[i].Count-1; ++j) {
                    if (jpegModel.cryptedSamples[i][j] == 0xff)
                        jpegModel.cryptedSamples[i][j] = 0x01; 
                }
            }
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
