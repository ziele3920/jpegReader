
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
        const int decrBlockSize = 256;
        private byte[] altSequence = new byte[] { 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00 };
        private RSAParameters privKey;
        private RSAParameters pubKey;

        internal void Rsa(JpegModel jpegModel) {
            RSA csp = new RSACryptoServiceProvider(2048);
            privKey = csp.ExportParameters(true);
            pubKey = csp.ExportParameters(false);

            string cipherFile = MakeCipherFile(jpegModel, csp, "Encrypted");
            string decriptedFile = DecryptFile(cipherFile, csp, "Decrypted");
        }

        private string DecryptFile(string encryptedFile, RSA netRsa, string fileAppend) {
            FileStream cipherStream = File.OpenRead(encryptedFile);
            BinaryReader cipherReader = new BinaryReader(cipherStream);
            JpegReader jpegReader = new JpegReader();
            JpegBuilder jpegBuilder = new JpegBuilder();
            JpegModel jpegModel = new JpegModel();

            jpegReader.ReadToEncryptedSamples(cipherReader, jpegModel);
            cipherReader.Close();
            cipherStream.Close();
            jpegModel.file = encryptedFile;
            DecyptImage(jpegModel);
            return jpegBuilder.BuildDecryptedImage(jpegModel, fileAppend);

        }

        private string MakeCipherFile(JpegModel jpegModel, RSA netRsa, string fileAppend) {
            FileStream oryginalImageStream = File.OpenRead(jpegModel.file);
            BinaryReader oryginalReader = new BinaryReader(oryginalImageStream);
            JpegReader jpgReader = new JpegReader();
            JpegBuilder builder = new JpegBuilder();

            jpgReader.ReadToCurrentSamples(oryginalReader, jpegModel);
            oryginalReader.Close();
            oryginalImageStream.Close();
            EncryptImage(jpegModel);

            return builder.BuildCryptedImage(jpegModel, fileAppend);

        }

        private void DecyptImage(JpegModel jpegModel) {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privKey);

            byte[] dataToDecrypt;
            RemoveSpecialSeqMarkers(jpegModel);
            for (int i = 0; i < jpegModel.cryptedSamples.Count; ++i) {
                jpegModel.currentSamples.Add(new List<byte>());
                List<byte> samples = new List<byte>(jpegModel.cryptedSamples[i]);
                if (i % 2 == 0)
                    do {
                        int currentBlockSize = samples.Count > decrBlockSize ? decrBlockSize : samples.Count;
                        if (currentBlockSize < decrBlockSize) {
                            samples.RemoveRange(0, currentBlockSize);
                            continue;
                        }
                        dataToDecrypt = samples.GetRange(0, currentBlockSize).ToArray();
                        samples.RemoveRange(0, currentBlockSize);
                        jpegModel.currentSamples[i].AddRange(csp.Decrypt(dataToDecrypt, false));
                    } while (samples.Count > 0);
                else
                    jpegModel.currentSamples[i] = samples;
            }
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
                        byte[] cryptedData = csp.Encrypt(dataToEncrypt, false);
                        jpegModel.cryptedSamples[i].AddRange(cryptedData);
                    } while (samples.Count > 0);
                else
                    jpegModel.cryptedSamples[i] = samples;
            }
            RemoveEOIMarkers(jpegModel);
        }

        private void RemoveEOIMarkers(JpegModel jpegModel) {
            for (int i = 0; i < jpegModel.cryptedSamples.Count; i+=2) {
                for(int j = 0; j < jpegModel.cryptedSamples[i].Count-1; ++j) {
                    if (jpegModel.cryptedSamples[i][j] == 0xff) {
                        jpegModel.cryptedSamples[i] = InsertSpecialSeqAfterFfAt(jpegModel.cryptedSamples[i], j);
                        j += altSequence.Length;
                    }
                }
            }
        }

        private void RemoveSpecialSeqMarkers(JpegModel jpegModel) {
            for (int i = 0; i < jpegModel.cryptedSamples.Count; i += 2) {
                for (int j = 0; j < jpegModel.cryptedSamples[i].Count - altSequence.Length; ++j) {
                    if ((jpegModel.cryptedSamples[i][j] == 0xff) &&
                         (jpegModel.cryptedSamples[i][j + 1] == altSequence[0]) &&
                         (jpegModel.cryptedSamples[i][j + 2] == altSequence[1]) &&
                         (jpegModel.cryptedSamples[i][j + 3] == altSequence[2]) &&
                         (jpegModel.cryptedSamples[i][j + 4] == altSequence[3]) &&
                         (jpegModel.cryptedSamples[i][j + 5] == altSequence[4]) &&
                         (jpegModel.cryptedSamples[i][j + 6] == altSequence[5]) &&
                         (jpegModel.cryptedSamples[i][j + 7] == altSequence[6])) 
                        jpegModel.cryptedSamples[i].RemoveRange(j + 1, altSequence.Length);                
                }
            }
        }

        private List<byte> InsertSpecialSeqAfterFfAt(List<byte> listToModyfy, int pos) {
            List<byte> newList = new List<byte>();
            for (int i = 0; i <= pos; ++i)
                newList.Add(listToModyfy[i]);
            newList.AddRange(altSequence);
            for (int j = pos+1; j < listToModyfy.Count; ++j)
                newList.Add(listToModyfy[j]);
            return newList;
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
