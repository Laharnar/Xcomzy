using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    public Vector3 defaultOffset;
    public float moveSpeed = 10f;
    
    // Update is called once per frame
    void LateUpdate () {
        Vector3 cam = transform.position;
        Vector3 dir = (GameplayManager.m.playerFlag.ActiveSoldier
            .transform.position+defaultOffset) - cam;

        transform.Translate(dir.normalized*Mathf.Clamp(dir.magnitude, 0f, 1f) *Time.deltaTime*moveSpeed);
    }

    internal static void LockOn(Soldier soldier) {
        //throw new NotImplementedException();
    }
}
