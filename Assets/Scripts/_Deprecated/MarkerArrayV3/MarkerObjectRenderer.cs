using UnityEngine;
using RosMessageTypes.Visualization;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
public class MarkerObjectRenderer : MonoBehaviour{
    private MarkerMsg _marker;
    private float _lifeTime;
    private int _activeMarkerType = -1;
    
    
    public void Update() {
        if(this._activeMarkerType > -1) {
            this._lifeTime -= Time.deltaTime;
        }
        if(this._activeMarkerType > -1 && this._lifeTime <= 0) {
            // Destroy(this.gameObject); ToDo
        }
    }
    
    public void Render(MarkerMsg marker) {
        this._lifeTime = 3;
        if(this._activeMarkerType != marker.type) {
            CalculateVisualization(marker);
        } 
        UpdateTransformation(marker);
    }
    
    private void UpdateTransformation(MarkerMsg marker) {
        // ToDo Lerp between position and rotation
        this.transform.position = this.ConvertVecToFloatVec(marker.pose.position.From<FLU>());
        this.transform.rotation = this.ConvertQuatToFloatQuat(marker.pose.orientation.From<FLU>());
        this.transform.localScale = new Vector3(
            this.ConvertDoubleToFloat(marker.scale.x), 
            this.ConvertDoubleToFloat(marker.scale.y), 
            this.ConvertDoubleToFloat(marker.scale.z)
        );
    }
    
    private void CalculateVisualization(MarkerMsg marker) {
        this._activeMarkerType = marker.type;
        if (ConvertMarkerTypeToPrimitiveType(this._activeMarkerType, out PrimitiveType primitiveType)) {
            // remove old gfx
            if (GetComponent<MeshFilter>() != null) {
                Destroy(GetComponent<MeshFilter>());
            }
            if (GetComponent<MeshRenderer>() != null) {
                Destroy(GetComponent<MeshRenderer>());
            }
            
            if (GetComponent<Collider>() != null) { 
                Destroy(GetComponent<Collider>());   
            }
        
            // set new mesh
            MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = GeneratePrimitiveMesh(primitiveType);
            this.gameObject.AddComponent<MeshRenderer>();
        
            // add collider
            switch (primitiveType){
                case PrimitiveType.Cube:
                    this.gameObject.AddComponent<BoxCollider>();
                    break;
                case PrimitiveType.Sphere:
                    this.gameObject.AddComponent<SphereCollider>();
                    break;
                case PrimitiveType.Capsule:
                    this.gameObject.AddComponent<CapsuleCollider>();
                    break;
                case PrimitiveType.Cylinder:
                    this.gameObject.AddComponent<CapsuleCollider>();
                    break;
                case PrimitiveType.Plane:
                    this.gameObject.AddComponent<MeshCollider>();
                    break;
                case PrimitiveType.Quad:
                    this.gameObject.AddComponent<MeshCollider>();
                    break;
            }
        
            // set color
            var renderer = this.GetComponent<Renderer>();
            renderer.material.color = new Color(marker.color.r, marker.color.g, marker.color.b, marker.color.a);
        }
        else {
            Destroy(this.gameObject);
        }
    }
    
    private bool ConvertMarkerTypeToPrimitiveType(int markerType, out PrimitiveType primitiveType) {
        switch (markerType) {
            case MarkerMsg.CUBE:
                primitiveType = PrimitiveType.Cube;
                return true;
            case MarkerMsg.SPHERE:
                primitiveType = PrimitiveType.Sphere;
                return true;
            default:
                primitiveType = 0;
                Debug.Log("Shape of marker object is not defined!");
                return false;
        }
    }
    
    private Mesh GeneratePrimitiveMesh(PrimitiveType primitiveType) {
        GameObject temp = GameObject.CreatePrimitive(primitiveType);
        Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
        return mesh;
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