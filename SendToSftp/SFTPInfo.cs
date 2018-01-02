// ReSharper disable UnassignedField.Global

namespace SendToSftp
{
    public struct SftpInfo
    {
        public string Ip;
        public string User;
        public string Password;
        public string DestinationPath;
        public bool Multithreaded;
    }
}