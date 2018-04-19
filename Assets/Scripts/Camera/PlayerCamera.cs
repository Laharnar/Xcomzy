using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    static PlayerCamera cam;

    public Vector3 defaultOffset;
    public float moveSpeed = 10f;
    Vector3 offset;
    Vector3 panOffset;

    private void Awake() {
        cam = this;
    }

    // Update is called once per frame
    void LateUpdate () {
        
        Vector3 wantedPos = (GameplayManager.m.playerFlag.ActiveSoldier
            .transform.position+ defaultOffset)
            + panOffset * moveSpeed;// pos behind the active unit+panning offset
        Vector3 dir = wantedPos - transform.position;
        transform.Translate(dir*Mathf.Clamp(dir.magnitude, 0f, 1f)*Time.deltaTime*10f, Space.World);
    }

    private void Update() {
        Vector2 v = Input.mousePosition;
        float max = 40;
        Vector3 off = offset;
        // prevents inf speed when mouse goes off screen
        v.x = Mathf.Clamp(v.x, 0, Screen.width);
        v.y = Mathf.Clamp(v.y, 0, Screen.height);
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
        panOffset = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * offset;

    }

    public static void ResetFocus() {
        if (cam != null)
            cam.offset = Vector3.zero;
    }

    internal static void LockOn(Soldier soldier) {
        //throw new NotImplementedException();
    }
}
