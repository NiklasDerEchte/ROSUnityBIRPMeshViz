using UnityEngine;
using RosMessageTypes.Visualization;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

class MarkerHolder : MonoBehaviour{
    private MarkerMsg _marker;
    private GameObject _gfx;
    private float _lifeTime;
    
    public void Update() {
        if(this._lifeTime <= 0) {}
    }
    
    public static string GetMarkerUID(MarkerMsg marker) {
        return marker.ns + "_" + marker.id;
    }
    
    public static bool IsMarkerValid(MarkerMsg marker) {
        // ToDo Iwie kommen manche Marker mit seltsamen Daten. Sie werden richtig von ROS Verarbeitet aber die Namen und Transformationen sind Quatsch
        return marker.ns.Trim().Length > 0;
    }
    
    public void Render(MarkerMsg marker) {
        if(this._gfx == null) {
            switch (marker.type) {
                case MarkerMsg.CUBE:
                    this._gfx = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case MarkerMsg.SPHERE:
                    this._gfx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                default:
                    Debug.Log("Marker object not defined!");
                    return;
            }
            this._gfx.name = "Gfx";
            this._gfx.transform.SetParent(this.transform);
    
            this._gfx.transform.position = this.ConvertVecToFloatVec(marker.pose.position.From<FLU>());
            this._gfx.transform.rotation = this.ConvertQuatToFloatQuat(marker.pose.orientation.From<FLU>());
            this._gfx.transform.localScale = new Vector3(
                this.ConvertDoubleToFloat(marker.scale.x), 
                this.ConvertDoubleToFloat(marker.scale.y), 
                this.ConvertDoubleToFloat(marker.scale.z)
            );
            
            var renderer = this._gfx.GetComponent<Renderer>();
            renderer.material.color = new Color(marker.color.r, marker.color.g, marker.color.b, marker.color.a);
        } else {
            // ToDo Lerp between position and rotation
            this._gfx.transform.position = this.ConvertVecToFloatVec(marker.pose.position.From<FLU>());
            this._gfx.transform.rotation = this.ConvertQuatToFloatQuat(marker.pose.orientation.From<FLU>());
            this._gfx.transform.localScale = new Vector3(
                this.ConvertDoubleToFloat(marker.scale.x), 
                this.ConvertDoubleToFloat(marker.scale.y), 
                this.ConvertDoubleToFloat(marker.scale.z)
            );
        }
    }
    
    private Vector3 ConvertVecToFloatVec(Vector3 input) {
        float x = this.ConvertDoubleToFloat(input.x);
        float y = this.ConvertDoubleToFloat(input.y);
        float z = this.ConvertDoubleToFloat(input.z);
        return new Vector3(x, y, z);
    }
    
    private Quaternion ConvertQuatToFloatQuat(Quaternion input) {
        float x = this.ConvertDoubleToFloat(input.x);
        float y = this.ConvertDoubleToFloat(input.y);
        float z = this.ConvertDoubleToFloat(input.z);
        float w = this.ConvertDoubleToFloat(input.w);
        return new Quaternion(x, y, z, w);
    }
    
    private float ConvertDoubleToFloat(double input) {
        float result = (float) input;
        if (float.IsPositiveInfinity(result)) {
            //result = float.MaxValue;
            result = 0;
        } else if (float.IsNegativeInfinity(result)) {
            //result = float.MinValue;
            result = 0;
        }
        return result;
    }
}
