using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;
using System.Text.Json;

namespace TelemetryLibrary
{
    class Register
    {
        public Register(string name, string url)
        {

            var hostname = "https://httpbin.org/post";

            var request = WebRequest.Create(hostname);
            request.Method = "POST";
            var options = new
            {
                name = name,
                url = url,
            };


            string jsonString = JsonSerializer.Serialize(options);


            Console.WriteLine(jsonString);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);


            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);

            var response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            var respStream = response.GetResponseStream();

            var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();
            Console.WriteLine(data);

            // record User(string Name, string Occupation);
        }
    }
}
