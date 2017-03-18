using System;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Battlehub.Dispatcher;
using System.IO;

public class SimpleWebServer : MonoBehaviour {
	private Thread serverTh;
	public Text text;

	private bool stoped = false;
	private bool Stoped {
		get {
			lock (this)
			{
				return stoped;
			}
		}
		set {
			lock (this)
			{
				stoped = value;
			}
		}
	}

	// Use this for initialization
	void Start () {
		serverTh = new Thread(new ThreadStart(RunWebServer));
		serverTh.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// アプリケーション終了時処理
	/// </summary>
	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit");
		Stoped = true;
	}

	private void RunWebServer()
	{
		try
		{
			HttpListener listener = new HttpListener();
			listener.Prefixes.Add("http://192.168.43.209:9090/");
			listener.Start();

			int cnt = 0;
			while (!Stoped)
			{
				HttpListenerContext context = listener.GetContext();
				string urlPath = context.Request.RawUrl;
				switch (urlPath)
				{
					case "/speed":
						if (context.Request.HasEntityBody)
						{
							StreamReader reader = new StreamReader(context.Request.InputStream);
							string json = reader.ReadToEnd();
							reader.Close();
							Dispatcher.Current.BeginInvoke(() =>
							{
								if (text != null)
								{
									text.text = json;
								}
								else
								{
									Debug.Log("no text object");
								}
							});
						}
						break;
					default:
						cnt++;
						string mes = "test" + cnt;
						Dispatcher.Current.BeginInvoke(() =>
						{
							if (text != null)
							{
								text.text = mes;
							}
							else
							{
								Debug.Log("Error: ");
							}
						});
						break;
				}

				HttpListenerResponse res = context.Response;
				res.StatusCode = 200;
				byte[] content = Encoding.UTF8.GetBytes("ok");
				res.OutputStream.Write(content, 0, content.Length);
				res.Close();
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Error: " + ex.Message);
		}
	}
}
