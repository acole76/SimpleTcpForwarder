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
				new ArgItem("dst-port", "dp", true, "Destination Port", "", ArgParse.ArgParseType.Int),
				new ArgItem("dst-ip", "di", true, "Destination ip", "", ArgParse.ArgParseType.String)
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
}
