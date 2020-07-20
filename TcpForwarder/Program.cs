using ArgumentParser;
using System;
using System.Net;
using System.Net.Sockets;

namespace TcpForwarder
{
	class Program
	{
		static void Main(string[] args)
		{
      ArgParse argparse = new ArgParse
      (
          new ArgItem("src-port", "sp", true, "Source Port", "7701", ArgParse.ArgParseType.Int),
          new ArgItem("src-ip", "si", false, "Source Ip", "0.0.0.0", ArgParse.ArgParseType.String),
          new ArgItem("dst-port", "dp", true, "password", "", ArgParse.ArgParseType.Int),
          new ArgItem("dst-ip", "di", true, "destination ip", "", ArgParse.ArgParseType.String)
      );

      argparse.parse(args);

      int srcPort = argparse.Get<int>("src-port");
      IPAddress srcIp = IPAddress.Parse(argparse.Get<string>("src-ip"));

      int dstPort = argparse.Get<int>("dst-port");
      IPAddress dstIp = IPAddress.Parse(argparse.Get<string>("dst-ip"));

      SimpleTcpForwarder fwder = new SimpleTcpForwarder();
      IPEndPoint sep = new IPEndPoint(srcIp, srcPort);
      IPEndPoint dep = new IPEndPoint(dstIp, dstPort);

      fwder.Start(sep, dep);
		}
  }

  public class ForwardStruct
  {
    public Socket SrcSocket { get; set; }
    public Socket DstSocket { get; set; }
    public byte[] Buf { get; set; }

    public ForwardStruct(Socket source, Socket destination)
    {
      SrcSocket = source;
      DstSocket = destination;
      Buf = new byte[8192];
    }
  }

  public class SimpleTcpForwarder
  {
    private Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public void Start(IPEndPoint lEndPoint, IPEndPoint rEndPoint)
    {
      sock.Bind(lEndPoint);
      sock.Listen(10);

      while (true)
      {
        Socket src = sock.Accept();
        SimpleTcpForwarder dst = new SimpleTcpForwarder();
        ForwardStruct state = new ForwardStruct(src, dst.sock);
        dst.Connect(rEndPoint, src);
        src.BeginReceive(state.Buf, 0, state.Buf.Length, 0, OnDataReceive, state);
      }
    }

    private void Connect(EndPoint srcEndPoint, Socket dstEndPoint)
    {
      var state = new ForwardStruct(sock, dstEndPoint);
      sock.Connect(srcEndPoint);
      sock.BeginReceive(state.Buf, 0, state.Buf.Length, SocketFlags.None, OnDataReceive, state);
    }

    private static void OnDataReceive(IAsyncResult data)
    
    {
      var state = (ForwardStruct)data.AsyncState;
      try
      {
        var bytesRead = state.SrcSocket.EndReceive(data);
        if (bytesRead > 0)
        {
          state.DstSocket.Send(state.Buf, bytesRead, SocketFlags.None);
          state.SrcSocket.BeginReceive(state.Buf, 0, state.Buf.Length, 0, OnDataReceive, state);
        }
      }
      catch
      {
        state.DstSocket.Close();
        state.SrcSocket.Close();
      }
    }
  }
}
