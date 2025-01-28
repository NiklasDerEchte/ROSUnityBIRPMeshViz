using RosMessageTypes.Geometry;
using RosMessageTypes.Visualization;
using UnityEngine;

public class ROSMarkerArrayProcessorController : MonoBehaviour {
    public ROSMarkerArrayProcessor processor;
    private MarkerMsg[] markerMsgList = new MarkerMsg[3];
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void TestCall() {
        for (int pos = 0; pos < markerMsgList.Length; pos++) {
            if (markerMsgList[pos] == null) {
                markerMsgList[pos] = new MarkerMsg();
                markerMsgList[pos].ns = "TEST_OBJECT";
                markerMsgList[pos].id = pos;
                markerMsgList[pos].type = 1;
                markerMsgList[pos].pose = new PoseMsg(
                    new PointMsg(
                        Random.Range(-10, 10), 
                        Random.Range(-10, 10), 
                        Random.Range(-10, 10)),
                    new QuaternionMsg(
                        Random.rotation.x, 
                        Random.rotation.y, 
                        Random.rotation.z, 
                        Random.rotation.w)
                    );
            }
            else {
                
            }
        }
        MarkerArrayMsg markerArrayMsg = new MarkerArrayMsg(markerMsgList);
        processor.ProcessMarkers(markerArrayMsg);
    }
}
