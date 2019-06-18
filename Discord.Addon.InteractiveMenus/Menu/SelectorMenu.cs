using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord.Addon.InteractiveMenus
{
	/// <summary>
	/// A menu with 2 Up/Down buttons and a Selector button. 
	/// Returns the selected entry.
	/// </summary>
	public class SelectorMenu : Menu
	{
		private string MenuMessage;
		public string[] ListedOptions;
		private object[] Objects;
		private Type StoredType;
		private int Cursor = 0;
		private bool Active = true;
		/// <summary>
		/// A menu which contains a simple message and a list of options to choose from. This type of menu returns the selected option.
		/// </summary>
		/// <param name="message">The message that appears above the listed options.</param>
		/// <param name="Options">The names of the options to be displayed.</param>
		/// <param name="Container">The array of objects from which the user will pick in the same order as the option strings.</param>
		public SelectorMenu(string message, string[] Options,object[] Container)
		{
			ListedOptions = Options;
			Objects = Container;
			StoredType = Objects[0].GetType();
			
		}
		public override async Task<bool> HandleButtonPress(SocketReaction reaction)
		{
			if (!Buttons.TryGetValue(reaction.Emote, out Func<Task<bool>> Logic))
			{
				return false;
			}
			return await Logic();
		}
		public override async Task<RestUserMessage> Initialize(SocketCommandContext commandContext,MenuService service)
		{
			Message = await base.Initialize(commandContext,service);
			Buttons.Add(new Emoji("⏫"), PreviousOptionAsync);
			Buttons.Add(new Emoji("⏏"), SelectAsync);
			Buttons.Add(new Emoji("⏬"), NextOptionAsync);
			await Message.AddReactionsAsync(Buttons.Select(x => x.Key).ToArray());
			await ReloadMenu();
			return Message;
		}
		private async Task<bool> PreviousOptionAsync()
		{
			if (Cursor - 1 < 0) Cursor = Objects.Length - 1;
			else Cursor--;
			await ReloadMenu();
			return false;
		}
		private async Task<bool> NextOptionAsync()
		{
			if (Cursor + 1 >= Objects.Length) Cursor = 0;
			else Cursor--;
			await ReloadMenu();
			return false;
		}
		private async Task<bool> SelectAsync()
		{
			Active = false;
			await Message.DeleteAsync();
			return true;
		}
		private async Task ReloadMenu()
		{
			var sb = new StringBuilder().AppendLine(MenuMessage);
			for (int i = 0;i<ListedOptions.Length;i++)
			{
				sb.AppendLine(i==Cursor?"> "+ListedOptions[i]+" <": ListedOptions[i]);
			}
			await Message.ModifyAsync(x => x.Content =MenuMessage+"\n"+ sb.ToString());
		}
		/// <summary>
		/// Gets the option selected by the user.
		/// </summary>
		/// <returns>Returns the object selected.</returns>
		public async Task<object> GetSelectedObject()
		{
			while (Active)
			{
				await Task.Delay(1000);
			}
			return Objects[Cursor];
		}
	}
}
