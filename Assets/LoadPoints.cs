using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class LoadPoints : MonoBehaviour
{
    public string PointSetPPath;
    public string PointSetQPath;

    public List<Vector3> GetPointSetP()
    {
        return LoadPointCloud(PointSetPPath);
    }

    public List<Vector3> GetPointSetQ()
    {
        return LoadPointCloud(PointSetQPath);
    }

    List<Vector3> LoadPointCloud(string filePath)
    {
        List<Vector3> pointCloud = new List<Vector3>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            int numPts = int.Parse(reader.ReadLine());

            for (int i = 0; i < numPts; i++)
            {
                string[] values = reader.ReadLine().Split(' ');
                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);
                pointCloud.Add(new Vector3(x, y, z));
            }
        }

        return pointCloud;
    }
}
