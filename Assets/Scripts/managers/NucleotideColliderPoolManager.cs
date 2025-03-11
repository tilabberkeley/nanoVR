using System.Collections.Generic;
using UnityEngine;

public class NucleotideColliderPoolManager : MonoBehaviour
{
    [Header("References")]
    public HelixManager helixManager;
    public GameObject colliderPrefab;

    [Header("Settings")]
    public int poolSize = 256;
    public float interactionRadius = 1f;
    public Transform player;

    private List<NucleotideColliderComponent> _colliderPool = new List<NucleotideColliderComponent>();

    private int _poolIndex;

    void Start()
    {
        // Create the initial pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject colObj = Instantiate(colliderPrefab);
            colObj.SetActive(false);

            // Keep track of the script reference
            _colliderPool.Add(colObj.GetComponent<NucleotideColliderComponent>());
        }
    }

    void Update()
    {
        // For this frame, keep track of how many colliders we've used so far
        _poolIndex = 0;

        // Go through each Helix
        foreach (Helix helix in helixManager.helixInstances)
        {
            AssignCollidersToHelix(helix, helix.NucleotideMatricesA, 1);
            AssignCollidersToHelix(helix, helix.NucleotideMatricesB, 0);
        }

        // Deactivate any leftover colliders that we didn't use this frame
        for (int i = _poolIndex; i < _colliderPool.Count; i++)
        {
            if (_colliderPool[i].gameObject.activeSelf)
            {
                _colliderPool[i].gameObject.SetActive(false);
            }
        }
    }

    private void AssignCollidersToHelix(Helix helix, List<Matrix4x4> matrices, int direciton)
    {
        for (int i = 0; i < matrices.Count; i++)
        {
            Matrix4x4 localMat = matrices[i];
            Matrix4x4 worldMat = helixManager.CurrentOffset * localMat;

            Vector3 position = worldMat.GetColumn(3); // the translation

            float distSqr = (player.position - position).sqrMagnitude;
            float radiusSqr = interactionRadius * interactionRadius;
            if (distSqr < radiusSqr)
            {
                // We want a real collider here
                if (_poolIndex < _colliderPool.Count)
                {
                    NucleotideColliderComponent nc = _colliderPool[_poolIndex];
                    GameObject colObj = nc.gameObject;
                    if (!colObj.activeSelf) colObj.SetActive(true);

                    // Position the pooled collider to match this instance
                    colObj.transform.position = position;

                    // Setup references so we know which helix/instance
                    nc.Setup(helix, i, /* direction= */ 1);

                    _poolIndex++;
                }
                else
                {
                    // Pool is exhausted. You could skip or dynamically expand
                    Debug.Log("Ran out of colliders in the pool!");
                }
            }
        }
    }
}
