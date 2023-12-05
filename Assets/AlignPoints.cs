using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PointCloudAlignment : MonoBehaviour
{
    public LoadPoints loader;
    public Text transformationMatrixText;
    public bool scaled; //True for scaled, false for normal rigid transformation

    private bool AlignVisualizationEnabled = false;
    private bool MovementVisualizationEnabled = false;
    private Matrix4x4 bestTransformation = Matrix4x4.identity;

    public void Start()
    {
        if (loader != null && loader.GetPointSetP() != null && loader.GetPointSetQ() != null)
        {
            AlignPointClouds();
        }
        else
        {
            if (loader.GetPointSetP() == null) Debug.LogError("Point cloud P not loaded. Check the LoadPoints configuration.");
            else if (loader.GetPointSetQ() == null) Debug.LogError("Point cloud Q not loaded. Check the LoadPoints configuration.");
            else Debug.LogError("Point clouds not loaded. Check the LoadPoints configuration.");
        }
    }

    void AlignPointClouds()
    {
        List<Vector3> pointSetP = loader.GetPointSetP();
        List<Vector3> pointSetQ = loader.GetPointSetQ();

        int iterations = 1000;
        float inlierThreshold = 0.1f;
        int minInliers = pointSetP.Count / 2;

        int bestInlierCount = 0;

        for (int iter = 0; iter < iterations; iter++)
        {
            List<Vector3> subsetP = RandomSubset(pointSetP, 3);
            List<Vector3> subsetQ = RandomSubset(pointSetQ, 3);

            Matrix4x4 transformation = ComputeRigidTransformation(subsetP, subsetQ);

            List<Vector3> transformedQ = ApplyTransformation(pointSetQ, transformation);

            int inlierCount = CountInliers(pointSetP, transformedQ, inlierThreshold);

            if (inlierCount > bestInlierCount)
            {
                bestInlierCount = inlierCount;
                bestTransformation = transformation;
            }
        }

        string matrixText = GetMatrixString(bestTransformation);
        UpdateTransformationMatrixText(matrixText);

        //Apply the final transformation to the second point cloud
        List<Vector3> registeredQ = ApplyTransformation(pointSetQ, bestTransformation);

        VisualizePointClouds();
    }

    Matrix4x4 ComputeRigidTransformation(List<Vector3> subsetP, List<Vector3> subsetQ)
    {
        Matrix4x4 transformation;

        Quaternion optimalRotation = Quaternion.FromToRotation(subsetQ[1] - subsetQ[0], subsetP[1] - subsetP[0]);

        Vector3 centroidP = CalculateCentroid(subsetP);
        Vector3 centroidQ = CalculateCentroid(subsetQ);

        Vector3 optimalTranslation = centroidP - optimalRotation * centroidQ;

        if (!scaled)
            transformation = Matrix4x4.TRS(optimalTranslation, optimalRotation, Vector3.one);

        else
        {
            // Calculations for the scaling..
            //To be implemented.

            transformation = Matrix4x4.TRS(optimalTranslation, optimalRotation, Vector3.one);
        }

        return transformation;
    }


    //Calculations
    List<Vector3> RandomSubset(List<Vector3> list, int subsetSize)
    {
        List<Vector3> subset = new List<Vector3>();
        List<Vector3> remaining = new List<Vector3>(list);

        subsetSize = Mathf.Min(subsetSize, list.Count);

        for (int i = 0; i < subsetSize; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, remaining.Count);

            subset.Add(remaining[randomIndex]);

            remaining.RemoveAt(randomIndex);
        }
        return subset;
    }

    Vector3 CalculateCentroid(List<Vector3> points)
    {
        Vector3 centroid = Vector3.zero;

        foreach (Vector3 point in points)
        {
            centroid += point;
        }

        centroid /= points.Count;
        return centroid;
    }

    List<Vector3> CenterSubset(List<Vector3> subset, Vector3 centroid)
    {
        List<Vector3> centeredSubset = new List<Vector3>();

        foreach (Vector3 point in subset)
        {
            centeredSubset.Add(point - centroid);
        }
        return centeredSubset;
    }

    List<Vector3> ApplyTransformation(List<Vector3> points, Matrix4x4 transformation)
    {
        List<Vector3> transformedPoints = new List<Vector3>();

        foreach (Vector3 point in points)
        {
            transformedPoints.Add(transformation.MultiplyPoint3x4(point));
        }
        return transformedPoints;
    }

    int CountInliers(List<Vector3> pointsP, List<Vector3> pointsQ, float threshold)
    {
        int inlierCount = 0;

        for (int i = 0; i < pointsP.Count && i < pointsQ.Count; i++)
        {
            if (Vector3.Distance(pointsP[i], pointsQ[i]) < threshold)
            {
                inlierCount++;
            }
        }

        return inlierCount;
    }


    //Visualizations
    public void ToggleVisualization1()
    {
        AlignVisualizationEnabled = !AlignVisualizationEnabled;
        VisualizePointClouds();
    }

    public void ToggleVisualization2()
    {
        MovementVisualizationEnabled = !MovementVisualizationEnabled;
        VisualizePointClouds();
    }

    void VisualizePointClouds()
    {
        // Clear existing visualizations
        ClearVisualizations();

        List<Vector3> pointSetP = loader.GetPointSetP();
        List<Vector3> pointSetQ = loader.GetPointSetQ();

        VisualizePoints(pointSetP, Color.red);  // Visualize pointSetP in red
        VisualizePoints(pointSetQ, Color.blue); // Visualize pointSetQ in blue

        List<Vector3> registeredQ = ApplyTransformation(pointSetQ, bestTransformation);

        if (AlignVisualizationEnabled)
        {
            VisualizePoints(registeredQ, Color.green);
        }

        if (MovementVisualizationEnabled)
        {
            VisualizeAlignmentMovement(pointSetQ, registeredQ, Color.yellow);
        }
    }

    void ClearVisualizations()
    {
        GameObject[] sphereAndLines = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in sphereAndLines)
        {
            if (obj.name == "Sphere" || obj.name == "AlignmentLineSegment")
            {
                Destroy(obj);
            }
        }
    }

    void VisualizeAlignmentMovement(List<Vector3> pointsBefore, List<Vector3> pointsAfter, Color lineColor)
    {
        if (pointsBefore.Count != pointsAfter.Count)
        {
            Debug.LogError("Cannot visualize alignment movement. Point sets have different sizes.");
            return;
        }

        for (int i = 0; i < pointsBefore.Count; i++)
        {
            Vector3 startPoint = pointsBefore[i];
            Vector3 endPoint = pointsAfter[i];

            GameObject lineSegment = new GameObject("AlignmentLineSegment");
            LineRenderer lineRenderer = lineSegment.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[] { startPoint, endPoint });
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = 0.03f;
            lineRenderer.endWidth = 0.03f;
        }
    }

    void VisualizePoints(List<Vector3> points, Color color)
    {
        foreach (Vector3 point in points)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = point;
            sphere.GetComponent<Renderer>().material.color = color;

            float sphereSize = 0.6f;
            sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
        }
    }


    //Matrix to Text
    string GetMatrixString(Matrix4x4 matrix)
    {
        return $"| {matrix.m00}, {matrix.m01}, {matrix.m02}, {matrix.m03} |\n" +
               $"| {matrix.m10}, {matrix.m11}, {matrix.m12}, {matrix.m13} |\n" +
               $"| {matrix.m20}, {matrix.m21}, {matrix.m22}, {matrix.m23} |\n" +
               $"| {matrix.m30}, {matrix.m31}, {matrix.m32}, {matrix.m33} |";
    }

    void UpdateTransformationMatrixText(string matrixText)
    {
        if (transformationMatrixText != null)
        {
            if (scaled)
                transformationMatrixText.text = "Best Scaled Transformation Matrix:\n" + matrixText;
            else
                transformationMatrixText.text = "Best Transformation Matrix:\n" + matrixText;

        }
    }
}
