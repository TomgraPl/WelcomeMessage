using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using System.IO;
namespace WelcomeMessage;

public class WelcomeMessage : BasePlugin{
	private string? _message;
	public Dictionary<CCSPlayerController, bool> MessageShown = new Dictionary<CCSPlayerController, bool>();
	public override string ModuleName => "tWelcomeMessage";
	public override string ModuleAuthor => "Tomgra";
	public override string ModuleVersion => "0.0.1";

	public override void Load(bool hotReload) {
		_message = ManageFile(ModuleDirectory + "/message.txt");
		RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
		RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);
	}

	private static string ManageFile(string filepath) {
		string s = "{GREY}Witamy na serwerze {RED}TestServer{GREY}, {GOLD}{NAME}{GREY}. Koniecznie zapisz nasze IP: {RED}1.1.1.1{GREY}.\n" +
					"Aktualnie na serwerze jest {PLAYERS}/{MAXPLAYERS} graczy.\n{GREY}Życzymy dobrej zabawy!\n" +
					"Aktualnie jest godzina {TIME}. Gramy mapę {MAP}";
		if (!File.Exists(filepath)){
			File.WriteAllText(filepath, s);
			return s;
		} else {
			string message = File.ReadAllText(filepath);
			if (message.Length <= 1) return s;
			else return message;
		}
	}

	[ConsoleCommand("css_twm_reload")]
	[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.SERVER_ONLY)]
	public void OnReloadConfig(CCSPlayerController? player, CommandInfo commandInfo) {
		_message = ManageFile(ModuleDirectory + "/tags.json");
		Server.PrintToConsole("[tWelcomeMessage] Config reloaded!");
	}

	private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo eventInfo) {
		var player = @event.Userid;
		if(player != null && player.IsValid && !player.IsBot) MessageShown.Add(player, false);
		return HookResult.Continue;
	}
	private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo eventInfo) {
		var player = @event.Userid;
        if (_message != null && player != null && player.IsValid && !player.IsBot && !MessageShown[player]) {
			MessageShown[player] = true;
			_message.Replace("{NAME}", player.PlayerName);
			_message.Replace("{PLAYERS}", System.Convert.ToString(PlayerCount()));
			_message.Replace("{MAXPLAYERS}", System.Convert.ToString(Server.MaxPlayers));
			_message.Replace("{MAP}", Server.MapName);
			_message.Replace("{TIME}", System.Convert.ToString(Server.CurrentTime));
			string[] s = _message.Split('\n');
			for(int i=0; i<s.Length; i++) {

			}
		}
		return HookResult.Continue;
    }

	public int PlayerCount() {
		int count = 0;
		for(int i = 0; i < Server.MaxPlayers; i++) {
			CCSPlayerController? player = Utilities.GetPlayerFromIndex(i);
			if(player != null && player.IsValid && !player.IsBot) count++;
		}
		return count;
	}
}
