/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using UnityEngine;

/// <summary>
/// Helps run coroutines in other classes.
/// </summary>
public class CoRunner : MonoBehaviour
{
    public static CoRunner Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Run(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}