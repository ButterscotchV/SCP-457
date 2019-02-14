using System;
using System.Collections.Generic;
using Smod2;
using Smod2.API;

namespace SCP_457
{
	internal class Scp457Controller
	{
		public readonly Scp457 plugin;
		public readonly EventLogic eventLogic;
		public readonly int playerId;

		public float Damage { get; private set; }

		private float damageRadius;
		public float SqrDamageRadius { get; private set; }

		public float DamageRadius
		{
			get => damageRadius;
			set
			{
				damageRadius = value;
				SqrDamageRadius = value * value;
			}
		}

		public Scp457Controller(Scp457 plugin, EventLogic eventLogic, int playerId)
		{
			this.plugin = plugin;
			this.eventLogic = eventLogic;
			this.playerId = playerId;

			Damage = this.eventLogic.BaseDamage;
			DamageRadius = this.eventLogic.BaseDamageRadius;
		}

		public Player GetPlayer()
		{
			return plugin.GetPlayerFromPlayerId(playerId);
		}

		public void AffectPlayers()
		{
			// For each affected player
			foreach (Player affectedPlayer in GetPlayersInRange())
			{
				// Heal teammates
				if (affectedPlayer.TeamRole.Team == Team.SCP || affectedPlayer.TeamRole.Team == Team.TUTORIAL && eventLogic.Scp457TutorialAllies)
				{
					TeamRole affectedPlayerRole = affectedPlayer.TeamRole;

					// If the health is already at max or higher, do not attempt to heal the player
					if (affectedPlayer.GetHealth() >= affectedPlayerRole.MaxHP) continue;

					// Get a random amount of health to heal for based on the configurable values
					int randHeal = EventLogic.Rng.Next((int)(affectedPlayerRole.MaxHP * eventLogic.MinHealHealth), (int)(affectedPlayerRole.MaxHP * eventLogic.MaxHealHealth));

					plugin.Debug($"Healing player \"{affectedPlayer.Name}\" for {randHeal} hp!");

					// Heal the affected player the random amount, limiting their health at the role's maximum health
					affectedPlayer.SetHealth(Math.Min(affectedPlayerRole.MaxHP, affectedPlayer.GetHealth() + randHeal));
				}
				// Damage enemies
				else
				{
					plugin.Debug($"Damaging player \"{affectedPlayer.Name}\" for {Damage} hp!");
					affectedPlayer.Damage((int)Damage, DamageType.POCKET);
				}
			}
		}

		public void DecreaseAbilities()
		{
			// Reduce damage over time (or increase if below the base value)
			float damageDiff = Damage > eventLogic.BaseDamage ? -eventLogic.DamageDecrease : eventLogic.DamageDecrease;
			Damage += damageDiff;

			// Limit the damage change to the base value
			Damage = damageDiff < 0 ? Math.Max(eventLogic.BaseDamage, Damage) : Math.Min(eventLogic.BaseDamage, Damage);

			// Limit the lowest possible value to 0
			Damage = Math.Max(0, Damage);


			// Reduce damage radius over time (or increase if below the base value)
			float radiusDiff = DamageRadius > eventLogic.BaseDamageRadius ? -eventLogic.DamageRadiusDecrease : eventLogic.DamageRadiusDecrease;
			DamageRadius += radiusDiff;

			// Limit the damage radius change to the base value
			DamageRadius = radiusDiff < 0 ? Math.Max(eventLogic.BaseDamageRadius, DamageRadius) : Math.Min(eventLogic.BaseDamageRadius, DamageRadius);

			// Limit the lowest possible value to 0
			DamageRadius = Math.Max(0, DamageRadius);
		}

		public void IncreaseAbilities()
		{
			// Increase the amount of damage to do within the damage radius
			Damage += eventLogic.DamageStep;
			Damage = Math.Min(Damage, eventLogic.MaxDamage);

			// Increase the damage radius
			DamageRadius += eventLogic.DamageRadiusStep;
			DamageRadius = Math.Min(DamageRadius, eventLogic.MaxDamageRadius);
		}

		public Player[] GetPlayersInRange()
		{
			List<Player> players = new List<Player>();

			Vector scp457PlayerPos = GetPlayer().GetPosition();
			foreach (Player affectedPlayer in PluginManager.Manager.Server.GetPlayers())
			{
				// If the possibly affected player is not a spectator, is not SCP-457, or is in range, add them to the list
				if (affectedPlayer.TeamRole.Team != Team.SPECTATOR && !plugin.IsScp457(affectedPlayer) && CheckDamageRadius(scp457PlayerPos, affectedPlayer.GetPosition()))
				{
					players.Add(affectedPlayer);
				}
			}

			return players.ToArray();
		}

		private bool CheckDamageRadius(Vector pos1, Vector pos2)
		{
			Vector.Distance(pos1, pos2);
			return (pos1 - pos2).SqrMagnitude <= SqrDamageRadius;
		}
	}
}
