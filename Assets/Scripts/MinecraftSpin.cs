
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MinecraftSpin : UdonSharpBehaviour
{
    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    float timePassed = 0;
    private void Update()
    {
        timePassed += Time.deltaTime;
        transform.position = new Vector3(startPos.x, startPos.y + Mathf.Sin(timePassed) / 6f, startPos.z);
        transform.Rotate(new Vector3(0, 60f * Time.deltaTime, 0));
    }
}
