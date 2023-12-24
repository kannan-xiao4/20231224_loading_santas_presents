using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PresentInstantiater : MonoBehaviour
{
    [SerializeField] private GameObject groundObject;
    [SerializeField] private List<DragAndDropObject> presentsPrefab;
    [SerializeField] private Transform smallParent;
    [SerializeField] private Transform mediumParent;
    [SerializeField] private Transform largeParent;

    public IReadOnlyDictionary<GameObject, DragAndDropObject> PresnetMap => instantiatePresents;
    private readonly Dictionary<GameObject, DragAndDropObject> instantiatePresents = new();

    private int poolPresentsSize = -1;
    private bool isCreating = false;

    private List<Vector3> targets = new()
    {
        Vector3.back,
        Vector3.zero,
        Vector3.one,
        Vector3.right,
        Vector3.left,
        Vector3.forward,
        Vector3.up,
        Vector3.down,
    };

    private void OnDestroy()
    {
        StopCreateAndClearPresent();
    }

    public void SetPoolSize(int poolSize)
    {
        poolPresentsSize = poolSize;
    }

    public void SetActiveGround(bool isActive)
    {
        groundObject.SetActive(isActive);
    }

    public void StartCreatePresent()
    {
        if (isCreating)
        {
            return;
        }

        isCreating = true;
        KeepPresentPoolTask().Forget();
    }

    private async UniTaskVoid KeepPresentPoolTask()
    {
        while (isCreating)
        {
            if (instantiatePresents.Count <= poolPresentsSize)
            {
                var createNum = poolPresentsSize - instantiatePresents.Count;
                await CreatePresents(Mathf.Min(5, createNum));
            }

            if (poolPresentsSize < 0)
            {
                await CreatePresents(1);
            }
            await UniTask.DelayFrame(10);
        }
    }

    private async UniTask CreatePresents(int num)
    {
        for (int i = 0; i < num; i++)
        {
            if (!isCreating)
            {
                return;
            }

            var j = Random.Range(0, presentsPrefab.Count);
            var ddobj = Instantiate(presentsPrefab[j]);
            switch (ddobj.Type)
            {
                case DragAndDropObject.Size.Small:
                    ddobj.transform.SetParent(smallParent, false);
                    break;
                case DragAndDropObject.Size.Medium:
                    ddobj.transform.SetParent(mediumParent, false);
                    break;
                case DragAndDropObject.Size.Large:
                    ddobj.transform.SetParent(largeParent, false);
                    break;
            }
            var id = ddobj.gameObject;
            ddobj.DestroySelf = () => { instantiatePresents.Remove(id); };
            instantiatePresents[id] = ddobj;
            var target = targets[Random.Range(0, targets.Count - 1)];
            var forceDirection = (target * 3 - ddobj.transform.position).normalized;
            ddobj.Rigid.AddForce(forceDirection * ddobj.Rigid.mass * 3, ForceMode.Impulse);
            await UniTask.WaitForSeconds(0.1f, delayTiming: PlayerLoopTiming.FixedUpdate) ;
        }
    }

    public void StopCreateAndClearPresent()
    {
        isCreating = false;
        foreach (var go in instantiatePresents.Values.Where(x => x != null))
        {
            DestroyImmediate(go.gameObject);
        }
        instantiatePresents?.Clear();
    }
}