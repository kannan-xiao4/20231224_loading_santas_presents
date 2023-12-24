using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class DragAndDropObject : MonoBehaviour
{
    public enum Size
    {
        Small,
        Medium,
        Large,
    }

    public Action DestroySelf;
    public Rigidbody Rigid;
    public Size Type;

    private readonly HashSet<GameObject> collisionObjects = new();
    private bool isTouch = false;

    private void Start()
    {
        EnablePhysics();
    }

    public void EnablePhysics()
    {
        EnablePhysics(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask EnablePhysics(CancellationToken cancellationToken)
    {
        if (!Rigid.isKinematic)
        {
            return;
        }

        Rigid.isKinematic = false;
        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
        await UniTask.WaitUntil(() => Rigid == null || Rigid.velocity.magnitude < 0.001f, cancellationToken: cancellationToken);
        await UniTask.DelayFrame(10, cancellationToken: cancellationToken);
        Rigid.isKinematic = true;
    }

    private void FixedUpdate()
    {
        var target = isTouch ? GameSettings.SledgeLayer : GameSettings.DefaultLayer;
        if (gameObject.layer != target)
        {
            gameObject.layer = target;
        }

        if (transform.position.y < GameSettings.DestroyHeight)
        {
            DestroySelf?.Invoke();
            DestroyImmediate(gameObject);
        }
    }

    private void OnTriggerEnter(Collider colider)
    {
        collisionObjects.Add(colider.gameObject);
        isTouch = collisionObjects.Any(x => x != null && x.layer == GameSettings.SledgeLayer);
        EnablePhysics();
    }

    private void OnTriggerExit(Collider colider)
    {
        collisionObjects.Remove(colider.gameObject);
        isTouch = collisionObjects.Any(x => x != null && x.layer == GameSettings.SledgeLayer);
    }
}