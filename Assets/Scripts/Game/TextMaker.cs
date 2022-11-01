using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMaker : MonoBehaviour
{
    Color[] colorMap = new Color[12] {
        new Color32(90, 128, 227, 255),
        new Color32(90, 210, 227, 255),
        new Color32(90, 227, 159, 255),
        new Color32(190, 227, 90, 255),
        new Color32(227, 189, 90, 255),
        new Color32(227, 90, 124, 255),
        new Color32(227, 90, 164, 255),
        new Color32(227, 90, 198, 255),
        new Color32(208, 90, 227, 255),
        new Color32(179, 90, 227, 255),
        new Color32(114, 90, 227, 255),
        new Color32(129, 120, 194, 255)
    };
    Color above = new Color32(228, 91, 106, 255);

    public GameObject GetText(Vector2 pos = new Vector2()) {
        GameObject go = new GameObject("MyText");
        go.transform.position = new Vector3(pos.x, pos.y);
        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        TextMesh tm = (TextMesh)go.AddComponent<TextMesh>();
        tm.fontSize = 60;
        tm.color = new Color(0.5f, 0, 0, 1);
        tm.anchor = TextAnchor.MiddleCenter;
        tm.text = "";
        go.GetComponent<MeshRenderer>().enabled = false;

        return go;
    }

    public void ChangeText(GameObject go, int n) {
        TextMesh tm = go.GetComponent<TextMesh>();
        if (n > 0) {
            tm.text = n.ToString();
            if (n < 13) {
                tm.color = colorMap[n-1];
            } else {
                tm.color = above;
            }
        }
    }

    public void ToggleText(GameObject go) {
        go.GetComponent<MeshRenderer>().enabled = !go.GetComponent<MeshRenderer>().enabled;
    }
}
