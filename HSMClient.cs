using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace KeysConversion
{
    public struct HSMReply
    {
        public string? ResponseCode;
        public string? ErrorCode;
        public byte[]? Data;
    }
    internal class HSMClient
    {
        string IP = "";
        int? port = 0;
        private int msgCounter = 0;
        public HSMClient(string IP, int? port)
        {
            this.IP = IP;
            this.port = port;
        }

        public HSMReply sendCommand(byte[] command)
        {
            ushort messageLength = 0;
            HSMReply reply = new HSMReply();
            reply.ResponseCode = null;
            reply.ErrorCode = null;
            reply.Data = null;

            if (command == null)
            {
                throw new ArgumentException("Null message is not allowed");
            }

            byte[] header = new byte[4];
            string headerStr = Convert.ToString(msgCounter);
            msgCounter++;
            if (msgCounter > 9999) msgCounter = 0;
            while (headerStr.Length < 4) headerStr = "0" + headerStr;
            string tmp = "";
            for (int i=0; i<4; i++)
            {
                tmp = tmp + "3" + headerStr[i];
            }
            header = Tools.HexStringToByteArray(tmp);

            messageLength = (ushort)(command.Length + 4);
            byte[] len = new byte[2];
            len = BitConverter.GetBytes(messageLength);
            
            byte tmpByte = len[0];
            len[0] = len[1];
            len[1] = tmpByte;

            byte[] cmd = Tools.ConcatArrays<byte>(len, header, command);

            byte[] data = new byte[3072];
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse(IP), (port == null)?0:(int)port);
            StringBuilder response = new StringBuilder();
            NetworkStream stream = tcpClient.GetStream();
            stream.Write(cmd, 0, cmd.Length);
            byte[] answer = new byte[] { };

            //Console.WriteLine("CMD=" + KeysConversion.Tools.ByteArrayToHexString(cmd));

            do
            {
                int bytes = stream.Read(data, 0, data.Length);
                answer = Tools.ConcatArrays<byte>(answer, data);
            }
            while (stream.DataAvailable);
            //Console.WriteLine("HSM received data: " + KeysConversion.Tools.ByteArrayToHexString(answer));

            stream.Close();
            tcpClient.Close();

            if (answer.Length < 10)
            {
                throw new Exception("Incoming message is too short");
            }
            len[0] = answer[1];
            len[1] = answer[0];

            //len[0] = answer[0];
            //len[1] = answer[1];

            messageLength = BitConverter.ToUInt16(len);
            //reply.ResponseCode = Encoding.ASCII.GetString(Tools.PartOfArray<byte>(answer, 8, 2));
            //reply.ErrorCode = Encoding.ASCII.GetString(Tools.PartOfArray<byte>(answer, 8, 2));

            reply.ResponseCode = Encoding.ASCII.GetString(Tools.PartOfArray<byte>(answer, 8, 2));
            reply.ErrorCode = Encoding.ASCII.GetString(Tools.PartOfArray<byte>(answer, 8, 2));

            if (answer.Length > 10 && answer.Length >= messageLength+2)
            {
                reply.Data = Tools.PartOfArray<byte>(answer, 10, messageLength - 8);
            }

            return reply;
        }
    }
}
