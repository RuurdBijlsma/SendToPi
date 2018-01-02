using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Renci.SshNet;

namespace SendToSftp
{
    internal static class Program
    {
        private static readonly object LockObject = new object();

        private static void Main(string[] args)
        {
            try
            {
                SftpInfo connection;
                using (var r = new StreamReader("connection.json"))
                {
                    var json = r.ReadToEnd();
                    connection = JsonConvert.DeserializeObject<SftpInfo>(json);
                }

                if (connection.Ip == null)
                {
                    Console.WriteLine("Couldn't read connection.json");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Connecting to " + connection.Ip + "...\n");
                using (var client = new SftpClient(connection.Ip, connection.User, connection.Password))
                {
                    client.Connect();

                    var line = Console.CursorTop;
                    if (connection.Multithreaded)
                    {
                        var tasks = args.Select(file =>
                            Task.Run(() => Upload(file, client, connection.DestinationPath, line++)));
                        Task.WaitAll(tasks.ToArray());
                        Thread.Sleep(500);
                    }
                    else
                    {
                        foreach (var file in args)
                            Upload(file, client, connection.DestinationPath, line++);
                    }

                    lock (LockObject)
                    {
                        Console.SetCursorPosition(0, line + 1);
                        Console.WriteLine("Done!");
                    }
                }
            }
            catch (Exception e)
            {
                lock (LockObject)
                {
                    Console.WriteLine("File upload/transfer Failed.\r\nError Message:\r\n" + e.Message);
                    Console.ReadKey();
                }
            }
        }

        private const string LoadingCharacter = ".";

        private static void Upload(string file, SftpClient client, string desinationPath, int row)
        {
            var uploadFile = new FileInfo(file);
            using (var fileStream = new FileStream(uploadFile.FullName, FileMode.Open))
            {
                var col = 0;
                var sendMsg = "Sending " + uploadFile.Name;

                float completion = 0;
                var consoleWidth = Console.WindowWidth;
                var step = 1f / (consoleWidth - sendMsg.Length);

                lock (LockObject)
                {
                    Console.SetCursorPosition(col, row);
                    Console.Write(sendMsg);
                }

                col += sendMsg.Length;

                client.UploadFile(fileStream, desinationPath + uploadFile.Name, progress =>
                {
                    var percentage = (float) progress / uploadFile.Length;

                    if (Math.Abs(percentage - 1) < 0.0001)
                    {
                        lock (LockObject)
                        {
                            Console.SetCursorPosition(col, row);
                            for (var i = 0; i < consoleWidth - col; i++)
                                Console.Write(LoadingCharacter);
                            Console.WriteLine();
                        }

                        return;
                    }

                    if (percentage - completion < step) return;
                    completion = percentage;
                    lock (LockObject)
                    {
                        Console.SetCursorPosition(col, row);
                        Console.Write(LoadingCharacter);
                    }

                    col++;
                });
            }
        }
    }
}