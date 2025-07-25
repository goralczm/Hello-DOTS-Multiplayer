using System.Collections;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class ConnectionCanvas : MonoBehaviour
{
    [SerializeField] private GameObject _connectionPanel;

    private World _clientWorld;

    private void Start()
    {
        foreach (World world in World.All)
        {
            if (world.IsClient())
            {
                _clientWorld = world;
                break;
            }
        }

        if (_clientWorld != null)
            StartCoroutine(WaitUnitlConnected());
    }

    private IEnumerator WaitUnitlConnected()
    {
        EntityManager entityManager = _clientWorld.EntityManager;

        while (true)
        {
            EntityQuery query = entityManager.CreateEntityQuery(typeof(NetworkId));
            if (!query.IsEmpty)
            {
                _connectionPanel.SetActive(false);
                yield break;
            }

            yield return null;
        }
    }
}
