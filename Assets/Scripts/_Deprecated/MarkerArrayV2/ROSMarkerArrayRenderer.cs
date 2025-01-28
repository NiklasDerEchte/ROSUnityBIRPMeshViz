using System;
using UnityEngine;
using RosMessageTypes.Visualization;
using Unity.Robotics.ROSTCPConnector;

class ROSMarkerArrayRenderer : MonoBehaviour {
    public string topicName = "/object_markers";
    private ROSConnection _rosConnection;
    private GameObject _markerHolderRootObj;

    void Start() {
        this._rosConnection = ROSConnection.GetOrCreateInstance();
        this._rosConnection.Subscribe<MarkerArrayMsg>(topicName, DisplayMarkers);
        this._markerHolderRootObj = new GameObject("MarkerArrayRenderer");
    }

    void DisplayMarkers(MarkerArrayMsg markerArray) {
        Debug.Log("Markers " + markerArray.markers.Length);
        foreach (var marker in markerArray.markers) {
            Debug.Log(marker.ToString());
            // ToDo Nachschauen warum manche nicht Valide sind, sie sehen in ROS gut aus
            if (!MarkerHolder.IsMarkerValid(marker)) {
                continue;
            }
            string markerUID = MarkerHolder.GetMarkerUID(marker);
            
            Transform childMarkerHolderTransform = this._markerHolderRootObj.transform.Find(markerUID);
            if(childMarkerHolderTransform) {
                GameObject markerHolderObj = childMarkerHolderTransform.gameObject;
                markerHolderObj.GetComponent<MarkerHolder>().Render(marker);
                
            } else {
                GameObject markerHolderObj = new GameObject(markerUID);
                markerHolderObj.transform.SetParent(this._markerHolderRootObj.transform);
                markerHolderObj.AddComponent<MarkerHolder>();
                markerHolderObj.GetComponent<MarkerHolder>().Render(marker);
            }
        }
    }
}
