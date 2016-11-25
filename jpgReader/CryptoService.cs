using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Math;



namespace jpgReader
{
    class CryptoService
    {
        const int keyBitsCount = 128;

        internal void RSA(JpegModel jpegModel) {
            BigInteger p, q, eulerFunction, mod, e;
            p = GetBigInteger(keyBitsCount);
            do {
                q = GetBigInteger(keyBitsCount);
            } while (p == q);
            eulerFunction = (p.Subtract(BigInteger.One).Multiply(p.Subtract(BigInteger.One)));
            mod = p.Multiply(q);
            e = GetENumber(eulerFunction);
        }

        private BigInteger GetBigInteger(int bits) {
            Org.BouncyCastle.Security.SecureRandom ran = new Org.BouncyCastle.Security.SecureRandom();

            BigInteger c = new BigInteger(bits, ran);

            for (;;) {
                if (c.IsProbablePrime(1) == true) break;

                c = c.Subtract(new BigInteger("1"));
            }
            return (c);
        }

        //private BigInteger GetInverseModulo(BigInteger a, BigInteger b) {
        //    BigInteger u, w, x, z, q;
        //    u = BigInteger.One;
        //    w = a;
        //    x = BigInteger.Zero;
        //    z = b;
        //    while(w != BigInteger.Zero) {
        //        if(w.)
        //    }
        //}

        private BigInteger GetENumber(BigInteger eulerFunction) {
            BigInteger eValue = BigInteger.Three.Add(BigInteger.Ten);
            while(NWD(eValue, eulerFunction) != BigInteger.One)
                eValue = eValue.Add(BigInteger.One);
            return eValue;
        }

        private BigInteger NWD(BigInteger a, BigInteger b) {
            BigInteger tmp;
            while(b != BigInteger.Zero) {
                tmp = b;
                b = a.Mod(b);
                a = tmp;
            }
            return a;
        }
    }
}
