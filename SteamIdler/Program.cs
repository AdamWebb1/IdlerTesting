using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

using SteamKit2;
using SteamKit2.Internal;

namespace SteamIdler {
	class SteamIdlerClient {
		static SteamClient steamClient;
		static CallbackManager manager;

		static SteamUser steamUser;
		static SteamFriends steamFriends;

		static bool isRunning;
		static string Username, Password;
		static List<int> AppIDs = new List<int>();

		static void Main(string[] args) {
			// Print program information.
			Console.WriteLine("SteamIdler, 'run' games without steam.\n(C) No STEAMGUARD Support");

			// Check for username and password arguments from stdin.
			if (args.Length < 3) {
				// Print usage and quit.
				Console.WriteLine("usage: <username> <password> <appID> [...]");
				return;
			}

			// Set username and password from stdin.
			Username = args[0];
			Password = args[1];

			// Add all game application IDs to list.
			foreach (string GameAppID in args) {
				int AppID;

				if (int.TryParse(GameAppID, out AppID)) {
					AppIDs.Add(Convert.ToInt32(GameAppID));
				}
			}

			// Create SteamClient interface and CallbackManager.
			steamClient = new SteamClient(System.Net.Sockets.ProtocolType.Tcp);
			manager = new CallbackManager(steamClient);

			// Get the steamuser handler, which is used for logging on after successfully connecting.
			steamUser = steamClient.GetHandler<SteamUser>();

			// Get the steam friends handler, which is used for interacting with friends on the network after logging on.
			steamFriends = steamClient.GetHandler<SteamFriends>();

			// Register Steam callbacks.
			new Callback<SteamClient.ConnectedCallback>(OnConnected, manager);
			new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, manager);
			new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, manager);
			new Callback<SteamUser.LoggedOffCallback>(OnLoggedOff, manager);
			new Callback<SteamUser.AccountInfoCallback>(OnAccountInfo, manager);

			// Set the program as running.
			Console.WriteLine(":: Connecting to Steam..");
			isRunning = true;

			// Connect to Steam.
			steamClient.Connect();

			// Create our callback handling loop.
			while (isRunning) {
				// In order for the callbacks to get routed, they need to be handled by the manager.
				manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
			}
		}

		static void OnConnected(SteamClient.ConnectedCallback callback) {
			if (callback.Result != EResult.OK) {
				// Connection failure.
				Console.WriteLine(":: Unable to connect to Steam! ({0})", callback.Result);
				isRunning = false;
				return;
			}

			Console.WriteLine(":: Connected to Steam! Logging in as '{0}'..", Username);
			steamUser.LogOn(new SteamUser.LogOnDetails{Username = Username, Password = Password});
		}

		static void OnDisconnected(SteamClient.DisconnectedCallback callback)
		{
			// Disconnected from Steam.
			Console.WriteLine(":: Disconnected from Steam..");
			isRunning = false;
		}

		static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
		{
			// Check to see if we've logged on successfully.
			if (callback.Result != EResult.OK) {
				if (callback.Result == EResult.AccountLogonDenied) {
					// Account has Steam Guard enabled.
					Console.WriteLine("Unable to logon to Steam: This account is SteamGuard protected.");
					isRunning = false;
					return;
				}

				Console.WriteLine(":: Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

				isRunning = false;
				return;
			}

			Console.WriteLine(":: Successfully logged on!");

			// Start playing a game.
			var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

			// Convert application IDs to array.
			int[] GameAppIDs = AppIDs.ToArray();

			// Loop application IDs to add.
			foreach (int AppID in GameAppIDs) {
				Console.WriteLine(":: Now playing game '{0}'!", AppID.ToString());
				playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed{ game_id = new GameID(AppID) });
			}

			steamClient.Send(playGame);
		}

		static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
		{
			// Log in to Steam friends..
			steamFriends.SetPersonaState(EPersonaState.Online);
		}

		static void OnLoggedOff(SteamUser.LoggedOffCallback callback) {
			// Signed out of Steam.
			Console.WriteLine(":: Signed out of Steam.. ({0})", callback.Result);
		}
    }
}
