using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    static PlayerCamera cam;

    public Vector3 defaultOffset;
    public float moveSpeed = 10f;
    Vector3 offset;

    private void Start() {
        cam = this;
    }

    // Update is called once per frame
    void LateUpdate () {
        Vector3 cam = transform.position;
        Vector3 dir = (GameplayManager.m.playerFlag.ActiveSoldier
            .transform.position+defaultOffset) - cam 
            + transform.TransformDirection(offset);

        transform.Translate(dir.normalized*Mathf.Clamp(dir.magnitude, 0f, 1f) *Time.deltaTime*moveSpeed);
    }

    private void Update() {
        Vector2 v = Input.mousePosition;
        float max = 40;
        if (v.x < max) {
            offset.x -= 1-(v.x/max);
        }
        if (v.x > Screen.width - max) {
            offset.x += 1 - (v.x / max);
        }
        if (v.y < max) {
            offset.z -= 1 - (v.y / max);
        }
        if (v.y > Screen.height - max) {
            offset.z += 1 - (v.y / max);
        }
    }

    public static void ResetFocus() {
        cam.offset = Vector3.zero;
    }

    internal static void LockOn(Soldier soldier) {
        //throw new NotImplementedException();
    }
}
