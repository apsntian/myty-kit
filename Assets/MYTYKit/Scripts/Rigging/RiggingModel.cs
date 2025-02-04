
using UnityEngine;

public class RiggingModel : MonoBehaviour
{
    protected Vector3[] rawPoints;

    public int GetNumPoints()
    {
        if (rawPoints == null) return 0;
        return rawPoints.Length;
    }

    public void Alloc(int numPoints)
    {
        rawPoints = new Vector3[numPoints];
    }

    public void SetPoint(int index, Vector3 point)
    {
        rawPoints[index] = point;
    }

}


