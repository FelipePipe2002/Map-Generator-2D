using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    public Text fpsd;
    private float deltaTime = 0.0f;
    static private float maxfps,minfps;

    private void Start() {
        maxfps = float.MinValue;
        minfps = float.MaxValue;
    }
    private void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        if(fps > maxfps)
            maxfps = fps;
        if(fps < minfps)
            minfps = fps;
        fpsd.text = "FPS:" + Mathf.Ceil(fps) + " MinFPS:" + Mathf.Ceil(minfps) + " MaxFPS:" + Mathf.Ceil(maxfps);
    }
}
