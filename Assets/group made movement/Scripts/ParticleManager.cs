using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [System.Serializable]
    public class ParticleEntry
    {
        public string name;        // Name used in code
        public GameObject prefab;  // The particle prefab
    }

    public List<ParticleEntry> particles;
    private Dictionary<string, GameObject> particleDict;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        particleDict = new Dictionary<string, GameObject>();

        foreach (var p in particles)
        {
            if (!particleDict.ContainsKey(p.name))
                particleDict.Add(p.name, p.prefab);
        }
    }

    /// Spawns a particle effect at a world position.
    /// Automatically destroys the effect when finished.
    public GameObject Play(string particleName, Vector3 position)
    {
        if (!particleDict.TryGetValue(particleName, out GameObject prefab))
        {
            Debug.LogWarning($"No particle named '{particleName}' found!");
            return null;
        }

        GameObject fx = Instantiate(prefab, position, Quaternion.identity);

        // Auto-destroy when the particle is done emitting
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(fx, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        return fx;
    }
}
