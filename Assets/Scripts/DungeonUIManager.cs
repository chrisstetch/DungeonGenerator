using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonUIManager : MonoBehaviour
{
    [Header("Generator Reference")]
    public DungeonGenerator dungeonGenerator;

    [Header("UI Controls")]
    public Slider roomCountSlider;
    public Slider minWidthSlider;
    public Slider maxWidthSlider;
    public Slider minHeightSlider;
    public Slider maxHeightSlider;
    public InputField seedInput;

    [Header("UI Buttons")]
    public Button generateButton;
    public Button resetButton;
    public Button saveButton;
    public Toggle showPathToggle;

    [Header("UI Labels")]
    public Text countLabel;
    public Text minWidthLabel;
    public Text maxWidthLabel;
    public Text minHeightLabel;
    public Text maxHeightLabel;

    void Start()
    {
        // Set default values
        SetDefaults();

        // Reset button
        if (resetButton != null) resetButton.onClick.AddListener(SetDefaults);

        // Save button
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);

        // Generate button
        if (generateButton != null) generateButton.onClick.AddListener(OnGenerateClicked);

        // Path toggle
        if (showPathToggle != null) showPathToggle.onValueChanged.AddListener(delegate { TogglePathVisibility(); });

        // Generate on start
        OnGenerateClicked();
    }

    void Update()
    {
        // UI text updating
        UpdateUILabels();

        // Regenerate rooms for testing (Hot key)
        if (Input.GetKeyDown(KeyCode.Space)) OnGenerateClicked();
    }

    // Handles reading the UI, setting up the seed, and triggering generation
    public void OnGenerateClicked()
    {
        if (dungeonGenerator == null) return;

        // 1. Setup Seed
        if (seedInput != null && seedInput.text.Length > 0)
        {
            int seed = seedInput.text.GetHashCode();
            Random.InitState(seed);
        }
        // Use current time to generate seed if no seed entered
        else
        {
            Random.InitState(System.DateTime.Now.Millisecond);
        }

        // 2. Read Values
        dungeonGenerator.roomCount = (int)roomCountSlider.value;

        // Widths
        dungeonGenerator.minW = (int)minWidthSlider.value;
        dungeonGenerator.maxW = (int)maxWidthSlider.value;
        if (dungeonGenerator.maxW < dungeonGenerator.minW) dungeonGenerator.maxW = dungeonGenerator.minW;

        // Heights
        dungeonGenerator.minH = (int)minHeightSlider.value;
        dungeonGenerator.maxH = (int)maxHeightSlider.value;
        if (dungeonGenerator.maxH < dungeonGenerator.minH) dungeonGenerator.maxH = dungeonGenerator.minH;

        // 3. Trigger Generation
        dungeonGenerator.GenerateDungeon();

        // 4. Force visibility update
        TogglePathVisibility();
    }

    public void OnSaveClicked()
    {
#if UNITY_EDITOR
        string seedName;
        if (seedInput != null && seedInput.text.Length > 0)
        {
            // Replaces spaces with "_"
            seedName = seedInput.text.Replace(" ", "_").Trim();
        }
        else
        {
            seedName = System.DateTime.Now.ToString("HHmmss");
        }

        dungeonGenerator.SaveDungeon(seedName);
#else
        Debug.Log("Saving is only supported in the Unity Editor.");
#endif
    }

    // Updates UI labels based on selection
    private void UpdateUILabels()
    {
        if (countLabel) countLabel.text = "Room count: " + roomCountSlider.value;
        if (minWidthLabel) minWidthLabel.text = "Min width: " + minWidthSlider.value;
        if (maxWidthLabel) maxWidthLabel.text = "Max width: " + maxWidthSlider.value;
        if (minHeightLabel) minHeightLabel.text = "Min height: " + minHeightSlider.value;
        if (maxHeightLabel) maxHeightLabel.text = "Max height: " + maxHeightSlider.value;
    }

    // Sets default room property values
    public void SetDefaults()
    {
        if (roomCountSlider) roomCountSlider.value = 10;

        if (minWidthSlider) minWidthSlider.value = 5;
        if (maxWidthSlider) maxWidthSlider.value = 15;

        if (minHeightSlider) minHeightSlider.value = 5;
        if (maxHeightSlider) maxHeightSlider.value = 15;

        if (seedInput) seedInput.text = "";
        if (showPathToggle) showPathToggle.isOn = false;
    }

    // Toggles visibility of generated optimal path
    public void TogglePathVisibility()
    {
        if (dungeonGenerator.pathTilemap != null && showPathToggle != null)
        {
            dungeonGenerator.pathTilemap.gameObject.SetActive(showPathToggle.isOn);
        }
    }
}