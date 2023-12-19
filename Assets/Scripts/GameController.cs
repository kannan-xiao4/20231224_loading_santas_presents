using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<SystemSound> clipList;

    [SerializeField] private Canvas titleCanvas;
    [SerializeField] private Canvas inGameCanvas;

    [SerializeField] private Button startGameButton;
    [SerializeField] private Button gotoTitleButton;
    [SerializeField] private Button restartGameButton;

    [SerializeField] private TMP_Text recipeObjectiveText;
    [SerializeField] private TMP_Text processObjectiveText;
    [SerializeField] private TMP_Text processingText;
    [SerializeField] private TMP_Text resultText;

    [SerializeField] private TMP_Text okSelectResultImage;
    [SerializeField] private TMP_Text ngSelectResultImage;

    private GameObject cookedObject;

    private void Awake()
    {
        startGameButton.onClick.AddListener(() => { GameLoop().Forget(); });
        restartGameButton.onClick.AddListener(() => { GameLoop().Forget(); });
        gotoTitleButton.onClick.AddListener(() =>
        {
            titleCanvas.gameObject.SetActive(true);
            inGameCanvas.gameObject.SetActive(false);
        });
    }

    private async UniTaskVoid GameLoop()
    {
        if (cookedObject != null)
        {
            DestroyImmediate(cookedObject);
        }

        titleCanvas.gameObject.SetActive(false);
        inGameCanvas.gameObject.SetActive(true);
        resultText.gameObject.SetActive(false);
        gotoTitleButton.gameObject.SetActive(false);
        restartGameButton.gameObject.SetActive(false);

        okSelectResultImage.gameObject.SetActive(true);
        PlaySE(Audio.Success);
        await UniTask.Delay(500);
        okSelectResultImage.gameObject.SetActive(false);

        okSelectResultImage.gameObject.SetActive(true);
        PlaySE(Audio.Success);
        await UniTask.Delay(500);
        okSelectResultImage.gameObject.SetActive(false);

        processObjectiveText.gameObject.SetActive(false);

        await PlayTextAnimation($"Prepareing", processingText, playZoom: false, playMove: true);
        processingText.gameObject.SetActive(false);

        await PlayTextAnimation($"Making", processObjectiveText);
        processObjectiveText.gameObject.SetActive(false);

        resultText.gameObject.SetActive(true);
        gotoTitleButton.gameObject.SetActive(true);
        restartGameButton.gameObject.SetActive(true);
    }

    private void PlaySE(Audio type)
    {
        var sound = clipList.First(x => x.type == type);
        if (sound != null)
        {
            audioSource.PlayOneShot(sound.clip);
        }
    }

    private async UniTask PlayTextAnimation(string prcessText, TMP_Text target, bool playZoom = true, bool playMove = true)
    {
        target.text = prcessText;
        target.gameObject.SetActive(true);
        var animator = target.GetComponent<Animator>();
        animator.Play("Default");

        if (playZoom)
        {
            animator.Play("TextZoom");
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        }

        if (playMove)
        {
            animator.Play("MoveToLeft");
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        }
    }
}