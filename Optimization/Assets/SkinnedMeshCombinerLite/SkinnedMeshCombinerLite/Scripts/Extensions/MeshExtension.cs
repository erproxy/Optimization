using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LylekGames.Tools
{
    public static class MeshExtension
    {
        public static Mesh GetSubmesh(this Mesh mesh, int sub)
        {
            if (sub < 0 || sub >= mesh.subMeshCount)
                return null;

            MeshData origin = new MeshData(mesh);
            MeshData newData = new MeshData();

            int[] index = mesh.GetTriangles(sub);
            Dictionary<int, int> filter = new Dictionary<int, int>();
            int[] newIndex = new int[index.Length];
            for (int i = 0; i < index.Length; i++)
            {
                int k = index[i];
                int n;
                if (!filter.TryGetValue(k, out n))
                {
                    n = newData.vertices.Count;

                    if (origin.vertices.Count > k)
                        newData.vertices.Add(origin.vertices[k]);
                    if (origin.uv1.Count > k)
                        newData.uv1.Add(origin.uv1[k]);
                    if (origin.uv2.Count > k)
                        newData.uv2.Add(origin.uv2[k]);
                    if (origin.uv3.Count > k)
                        newData.uv3.Add(origin.uv3[k]);
                    if (origin.uv4.Count > k)
                        newData.uv4.Add(origin.uv4[k]);
                    if (origin.normals.Count > k)
                        newData.normals.Add(origin.normals[k]);
                    if (origin.tangents.Count > k)
                        newData.tangents.Add(origin.tangents[k]);
                    if (origin.colors.Count > k)
                        newData.colors.Add(origin.colors[k]);
                    if (origin.boneWeights.Count > k)
                        newData.boneWeights.Add(origin.boneWeights[k]);

                    filter.Add(k, n);
                }
                newIndex[i] = n;
            }

            Mesh newMesh = new Mesh();

            newMesh.SetVertices(newData.vertices);
            newMesh.SetUVs(0, newData.uv1);
            newMesh.SetUVs(1, newData.uv2);
            newMesh.SetUVs(2, newData.uv3);
            newMesh.SetUVs(3, newData.uv4);
            newMesh.SetTangents(newData.tangents);
            newMesh.SetColors(newData.colors);
            newMesh.SetNormals(newData.normals);
            newMesh.boneWeights = newData.boneWeights.ToArray();
            newMesh.triangles = newIndex;

            return newMesh;
        }

        public class MeshData
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector2> uv1 = new List<Vector2>();
            public List<Vector2> uv2 = new List<Vector2>();
            public List<Vector2> uv3 = new List<Vector2>();
            public List<Vector2> uv4 = new List<Vector2>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector4> tangents = new List<Vector4>();
            public List<Color32> colors = new List<Color32>();
            public List<BoneWeight> boneWeights = new List<BoneWeight>();

            public MeshData()
            {

            }
            public MeshData(Mesh mesh)
            {
                vertices = CreateList(mesh.vertices);
                uv1 = CreateList(mesh.uv);
                uv2 = CreateList(mesh.uv);
                uv3 = CreateList(mesh.uv3);
                uv4 = CreateList(mesh.uv4);
                normals = CreateList(mesh.normals);
                tangents = CreateList(mesh.tangents);
                colors = CreateList(mesh.colors32);
                boneWeights = CreateList(mesh.boneWeights);
            }
            private List<T> CreateList<T>(T[] array)
            {
                return new List<T>(array);
            }
        }
    }
}
