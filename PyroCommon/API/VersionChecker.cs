using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Rage;

namespace PyroCommon.API;

internal static class VersionChecker
{
	private static readonly Dictionary<string, string> OutdatededPyroPlugins = new();
	
	private enum State
	{
		Failed,
		Update,
		Current
	}

	private static State _state = State.Current;
	private static string _receivedData = string.Empty;

	internal static void IsUpdateAvailable(Dictionary<string, string> pluginDict)
	{
		try
		{
			var UpdateThread = new Thread(() => CheckVersion(pluginDict));
			UpdateThread.Start();
			GameFiber.Sleep(1000);

			while (UpdateThread.IsAlive) GameFiber.Wait(1000);

			switch (_state)
			{
				case State.Failed:
					Log.Warning("Unable to check for updates! No internet or LSPDFR is down?");
					break;
					
					case State.Update:
						var ingameNotice = String.Empty;
						var logNotice = "Plugin updates available!";
						
						foreach ( var plug in OutdatededPyroPlugins )
						{
							ingameNotice += $"~w~{plug.Key}: ~r~{pluginDict[plug.Key]} <br>~w~New Version: ~g~{plug.Value}<br>";
							logNotice += $"\r\n{plug.Key}: Current Version: {pluginDict[plug.Key]} New Version: {plug.Value}";
						}
						
						Game.DisplayNotification("commonmenu", "mp_alerttriangle",
							"~r~SuperPlugins Warning", "~y~New updates available!", ingameNotice);
						Log.Warning(logNotice);
						break;
					
				case State.Current:
					Log.Info("Plugins are up to date!");
					break;
			}
		}
		catch (Exception)
		{
			_state = State.Failed;
			Log.Info("VersionChecker failed due to rapid reloads!");
		}
	}

	private static void CheckVersion(Dictionary<string, string> plugDict)
	{
		foreach ( var plug in plugDict )
		{
			try
			{
				string id = String.Empty;
				switch ( plug.Key )
				{
					case "SuperCallouts":
						id = "23995";
						break;
					case "SuperEvents":
						id = "24437";
						break;
					case "DeadlyWeapons":
						id = "27453";
						break;
				}
				_receivedData = new WebClient().DownloadString($"https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId={id}&textOnly=1").Trim();
			}
			catch (WebException)
			{
				_state = State.Failed;
			}
			
			if (_receivedData == plug.Value) return;
			OutdatededPyroPlugins.Add(plug.Key, _receivedData);
			_state = State.Update;
		}
	}
}