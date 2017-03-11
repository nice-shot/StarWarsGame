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

	private const float DANGER_RADIUS = 3f;

	private bool ShouldRaiseShield () {
		var position = spaceship.Position;
		foreach (var shot in Space.Shots) {
			// Ignore non-existing shots
			if (!shot.IsAlive) continue;
			var shotRelPos = spaceship.ClosestRelativePosition (shot);
			if (Vector2.Distance (position, shot.Position) < DANGER_RADIUS) {
				Debug.Log ("Shot is close");
				return true;
			}
		}

		foreach (var ship in Space.Spaceships) {
			if (spaceship == ship || !ship.IsAlive)	continue;
			var shipRelPos = spaceship.ClosestRelativePosition (ship);
			var distance = Vector2.Distance (position, ship.Position);
//			Debug.Log (ship.Name + " is at: " + ship.Position);
//			Debug.Log ("Relative position is: " + shipRelPos);
//			Debug.Log ("I'm at: " + position);
//			Debug.Log ("Simple distance is: " + distance);
//			Debug.Log ("Short distnace is: " + Vector2.Distance (position, shipRelPos));
			if (distance < DANGER_RADIUS) {
				Debug.Log (ship.Name + " is close");
				return true;
			}
		}
		return false;
	}
	// <summary>
	// Tries to ram into other ships while using the shield
	// </summary>
	public override Action NextAction () {
		if (ShouldRaiseShield ()) {
			if (spaceship.CanRaiseShield) {
				return ShieldUp.action;
			}
		} else if (spaceship.IsShieldUp) {
			return ShieldDown.action;
		}

		return TurnLeft.action;
	}
}
}