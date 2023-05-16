using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HighlightOnHover : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    public Material highlightMaterial; // The material to use for highlighting

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;
    }

    private void OnEnable()
    {
        //GetComponent<XRSimpleInteractable>().hoverEntered.AddListener(Highlight);
        //GetComponent<XRSimpleInteractable>().hoverExited.AddListener(Unhighlight);
    }

    private void OnDisable()
    {
        //GetComponent<XRSimpleInteractable>().hoverEntered.RemoveListener(Highlight);
        //GetComponent<XRSimpleInteractable>().hoverExited.RemoveListener(Unhighlight);
    }

    private void Highlight(XRRayInteractor interactor)
    {
        meshRenderer.material = highlightMaterial;
    }

    private void Unhighlight(XRRayInteractor interactor)
    {
        meshRenderer.material = originalMaterial;
    }
}
