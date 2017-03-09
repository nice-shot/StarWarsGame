﻿using UnityEngine;
using Infra.Collections;
using Infra.Utils;

namespace StarWars {
/// <summary>
/// A visible body. Can move and rotate.
/// </summary>
public abstract class Body : MonoBehaviour, IPoolable {

    public float Rotation {
        get {
            return transform.eulerAngles.z;
        }
        set {
            var rot = transform.eulerAngles;
            rot.z = value;
            transform.eulerAngles = rot;
        }
    }

    public Vector2 Position {
        get {
            return transform.position;
        }
        set {
            transform.position = value;
        }
    }

    public Vector2 Forward {
        get {
            return Vector2.right.Rotate(Rotation * Mathf.Deg2Rad);
        }
        set {
            transform.position = value;
        }
    }

    public void FixPosition() {
        var pos = Position;
        var bounds = Game.Size / 2;
        if (pos.x < -bounds.x) pos.x += bounds.x * 2;
        if (pos.x >= bounds.x) pos.x -= bounds.x * 2;
        if (pos.y < -bounds.y) pos.y += bounds.y * 2;
        if (pos.y >= bounds.y) pos.y -= bounds.y * 2;
        Position = pos;
    }

    public void MoveForward(float speed) {
        var direction = Forward.GetWithMagnitude(speed);
        Position += direction;
        FixPosition();
    }

    /// <summary>
    /// Expected params: (Vector2)position, (float)angle.
    /// </summary>
    public virtual int Activate(params object[] activateParams) {
        var index = 0;
        Position = (Vector2)activateParams[index++];
        Rotation = (float)activateParams[index++];

        return index;
    }

    public void ReturnSelf() {
        gameObject.SetActive(false);
    }

}
}
