using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Visualization;

public class AsyncROSMeshVisualizer : MonoBehaviour {
    public GameObject renderTargetObject;
    public string rosTopicName = "/object_markers";

    private Material meshMaterial;
    private ROSConnection rosConnection;
    private GameObject meshObject;
    private Mesh unityMesh;

    void Start() {
        rosConnection = ROSConnection.GetOrCreateInstance();
        if (rosConnection == null) {
            Debug.LogError("ROSConnection init failed.");
            return;
        }

        rosConnection.Subscribe<MarkerArrayMsg>(rosTopicName, OnMeshReceived);
        
        meshMaterial = new Material(Shader.Find("Unlit/VertexColor"));
        if (meshMaterial == null) {
            Debug.LogError("UnlitVertexColorShader not found.");
            return;
        }
        
        if(renderTargetObject == null) {
            meshObject = new GameObject("VisualizedMesh");
        } else {
            meshObject = renderTargetObject;
        }
        
        meshObject.AddComponent<MeshFilter>();
        var renderer = meshObject.AddComponent<MeshRenderer>();
        renderer.material = meshMaterial;
        
        unityMesh = new Mesh {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
    }

    void OnMeshReceived(MarkerArrayMsg markerArrayMsg) {
        if (markerArrayMsg == null || markerArrayMsg.markers.Length == 0) {
            Debug.LogWarning("MarkerArrayMsg is empty.");
            return;
        }

        foreach (var marker in markerArrayMsg.markers) {
            if (marker == null || marker.type != MarkerMsg.TRIANGLE_LIST || marker.points == null || marker.points.Length == 0) {
                Debug.LogWarning("Marker is invalid or is empty.");
                continue;
            }
            StartCoroutine(ProcessMesh(marker));
            break;
        }
    }

    IEnumerator ProcessMesh(MarkerMsg marker) {
        Vector3[] vertices = new Vector3[marker.points.Length];
        List<int> triangles = new List<int>();
        Color[] vertexColors = new Color[marker.points.Length];
        for (int i = 0; i < marker.points.Length; i++) {
            vertices[i] = new Vector3(
                (float)marker.points[i].x,
                (float)marker.points[i].z,
                (float)marker.points[i].y * -1f // ROS z is inverted to Unity y
            );

            if (marker.colors != null && marker.colors.Length > i) {
                var rosColor = marker.colors[i];
                vertexColors[i] = new Color((float)rosColor.r, (float)rosColor.g, (float)rosColor.b, (float)rosColor.a);
            } else {
                vertexColors[i] = Color.cyan;
            }
            if (i % 100 == 0) {
                yield return null;
            }
        }

        for (int i = 0; i < marker.points.Length; i += 3) {
            if (i + 2 < marker.points.Length) {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
            }

            if (i % 300 == 0) {
                yield return null;
            }
        }

        unityMesh.Clear();
        unityMesh.vertices = vertices;
        unityMesh.triangles = triangles.ToArray();
        unityMesh.colors = vertexColors;
        unityMesh.RecalculateNormals();
        unityMesh.RecalculateBounds();
        
        var meshFilter = meshObject.GetComponent<MeshFilter>();
        if (meshFilter != null) {
            meshFilter.mesh = unityMesh;
        } else {
            Debug.LogError("MeshFilter not found.");
        }
    }
}
