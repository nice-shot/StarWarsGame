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

	// <summary>
	// Tries to ram into other ships while using the shield
	// </summary>
	public override Action NextAction () {
		return DoNothing.action;	
	}
}
}