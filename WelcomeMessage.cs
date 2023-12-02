using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
namespace WelcomeMessage;

public class WelcomeMessage : BasePlugin{
	private string? _message;
	public override string ModuleName => "tWelcomeMessage";
	public override string ModuleAuthor => "Tomgra";
	public override string ModuleVersion => "1.1.3";

	public override void Load(bool hotReload) {
		_message = ManageFile(ModuleDirectory + "/message.txt");
		RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
	}

	private static string ManageFile(string filepath) {
		string s = "{SERVERNAME} - nazwa serwera, {NAME} - nazwa gracza, {PLAYERS} - liczba graczy na serwerze, {MAXPLAYERS} - liczba slotów" +
					"{TIME} - godzina (HH:mm), {DATE} - data (dd.MM.yyyy), {IP} - ip serwera, {PORT} - port serwera, {MAP} - nazwa aktualnie granej mapy\n" +
					"message:Witamy na serwerze {RED}{SERVERNAME}{Default}, {Gold}{NAME}{Default}. Koniecznie zapisz nasze IP: {Red}{IP}:{PORT}{Default}.\n" +
					"Aktualnie na serwerze jest {Green}{PLAYERS}/{MAXPLAYERS}{Default} graczy.\n" +
					"Życzymy dobrej zabawy!\n" +
					"{Red}Aktualnie jest {Green}{TIME} {DATE}{Default}. Gramy mapę {Green}{MAP}";
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
		_message = ManageFile(ModuleDirectory + "/message.txt");
		Server.PrintToConsole("[tWelcomeMessage] Config reloaded!");
	}

	private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo eventInfo) {
		var player = @event.Userid;
		if (_message != null && player != null && player.IsValid && !player.IsBot) {
			string message = _message
			.Replace("{NAME}", player.PlayerName)
			.Replace("{PLAYERS}", Utilities.GetPlayers().Count().ToString())
			.Replace("{MAXPLAYERS}", Server.MaxPlayers.ToString())
			.Replace("{MAP}", Server.MapName)
			.Replace("{TIME}", DateTime.Now.ToString("HH:mm"))
			.Replace("{DATE}", DateTime.Now.ToString("dd.MM.yyyy"))
			.Replace("{IP}", ConVar.Find("ip")!.StringValue)
			.Replace("{PORT}", ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString())
			.Replace("{SERVERNAME}", ConVar.Find("hostname")!.StringValue);
			string[] s = message.Substring(message.IndexOf("message:") + 8).Split('\n');
			for (int i = 0; i < s.Length; i++) player.PrintToChat(ReplaceColors(s[i]));
		}
		return HookResult.Continue;
	}

	private string ReplaceColors(string message) {
		if (message.Contains('{')) {
			string modifiedValue = message;
			foreach (FieldInfo field in typeof(ChatColors).GetFields()) {
				string pattern = '{' + field.Name + '}';
				modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
			}
			return modifiedValue;
		}
		return message;
	}
}