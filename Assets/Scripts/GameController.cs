using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private static GameController s_instance;
    public static GameController Instance => s_instance;

    [Header("System Component")]
    [SerializeField] private int poolPresentsSize = 50;
    [SerializeField] private float limitTimeSeconds;
    [SerializeField] private float countingSeconds;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> clipList;
    [SerializeField] private PresentInstantiater instantiater;
    [SerializeField] private InputController inputController;
    [SerializeField] private SantaDeparture santaDepature;

    [Space, Header("Title UI")]
    [SerializeField] private Canvas titleCanvas;
    [SerializeField] private Button startGameButton;

    [Space, Header("InGame UI")]
    [SerializeField] private Canvas inGameCanvas;
    [SerializeField] private TMP_Text loadingPresentTotalText;
    [SerializeField] private TMP_Text loadingPresentCountText;
    [SerializeField] private TMP_Text animationText;
    [SerializeField] private TMP_Text remaingTimeText;

    [Space, Header("Result UI")]
    [SerializeField] private Canvas resultCanvas;
    [SerializeField] private Button gotoTitleButton;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private TMP_Text resultText;

    public IReadOnlyDictionary<GameObject, DragAndDropObject> PresnetMap => instantiater.PresnetMap;
    public IEnumerable<DragAndDropObject> OnSledgePresets =>
        PresnetMap.Values.Where(x => x != null && x.gameObject.layer == GameSettings.SledgeLayer);

    private float currentTime = 0f;
    private int completeCount = 0;

    private void Awake()
    {
        if (s_instance != this && s_instance != null)
        {
            Destroy(s_instance.gameObject);
        }

        s_instance = this;

        startGameButton.onClick.AddListener(() => { GameLoop().Forget(); });
        restartGameButton.onClick.AddListener(() => { GameLoop().Forget(); });
        gotoTitleButton.onClick.AddListener(() =>
        {
            titleCanvas.gameObject.SetActive(true);
            inGameCanvas.gameObject.SetActive(false);
            resultCanvas.gameObject.SetActive(false);
            instantiater.SetPoolSize(-1);
            instantiater.StartCreatePresent();
            santaDepature.SetActiveSantaPrefab(true);
        });
        santaDepature.OnCurrentTotalPresentCount = (count) => { loadingPresentTotalText.text = count.ToString(); };
        santaDepature.OnUpdateCurrentPresentCount = (text) => { loadingPresentCountText.text = text; };
        santaDepature.OnCompleteObject = () => { completeCount++; };

        instantiater.SetPoolSize(-1);
        instantiater.StartCreatePresent();
        santaDepature.SetActiveSantaPrefab(true);
    }

    private async UniTaskVoid GameLoop()
    {
        titleCanvas.gameObject.SetActive(false);
        inGameCanvas.gameObject.SetActive(true);
        resultCanvas.gameObject.SetActive(false);
        instantiater.StopCreateAndClearPresent();

        currentTime = limitTimeSeconds;
        completeCount = 0;
        remaingTimeText.text = currentTime.ToString("F2");

        var cancelSource = new CancellationTokenSource();
        santaDepature.SetActiveSantaPrefab(false);
        santaDepature.StartLoop(countingSeconds, cancelSource.Token).Forget();
        instantiater.SetActiveGround(true);
        instantiater.SetPoolSize(poolPresentsSize);
        instantiater.StartCreatePresent();

        while (currentTime > 0f)
        {
            await UniTask.Yield();
            currentTime -= Time.deltaTime;
            remaingTimeText.text = currentTime.ToString("F2");
        }
        cancelSource.Cancel();
        cancelSource.Dispose();
        instantiater.StopCreateAndClearPresent();

        titleCanvas.gameObject.SetActive(false);
        inGameCanvas.gameObject.SetActive(false);
        resultCanvas.gameObject.SetActive(true);
        resultText.text = completeCount.ToString();
    }

    public void PlaySE()
    {
        var sound = clipList[Random.Range(0, clipList.Count)];
        if (sound != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }

    public void PlayTextAnimation(string text, string stateName)
    {
        var animator = animationText.GetComponent<Animator>();
        animationText.text = text;
        animator.Rebind();
        animator.Play(stateName, 0, 0);
    }
}