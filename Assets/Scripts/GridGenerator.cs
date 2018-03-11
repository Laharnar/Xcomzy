using UnityEngine;
/// <summary>
/// Generates temporary grid for testing.
/// </summary>
public class GridGenerator:MonoBehaviour {

    public Transform gridItem;

    private void Awake() {
        MakeGrid(20, 20, transform.position.z);
    }

    private void MakeGrid(int w, int l, float hpos) {
        for (int i = 0; i < w; i++) {
            for (int j = 0; j < l; j++) {
                Instantiate(gridItem, new Vector3(i, hpos, j), new Quaternion(), transform);
            }
        }
    }
}