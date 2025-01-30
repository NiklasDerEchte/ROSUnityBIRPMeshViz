using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Visualization;

public class AsyncROSMeshVisualizer : MonoBehaviour {
    public GameObject renderTargetObject; // object to select the root of the mesh
    public string rosTopicName = "/object_markers";

    private Material meshMaterial;
    private ROSConnection rosConnection;
    private GameObject meshObject; // actual root mesh object
    private Mesh mesh; // draw mesh

    private Queue<MarkerArrayMsg> meshQueue = new Queue<MarkerArrayMsg>();
    private bool semaphore = true;

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
        
        this.mesh = new Mesh {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
    }

    void OnMeshReceived(MarkerArrayMsg markerArrayMsg) {
        if (markerArrayMsg == null || markerArrayMsg.markers.Length == 0) {
            Debug.LogWarning("MarkerArrayMsg is empty.");
            return;
        }
        this.meshQueue.Enqueue(markerArrayMsg);
    }

    void Update() {
        Debug.Log("Queue size: " + this.meshQueue.Count);
        if (this.meshQueue.Count == 0) {
            return;
        }
        this.UpdateQueue();
    }

    void UpdateQueue() {
        if(!this.semaphore) {
            return;
        }
        this.semaphore = false;
        Debug.Log("Processing mesh.");
        MarkerArrayMsg markerArrayMsg = this.meshQueue.Dequeue();
        foreach (var marker in markerArrayMsg.markers) {
            if (marker == null || marker.type != MarkerMsg.TRIANGLE_LIST || marker.points == null || marker.points.Length == 0) {
                Debug.LogWarning("Marker is invalid or is empty.");
                continue;
            }
            StartCoroutine(ProcessMesh(marker));
            break;
        }
    }

     IEnumerator ResetSemaphore(float seconds=0.1f) {
        yield return new WaitForSeconds(seconds);
        this.semaphore = true;
        Debug.Log("Semaphore reset.");
     }

    IEnumerator ProcessMesh(MarkerMsg marker) {
        long startTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Vector3[] vertices = new Vector3[marker.points.Length];
        List<int> triangles = new List<int>();
        Color[] vertexColors = new Color[marker.points.Length];
        Color lastColor = Color.cyan;
        for (int i = 0; i < marker.points.Length; i++) {
            vertices[i] = new Vector3(
                (float)marker.points[i].x,
                (float)marker.points[i].z,
                (float)marker.points[i].y * -1f // ROS z is inverted to Unity y
            );

            if (marker.colors != null && marker.colors.Length > i) {
                var rosColor = marker.colors[i];
                lastColor = new Color((float)rosColor.r, (float)rosColor.g, (float)rosColor.b, (float)rosColor.a);
                vertexColors[i] = lastColor;
            } else {
                vertexColors[i] = lastColor;
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

        this.mesh.Clear();
        this.mesh.vertices = vertices;
        this.mesh.triangles = triangles.ToArray();
        this.mesh.colors = vertexColors;
        this.mesh.RecalculateNormals();
        this.mesh.RecalculateBounds();
        
        var meshFilter = meshObject.GetComponent<MeshFilter>();
        if (meshFilter != null) {
            meshFilter.mesh = this.mesh;
        } else {
            Debug.LogError("MeshFilter not found.");
        }
        Debug.Log("Mesh processed. Time: " + (System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime) + "ms");
        StartCoroutine(ResetSemaphore());
    }


    void OnApplicationQuit() {
        if (rosConnection != null) {
            rosConnection.Unsubscribe(rosTopicName);
        }
    }

    void OnDestroy() {
        if (rosConnection != null) {
            rosConnection.Unsubscribe(rosTopicName);
        }
    }
}
