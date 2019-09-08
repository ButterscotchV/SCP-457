using System.Collections.Generic;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;

namespace SCP_457
{
	[PluginDetails(
		author = "Prince Frizzy & Dankrushen",
		name = "SCP-457",
		description = "SCP-457 is the \"living\" flame and takes a humanoid shape, he kills his enemies with the heat that he possesses.",
		id = "frizzy.scp457",
		version = "1.1.0",
		SmodMajor = 3, SmodMinor = 2, SmodRevision = 3
	)]
	internal class Scp457 : Plugin
	{
		private readonly List<Scp457Controller> active457List = new List<Scp457Controller>();

		public bool hasScp457;

		public string[] RaRanks { get; private set; }

		public EventLogic EventLogic { get; private set; }

		public override void OnDisable()
		{
			Info($"Plugin {Details.name} v{Details.version} is no longer active.");
		}

		public override void OnEnable()
		{
			Info($"Plugin {Details.name} v{Details.version} is now active.");
		}

		public override void Register()
		{
			EventLogic = new EventLogic(this);
			AddEventHandlers(EventLogic);
			AddCommand("SPAWNSCP457", new SpawnScp457Command(this));

			AddConfig(new ConfigSetting("scp457_min_health_heal", 0.35f, true, "The minimum percent of health SCP-457 can randomly heal."));
			AddConfig(new ConfigSetting("scp457_max_health_heal", 0.45f, true, "The maximum percent of health SCP-457 can randomly heal."));

			AddConfig(new ConfigSetting("scp457_basedamage", 4f, true, "The base damage per second the radius around SCP-457 will inflict on enemies."));
			AddConfig(new ConfigSetting("scp457_maxdamage", 30f, true, "The max damage per second the radius around SCP-457 will inflict on enemies."));
			AddConfig(new ConfigSetting("scp457_damagedecrease", 0.2f, true, "The amount that the damage the radius around SCP-457 inflicts on enemies decreases per second."));
			AddConfig(new ConfigSetting("scp457_damagestep", 5f, true, "The amount that the damage the radius around SCP-457 inflicts on enemies increases per hit."));

			AddConfig(new ConfigSetting("scp457_baseradius", 3f, true, "The base radius around SCP-457 that damages enemies."));
			AddConfig(new ConfigSetting("scp457_maxradius", 7f, true, "The maximum radius around SCP-457 that damages enemies."));
			AddConfig(new ConfigSetting("scp457_radiusdecrease", 0.1f, true, "The amount per second the SCP-457's damage radius will decrease."));
			AddConfig(new ConfigSetting("scp457_radiusstep", 1.5f, true, "The amount the radius will increase every time SCP-457 hits an enemy."));

			AddConfig(new ConfigSetting("scp457_health", 500, true, "The amount of health points that SCP-457 has."));
			AddConfig(new ConfigSetting("scp457_tutorialallies", true, true, "If true, SCP-457 will not hurt tutorials. Useful for the Serpents Hand plugin."));
			AddConfig(new ConfigSetting("scp457_spawnchance", 15f, true, "The percent chance for SPC-457 to spawn."));

			AddConfig(new ConfigSetting("scp457_rank", new string[]
			{
				"owner",
				"dev",
				"developer"
			}, true, "Ranks allowed to spawn SCP-457."));
		}

		public void RefreshConfig()
		{
			RaRanks = GetConfigList("scp457_rank");
		}

		public Player GetPlayerFromPlayerId(int playerId)
		{
			if (playerId >= 0)
			{
				foreach (Player player in Server.GetPlayers())
					if (playerId == player.PlayerId)
						return player;
			}

			return null;
		}

		public Scp457Controller[] GetScp457Controllers()
		{
			lock (active457List)
			{
				return active457List.ToArray();
			}
		}

		public Scp457Controller GetScp457Controller(int playerId)
		{
			lock (active457List)
			{
				if (playerId < 0) return null;

				foreach (Scp457Controller scp457Controller in active457List)
				{
					if (scp457Controller.playerId == playerId)
						return scp457Controller;
				}

				return null;
			}
		}

		public Scp457Controller GetScp457Controller(Player player)
		{
			lock (active457List)
			{
				return player == null ? null : GetScp457Controller(player.PlayerId);
			}
		}

		public bool IsScp457(int playerId)
		{
			lock (active457List)
			{
				return GetScp457Controller(playerId) != null;
			}
		}

		public bool IsScp457(Player player)
		{
			lock (active457List)
			{
				return player != null && IsScp457(player.PlayerId);
			}
		}

		public void SpawnScp457(Player player)
		{
			lock (active457List)
			{
				if (player == null)
					return;

				active457List.Add(new Scp457Controller(this, EventLogic, player.PlayerId));
				player.ChangeRole(Role.SCP_106);
				player.SetRank("red", "SCP-457");

				hasScp457 = active457List.Count > 0;
			}
		}

		public void SpawnScp457(int playerId)
		{
			lock (active457List)
			{
				SpawnScp457(GetPlayerFromPlayerId(playerId));
			}
		}

		public void RemoveScp457(Player player)
		{
			lock (active457List)
			{
				Scp457Controller controller = GetScp457Controller(player);

				if (controller == null)
					return;

				player.SetRank("default", "");
				active457List.Remove(controller);

				hasScp457 = active457List.Count > 0;
			}
		}

		public void RemoveScp457(int playerId)
		{
			lock (active457List)
			{
				RemoveScp457(GetPlayerFromPlayerId(playerId));
			}
		}

		public void RemoveAllScp457()
		{
			lock (active457List)
			{
				active457List.Clear();
			}
		}
	}
}