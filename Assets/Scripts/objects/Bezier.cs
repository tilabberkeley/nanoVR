using UnityEngine;

/// <summary>
/// Encapsulates the tube and both endpoints of a bezier.
/// </summary>
public class Bezier
{
    public GameObject Tube { get; }
    public GameObject Endpoint0 { get; }
    public GameObject Endpoint1 { get; }

    private Renderer tubeRenderer;
    private Renderer e0Renderer;
    private Renderer e1Renderer;

    public Bezier(GameObject tube, GameObject endpoint0, GameObject endpoint1)
    {
        Tube = tube;
        Endpoint0 = endpoint0;
        Endpoint1 = endpoint1;

        tubeRenderer = tube.GetComponent<Renderer>();
        e0Renderer = endpoint0.GetComponent<Renderer>();
        e1Renderer = endpoint1.GetComponent<Renderer>();
    }

    public void Destroy()
    {
        GameObject.Destroy(Tube);
        GameObject.Destroy(Endpoint0);
        GameObject.Destroy(Endpoint1);
    }

    public void SetActive(bool activity)
    {
        Tube.SetActive(activity);
        Endpoint0.SetActive(activity);
        Endpoint1.SetActive(activity);
    }

    public void Enabled(bool enabled)
    {
        tubeRenderer.enabled = enabled;
        e0Renderer.enabled = enabled;
        e1Renderer.enabled = enabled;
    }
}
