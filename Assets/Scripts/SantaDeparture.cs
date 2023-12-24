using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class SantaDeparture : MonoBehaviour
{
    [SerializeField] private GameObject sledgePrefab;
    [SerializeField] private int targetMin = 3;
    [SerializeField] private int targetMax = 7;

    public Action<long> OnCurrentTotalPresentCount;
    public Action<string> OnUpdateCurrentPresentCount;
    public Action OnCompleteObject;
    private Dictionary<DragAndDropObject.Size, uint> currentObjective;
    private GameObject currentSledge;

    public void SetActiveSantaPrefab(bool isActive)
    {
        ClearCurrentState();
        sledgePrefab.SetActive(isActive);
    }

    public async UniTask StartLoop(float countingSeconds, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            currentObjective = CreateObjective();
            currentSledge = Instantiate(sledgePrefab);
            currentSledge.SetActive(true);

            await UniTask.WhenAll(
                WaitForCondition(countingSeconds, token),
                PlayAnimation("Arrival", currentSledge)
            );
            if (token.IsCancellationRequested)
            {
                ClearCurrentState();
                return;
            }

            var sledgeOnPresents = GameController.Instance.OnSledgePresets;
            foreach(var item in sledgeOnPresents)
            {
                item.Rigid.isKinematic = false;
                item.transform.SetParent(currentSledge.transform);
            }

            await PlayAnimation("Departure", currentSledge);
            if (token.IsCancellationRequested)
            {
                ClearCurrentState();
                return;
            }

            foreach (var item in sledgeOnPresents)
            {
                DestroyImmediate(item.gameObject);
            }

            OnCompleteObject?.Invoke();
            ClearCurrentState();
            await UniTask.Yield(cancellationToken: token);
        }
    }

    private async UniTask WaitForCondition(float duration, CancellationToken token)
    {
        float elapsedTime = 0f;
        var wait = TimeSpan.FromMilliseconds(100);
        var lastPlayText = 0;

        while (elapsedTime < duration)
        {
            if (CheckObjectiveComplete())
            {
                elapsedTime += (float) wait.TotalMilliseconds / 1000 * 2;
                var r = duration - elapsedTime;
                var newText = Mathf.RoundToInt(r) + 1;
                if(newText != lastPlayText)
                {
                    GameController.Instance.PlayTextAnimation(newText.ToString(), "CountingStart");
                    lastPlayText = newText;
                }
            }
            else
            {
                elapsedTime = 0f;
                GameController.Instance.PlayTextAnimation("", "CountingDefault");
                lastPlayText = 0;
            }

            await UniTask.Delay(wait, cancellationToken: token);
        }
        GameController.Instance.PlayTextAnimation("", "CountingDefault");
    }

    private bool CheckObjectiveComplete()
    {
        if (currentObjective == null)
        {
            return false;
        }

        var sledgeOnPresents = GameController.Instance.OnSledgePresets;
        var currentOnSmall = sledgeOnPresents.Count(x => x.Type == DragAndDropObject.Size.Small);
        var currentOnMedium = sledgeOnPresents.Count(x => x.Type == DragAndDropObject.Size.Medium);
        var currentOnLarge = sledgeOnPresents.Count(x => x.Type == DragAndDropObject.Size.Large);
        var targetOnSmall = currentObjective[DragAndDropObject.Size.Small];
        var targetOnMedium = currentObjective[DragAndDropObject.Size.Medium];
        var targetOnLarge = currentObjective[DragAndDropObject.Size.Large];

        var builder = new StringBuilder();
        builder.AppendLine($"{currentOnSmall}/{targetOnSmall}");
        builder.AppendLine($"{currentOnMedium}/{targetOnMedium}");
        builder.AppendLine($"{currentOnLarge}/{targetOnLarge}");
        OnUpdateCurrentPresentCount?.Invoke(builder.ToString());
        OnCurrentTotalPresentCount?.Invoke(currentObjective.Sum(x => x.Value));

        return currentOnSmall == targetOnSmall 
            && currentOnMedium == targetOnMedium 
            && currentOnLarge == targetOnLarge;
    }

    private void ClearCurrentState()
    {
        currentObjective = null;
        if (currentSledge != null )
        {
            currentSledge.SetActive(false);
            Destroy(currentSledge.gameObject);
            currentSledge = null;
        }
    }

    private Dictionary<DragAndDropObject.Size, uint> CreateObjective()
    {
        var count = (uint) Enum.GetValues(typeof(DragAndDropObject.Size)).Length;
        var total = (uint) UnityEngine.Random.Range(targetMin, targetMax);
        var pattern = GetOnePartition(total, count);
        return new Dictionary<DragAndDropObject.Size, uint>
        {
            { DragAndDropObject.Size.Small, pattern[0] },
            { DragAndDropObject.Size.Medium, pattern[1] },
            { DragAndDropObject.Size.Large, pattern[2] },
        };
    }

    private static uint[] GetOnePartition(uint total, uint divid)
    {
        var result = new uint[divid];
        uint remaing = total;
        for (int i = 0; i < divid - 1; i++)
        {
            result[i] = (uint) UnityEngine.Random.Range(0, remaing);
            remaing -= result[i];
        }
        result[divid - 1] = remaing;
        return result;
    }

    private static async UniTask PlayAnimation(string animationName, GameObject target)
    {
        var animator = target.GetComponent<Animator>();
        animator.Play(animationName);
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
    }
}