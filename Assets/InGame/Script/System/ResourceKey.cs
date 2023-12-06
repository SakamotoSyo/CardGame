//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceKey
{
    private const string PlayerFolder = "Player";
    public static class Prefabs
    {
        private const string Prefix = "Prefab";
        private const string UiFolder = "UIPrefab";
        private const string AnimFolder = "AnimPrefab";
        private const string Screen = "ScreenPrefab";

        public const string PlayerView = UiFolder + "/" + Prefix + "PlayerView.prefab";
        public const string EnemyView = UiFolder + "/" + Prefix + "EnemyView.prefab";
        public const string Card = UiFolder + "/" + Prefix + "Card.prefab";
        public const string CardField = UiFolder + "/" + Prefix + "CardField.prefab";
        public const string MainBattleScreen = Screen + "/" + Prefix + "BattleMainScreen.prefab";
        public const string TitleScreen = Screen + "/" + Prefix + "TitleScreen.prefab";
        public const string StateTrun = AnimFolder + "/" + Prefix + "StateTurn.prefab";
    }

    public static class AnimPrefab 
    {

    }

    public static class MasterData
    {
        private const string Prefix = "MasterData";
        public const string PlayerInitStatus = Prefix + "/" + PlayerFolder + "/" + "InitialStatus.asset";
        public const string EffectMasterTableAsset = Prefix + "/" + "EffectMasterTableAsset.asset";
    }


    //public const string OthelloBoard = OthelloFolder + "/" + Prefix + "OthelloBoard.prefab";
    //public const string TimeSlider = UiFolder + "/" + Prefix + "TimeSlider.prefab";
}
