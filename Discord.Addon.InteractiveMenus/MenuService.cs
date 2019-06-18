using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Addon.InteractiveMenus
{
	public class MenuService
	{
		private DiscordSocketClient discord;
		private Dictionary<ulong,Menu> Menus;
		private TimeSpan? DefaultTimeout; 
		/// <summary>
		/// Initalize the Menu Service
		/// </summary>
		/// <param name="_discord">The Discord Socket Client.</param>
		/// <param name="ResponseTimeout">Default timeout for awaiting text reponses.</param>
		public MenuService(DiscordSocketClient _discord,TimeSpan? ResponseTimeout = null)
		{
			discord = _discord;
			discord.ReactionAdded += HandleReaction;
			Menus = new Dictionary<ulong, Menu>();
			DefaultTimeout = ResponseTimeout ?? TimeSpan.FromMinutes(1);
		}
		/// <summary>
		/// Initializes a menu and adds it to the service handler.
		/// </summary>
		/// <param name="context">The Command Context.</param>
		/// <param name="menu">The menu being initialized.</param>
		/// <returns>The RestUserMessage the Menu is being ran at.</returns>
		public async Task<RestUserMessage> CreateMenu(SocketCommandContext context, Menu menu, bool FromSameUser)
		{
			List<ICriteria<SocketReaction>> criterias = new List<ICriteria<SocketReaction>>();
			if(FromSameUser)
			{
				criterias.Add(new SameUserCriteria(context.User.Id));
			}
			return await CreateMenu(context, menu, criterias.ToArray());
		}
		/// <summary>
		/// Initializes a menu and adds it to the service handler.
		/// </summary>
		/// <param name="context">The Command Context.</param>
		/// <param name="menu">The menu being initialized.</param>
		/// <returns>The RestUserMessage the Menu is being ran at.</returns>
		public async Task<RestUserMessage> CreateMenu(SocketCommandContext context, Menu menu, ICriteria<SocketReaction>[] criteria)
		{
			menu.Criterias = criteria;
			RestUserMessage message = await menu.Initialize(context,this);
			Menus.Add(message.Id, menu);
			return message;
		}
		/// <summary>
		/// Remove a menu from the service.
		/// </summary>
		/// <param name="message"> The Ulong Id of the message.</param>
		public void RemoveMenu(ulong message) => Menus.Remove(message);

		/// <summary>
		/// waits for the next message in the channel
		/// </summary>
		/// <param name="context">
		/// The context.
		/// </param>
		/// <param name="criterion">
		/// The criterion.
		/// </param>
		/// <param name="timeout">
		/// The timeout.
		/// </param>
		/// <returns>
		/// The <see cref="Task"/>.
		/// </returns>
		public async Task<SocketMessage> NextMessageAsync(SocketCommandContext context, TimeSpan? timeout = null)
		{
			timeout = timeout ?? DefaultTimeout;

			var eventTrigger = new TaskCompletionSource<SocketMessage>();

			Task Func(SocketMessage m) => HandlerAsync(m, context, eventTrigger);

			context.Client.MessageReceived += Func;

			var trigger = eventTrigger.Task;
			var delay = Task.Delay(timeout.Value);
			var task = await Task.WhenAny(trigger, delay).ConfigureAwait(false);

			context.Client.MessageReceived -= Func;

			if (task == trigger)
			{
				return await trigger.ConfigureAwait(false);
			}

			return null;
		}

		/// <summary>
		/// Handles messages for NextMessageAsync
		/// </summary>
		/// <param name="message">
		/// The message.
		/// </param>
		/// <param name="context">
		/// The context.
		/// </param>
		/// <param name="eventTrigger">
		/// The event trigger.
		/// </param>
		/// <param name="criterion">
		/// The criterion.
		/// </param>
		/// <returns>
		/// The <see cref="Task"/>.
		/// </returns>
		private static async Task HandlerAsync(SocketMessage message, SocketCommandContext context, TaskCompletionSource<SocketMessage> eventTrigger)
		{
			var result = (message.Channel.Id==context.Channel.Id&&message.Author.Id==context.User.Id);
			if (result)
			{
				eventTrigger.SetResult(message);
			}
		}

		private async Task HandleReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (reaction.UserId == discord.CurrentUser.Id)
			{
				return; // If the reaction was added by the bot, ignore.
			}
			if (!Menus.TryGetValue(message.Id, out Menu menu))
			{
				return; // If it can't find a menu attached to this message, ignore.
			}
			if (!await menu.JudgeCriteriaAsync(reaction))
			{
				return; // If the conditions on the menu fail, ignore.
			}

			// Let the menu handle the reaction
			_ = Task.Run(async () =>
			   {
					var msg =  (RestUserMessage)await channel.GetMessageAsync(message.Id);
				   msg?.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
					if(await menu.HandleButtonPress(reaction))
					{
						Menus.Remove(message.Id);
					}
					await Task.Delay(500);
			   });
		}
	}
}
