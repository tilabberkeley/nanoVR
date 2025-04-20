using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NucleotideColliderPoolManager : MonoBehaviour
{
    [Header("References")]
    public HelixManager helixManager;
    public GameObject colliderPrefab;

    [Header("Settings")]
    public float interactionRadius = 1f;
    public Transform player;

    // Pool
    [Tooltip("Initial size of the collider pool.")]
    public int initialPoolSize = 256;

    // If we use the entire pool, we double it.
    // If we use < 75% after a full pass, we shrink it to half.
    private List<NucleotideColliderComponent> _colliderPool = new List<NucleotideColliderComponent>();
    private int _poolIndex;

    // Chunking
    [Tooltip("Number of helices to process per frame (to avoid big frame spikes).")]
    public int helicesPerFrame = 5;

    void Start()
    {
        // Create the initial pool
        ExpandPool(initialPoolSize);

        // Start a coroutine that continuously updates colliders every X seconds
        StartCoroutine(ColliderAssignmentLoop());
    }

    /// <summary>
    /// Builds a snapshot of all helices from s_helixDict and processes them in chunks.
    /// After a full pass, it re-checks the dictionary to immediately handle new/removed helices.
    /// </summary>
    private IEnumerator SnapshotColliderAssignmentLoop()
    {
        while (true)
        {
            // Make a snapshot array of the current helix set from the dictionary
            // so if the dictionary is modified mid-iteration, we won't get errors.
            Helix[] helixArray = new List<Helix>(GlobalVariables.s_helixDict.Values).ToArray();

            int currentIndex = 0;
            int total = helixArray.Length;

            if (total == 0)
            {
                // No helices => yield so we don't freeze
                yield return null;
                continue;
            }

            // We'll do a "full pass" over this snapshot,
            // chunking by helicesPerFrame each frame.
            while (currentIndex < total)
            {
                // Reset pool usage at the *start* of each frame chunk
                _poolIndex = 0;

                // End index for this chunk
                int endIndex = Mathf.Min(currentIndex + helicesPerFrame, total);

                // Process each helix in [currentIndex, endIndex)
                for (int i = currentIndex; i < endIndex; i++)
                {
                    Helix helix = helixArray[i];

                    // Bounding box test
                    if (helix.BoundingBox.IntersectsSphere(player.position, interactionRadius))
                    {
                        // Assign colliders to nucleotides in range
                        AssignCollidersToHelix(helix, helix.NucleotideMatricesA, 1);
                        AssignCollidersToHelix(helix, helix.NucleotideMatricesB, 0);
                    }
                }

                // Move index forward
                currentIndex = endIndex;

                // Deactivate leftover colliders not used in this chunk
                for (int j = _poolIndex; j < _colliderPool.Count; j++)
                {
                    if (_colliderPool[j].gameObject.activeSelf)
                    {
                        _colliderPool[j].gameObject.SetActive(false);
                    }
                }

                // Yield to next frame so we don't do all chunks in one frame
                yield return null;
            }

            ManagePoolSize();
            yield return null;
        }
    }

    /// <summary>
    /// This coroutine replaces the logic that was in Update().
    /// It runs in a loop, waits some time, then runs again.
    /// </summary>
    IEnumerator ColliderAssignmentLoop()
    {
        // Option 2 approach: run the entire collider assignment,
        // then yield for a fixed wait, then repeat.
        while (true)
        {
            // 1) Reset pool index
            _poolIndex = 0;

            // 2) Assign colliders to helices in one pass
            foreach (Helix helix in GlobalVariables.s_helixDict.Values)
            {
                // Quick bounding box check:
                if (helix.BoundingBox.IntersectsSphere(player.position, interactionRadius))
                {
                    AssignCollidersToHelix(helix, helix.NucleotideMatricesA, 1);
                    AssignCollidersToHelix(helix, helix.NucleotideMatricesB, 0);
                }
            }

            // 3) Deactivate leftover colliders that aren't used this pass
            for (int i = _poolIndex; i < _colliderPool.Count; i++)
            {
                if (_colliderPool[i].gameObject.activeSelf)
                {
                    _colliderPool[i].gameObject.SetActive(false);
                }
            }

            // 4) Wait 0.1s before the next collider assignment cycle
            //    (Adjust the wait time as you like.)
            yield return new WaitForSeconds(0.1f);
        }
    }

    //void Update()
    //{
    //    // For this frame, keep track of how many colliders we've used so far
    //    _poolIndex = 0;

    //    // Go through each Helix
    //    foreach (Helix helix in GlobalVariables.s_helixDict.Values)
    //    {
    //        if (helix.BoundingBox.IntersectsSphere(player.position, interactionRadius))
    //        {
    //            AssignCollidersToHelix(helix, helix.NucleotideMatricesA, 1);
    //            AssignCollidersToHelix(helix, helix.NucleotideMatricesB, 0);
    //        }
    //    }

    //    // Deactivate any leftover colliders that we didn't use this frame
    //    for (int i = _poolIndex; i < _colliderPool.Count; i++)
    //    {
    //        if (_colliderPool[i].gameObject.activeSelf)
    //        {
    //            _colliderPool[i].gameObject.SetActive(false);
    //        }
    //    }
    //}

    /// <summary>
    /// Assigns colliders to the given helix's nucleotides if they are in range of the player.
    /// </summary>
    /// <summary>
    /// Assign colliders to nucleotides if they are in range of the player.
    /// If we run out of colliders, we expand the pool immediately.
    /// </summary>
    private void AssignCollidersToHelix(Helix helix, List<Matrix4x4> matrices, int direction)
    {
        for (int i = 0; i < matrices.Count; i++)
        {
            Matrix4x4 localMat = matrices[i];
            Matrix4x4 worldMat = helix.CurrTransformOffset * localMat;
            Vector3 position = worldMat.GetColumn(3);

            float distSqr = (player.position - position).sqrMagnitude;
            float radiusSqr = interactionRadius * interactionRadius;

            if (distSqr < radiusSqr)
            {
                // We want a real collider
                if (_poolIndex >= _colliderPool.Count)
                {
                    // We have no colliders left => Expand the pool right now
                    ExpandPool(_colliderPool.Count); // double the current size
                }

                NucleotideColliderComponent nucleotideColliderComponent = _colliderPool[_poolIndex];
                GameObject colObj = nucleotideColliderComponent.gameObject;
                if (!colObj.activeSelf) colObj.SetActive(true);

                // Position the pooled collider
                colObj.transform.position = position;

                // Setup references
                nucleotideColliderComponent.Setup(helix, i, direction);

                _poolIndex++;
            }
        }
    }

    /// <summary>
    /// After a full pass, decide if we should shrink the pool based on usage.
    /// If we used fewer than 75% of colliders, shrink by half.
    /// Example threshold; adjust logic as desired.
    /// </summary>
    private void ManagePoolSize()
    {
        // How many colliders we actually used
        int usedCount = _poolIndex;
        int capacity = _colliderPool.Count;

        // if used is < 75% of capacity, shrink to half
        // e.g. used 128, capacity 256 => 128 < 192 => shrink => 128
        if (usedCount < capacity * 0.75f)
        {
            int newSize = capacity / 2;
            ShrinkPool(newSize);
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

    /// <summary>
    /// Shrink the pool down to newSize by removing GameObjects from the end
    /// that are presumably not in use (deactivated).
    /// </summary>
    private void ShrinkPool(int newSize)
    {
        int currentCount = _colliderPool.Count;
        if (newSize < 1) newSize = 1; // keep at least 1

        if (newSize >= currentCount)
        {
            // Nothing to do if the new size is bigger or equal
            return;
        }

        // remove from the end of the list
        for (int i = currentCount - 1; i >= newSize; i--)
        {
            var colComp = _colliderPool[i];
            if (colComp.gameObject.activeSelf)
            {
                // If it's active, disable it
                colComp.gameObject.SetActive(false);
            }
            Destroy(colComp.gameObject);
            _colliderPool.RemoveAt(i);
        }
    }
}