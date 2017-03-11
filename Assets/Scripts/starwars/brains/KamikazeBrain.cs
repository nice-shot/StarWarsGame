using UnityEngine;
using StarWars.Actions;
using Infra.Utils;

namespace StarWars.Brains {
public class KamikazeBrain : SpaceshipBrain {
	public override string DefaultName {
		get {
			return "Kamikaze";
		}
	}

	public override Color PrimaryColor {
		get {
			return Color.yellow;
		}
	}

	public override SpaceshipBody.Type BodyType {
		get {
			return SpaceshipBody.Type.XWing;
		}
	}

	private const float DANGER_RADIUS = 4f;
	private Spaceship target = null;

	// <summary>
	// Tries to ram into other ships while using the shield
	// </summary>
	public override Action NextAction ()
	{
		if (ShouldRaiseShield()) {
			if (spaceship.CanRaiseShield) {
				return ShieldUp.action;
			}
		} else if (spaceship.IsShieldUp) {
			return ShieldDown.action;
		}
			

		var position = spaceship.Position;

		// Target closest ship
		foreach (var ship in Space.Spaceships) {
			if (spaceship == ship || target == ship || !ship.IsAlive) continue;
			if (target != null) { 
				var currentDistance = Vector2.Distance(position, target.Position);
				var shipDistance = Vector2.Distance(position, ship.Position);
				if (currentDistance > shipDistance) {
					target = ship;
				}

			} else {
				target = ship;
			}
		}

		if (target != null) {
			// Hunt (totally copied hunter brain...)
			var pos = spaceship.ClosestRelativePosition(target);
			var forwardVector = spaceship.Forward;
			var angle = pos.GetAngle(forwardVector);
			if (angle >= 10) return TurnLeft.action;
			if (angle <= -10) return TurnRight.action;
		}

		return DoNothing.action;
	}

	// Raises shiled whenever a shot or spaceship is nearby
	private bool ShouldRaiseShield ()
	{
		var position = spaceship.Position;

		foreach (var shot in Space.Shots) {
			// Ignore non-existing shots
			if (!shot.IsAlive) continue;
			if (Vector2.Distance(position, shot.Position) < DANGER_RADIUS) {
				Debug.Log("Shot is close");
				return true;
			}
		}

		foreach (var ship in Space.Spaceships) {
			// Ignore self and dead ships
			if (spaceship == ship || !ship.IsAlive) continue;
			if (Vector2.Distance(position, ship.Position) < DANGER_RADIUS) {
				Debug.Log(ship.Name + " is close");
				return true;
			}
		}
		return false;
	}
}
}