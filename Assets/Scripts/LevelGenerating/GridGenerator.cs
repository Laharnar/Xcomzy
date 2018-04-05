using UnityEngine;
/// <summary>
/// Generates temporary grid for testing.
/// </summary>
public class GridGenerator:MonoBehaviour {

    public static GridGenerator gen;

    public Transform gridItem;

    public int w = 20;
    public int l = 20;

    public Vector2 scale = new Vector2(1,1);

    private void Awake() {
        gen = this;
        MakeGrid(w, l, transform.position.z);

    }

    private void Start() {
        MapGrid.InitSingleton();

    }

    private void MakeGrid(int w, int l, float hpos) {
        for (int i = 0; i < w; i++) {
            for (int j = 0; j < l; j++) {
                Instantiate(gridItem, transform.position+new Vector3(i*scale.x, hpos, j*scale.y), new Quaternion(), transform);
            }
        }
    }
}