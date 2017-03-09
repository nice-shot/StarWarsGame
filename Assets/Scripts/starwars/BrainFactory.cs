using UnityEngine;
using System.Collections.Generic;
using Infra;
using Infra.Utils;
using StarWars.Brains;

namespace StarWars {
public class BrainFactory : MonoBehaviour {

    private class BrainPool {
        public List<SpaceshipBrain> brains;
        private int nextIndex = 0;
        private readonly Transform factory;

        public BrainPool(SpaceshipBrain brain, Transform factory) {
            brains = new List<SpaceshipBrain>(1);
            brains.Add(brain);
            nextIndex = 0;
            this.factory = factory;
        }

        public SpaceshipBrain Get() {
            if (nextIndex >= brains.Count) {
                var newBrain = ComponentUtils.Duplicate(brains[0], factory);
                brains.Add(newBrain);
            }
            return brains[nextIndex++];
        }
    }

    private readonly Dictionary<string, BrainPool> brainPools = new Dictionary<string, BrainPool>();
    private static BrainFactory instance;

    protected void Awake() {
        // Register child brains as prefab brains.
        var brains = GetComponentsInChildren<SpaceshipBrain>();
        foreach (var brain in brains) {
            brainPools[brain.DefaultName] = new BrainPool(brain, transform);
        }

        instance = this;
    }

    public static SpaceshipBrain GetBrain(string brainName) {
        if (brainName == "Random") {
            var names = instance.brainPools.Keys;
            var i = Random.Range(0, names.Count);
            foreach (var name in names) {
                // Skip the player.
                if (name != "Player") {
                    brainName = name;
                    if (i <= 0) break;
                }
                --i;
            }
        }
        DebugUtils.Assert(instance.brainPools.ContainsKey(brainName), "No brain named '" + brainName + "' in factory");
        var pool = instance.brainPools[brainName];
        return pool.Get();
    }
}
}
