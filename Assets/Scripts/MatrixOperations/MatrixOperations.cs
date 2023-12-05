using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixOperations : MonoBehaviour
{
    private Matrix4x4 _matrix;
    public Vector4 v;

    // Start is called before the first frame update
    void Start()
    {
        _matrix.SetTRS(transform.position, transform.rotation, transform.localScale);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MyTranslate(new Vector3(Random.Range(-2,2), Random.Range(-2,2), Random.Range(-2,2)));
        }
    }
    
    public void MyTranslate(Vector3 pos)
    {
        // _matrix.SetTRS(transform.position, transform.rotation, transform.localScale);
        // _matrix = _matrix * Matrix4x4.Translate(v);
        // transform.position = _matrix.GetColumn(3);
        
        v = new Vector4(transform.position.x, transform.position.y, transform.position.z, 1);
        // v = new Vector4(0, 0, 0, 1);
        _matrix = Matrix4x4.identity;

        _matrix.m03 = pos.x;
        _matrix.m13 = pos.y;
        _matrix.m23 = pos.z;

        v = _matrix * v;
        transform.position = new Vector3(v.x, v.y, v.z);
    }
}