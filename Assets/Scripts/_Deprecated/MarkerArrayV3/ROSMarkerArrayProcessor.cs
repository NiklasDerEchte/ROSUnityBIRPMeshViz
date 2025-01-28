using System;
using UnityEngine;
using RosMessageTypes.Visualization;
using Unity.Robotics.ROSTCPConnector;

public class ROSMarkerArrayProcessor : MonoBehaviour {
    public string topicName = "/object_markers";
    private ROSConnection _rosConnection;

    void Start() {
        this._rosConnection = ROSConnection.GetOrCreateInstance();
        this._rosConnection.Subscribe<MarkerArrayMsg>(topicName, ProcessMarkers);
    }

    public void ProcessMarkers(MarkerArrayMsg markerArray) {
        Debug.Log("Markers " + markerArray.markers.Length);
        for(int markerPosition = 0; markerPosition < markerArray.markers.Length; markerPosition++) {
            MarkerMsg marker = markerArray.markers[markerPosition];
            // ToDo Nachschauen warum manche nicht Valide sind, sie sehen in ROS gut aus
            bool isValidMarker = IsMarkerValid(marker);
            LogMarker(marker, isValidMarker, markerPosition);
            if (!isValidMarker) {
                continue;
            }
            HandleMarkerRenderer(marker);
        }
    }
    
    private void HandleMarkerRenderer(MarkerMsg marker) {
        string markerUID = GetMarkerUID(marker);
        Transform childMarkerRendererTransform = this.transform.Find(markerUID);
        if(childMarkerRendererTransform) { // update
            GameObject markerRendererObj = childMarkerRendererTransform.gameObject;
            markerRendererObj.GetComponent<MarkerObjectRenderer>().Render(marker);
        } else { // create
            GameObject markerRendererObj = new GameObject(markerUID);
            markerRendererObj.transform.SetParent(this.transform);
            markerRendererObj.AddComponent<MarkerObjectRenderer>();
            markerRendererObj.GetComponent<MarkerObjectRenderer>().Render(marker);
        }
    }
    
    private string GetMarkerUID(MarkerMsg marker) {
        return marker.ns + "_" + marker.id;
    }
    
    private bool IsMarkerValid(MarkerMsg marker) {
        // ToDo Iwie kommen manche Marker mit seltsamen Daten. Sie werden richtig von ROS Verarbeitet aber die Namen und Transformationen sind Quatsch
        return marker.ns.Trim().Length > 0;
    }
    
    private void LogMarker(MarkerMsg marker, bool isValidMarker, int markerPosition) {
        string message = "Marker/" + markerPosition + " ";
        if(isValidMarker) {
            message += "[VALID]";
        } else {
            message += "[NOT VALID]";
        }

        message += "\n" + marker.ToString();
        Debug.Log(message);
    }
}