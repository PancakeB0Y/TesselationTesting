using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public static FPSCounter instance { get; private set; }

    int frameCount = 0;
    float totalFrameRate = 0;
    float averageFrameRate = 0;

    bool countFrames = false;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Update()
    {
        if (!countFrames) {
            return;
        }

        frameCount++;

        // get current fps
        float frameRate = 1f / Time.deltaTime;

        totalFrameRate += frameRate;

        //get average fps across the whole lifetime of the object
        averageFrameRate = totalFrameRate / frameCount;
    }

    public void StartTrackingFPS()
    {
        countFrames = true;
    }

    public void StopTrackingFPS()
    {
        countFrames = false;
        Debug.Log("Average FPS: " + averageFrameRate.ToString());
    }
}
