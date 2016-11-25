using System.Collections.Generic;

namespace jpgReader
{

    public enum UnitsOfPixelDensity
    {
        NoUnits = 0, // width:height pixel aspect ratio = Xdensity:Ydensity
        PixelsPerInch = 1,
        PixelsPecCentimeter = 2
    }

    struct APP0
    {
        public string identifier { get; set; }
        public int majorVersion { get; set; }
        public int minorVersion { get; set; }
        public UnitsOfPixelDensity unitsOfPixelDensity { get; set; }
        public int xDensity { get; set; }
        public int yDensity { get; set; }
        public int xThumbnail { get; set; }
        public int yThumbnail { get; set; }  
    }

    struct SOF1
    {
        public int P;
        public int Y; //2B
        public int X; //2B
        public int Nf;
        public int C1;
        public int H1; //V1
        public int Tq1;
        public int C2;
        public int H2; //V2
        public int Tq2;
        public int C3;
        public int H3; //v3;
        public int Tq3;
    }

    struct DHT
    {
        public byte[] L; // 
        public byte[] V;
    }

    class JpegModel
    {
        public APP0 app0;
        public SOF1 sof1;
        public Dictionary<int, DHT> dht;
        public List<byte[]> quantizationTables;
        public List<byte[]> comments;
        public List<byte[]> imageSegments;
        public Queue<byte> scannedData;
        public string file;
        public Queue<byte> headerData;
        public Queue<byte> sampleData;
        public List<byte> beforeQT1;
        public List<byte> afterQT1;
        public byte[] QT1;
        public byte[] cryptedQT1;
        public JpegModel() {
            app0 = new APP0();
            sof1 = new SOF1();
            dht = new Dictionary<int, DHT>();
            quantizationTables = new List<byte[]>();
            imageSegments = new List<byte[]>();
            comments = new List<byte[]>();
            scannedData = new Queue<byte>();
            headerData = new Queue<byte>();
            sampleData = new Queue<byte>();
            beforeQT1 = new List<byte>();
            afterQT1 = new List<byte>();
        }
  
    }
}
