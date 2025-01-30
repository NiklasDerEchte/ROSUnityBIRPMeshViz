using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace ComputeRendering {
    public struct VertexData {
        public Vector3 position;
        public Color color;
    }
    
    public class MeshProcessingUnit : MonoBehaviour {
        [Header("GPU Settings")]
        public bool useGPU = false;
        public ComputeShader computeShader;
        
        public void VertexDataToMesh(VertexData[] vertexArray, Mesh mesh) {
            if (useGPU) {
                Debug.Log("Using GPU for mesh processing.");
                StartCoroutine(ProcessMeshOnGPU(vertexArray, mesh));
            } else {
                Debug.Log("Using CPU for mesh processing.");
                StartCoroutine(ProcessMeshOnCPU(vertexArray, mesh));
            }
        }
        
        IEnumerator ProcessMeshOnCPU(VertexData[] vertexArray, Mesh mesh) {
            int vertexCount = vertexArray.Length;
            Vector3[] vertices = new Vector3[vertexCount];
            Color[] vertexColors = new Color[vertexCount];
            List<int> triangles = new List<int>();

            for (int i = 0; i < vertexCount; i++) {
                vertices[i] = vertexArray[i].position;
                vertexColors[i] = vertexArray[i].color;
                
                if (i % 3 == 0 && i + 2 < vertexCount) {
                    triangles.Add(i);
                    triangles.Add(i + 1);
                    triangles.Add(i + 2);
                }
                
                if (i % 300 == 0) {
                    yield return null;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.colors = vertexColors;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            yield return null;
        }
        
        IEnumerator ProcessMeshOnGPU(VertexData[] vertexArray, Mesh mesh) {
            int vertexCount = vertexArray.Length;
            ComputeBuffer vertexBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 7);
            vertexBuffer.SetData(vertexArray);

            int kernel = computeShader.FindKernel("CSMain");
            computeShader.SetBuffer(kernel, "vertices", vertexBuffer);
            computeShader.Dispatch(kernel, vertexCount / 256 + 1, 1, 1);

            vertexBuffer.GetData(vertexArray);
            vertexBuffer.Release();
            
            Vector3[] vertices = new Vector3[vertexCount];
            Color[] colors = new Color[vertexCount];
            List<int> triangles = new List<int>();

            for (int i = 0; i < vertexCount; i++) {
                vertices[i] = vertexArray[i].position;
                colors[i] = vertexArray[i].color;
                
                if (i % 3 == 0 && i + 2 < vertexCount) {
                    triangles.Add(i);
                    triangles.Add(i + 1);
                    triangles.Add(i + 2);
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            yield return null;
        }
    }
}

