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
	/// <para>A simple menu that allows the user to scroll through multiple pages in the form of embeds.</para>
	/// This menu contains a total of 5 buttons: First/Last page, Prev/Next page a minimize button that minimizes the embed.
	/// </summary>
	public class PagedEmbed : Menu
	{
		private string _Name;
		private Embed[] _Pages;
		private int Index = 0;
		private bool Minimized = false;
		/// <summary>
		/// A menu that only displayes a list of embeds. Includes First/Last, Next/Prev and a Minimize/Maximize Button.
		/// </summary>
		/// <param name="Name">The name of this Menu, This is displayed when minimized.</param>
		/// <param name="Pages">The list of embed pages.</param>
		public PagedEmbed(string Name,Embed[] Pages)
		{
			_Name = Name;
			_Pages = Pages;
		}
		public async override Task<RestUserMessage> Initialize(SocketCommandContext commandContext, MenuService service)
		{
			Message = await base.Initialize(commandContext, service);
			Buttons.Add(new Emoji("⏮"), FirstPageAsync);
			Buttons.Add(new Emoji("⏪"), PreviousPageAsync);
			Buttons.Add(new Emoji("⏯"), Minimize);
			Buttons.Add(new Emoji("⏩"), NextPageAsync);
			Buttons.Add(new Emoji("⏭"), LastPageAsync);
			await Message.AddReactionsAsync(Buttons.Select(x => x.Key).ToArray());
			await RefeshEmbed();
			return Message;
		}
		public async override Task<bool> HandleButtonPress(SocketReaction reaction)
		{
			if (!Buttons.TryGetValue(reaction.Emote, out Func<Task<bool>> Logic))
			{
				return false;
			}
			return await Logic();
		}

		public async Task<bool> FirstPageAsync()
		{
			Index = 0;
			await RefeshEmbed();
			return false;
		}
		public async Task<bool> LastPageAsync()
		{
			Index = _Pages.Length - 1;
			await RefeshEmbed();
			return false;
		}
		public async Task<bool> NextPageAsync()
		{
			if (Index + 1 >= _Pages.Length) Index = 0;
			else Index++;
			await RefeshEmbed();
			return false;
		}
		public async Task<bool> PreviousPageAsync()
		{
			if (Index - 1 < 0) Index = _Pages.Length - 1;
			else Index--;
			await RefeshEmbed();
			return false;
		}
		public async Task<bool> Minimize()
		{
			Minimized ^= true;
			await RefeshEmbed();
			return false;
		}
		public async Task RefeshEmbed()
		{
			await Message.ModifyAsync(x => x.Content = ".");
			if (Minimized)
			{
				await Message.ModifyAsync(x => x.Embed = new EmbedBuilder().WithDescription("Minimized \"" + _Name + "\".").Build());
			}
			else
			{
				await Message.ModifyAsync(x => x.Embed = _Pages[Index]);
			}
		}
	}
}
