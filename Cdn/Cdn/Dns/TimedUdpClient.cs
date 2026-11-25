using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TomPIT.Cdn.Dns
{
	internal class TimedUdpClient : UdpClient
	{
		private byte[] _returnReceive = null;
		private bool _errorOccurs = false;
		private Thread _threadReceive = null;
		private IPEndPoint _remote = null;
		private Mutex _mutexReturnReceive = new Mutex(false);
		private Mutex _mutexErrorOccurs = new Mutex(false);
		private int _timeout = 2000;
		public int Timeout
		{
			get { return _timeout; }
			set { _timeout = value; }
		}
		public new byte[] Receive(ref IPEndPoint remote)
		{
			_remote = remote;

			_threadReceive = new Thread(new ThreadStart(StartReceive));
			_threadReceive.Start();

			Thread.Sleep(_timeout);

			_mutexErrorOccurs.WaitOne();

			if (_errorOccurs == true)
			{
				_mutexErrorOccurs.ReleaseMutex();
				//_threadReceive.Abort();

				throw new Exception("Connection timed out");
			}
			else
				_mutexErrorOccurs.ReleaseMutex();

			return _returnReceive;
		}

		private void StartReceive()
		{
			_mutexErrorOccurs.WaitOne();
			_errorOccurs = true;
			_mutexErrorOccurs.ReleaseMutex();

			try
			{
				byte[] ret = base.Receive(ref _remote);

				_mutexReturnReceive.WaitOne();
				_returnReceive = ret;
				_mutexReturnReceive.ReleaseMutex();
				_errorOccurs = false;
			}
			catch (SocketException) { }
			catch (ThreadAbortException) { }
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			_mutexErrorOccurs = null;
			_mutexReturnReceive = null;
		}
	}
}