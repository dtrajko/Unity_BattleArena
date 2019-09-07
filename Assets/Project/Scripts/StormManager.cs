using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StormManager : NetworkBehaviour
{
    public delegate void shrinkHandler();
    public event shrinkHandler OnShrink;

    [SerializeField] private float[] shrinkTimes;
    [SerializeField] private float[] distancesFromCenter;
    [SerializeField] private GameObject[] stormObjects;
    [SerializeField] private GameObject stormObjectTop;
    [SerializeField] private AudioSource soundSinister;

    private float timer = 0;
    private int stormIndex = -1;

    private bool shouldShrink;
    public bool ShouldShrink {
        set { shouldShrink = value; }
    }

    public AudioSource SoundSinister {
        get {
            return soundSinister;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        foreach (GameObject stormObject in stormObjects)
        {
            stormObject.GetComponent<StormObject>().gameObject.SetActive(true);
        }
        stormObjectTop.GetComponent<StormObjectTop>().gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;

        if (!shouldShrink) return;

        // Update the storm timer
        timer += Time.deltaTime;
        for (int i = 0; i < shrinkTimes.Length; i++) {
            float currentShrinkTime = shrinkTimes[i];
            if (timer > currentShrinkTime && stormIndex < i) {
                // The storm area is going to shrink
                stormIndex = i;

                float targetDistance = distancesFromCenter[i];
                foreach (GameObject stormObject in stormObjects) {
                    stormObject.GetComponent<StormObject>().MoveToDistance(targetDistance);
                }
                stormObjectTop.GetComponent<StormObjectTop>().MoveToDistance(targetDistance);

                // Alert
                if (OnShrink != null) {
                    OnShrink();
                }
            }
        }
    }
}

