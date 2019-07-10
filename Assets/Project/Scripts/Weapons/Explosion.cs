using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public void Explode(float range, float damage) {
        transform.GetChild(0).localScale = Vector3.one * range * 2;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, range, transform.up);
        foreach (RaycastHit hit in hits) {
            // Destroy(hit.transform.gameObject);
            if (hit.transform.GetComponent<IDamageable>() != null) {
                hit.transform.GetComponent<IDamageable>().Damage(damage);
            }
            if (hit.transform.GetComponentInParent<IDamageable>() != null) {
                hit.transform.GetComponentInParent<IDamageable>().Damage(damage);
            }
        }
        Destroy(gameObject, 0.5f);
    }
}
