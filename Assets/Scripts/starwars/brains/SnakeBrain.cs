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

        /// <summary>
        /// The snake make sharp turns left and right and shoots whenever it can
        /// </summary>
        public override Action NextAction() {
            // Make sure to move in a stright line
            var rotation = spaceship.Rotation;
            if (rotation % 90 > Spaceship.ROTATION_PER_ACTION) {
                return TurnLeft.action;
            } // If we don't need to rotate check a random to see if we need to turn


            return spaceship.CanShoot ? Shoot.action : DoNothing.action;
        }
    }
}