using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    static PlayerCamera cam;

    public Vector3 defaultOffset;
    public float moveSpeed = 10f;
    Vector3 offset;
    Vector3 offsetDir;

    private void Start() {
        cam = this;
    }

    // Update is called once per frame
    void LateUpdate () {
        
        Vector3 dir = (GameplayManager.m.playerFlag.ActiveSoldier
            .transform.position+ defaultOffset)
            + offsetDir * moveSpeed;
        
        transform.position = dir;
    }

    private void Update() {
        Vector2 v = Input.mousePosition;
        float max = 40;
        Vector3 off = offset;
        if (v.x < max) {
            off.x -= (1 - (v.x/max));
        }
        if (v.x > Screen.width - max) {
            off.x += (1 - ((Screen.width-v.x) / max));
        }
        if (v.y < max) {
            off.z -= (1 - (v.y / max));
        }
        if (v.y > Screen.height - max) {
            off.z += (1 - ((Screen.height-v.y) / max));
        }
        offset = off;
        // point offset in cam's direction
        offsetDir = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * offset;

    }

    public static void ResetFocus() {
        cam.offset = Vector3.zero;
    }

    internal static void LockOn(Soldier soldier) {
        //throw new NotImplementedException();
    }
}
