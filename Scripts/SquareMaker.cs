using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareMaker : MonoBehaviour
{

    public Material material;
    public Color color;

    public GameObject GetSquare(Vector2 pos, float size) {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        vertices[0] = new Vector3(pos.x, pos.y + size);
        vertices[1] = new Vector3(pos.x + size, pos.y + size);
        vertices[2] = new Vector3(pos.x, pos.y);
        vertices[3] = new Vector3(pos.x + size, pos.y);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 1;
        triangles[5] = 3;

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        GameObject gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        //gameObject.transform.localScale = new Vector3(30, 30, 1);
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();

        renderer.material = material;

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", color);
        renderer.SetPropertyBlock(mpb);

        return gameObject;
    }

    public void ChangeColor(GameObject go, Color c) {
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", c);
        renderer.SetPropertyBlock(mpb);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
