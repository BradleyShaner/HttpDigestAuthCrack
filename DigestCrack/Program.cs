using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace DigestCrack
{
    internal static class Program
    {

        private static void Main(string[] args)
        {
            string qop = "auth";
            string realm = "AXIS_ACCC8E9EE842";
            string nonce = "arJMJyxlBQA=3d5f1f2ed223a47b69768ba33f36296d4f58e9ed";
            string uri = "/axis-cgi/usergroup.cgi?timestamp=1527035457956";
            string cnonce = "d029db17681a16d1";
            string nc = "00000001";
            string username = "test";
            string password = "test";
            string method = "GET";
            string expectedResponse = "b1a7701e8c087beebb3c9c3cd201014f";
            string expectedHa1 = "62bc0fa26b4edf5efe20356fbb66b5cc";
            string expectedHa2 = "2a140faf4b778008c9e42569eb00c22a";

            // This will calculate the ha1/ha2 easily so long as you have the correct values above.
            // All that is left to do is brute force calculate the correct response by testing
            // different passwords.

            Console.WriteLine("Calculating ha1..");
            string ha1 = (CreateHa1(username, realm, password, expectedHa1));
            Console.WriteLine("Calculating ha2..");
            string ha2 = (CreateHa2(method, uri, expectedHa2));
            Console.WriteLine("Calculating response..");
            string response = (CreateResponse(ha1, nonce, nc, cnonce, qop, ha2, expectedResponse));

            Benchmark(() => { CreateResponse(ha1, nonce, nc, cnonce, qop, ha2, expectedResponse, false); }, 1000000);

            Console.ReadLine();
        }

        private static void Benchmark(Action act, int iterations)
        {
            GC.Collect();
            act.Invoke(); // run once outside of loop to avoid initialization costs
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                act.Invoke();
            }
            sw.Stop();
            Console.WriteLine("Calculated " + iterations + " in " + sw.ElapsedMilliseconds + "ms");

            long perMs = (iterations / sw.ElapsedMilliseconds);

            long perSecond = perMs * 1000;

            Console.WriteLine("Calculations per second: " + perSecond);
        }

        private static string CreateHa1(string username, string realm, string password, string expectedMd5 = "")
        {
            byte[] input = System.Text.Encoding.ASCII.GetBytes($"{username}:{realm}:{password}");

            string md5 = CreateMD5(input).ToLower();

            Console.WriteLine($"ha1 result: {md5}");

            if (expectedMd5 == md5)
                Console.WriteLine("ha1 matches!");

            return md5;
        }

        private static string CreateHa2(string method, string uri, string expectedMd5 = "")
        {
            byte[] input = System.Text.Encoding.ASCII.GetBytes($"{method}:{uri}");

            string md5 = CreateMD5(input).ToLower();

            Console.WriteLine($"ha2 result: {md5}");

            if (expectedMd5 == md5)
                Console.WriteLine("ha2 matches!");

            return md5;
        }

        private static string CreateMD5(byte[] input)
        {
            MD5 md5 = MD5.Create();

            byte[] hash = md5.ComputeHash(input); //bytes

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return (sb.ToString());
        }

        private static string CreateResponse(string ha1, string nonce, string nc, string cnonce, string qop, string ha2, string expectedMd5 = "", bool showOutput = true)
        {
            byte[] input = System.Text.Encoding.ASCII.GetBytes($"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}");

            string md5 = CreateMD5(input).ToLower();

            if (showOutput)
            {
                Console.WriteLine($"Response result: {md5}");

                if (expectedMd5 == md5)
                    Console.WriteLine("response matches!");
            }
            return md5;
        }
        
    }
}