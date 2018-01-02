using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                using (var client = new SftpClient(connection.Ip, connection.User, connection.Password))
                {
                    client.Connect();
                    var line = Console.CursorTop;
                    var tasks = args.Select(file =>
                        Task.Run(() => Upload(file, client, connection.DestinationPath, line++)));
                    Task.WaitAll(tasks.ToArray());
                }

                Console.WriteLine("Done");
            }
            catch (Exception e)
            {
                Console.WriteLine("File upload/transfer Failed.\r\nError Message:\r\n" + e.Message);
            }
        }

        private static readonly object LockObject = new object();

        private static void Upload(string file, SftpClient client, string desinationPath, int row)
        {
            var uploadFile = new FileInfo(file);
            using (var fileStream = new FileStream(uploadFile.FullName, FileMode.Open))
            {
                var col = 0;
                var sendMsg = "Sending " + uploadFile.Name + " ";

                float completion = 0;
                var step = 1f / (Console.WindowWidth - sendMsg.Length);

                lock (LockObject)
                {
                    Console.SetCursorPosition(col, row);
                    Console.Write(sendMsg + "=");
                }
                col += sendMsg.Length + 1;

                client.UploadFile(fileStream, desinationPath + uploadFile.Name, progress =>
                {
                    var percentage = (float) progress / uploadFile.Length;
                    if (percentage - completion < step) return;
                    completion = percentage;
                    lock (LockObject)
                    {
                        Console.SetCursorPosition(col, row);
                        Console.Write("=");
                    }
                    col++;
                });
                lock (LockObject)
                {
                    Console.WriteLine();
                }
            }
        }
    }
}