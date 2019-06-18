using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Discord.Addon.InteractiveMenus
{
	public class SameUserCriteria : ICriteria <SocketReaction>
	{
		private ulong user;
		public SameUserCriteria(ulong id)
		{
			user = id;
		}
		public Task<bool> JudgeCriteria(SocketReaction Reaction)
		{
			return Task.FromResult(user == Reaction.UserId);
		}
	}
}
