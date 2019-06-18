using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;

namespace Discord.Addon.InteractiveMenus
{
	public class SameChannelCriteria : ICriteria<RestUserMessage>
	{
		private ulong channel;
		public SameChannelCriteria(ulong id)
		{
			channel = id;
		}
		public Task<bool> JudgeCriteria(RestUserMessage message)
		{
			return Task.FromResult(message.Channel.Id == channel);
		}
	}
}
