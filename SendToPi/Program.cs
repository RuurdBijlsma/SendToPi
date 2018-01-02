using System;
using System.IO;
using Newtonsoft.Json;
using Renci.SshNet;

namespace SendToPi
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                RaspberryInfo connection;
                using (var r = new StreamReader("connection.json"))
                {
                    var json = r.ReadToEnd();
                    connection = JsonConvert.DeserializeObject<RaspberryInfo>(json);
                }

                if (connection.Ip == null)
                {
                    Console.WriteLine("Couldn't read connection.json");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Connecting to " + connection.Ip + "...");
                var uploadFile = new FileInfo(args[0]);
                using (var fileStream = new FileStream(uploadFile.FullName, FileMode.Open))
                using (var client = new SftpClient(connection.Ip, connection.User, connection.Password))
                {
                    client.Connect();
                    Console.WriteLine("Sending " + uploadFile.Name);

                    float completion = 0;
                    var step = 1f / Console.WindowWidth;
                    Console.Write("=");
                    client.UploadFile(fileStream, connection.DestinationPath + uploadFile.Name, progress =>
                    {
                        var percentage = (float) (progress) / uploadFile.Length;
                        if (!(percentage - completion > step)) return;
                        completion = percentage;
                        Console.Write("=");
                    });
                    Console.WriteLine();
                }

                Console.WriteLine("Done");
            }
            catch (Exception e)
            {
                Console.WriteLine("File upload/transfer Failed.\r\nError Message:\r\n" + e.Message);
            }
        }
    }
}