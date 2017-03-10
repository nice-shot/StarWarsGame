using UnityEngine;
using StarWars.Actions;
using Infra.Utils;

namespace StarWars.Brains {
    public class SnakeBrain : SpaceshipBrain {
        public override string DefaultName {
            get {
                return "Snake";
            }
        }
        public override Color PrimaryColor {
            get {
                return Color.cyan;
            }
        }
        public override SpaceshipBody.Type BodyType {
            get {
                return SpaceshipBody.Type.TieFighter;
            }
        }

		private const float ROTATION_CHANCE = 0.97f;
		private Action turnDirection;

		private void ChooseDirection() {
			if (Random.value > 0.5) {
				turnDirection = TurnLeft.action;
			} else {
				turnDirection = TurnRight.action;
			}
		}
		protected void Awake() {
			ChooseDirection ();
		}

        /// <summary>
        /// The snake make sharp turns left and right and shoots whenever it can
        /// </summary>
        public override Action NextAction() {
            // Make sure to move in a stright line
            var rotation = spaceship.Rotation;
			// Rotation is in specific degrees so we won't allways be exactly 90 degrees
			if (rotation % 90 > Spaceship.ROTATION_PER_ACTION) {
				return turnDirection;
			}

			// Checks if we should rotate
			if (Random.value > ROTATION_CHANCE) {
				ChooseDirection ();
				// Initialize Rotation
				return turnDirection;
			}
				
            return spaceship.CanShoot ? Shoot.action : DoNothing.action;
        }
    }
}