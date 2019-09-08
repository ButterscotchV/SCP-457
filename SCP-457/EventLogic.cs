using System;
using System.Collections.Generic;
using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCP_457
{
	internal class EventLogic : EventArgs, IEventHandlerRoundStart, IEventHandlerWaitingForPlayers, IEventHandlerUpdate, IEventHandlerPlayerHurt, IEventHandlerPocketDimensionEnter, IEventHandler106Teleport, IEventHandler106CreatePortal, IEventHandlerSetRole, IEventHandlerPlayerDie, IEventHandlerContain106
	{
		public static readonly Random Rng = new Random();

		private readonly Scp457 plugin;

		private DateTime nextCheckTime;

		public float MinHealHealth { get; private set; }
		public float MaxHealHealth { get; private set; }

		public int Scp457DefaultHp { get; private set; }
		public bool Scp457TutorialAllies { get; private set; }
		public float Scp457SpawnChance { get; private set; }

		public float BaseDamage { get; private set; }
		public float MaxDamage { get; private set; }
		public float DamageDecrease { get; private set; }
		public float DamageStep { get; private set; }

		public float BaseDamageRadius { get; private set; }
		public float MaxDamageRadius { get; private set; }
		public float DamageRadiusDecrease { get; private set; }
		public float DamageRadiusStep { get; private set; }

		public EventLogic(Scp457 plugin)
		{
			this.plugin = plugin;
		}

		public void On106CreatePortal(Player106CreatePortalEvent e)
		{
			if (plugin.IsScp457(e.Player)) e.Position = null;
		}

		public void On106Teleport(Player106TeleportEvent e)
		{
			if (plugin.IsScp457(e.Player)) e.Position = null;
		}

		public void OnContain106(PlayerContain106Event e)
		{
			if (!plugin.hasScp457) return;
			e.ActivateContainment = false;
		}

		public void OnPlayerDie(PlayerDeathEvent e)
		{
			Player player = e.Player;
			if (plugin.IsScp457(player)) plugin.RemoveScp457(player);
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (!plugin.hasScp457 || !plugin.IsScp457(ev.Attacker) || plugin.IsScp457(ev.Player) || ev.Player.TeamRole.Team == Team.SCP || ev.Player.TeamRole.Team == Team.TUTORIAL && Scp457TutorialAllies) return;

			// Don't actually do any damage
			ev.Damage = 0f;
			ev.DamageType = DamageType.POCKET;

			plugin.GetScp457Controller(ev.Attacker)?.IncreaseAbilities();
		}

		public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent e)
		{
			if (plugin.hasScp457 && plugin.IsScp457(e.Attacker)) e.TargetPosition = null;
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			// Spawn in 457 if the chances are high enough
			if (Scp457SpawnChance <= 100 * Rng.NextDouble()) return;

			Player scp457Player = null;

			List<Player> list = new List<Player>();
			foreach (Player player in PluginManager.Manager.Server.GetPlayers())
			{
				if (player.TeamRole.Role == Role.SCP_106)
				{
					scp457Player = player;
					break;
				}

				if (player.TeamRole.Team == Team.SCP) list.Add(player);
			}

			if (scp457Player == null && list.Count > 0) scp457Player = list[new Random().Next(list.Count)];

			plugin.SpawnScp457(scp457Player);
		}

		public void OnSetRole(PlayerSetRoleEvent e)
		{
			if (!plugin.hasScp457) return;
			if (plugin.IsScp457(e.Player) && e.Role == Role.SCP_106) e.Player.SetHealth(Scp457DefaultHp);
			if (plugin.IsScp457(e.Player) && e.Role != Role.SCP_106) plugin.RemoveScp457(e.Player);
		}

		public void OnUpdate(UpdateEvent e)
		{
			if (!plugin.hasScp457 || DateTime.UtcNow < nextCheckTime) return;
			nextCheckTime = DateTime.UtcNow.AddSeconds(1);

			// For each player that is an instance of SCP-457
			foreach (Scp457Controller scp457Controller in plugin.GetScp457Controllers())
			{
				scp457Controller.AffectPlayers();
				scp457Controller.DecreaseAbilities();
			}
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			// Clear the list of SCP-457 players
			plugin.RemoveAllScp457();

			// Cache all the config values for this round
			plugin.RefreshConfig();

			MinHealHealth = plugin.GetConfigFloat("scp457_min_health_heal") * 0.1f;
			MaxHealHealth = plugin.GetConfigFloat("scp457_max_health_heal") * 0.1f;

			BaseDamage = plugin.GetConfigFloat("scp457_basedamage");
			MaxDamage = plugin.GetConfigFloat("scp457_maxdamage");
			DamageDecrease = plugin.GetConfigFloat("scp457_damagedecrease");
			DamageStep = plugin.GetConfigFloat("scp457_damagestep");

			BaseDamageRadius = plugin.GetConfigFloat("scp457_baseradius");
			MaxDamageRadius = plugin.GetConfigFloat("scp457_maxradius");
			DamageRadiusDecrease = plugin.GetConfigFloat("scp457_radiusdecrease");
			DamageRadiusStep = plugin.GetConfigFloat("scp457_radiusstep");

			Scp457DefaultHp = plugin.GetConfigInt("scp457_health");
			Scp457TutorialAllies = plugin.GetConfigBool("scp457_tutorialallies");
			Scp457SpawnChance = plugin.GetConfigFloat("scp457_spawnchance");
		}
	}
}