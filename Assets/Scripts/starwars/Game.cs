using UnityEngine;
using System;
using System.Collections.Generic;
using Infra.Collections;
using Infra.Utils;
using Infra;
using Infra.Audio;
using StarWars.UI;

namespace StarWars {
public class Game : MonoBehaviour {
    private readonly Space.Mutable space = new Space.Mutable();

    [Serializable]
    public class BodyTypeToSprite {
        public SpaceshipBody.Type bodyType;
        public Sprite primaryImage;
        public Sprite secondaryImage;
        public AdjustableAudioClip shotSound;
    }

    public string[] spaceshipNames;
    public GameObjectPool spaceshipBodiesPool;
    public GameObjectPool shotBodiesPool;
    public GameObjectPool explosionEffectsPool;
    public GameObjectPool respawnEffectsPool;
    public ScoreBoard scoreBoard;

    public AdjustableAudioClip killSound;
    public AdjustableAudioClip collisionSound;
    public AdjustableAudioClip shieldBurnShipSound;
    public AdjustableAudioClip shieldBurnShotSound;
    public AdjustableAudioClip shieldUpSound;

    public BodyTypeToSprite[] _spaceshipImages;
    public static Dictionary<SpaceshipBody.Type, BodyTypeToSprite> spaceshipImages =
        new Dictionary<SpaceshipBody.Type, BodyTypeToSprite>();

    public Color[] secondaryColors;

    private readonly HashSet<Spaceship.Mutable> deadShips = new HashSet<Spaceship.Mutable>();
    private readonly HashSet<Shot.Mutable> deadShots = new HashSet<Shot.Mutable>();

    private static Game instance;

    public static Vector2 Size {
        get {
            var res = UIUtils.ScreenResolution;
            return Camera.main.ScreenToWorldPoint(new Vector3(res.width, res.height)) * 2f;
        }
    }

    protected void Awake() {
        instance = this;

        foreach (var entry in _spaceshipImages) {
            spaceshipImages[entry.bodyType] = entry;
        }

        DebugUtils.Assert(0 < spaceshipNames.Length, "Define spaceship names at 'Game'");
        DebugUtils.Assert(spaceshipNames.Length <= 6, "Max 6 spaceships allowed!");
    }
    
    protected void Start() {
        // Create all spaceships.
        var secondaryColorIndex = 0;
        var type2colors = new Dictionary<SpaceshipBody.Type, HashSet<Color>>();
        var nameCount = new Dictionary<string, int>();
        foreach (var spaceshipName in spaceshipNames) {
            var brain = BrainFactory.GetBrain(spaceshipName);
            var spawnPoint = Space.GetSpawnPoint();
            var angle = UnityEngine.Random.Range(1, 360 / Spaceship.ROTATION_PER_ACTION) * Spaceship.ROTATION_PER_ACTION;
            var body = spaceshipBodiesPool.Borrow<SpaceshipBody>(spawnPoint, angle, space, brain);

            // Check for duplicate type and color.
            var bodyType = body.spaceshipType;
            var color = body.primaryColor;
            if (!type2colors.ContainsKey(bodyType)) {
                type2colors[bodyType] = new HashSet<Color>();
                type2colors[bodyType].Add(color);
            } else if (type2colors[bodyType].Contains(color)) {
                // Color clash! Set secondary color.
                body.SetSecondaryColor(secondaryColors[secondaryColorIndex++]);
            } else {
                type2colors[bodyType].Add(color);
            }

            if (nameCount.ContainsKey(brain.DefaultName)) {
                ++nameCount[brain.DefaultName];
                brain.Name = brain.DefaultName + " " + nameCount[brain.DefaultName];
            } else {
                nameCount[brain.DefaultName] = 1;
            }

            // Register to score board.
            scoreBoard.Add(brain.Name, color, body.secondaryColor);
        }
    }

    protected void FixedUpdate() {
        var spaceships = space.Spaceships;
        // First all alive spaceships select their next action.
        foreach (var spaceship in spaceships) {
            if (spaceship.IsAlive) {
                spaceship.SelectAction();
            }
        }
        // Then all spaceships do their turn.
        foreach (var spaceship in spaceships) {
            spaceship.DoTurn();
        }

        var shots = space.Shots;
        foreach (var shot in shots) {
            if (shot.IsAlive) {
                shot.DoTurn();
            }
        }

        deadShips.Clear();
        deadShots.Clear();
        // Check for collisions.
        // Spaceship to spaceship.
        for (int i = 0; i < spaceships.Count - 1; i++) {
            var ship = spaceships[i];
            if (!ship.IsAlive) continue;
            for (int j = i + 1; j < spaceships.Count; j++) {
                var other = spaceships[j];
                if (!other.IsAlive) continue;
                if (ship.obj.CheckCollision(other.obj)) {
                    DebugUtils.Log(ship.Name + " collided with ship " + other.Name);
                    if (ship.IsShieldUp != other.IsShieldUp) {
                        // The ship without the shield is dead.
                        var killer = ship.IsShieldUp ? ship : other;
                        var dead = ship.IsShieldUp ? other : ship;
                        scoreBoard.AddScore(killer.Name, 1);
                        SpaceshipKilled(dead);
                        AudioManager.PlayClip(shieldBurnShipSound);
                        if (ship == dead) break;
                    } else {
                        SpaceshipKilled(other);
                        SpaceshipKilled(ship);
                        AudioManager.PlayClip(collisionSound);
                        break;
                    }
                }
            }
        }
        // Shot to spaceship.
        foreach (var ship in spaceships) {
            if (!ship.IsAlive) continue;
            foreach (var shot in shots) {
                if (!shot.IsAlive) continue;
                if (shot.obj.CheckCollision(ship.obj)) {
                    DebugUtils.Log(ship.Name + " collided with shot " + shot.Name);
                    if (!ship.IsShieldUp) {
                        scoreBoard.AddScore(shot.Shooter.Name, 1);
                        SpaceshipKilled(ship);
                        AudioManager.PlayClip(killSound);
                    } else {
                        AudioManager.PlayClip(shieldBurnShotSound);
                    }
                    shot.BeDead();
                    break;
                }
            }
        }

        // Clear dead ships.
        foreach (var spaceship in spaceships) {
            if (!spaceship.IsAlive) {
                deadShips.Add(spaceship);
            }
        }
        foreach (var spaceship in deadShips) {
            space.RemoveSpaceship(spaceship);
        }

        // Clear dead shots.
        foreach (var shot in shots) {
            if (!shot.IsAlive) {
                deadShots.Add(shot);
            }
        }
        foreach (var shot in deadShots) {
            space.RemoveShot(shot);
        }
    }

    public static void SpawnShot(Spaceship.Mutable shooter) {
        instance.shotBodiesPool.Borrow<ShotBody>(shooter.Position, shooter.Rotation, instance.space, shooter);
        AudioManager.PlayClip(spaceshipImages[shooter.Brain.BodyType].shotSound);
    }

    public static void OnShieldUp() {
        AudioManager.PlayClip(instance.shieldUpSound);
    }

    public static void RespawnedSpaceship(Spaceship.Mutable spaceship) {
        instance.respawnEffectsPool.Borrow<IPoolable>(spaceship.Position);
    }

    private void SpaceshipKilled(Spaceship.Mutable spaceship) {
        scoreBoard.AddScore(spaceship.Name, -1);
        spaceship.BeDead();
        instance.explosionEffectsPool.Borrow<IPoolable>(spaceship.Position);
    }
}
}
