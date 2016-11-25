using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jpgReader
{
    class JpegReader
    {
        int SOI = 0xffd8;
        int APP0 = 0xffe0;
        int APP1 = 0xffe1;
        int QuantizationTable = 0xffdb;
        int SOF1 = 0xffc0;
        int DHT = 0xffc4;
        int SOS = 0xffda;
        int EOI = 0xffd9;
        int EOIls = 0xd9;
        int Comment = 0xfffe;
        int markerMS = 0xff;

        public JpegModel ReadImage(Stream imageStream) {
            JpegModel jpegModel = new JpegModel();
            try {
                byte[] markerBytes = { 0 };
                BinaryReader reader = new BinaryReader(imageStream);
                ReadMarker(reader, SOI, "SOI");
                ReadMarker(reader, APP0, "APP0");
                ReadApp0Segment(reader, jpegModel);
                while ((markerBytes = reader.ReadBytes(2).Reverse().ToArray()).Length != 0) {
                    int marker = BitConverter.ToUInt16(markerBytes, 0);
                    if (marker == APP1)
                        ReadAPP1Segment(reader, jpegModel);
                    else if (marker == QuantizationTable)
                        ReadQuantizationTable(reader, jpegModel);
                    else if (marker == SOF1)
                        ReadSof1Segment(reader, jpegModel);
                    else if (marker == DHT)
                        ReadDhtSegment(reader, jpegModel);
                    else if (marker == SOS) {
                        ReadScanSegment(reader, jpegModel);
                        return jpegModel;
                    }
                    else if (marker == Comment)
                        ReadComment(reader, jpegModel);
                    else if (marker == EOI)
                        return jpegModel;
                    else
                        Console.WriteLine("Unknown marker: " + BitConverter.ToString(markerBytes));
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Error with jpeg format " + ex.Message);
            }

            return jpegModel;
        }

        private void ReadComment(BinaryReader reader, JpegModel jpegModel) {
            int segmentLength = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            jpegModel.comments.Add(reader.ReadBytes(segmentLength - 2));
        }

        private void ReadScanSegment(BinaryReader reader, JpegModel jpegModel) {
            int segmentLength = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            jpegModel.imageSegments.Add(reader.ReadBytes(segmentLength - 2));
            byte[] buffer;
            byte marker;
            while((buffer = reader.ReadBytes(1)).Length > 0) {
                if(buffer[0] == markerMS) {
                    marker = reader.ReadByte();
                    if (marker == EOIls)
                        break;
                    jpegModel.scannedData.Enqueue(buffer[0]);
                    jpegModel.scannedData.Enqueue(marker);
                    continue;
                }
                jpegModel.scannedData.Enqueue(buffer[0]);
            }

        }

        private void ReadDhtSegment(BinaryReader reader, JpegModel jpegModel) {
            int segmentLength = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            Stream stream = new MemoryStream(reader.ReadBytes(segmentLength - 2));
            BinaryReader mReader = new BinaryReader(stream);
            int key = mReader.ReadBytes(1)[0];
            DHT value = new DHT
            {
                L = mReader.ReadBytes((segmentLength - 3) / 2),
                V = mReader.ReadBytes((segmentLength - 3) / 2)
            };
            jpegModel.dht.Add(key, value);
        }

        private void ReadSof1Segment(BinaryReader reader, JpegModel jpegModel) {
            int segmentLength = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            Stream stream = new MemoryStream(reader.ReadBytes(segmentLength - 2));
            BinaryReader mReader = new BinaryReader(stream);
            jpegModel.sof1.P = mReader.ReadBytes(1)[0];
            jpegModel.sof1.Y = BitConverter.ToUInt16(mReader.ReadBytes(2).Reverse().ToArray(), 0);
            jpegModel.sof1.X = BitConverter.ToUInt16(mReader.ReadBytes(2).Reverse().ToArray(), 0);
            jpegModel.sof1.Nf = mReader.ReadBytes(1)[0];
            jpegModel.sof1.C1 = mReader.ReadBytes(1)[0];
            jpegModel.sof1.H1 = mReader.ReadBytes(1)[0];
            jpegModel.sof1.Tq1 = mReader.ReadBytes(1)[0];
            jpegModel.sof1.C2 = mReader.ReadBytes(1)[0];
            jpegModel.sof1.H2 = mReader.ReadBytes(1)[0];
            jpegModel.sof1.Tq2 = mReader.ReadBytes(1)[0];
            jpegModel.sof1.C3 = mReader.ReadBytes(1)[0];
            jpegModel.sof1.H3 = mReader.ReadBytes(1)[0];
            jpegModel.sof1.Tq3 = mReader.ReadBytes(1)[0];
        }

        private void ReadQuantizationTable(BinaryReader reader, JpegModel jpegModel) {
            int segmentLength = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            jpegModel.quantizationTables.Add(reader.ReadBytes(segmentLength - 2));
        }

        private void ReadAPP1Segment(BinaryReader reader, JpegModel jpegModel) {
            int segmentLength = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            Stream stream = new MemoryStream(reader.ReadBytes(segmentLength - 2));
        }

        private void ReadApp0Segment(BinaryReader fileReader, JpegModel jpegModel) {
            
            int segmentLength = BitConverter.ToUInt16(fileReader.ReadBytes(2).Reverse().ToArray(), 0);
            Stream stream = new MemoryStream(fileReader.ReadBytes(segmentLength-2));
            BinaryReader reader = new BinaryReader(stream);
            ReadIdentifier(reader, jpegModel);
            jpegModel.app0.majorVersion = reader.ReadBytes(1)[0];
            jpegModel.app0.minorVersion = reader.ReadBytes(1)[0];
            jpegModel.app0.unitsOfPixelDensity = (UnitsOfPixelDensity)reader.ReadBytes(1)[0];
            jpegModel.app0.xDensity = BitConverter.ToInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            if (jpegModel.app0.xDensity == 0)
                throw new Exception("yDensity can not be 0");
            jpegModel.app0.yDensity = BitConverter.ToInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            if (jpegModel.app0.yDensity == 0)
                throw new Exception("yDensity can not be 0");
            jpegModel.app0.xThumbnail = reader.ReadBytes(1)[0];
            jpegModel.app0.yThumbnail = reader.ReadBytes(1)[0];
            Console.WriteLine(stream.Length);
        }

        private void ReadIdentifier(BinaryReader reader, JpegModel jpegModel) {
            byte[] byteArray = reader.ReadBytes(4);
            string id = "";
            foreach (byte b in byteArray)
                id += Convert.ToChar(b);
            jpegModel.app0.identifier = id;
            int nullByte = reader.ReadBytes(1)[0];
            if (nullByte != 0)
                throw new Exception("Missing null byte after APP0 identifier");
        }

        private void ReadMarker(BinaryReader reader, int marker, string markerName) {
            byte[] byteArray = reader.ReadBytes(2);
            int rodeMarker = BitConverter.ToUInt16(byteArray.Reverse().ToArray(), 0);
            if (marker != rodeMarker)
                throw new Exception(markerName + " marker not found.");
        }

    }
}
