using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralPolyMesh : MonoBehaviour
{
    [SerializeField]
    private int numberOfVertices = 3;
    [SerializeField]
    private float radius = 3F;
    // Start is called before the first frame update

    private Vector3 getVertexCoord(int index) {
        // Angle is in radians
        var angle = (2 * Math.PI / (numberOfVertices)) * index;
        return new Vector3 (
            (float) Math.Cos(angle) * radius, 0, (float) Math.Sin(angle) * radius
        );
    }

    private List<int> getTriangleIndexes(int index) {
        var res = new List<int>();
        res.Add(0);
        res.Add(index + 2);
        res.Add(index + 1);
        return res;
    }

    void OnEnable() {
        GenerateMesh(numberOfVertices);
    }
    public void GenerateMesh(int _numberOfVertices) {
        numberOfVertices = _numberOfVertices;
        var mesh = new Mesh {
            name = "Simple Poly Mesh"
        };
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        vertices.Add(Vector3.zero);
        normals.Add(Vector3.up);
        for (int i = 0; i < numberOfVertices; i++) {
            vertices.Add(getVertexCoord(i));
            normals.Add(Vector3.up);
            triangles.AddRange(getTriangleIndexes(i));
        }
        vertices.Add(getVertexCoord(0));
        normals.Add(Vector3.up);
        var uvs = new List<Vector2>();
        foreach (var vertex in vertices) {
            var uv = new Vector2(
                vertex.x / (radius * 2) + .5F,
                vertex.z / (radius * 2) + .5F
            );
            uvs.Add(uv);
        }
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.Optimize();
        mesh.RecalculateTangents();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
