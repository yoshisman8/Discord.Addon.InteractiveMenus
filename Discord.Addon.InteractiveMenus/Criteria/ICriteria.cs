using System.Threading.Tasks;
using Discord.WebSocket;

namespace Discord.Addon.InteractiveMenus
{
	public interface ICriteria<in T>
	{
		Task<bool> JudgeCriteria(T arg);
	}
}
