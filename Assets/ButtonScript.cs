using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    public PointCloudAlignment pointCloudAlignment;

    void Start()
    {
        // Get a reference to the PointCloudAlignment script
        if (pointCloudAlignment == null)
        {
            pointCloudAlignment = FindObjectOfType<PointCloudAlignment>();
            if (pointCloudAlignment == null)
            {
                Debug.LogError("Point Cloud Alignment script not found.");
                return;
            }
        }

        // Find and set button click listeners
        SetButtonClickListeners();
    }

    void SetButtonClickListeners()
    {
        // Switch Mode Buttons
        Button switchModeButton1 = GameObject.Find("RigidButton").GetComponent<Button>();
        switchModeButton1.onClick.AddListener(SwitchMode1);

        Button switchModeButton2 = GameObject.Find("ScaleButton").GetComponent<Button>();
        switchModeButton2.onClick.AddListener(SwitchMode2);

        // Change Visualization Toggles
        Toggle toggleVis1 = GameObject.Find("AlignButton").GetComponent<Toggle>();
        toggleVis1.onValueChanged.AddListener((value) => ToggleVisualization1(value));

        Toggle toggleVis2 = GameObject.Find("LineButton").GetComponent<Toggle>();
        toggleVis2.onValueChanged.AddListener((value) => ToggleVisualization2(value));
    }

    void SwitchMode1()
    {
        SceneManager.LoadScene("SampleScene");
    }

    void SwitchMode2()
    {
        SceneManager.LoadScene("SceneScaled");
    }

    void ToggleVisualization1(bool value)
    {
        pointCloudAlignment.ToggleVisualization1();
    }

    void ToggleVisualization2(bool value)
    {
        pointCloudAlignment.ToggleVisualization2();
    }
}
