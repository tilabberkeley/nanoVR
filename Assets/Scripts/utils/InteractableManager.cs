/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractableManager : MonoBehaviour
{
    Collider[] nucleotides = null;

    void FixedUpdate()
    {
        if (nucleotides != null)
        {
            foreach (Collider col in nucleotides)
            {
                XRSimpleInteractable xrsi = col.gameObject.GetComponent<XRSimpleInteractable>();
                if (xrsi != null)
                {
                    xrsi.enabled = false;
                }
            }
        }

        nucleotides = Physics.OverlapBox(transform.position, new Vector3(0.0001f, 0.18f, 0.0001f), transform.rotation);

        foreach (Collider col in nucleotides)
        {
            XRSimpleInteractable xrsi = col.gameObject.GetComponent<XRSimpleInteractable>();
            if (xrsi != null)
            {
                xrsi.enabled = true;
            }
        }
    }
}
