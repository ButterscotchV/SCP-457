using System.Collections.Generic;
using System.Linq;
using Smod2.API;
using Smod2.Commands;

namespace SCP_457
{
	internal class SpawnScp457Command : ICommandHandler
	{
		private readonly Scp457 plugin;

		public SpawnScp457Command(Scp457 plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Spawn a player as SCP-457.";
		}

		public string GetUsage()
		{
			return "SPAWNSCP457 <Player>";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			List<string> response = new List<string>();

			Player player;

			if (!(sender is Server) && (player = sender as Player) != null && !plugin.RaRanks.Contains(player.GetRankName()))
			{
				response.Add($"You (rank {player.GetRankName() ?? "NULL"}) do not have permissions to run this command.");
				return response.ToArray();
			}

			if (args.Length > 0)
			{
				player = GetPlayerFromString.GetPlayer(args[0]);

				if (player != null)
				{
					plugin.SpawnScp457(player);

					response.Add($"Made {player.Name} SCP-457!");
				}
				else
				{
					response.Add($"{args[0]} is not a valid player name or player id!");
				}
			}
			else
			{
				response.Add(GetUsage());
			}

			return response.ToArray();
		}
	}
}