using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

/// <summary>
/// �X�e�[�W��I������ۂɃJ�[�h��Image�Ȃǂ𓖂Ă͂߂�N���X
/// </summary>
public class StageMapView : MonoBehaviour
{
    public event Action<StageType, MapPosData> OnCellClick;

    [Tooltip("�}�b�v��List�̉��Ԗڂ�K�������邩")]
    [SerializeField] private int _mapListSelectNum;
    [Tooltip("�X�e�[�W��Sprite���i�[����")]
    [SerializeField] private Sprite[] _spriteArray;
    [Tooltip("�X�e�[�W������͂���")]
    [SerializeField] private string[] _stageInfo;

    private MapPosData _mapPos;

    public void SetUp(MapPosData mapPosition)
    {
        var cardArray = GetComponentsInChildren<StageSelectCard>();
        _mapPos = mapPosition;
        SetCardView(cardArray);
    }

    /// <summary>
    /// �_���W�����̃}�b�v���擾���J�[�h�̌����ڂ�ݒ肷��
    /// </summary>
    /// <param name="cardArray"></param>
    /// <param name="cardInfoText"></param>
    private void SetCardView(StageSelectCard[] cardArray)
    {
        int[,] dungeonMapData = GenerationDungeonMap.DungeonMapData;
        var splitNum = (cardArray.Length - 1) / 2;

        for (int i = 0; i < cardArray.Length; i++)
        {
            //�͈͊O����Ȃ�������
            if (ArrayOutOfRangeCheck(dungeonMapData, i - (splitNum - _mapPos.X), _mapPos.Y + _mapListSelectNum))
            {
                var surroundingsNum = dungeonMapData[_mapPos.Y + _mapListSelectNum, i - (splitNum - _mapPos.X)];

                cardArray[i].SetView(_spriteArray[surroundingsNum - 1], _stageInfo[surroundingsNum - 1]);
                //��ԑO�̃J�[�h��ł͂Ȃ��ꍇcontinue����
                if (_mapListSelectNum != 0) continue;
                //�J�[�h�Ɏ������ǂ̃X�e�[�W�̃J�[�h�Ȃ̂��̏���n��
                cardArray[i].SetStageType((StageType)surroundingsNum);
                cardArray[i].SetCardMapPos(i - (splitNum - _mapPos.X), _mapPos.Y + 1);
                cardArray[i].OnClick += OnCellClick;
            }
        }
    }

    /// <summary>
    /// �z��͈̔͊O���ǂ���Check����
    /// </summary>
    /// <param name="array"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private bool ArrayOutOfRangeCheck(int[,] array, int x, int y) 
    {
        if (array.GetLength(0) <= y || array.GetLength(1) <= x || x < 0 || y < 0) 
        {
            return false;
        }

        if (array[y, x] == 0) 
        {
            return false;
        }

        return true;
    }
}