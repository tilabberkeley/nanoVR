/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static Utils;

public class NucleotideColliderPoolManager : MonoBehaviour
{
    [Header("References")]
    public HelixManager helixManager;
    public GameObject colliderPrefab;

    // Initial size of the collider pool.
    private const int initialPoolSize = 5 * 128; // 5 helices, 128 colliders per helix (64 for each direction).

    // If we use the entire pool, we double it.
    // If we use < 75% after a full pass, we shrink it to half.
    private List<NucleotideColliderComponent> _colliderPool = new List<NucleotideColliderComponent>();

    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    
    private const int LEFT_POOL_INDEX = 0;
    private const int RIGHT_POOL_INDEX = initialPoolSize / 2;

    int _poolIndex = 0;

    void Start()
    {
        ExpandPool(initialPoolSize);
    }

    void Update()
    {
        _poolIndex = 0;
        AssignCollidersForRayInteractor(leftRayInteractor, LEFT_POOL_INDEX);
        AssignCollidersForRayInteractor(rightRayInteractor, RIGHT_POOL_INDEX);
    }

    public void AssignCollidersForRayInteractor(XRRayInteractor rayInteractor, int rayPoolIndex)
    {
        Ray ray = new Ray(rayInteractor.rayOriginTransform.position, rayInteractor.rayOriginTransform.forward);

        List<(Helix, Vector3)> hitHelices = new List<(Helix, Vector3)>();

        // First pass: find the closest helix hit by the ray
        foreach (Helix helix in GlobalVariables.s_helixDict.Values)
        {
            if (helix.BoundingBox.IntersectRay(ray, rayInteractor.maxRaycastDistance, out Vector3 hitPoint))
            {
                hitHelices.Add((helix, hitPoint));
            }
        }

        foreach ((Helix, Vector3) item in hitHelices)
        { 
            Helix helix = item.Item1;
            Vector3 hitPoint = item.Item2;
            Vector3 gridCirclePos = helix.GridComponent.transform.position;
            float distance = (hitPoint - gridCirclePos).magnitude;
            float distAlongHelix = (float) Math.Sqrt(distance * distance - RADIUS * RADIUS);
            int nucleotideIndex = Mathf.RoundToInt(distAlongHelix / RISE);
            int binIndex = nucleotideIndex / 64;

            AssignCollidersToHelix(helix, helix.NucleotideMatricesA, 1, binIndex, rayPoolIndex);
            AssignCollidersToHelix(helix, helix.NucleotideMatricesB, 0, binIndex, rayPoolIndex);
        }
    }

    private void AssignCollidersToHelix(Helix helix, List<Matrix4x4> matrices, int direction, int binIndex, int poolIdx)
    {
        int start = binIndex * 64;
        int end = Mathf.Min(start + 64, matrices.Count);

        for (int i = start; i < end; i++)
        {
            if (i < 0 || i >= matrices.Count)
            {
                Debug.Log($"[i] {i} out of range. matrices size: {matrices.Count}");
            }
            Matrix4x4 localMat = matrices[i];
            Matrix4x4 worldMat = helix.GetCurrentOffset() * localMat;
            Vector3 pos = worldMat.GetColumn(3);

            if (_poolIndex >= _colliderPool.Count || _poolIndex < 0)
                ExpandPool(_colliderPool.Count);

            var col = _colliderPool[_poolIndex];
            col.gameObject.SetActive(true);
            col.transform.position = pos;
            col.Setup(helix, i, direction, -1);
            _poolIndex++;
        }
    }

    /// <summary>
    /// Expand the pool by at least 'amount' more colliders (or double if you like).
    /// If 'amount' is the current count, that means doubling the pool size.
    /// </summary>
    private void ExpandPool(int amount)
    {
        int currentCount = _colliderPool.Count;
        int newCount = currentCount + amount;  // "double" approach

        for (int i = currentCount; i < newCount; i++)
        {
            GameObject colObj = Instantiate(colliderPrefab);
            colObj.SetActive(false);
            _colliderPool.Add(colObj.GetComponent<NucleotideColliderComponent>());
        }
    }
}