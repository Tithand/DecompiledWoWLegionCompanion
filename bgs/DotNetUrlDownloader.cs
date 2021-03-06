using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace bgs
{
	public class DotNetUrlDownloader : IUrlDownloader
	{
		internal class DownloadState
		{
			private const int bufferSize = 1024;

			public DotNetUrlDownloader downloader;

			public string host;

			public WebRequest request;

			public Stream responseStream;

			public int numRetriesLeft;

			public int timeoutMs;

			public RegisteredWaitHandle timeoutWaitHandle;

			public WaitHandle timeoutWatchHandle;

			public byte[] readBuffer;

			public int readPos;

			public DotNetUrlDownloader.DownloadResult downloadResult;

			public DownloadState()
			{
				this.readBuffer = new byte[1024];
			}

			public bool UnregisterTimeout()
			{
				bool result = false;
				if (this.timeoutWaitHandle != null && this.timeoutWatchHandle != null)
				{
					result = this.timeoutWaitHandle.Unregister(this.timeoutWatchHandle);
					this.timeoutWaitHandle = null;
					this.timeoutWatchHandle = null;
				}
				return result;
			}
		}

		internal class DownloadResult
		{
			public UrlDownloadCompletedCallback callback;

			public byte[] downloadData;

			public bool succeeded;
		}

		private List<DotNetUrlDownloader.DownloadResult> m_completedDownloads = new List<DotNetUrlDownloader.DownloadResult>();

		public void Process()
		{
			List<DotNetUrlDownloader.DownloadResult> completedDownloads = this.m_completedDownloads;
			lock (completedDownloads)
			{
				using (List<DotNetUrlDownloader.DownloadResult>.Enumerator enumerator = this.m_completedDownloads.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DotNetUrlDownloader.DownloadResult current = enumerator.get_Current();
						current.callback(current.succeeded, current.downloadData);
					}
				}
				this.m_completedDownloads.Clear();
			}
		}

		public void Download(string url, UrlDownloadCompletedCallback cb)
		{
			UrlDownloaderConfig config = new UrlDownloaderConfig();
			this.Download(url, cb, config);
		}

		public void Download(string url, UrlDownloadCompletedCallback cb, UrlDownloaderConfig config)
		{
			WebRequest request = WebRequest.Create(url);
			DotNetUrlDownloader.DownloadResult downloadResult = new DotNetUrlDownloader.DownloadResult();
			downloadResult.callback = cb;
			DotNetUrlDownloader.Download(new DotNetUrlDownloader.DownloadState
			{
				downloader = this,
				host = url,
				downloadResult = downloadResult,
				request = request,
				numRetriesLeft = config.numRetries,
				timeoutMs = config.timeoutMs
			});
		}

		private static void Download(DotNetUrlDownloader.DownloadState state)
		{
			try
			{
				IAsyncResult asyncResult = state.request.BeginGetResponse(new AsyncCallback(DotNetUrlDownloader.ResponseCallback), state);
				int num = state.timeoutMs;
				if (num < 0)
				{
					num = -1;
				}
				state.timeoutWatchHandle = asyncResult.get_AsyncWaitHandle();
				state.timeoutWaitHandle = ThreadPool.RegisterWaitForSingleObject(state.timeoutWatchHandle, new WaitOrTimerCallback(DotNetUrlDownloader.TimeoutCallback), state, num, true);
			}
			catch (Exception)
			{
				DotNetUrlDownloader.FinishDownload(state);
			}
		}

		private static void ResponseCallback(IAsyncResult ar)
		{
			DotNetUrlDownloader.DownloadState downloadState = (DotNetUrlDownloader.DownloadState)ar.get_AsyncState();
			try
			{
				WebRequest request = downloadState.request;
				WebResponse webResponse = request.EndGetResponse(ar);
				Stream responseStream = webResponse.GetResponseStream();
				downloadState.responseStream = responseStream;
				downloadState.downloadResult.downloadData = new byte[webResponse.get_ContentLength()];
				responseStream.BeginRead(downloadState.readBuffer, 0, downloadState.readBuffer.Length, new AsyncCallback(DotNetUrlDownloader.ReadCallback), downloadState);
			}
			catch (Exception)
			{
				DotNetUrlDownloader.FinishDownload(downloadState);
			}
		}

		private static void ReadCallback(IAsyncResult ar)
		{
			DotNetUrlDownloader.DownloadState downloadState = (DotNetUrlDownloader.DownloadState)ar.get_AsyncState();
			bool flag = true;
			try
			{
				Stream responseStream = downloadState.responseStream;
				int num = responseStream.EndRead(ar);
				if (num > 0)
				{
					flag = false;
					Array.Copy(downloadState.readBuffer, 0, downloadState.downloadResult.downloadData, downloadState.readPos, num);
					downloadState.readPos += num;
					responseStream.BeginRead(downloadState.readBuffer, 0, downloadState.readBuffer.Length, new AsyncCallback(DotNetUrlDownloader.ReadCallback), downloadState);
				}
				else if (num == 0)
				{
					downloadState.downloadResult.succeeded = true;
				}
			}
			catch (Exception)
			{
			}
			if (flag)
			{
				DotNetUrlDownloader.FinishDownload(downloadState);
			}
		}

		private static void TimeoutCallback(object context, bool timedOut)
		{
			DotNetUrlDownloader.DownloadState downloadState = (DotNetUrlDownloader.DownloadState)context;
			downloadState.UnregisterTimeout();
			if (timedOut)
			{
				downloadState.request.Abort();
			}
		}

		private static void FinishDownload(DotNetUrlDownloader.DownloadState state)
		{
			if (!state.downloadResult.succeeded && state.numRetriesLeft > 0)
			{
				state.numRetriesLeft--;
				DotNetUrlDownloader.Download(state);
				return;
			}
			List<DotNetUrlDownloader.DownloadResult> completedDownloads = state.downloader.m_completedDownloads;
			lock (completedDownloads)
			{
				state.downloader.m_completedDownloads.Add(state.downloadResult);
			}
		}
	}
}
