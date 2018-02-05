using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Sprite))]
public class BackgroundComponent : MonoBehaviour {
    
    public BackgroundIndexData backgrounds;

    private BackgroundData currentBackground;

	public void SetBackground(string backgroundTag) {
        currentBackground = backgrounds.GetData(backgroundTag);
        UpdateDisplay();
    }

    public void PopuateMemory(ScreenMemory memory) {
        if (currentBackground != null) {
            memory.backgroundTag = currentBackground.tag;
        }
    }

    public void PopulateFromMemory(ScreenMemory memory) {
        if (memory.backgroundTag != null && memory.backgroundTag.Length > 0) {
            currentBackground = backgrounds.GetData(memory.backgroundTag);
            UpdateDisplay();
        }
    }

    private void UpdateDisplay() {
        GetComponent<SpriteRenderer>().sprite = currentBackground.background;
    }
}
