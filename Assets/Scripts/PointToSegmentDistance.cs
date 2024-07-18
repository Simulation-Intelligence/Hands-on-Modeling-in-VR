using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PointToSegmentDistance : MonoBehaviour
{
    // Import the function from the DLL
    [DllImport("CalDistance")]
    private static extern double pointToSegmentDistance(double px, double py, double pz, double sx, double sy, double sz, double ex, double ey, double ez);

    void Start()
    {
        // discuss: input, 3-dim array, skeleton_array (ovrskeleton), mpm nodes data is on gpu
        // tachi kernel function
        double distance = pointToSegmentDistance(0.0, 0.0, 3.0, 0.0, 0.0, 0.0, 3.0, 0.0, 0.0);
        Debug.Log("Distance: " + distance);
    }
}