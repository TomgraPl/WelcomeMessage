using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using Newtonsoft.Json.Linq;
namespace WelcomeMessage;

public class WelcomeMessage : BasePlugin{
	public static JObject? JsonMessage { get; private set; }
	public override string ModuleName => "tWelcomeMessage";
	public override string ModuleAuthor => "Tomgra";
	public override string ModuleVersion => "1.1.6";

	public override void Load(bool hotReload) {
		CreateOrLoadJsonFile(ModuleDirectory + "/message.json");
		RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
	}
	private static void CreateOrLoadJsonFile(string filepath) {
		if (!File.Exists(filepath)) {
			var templateData = new JObject
			{
				["WelcomeMessage"] = new JObject
				{
					["message"] = "Witamy na serwerze {RED}{SERVERNAME}{Default}, {Gold}{NAME}{Default}." +
					"\nKoniecznie zapisz nasze IP: {Red}{IP}:{PORT}{Default}." +
					"\nAktualnie na serwerze jest {Green}{PLAYERS}/{MAXPLAYERS}{Default} graczy." +
					"\nŻyczymy dobrej zabawy!" +
					"\n{Red}Aktualnie jest {Green}{TIME} {DATE}{Default}. Gramy mapę {Green}{MAP}",
				},
			};
			File.WriteAllText(filepath, templateData.ToString());
			var jsonData = File.ReadAllText(filepath);
			JsonMessage = JObject.Parse(jsonData);
		} else {
			var jsonData = File.ReadAllText(filepath);
			JsonMessage = JObject.Parse(jsonData);
		}
	}

	[ConsoleCommand("css_twm_reload")]
	[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.SERVER_ONLY)]
	public void OnReloadConfig(CCSPlayerController? player, CommandInfo info) {
		CreateOrLoadJsonFile(ModuleDirectory + "/message.json");
		Server.PrintToConsole("[tWelcomeMessage] Config reloaded!");
	}

	private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo eventInfo) {
		var player = @event.Userid;
		if (JsonMessage != null && player != null && player.IsValid && !player.IsBot && JsonMessage.TryGetValue("WelcomeMessage", out var wMessage) && wMessage is JObject messageObject) {
			string message = messageObject["message"]?.ToString() ?? string.Empty;
			if(message != string.Empty) {
				string newMessage = message
				.Replace("{NAME}", player.PlayerName)
				.Replace("{PLAYERS}", Utilities.GetPlayers().Count().ToString())
				.Replace("{MAXPLAYERS}", Server.MaxPlayers.ToString())
				.Replace("{MAP}", Server.MapName)
				.Replace("{TIME}", DateTime.Now.ToString("HH:mm"))
				.Replace("{DATE}", DateTime.Now.ToString("dd.MM.yyyy"))
				.Replace("{IP}", ConVar.Find("ip")!.StringValue)
				.Replace("{PORT}", ConVar.Find("hostport")!.GetPrimitiveValue<int>().ToString())
				.Replace("{SERVERNAME}", ConVar.Find("hostname")!.StringValue);
				newMessage = ReplaceColors(newMessage);
				string[] s = newMessage.Split('\n');
				for (int i = 0; i < s.Length; i++) player.PrintToChat(ReplaceColors(s[i]));
			}		
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