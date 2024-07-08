using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixOperations : MonoBehaviour
{
    private Mesh mesh;
    private Vector3 _initialLocalScale;
    private float _initialScale = 1.0f;
    private Quaternion _initialRotation;
    private Matrix4x4 _matrix;
    private Vector4 v4;
    public enum Axis {X, Y, Z};
    
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        
        _initialScale = transform.localScale.x;
        _initialLocalScale = transform.localScale / _initialScale;
        _initialRotation = transform.rotation;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Method1: use the Matrix4x4 in unity
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Vector3 trans = new Vector3(0, 0.0f, 0.0f);
            
            float scale_factor = 1.0f;
            Vector3 scale = _initialLocalScale * scale_factor;
            
            // Vector3 eulerAngles = new Vector3(15f, 0, 0);
            Vector3 eulerAngles = new Vector3(0, 0, 0);
            Quaternion rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
            
            // shear the object
            float shear_angle1 = 45f;
            float shear_angle2 = 0f;
            
            MatrixTransform(trans, rotation, scale, Axis.X, shear_angle1, shear_angle2);
        }
        
        // Method2: use the four-dimensional vector in unity to translate
        if (Input.GetKeyDown(KeyCode.W))
        {
            // Vector4Translate(new Vector3(Random.Range(-2,2), Random.Range(-2,2), Random.Range(-2,2)));

            // Method2: use the four-dimensional vector in unity to scale
            // float scale = Random.Range(-2, 2);
            // Vector4Scale(new Vector3(scale, scale, scale));

            // Method2: use the four-dimensional vector in unity to rotate
            // Vector4Rotation(Axis.X, 15f);
            // Vector4Rotation(Axis.Y, 15f);
            // Vector4Rotation(Axis.Z, 15f);
        }
    }
    
    public void MatrixTransform(Vector3 trans, Quaternion rotation, Vector3 scale, Axis axis, float shear_angle1, float shear_angle2)
    {
        // include translation, rotation, and scale
        _matrix.SetTRS(trans, rotation, scale);
        
        // shear the object
        shearObject(axis, shear_angle1, shear_angle2);
        
        Debug.Log(_matrix);
        UpdateMeshVertices();
    }
    
    public void shearObject(Axis axis, float shear_angle1, float shear_angle2)
    {
        switch (axis)
        {
            case Axis.X:
                _matrix.m01 = Mathf.Tan(shear_angle1 * Mathf.Deg2Rad); // Shearing along X-axis with respect to Y
                _matrix.m02 = Mathf.Tan(shear_angle2 * Mathf.Deg2Rad); // Shearing along X-axis with respect to Z
                break;
            case Axis.Y:
                _matrix.m10 = Mathf.Tan(shear_angle1 * Mathf.Deg2Rad); // Shearing along Y-axis with respect to X
                _matrix.m12 = Mathf.Tan(shear_angle2 * Mathf.Deg2Rad); // Shearing along Y-axis with respect to Z
                break;
            case Axis.Z:
                _matrix.m20 = Mathf.Tan(shear_angle1 * Mathf.Deg2Rad); // Shearing along Z-axis with respect to X
                _matrix.m21 = Mathf.Tan(shear_angle2 * Mathf.Deg2Rad); // Shearing along Z-axis with respect to Y
                break;
        } 
    }
    
    public void UpdateMeshVertices()
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = _matrix.MultiplyPoint3x4(vertices[i]);
        }
        mesh.vertices = vertices;
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    
    public void Vector4Translate(Vector3 trans)
    {
        // v = new Vector4(transform.position.x, transform.position.y, transform.position.z, 1);
        v4 = new Vector4(0, 0, 0, 1);
        
        _matrix = Matrix4x4.identity;
        _matrix.m03 = trans.x;
        _matrix.m13 = trans.y;
        _matrix.m23 = trans.z;
        
        v4 = _matrix * v4;
        transform.position = new Vector3(v4.x, v4.y, v4.z);
    }
    
    public void Vector4Scale(Vector3 scale)
    {
        v4 = new Vector4(1, 1, 1, 1);
        
        _matrix = Matrix4x4.identity;
        _matrix.m00 = scale.x;
        _matrix.m11 = scale.y;
        _matrix.m22 = scale.z;
        
        v4 = _matrix * v4;
        transform.localScale = new Vector3(v4.x, v4.y, v4.z);
    }
    
    public void Vector4Rotation(Axis axis, float angle)
    {
        _matrix = Matrix4x4.identity;
        
        switch (axis)
        {
            case Axis.X:
                // rotate along the x-axis
                _matrix.m11 = Mathf.Cos(angle * Mathf.Deg2Rad);
                _matrix.m12 = -Mathf.Sin(angle * Mathf.Deg2Rad);
                _matrix.m21 = Mathf.Sin(angle * Mathf.Deg2Rad);
                _matrix.m22 = Mathf.Cos(angle * Mathf.Deg2Rad);
                break;
            case Axis.Y:
                // rotate along the y-axis
                _matrix.m00 = Mathf.Cos(angle * Mathf.Deg2Rad);
                _matrix.m02 = Mathf.Sin(angle * Mathf.Deg2Rad);
                _matrix.m20 = -Mathf.Sin(angle * Mathf.Deg2Rad);
                _matrix.m22 = Mathf.Cos(angle * Mathf.Deg2Rad);
                break;
            case Axis.Z:
                // rotate along the z-axis
                _matrix.m00 = Mathf.Cos(angle * Mathf.Deg2Rad);
                _matrix.m01 = -Mathf.Sin(angle * Mathf.Deg2Rad);
                _matrix.m10 = Mathf.Sin(angle * Mathf.Deg2Rad);
                _matrix.m11 = Mathf.Cos(angle * Mathf.Deg2Rad);
                break;
            default:
                break;
        }
        
        float qw = Mathf.Sqrt(1f + _matrix.m00 + _matrix.m11 + _matrix.m22) / 2f;
        float w = 4f * qw;
        float qx = (_matrix.m21 - _matrix.m12) / w;
        float qy = (_matrix.m02 - _matrix.m20) / w;
        float qz = (_matrix.m10 - _matrix.m01) / w;
        
        transform.rotation = transform.rotation * new Quaternion(qx, qy, qz, qw);
    }
}