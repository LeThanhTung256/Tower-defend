using UnityEngine;
using TMPro;

public class FpsDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsDisplay;

    private float pollingTime = 1f;
    private float time = 0;
    private int frameCount = 0;
    

    private void Update()
    {
        time += Time.deltaTime;
        frameCount++;

        if (time >= pollingTime)
        {
            int fps = Mathf.RoundToInt(frameCount / time);

            fpsDisplay.text = $"{fps} FPS";
            time -= pollingTime;
            frameCount = 0;
        }
        

    }
}
