using UnityEngine;
using StarWars.Actions;

namespace StarWars.Brains {
public abstract class SpaceshipBrain : MonoBehaviour {
    protected Spaceship spaceship;
    public void Activate(Spaceship spaceship) {
        this.spaceship = spaceship;
        Name = DefaultName;
    }

    public string Name { get; set; }
    public abstract string DefaultName { get; }
    public abstract Color PrimaryColor { get; }
    public abstract SpaceshipBody.Type BodyType { get; }
    public abstract Action NextAction();
}
}