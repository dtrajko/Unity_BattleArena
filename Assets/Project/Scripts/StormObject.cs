using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormObject : MonoBehaviour
{
    [SerializeField] private float initialDistance;
    [SerializeField] private float shrinkSmoothness;
    [SerializeField] private Light directionalLight;

    private float targetDistance;
    private Color directionalLightDefaultColor;
    private Color directionalLightStormColor;

    // Start is called before the first frame update
    void Start()
    {
        directionalLightStormColor = new Color(0.6f, 0, 1.0f, 0.5f);
        directionalLightDefaultColor = directionalLight.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetDistance <= 0) return;

        Vector3 direction = transform.position.normalized;
        Vector3 targetPosition = direction * targetDistance;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * shrinkSmoothness);

        /*
        transform.localScale = Vector3.Lerp(
            transform.localScale, 
            new Vector3(targetDistance / initialDistance, transform.localScale.y, transform.localScale.z),
            Time.deltaTime * shrinkSmoothness
        );
        */
    }

    public void MoveToDistance(float distance) {
        targetDistance = distance;
    }

    private void OnTriggerStay(Collider otherCollider) {
        if (otherCollider.GetComponent<Player>() != null) {
            otherCollider.GetComponent<Player>().StormDamage();
        }
        directionalLight.color = directionalLightStormColor;
    }

    private void OnTriggerEnter(Collider otherCollider) {
        FindObjectOfType<StormManager>().SoundSinister.Play();
    }

    private void OnTriggerExit(Collider otherCollider) {
        directionalLight.color = directionalLightDefaultColor;
        FindObjectOfType<StormManager>().SoundSinister.Stop();
    }
}
