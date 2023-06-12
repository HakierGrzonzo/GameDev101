using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ProceduralPolyMesh : MonoBehaviour
{
    [SerializeField]
    private int numberOfVertices = 3;
    [SerializeField]
    private float radius = 3F;
    // Start is called before the first frame update

    static double radians(double degrees) {
        return Math.PI * degrees / 180;
    }

    static double toUnsignedAngle(double degrees) {
        if (degrees < 0) {
            return 360 + degrees;
        }
        return degrees;
    }

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

    private double getMaxSafeLength(Vector3 position) {
        var center = this.transform.position;
        var zeroPoint = getVertexCoord(0);
        var Sangle = toUnsignedAngle(
            Vector3.SignedAngle(
                position - center, 
                zeroPoint - center, 
                Vector3.up
            )
        );
        var innerAngle = 360 / numberOfVertices;
        var S = radians(Sangle % innerAngle);
        var outsideAngle = radians((180 - innerAngle) / 2);
        var bottomTerm = Math.Sin(S) * Math.Cos(outsideAngle) + Math.Sin(outsideAngle) * Math.Cos(S); 
        var maxSafeLength = radius * (Math.Sin(outsideAngle) / bottomTerm);
        return maxSafeLength;
    }

    public bool IsOutOfBounds(Vector3 position) {
        position.y = this.transform.position.y;
        var center = this.transform.position;
        var currentLength = (position - center).magnitude;
        var maxSafeLength = getMaxSafeLength(position); 
        return (currentLength > maxSafeLength);
    }

    void OnEnable() {
        GenerateMesh(numberOfVertices);
    }

    void OnDrawGizmos() {
        for (double i = 0; i < 360; i = i + 20)
        {
            var rad = radians(i);
            var vec = new Vector3((float) Math.Sin(rad), 0, (float) Math.Cos(rad)) + this.transform.position;
            vec = vec * (float) getMaxSafeLength(vec);
            vec.y = this.transform.position.y;
            Gizmos.DrawLine(this.transform.position, vec);
        }
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
