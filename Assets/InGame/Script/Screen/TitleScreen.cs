//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class TitleScreen : ScreenBase
{
    [SerializeField] private Button _titleButton;
    [FormerlySerializedAs("_testButton")]
    [SerializeField] private Button _deckBuildButton;
    [SerializeField] private Button _shopButton;
    [SerializeField] private Text _stoneText;

    [Header("ランの結果(負けて戻ってきたときに出す)")]
    [SerializeField] private CanvasGroup _rewardPopup;
    [SerializeField] private Text _rewardBodyText;
    [SerializeField] private Button _rewardOkButton;

    private TransitionService _transitionService;
    private IWalletRepository _walletRepository;

    [VContainer.Inject]
    public void Construct(TransitionService transitionService, IWalletRepository walletRepository)
    {
        _transitionService = transitionService;
        _walletRepository = walletRepository;
    }

    private void Start()
    {
        _rewardPopup.gameObject.SetActive(false);
        _stoneText.text = "";

        _titleButton.onClick.AddListener(() =>
        {
            //_transitionService.BattleStart().Forget();
            Transition(_transitionService.StageSelectScreen).Forget();
        });

        _deckBuildButton.onClick.AddListener(() =>
        {
            Transition(_transitionService.DeckBuildScreen).Forget();
        });

        _shopButton.onClick.AddListener(() =>
        {
            Transition(_transitionService.ShopScreen).Forget();
        });

        _rewardOkButton.onClick.AddListener(CloseRewardPopup);
        _walletRepository.OnGachaStoneChanged += UpdateStoneText;
        ShowWallet().Forget();
    }

    private void UpdateStoneText(int stone)
    {
        _stoneText.text = $"ガチャ石 {stone}";
    }

    /// <summary>
    /// ガチャ石の所持数と、直前に終わったランの報酬を表示する
    /// </summary>
    private async UniTask ShowWallet()
    {
        try
        {
            await _walletRepository.LoadData();
            if (this == null) return;
            UpdateStoneText(_walletRepository.GachaStone);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            if (this == null) return;
            _stoneText.text = "ガチャ石 ?";
        }

        var reward = _walletRepository.PendingRunReward;
        if (reward == null) return;
        _walletRepository.ClearPendingRunReward();
        _rewardBodyText.text = reward.IsReceived
            ? $"DAY {reward.Day} で力尽きた…\n{reward.AdvancedDays}日進んだので\n<color=#FFCC59>ガチャ石 +{reward.GachaStone}</color>"
            : $"DAY {reward.Day} で力尽きた…\n<color=#FF6666>ガチャ石 +{reward.GachaStone} を保存できませんでした</color>";
        _rewardPopup.gameObject.SetActive(true);
        _rewardPopup.alpha = 0f;
        _rewardPopup.transform.localScale = Vector3.one * 0.9f;
        DOTween.Sequence()
            .Join(_rewardPopup.DOFade(1f, 0.2f))
            .Join(_rewardPopup.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
            .SetLink(gameObject);
    }

    private void CloseRewardPopup()
    {
        _rewardPopup.DOKill();
        _rewardPopup.DOFade(0f, 0.15f).OnComplete(() => _rewardPopup.gameObject.SetActive(false)).SetLink(gameObject);
    }

    /// <summary>
    /// 連打で遷移が重ならないよう、遷移中はボタンを押せなくする。失敗したら押せるように戻す
    /// </summary>
    private async UniTask Transition(System.Func<UniTask> transition)
    {
        if (!_titleButton.interactable || !_deckBuildButton.interactable || !_shopButton.interactable) return;
        SetButtonsInteractable(false);
        try
        {
            await transition();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            if (this != null) SetButtonsInteractable(true);
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        _titleButton.interactable = interactable;
        _deckBuildButton.interactable = interactable;
        _shopButton.interactable = interactable;
    }
    public void PopExit()
    {

    }

    public void PushEnter()
    {

    }

    private void OnDestroy()
    {
        if (_walletRepository != null) _walletRepository.OnGachaStoneChanged -= UpdateStoneText;
        AssetLoader.Release(ResourceKey.Prefabs.TitleScreen);
    }
}
