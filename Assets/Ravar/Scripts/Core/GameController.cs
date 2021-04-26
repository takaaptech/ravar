using System.Collections;
using Itsdits.Ravar.Battle;
using Itsdits.Ravar.Character;
using Itsdits.Ravar.Core.Signal;
using Itsdits.Ravar.Levels;
using Itsdits.Ravar.Monster;
using Itsdits.Ravar.Monster.Condition;
using Itsdits.Ravar.Util;
using UnityEngine;

namespace Itsdits.Ravar.Core
{
    /// <summary>
    /// Static controller for game state management.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        /// <summary>
        /// Static instance of the game controller.
        /// </summary>
        public static GameController Instance { get; private set; }

        [Tooltip("GameObject that holds the PlayerController component.")]
        [SerializeField] private PlayerController _playerController;
        [Tooltip("GameObject that holds the BattleSystem component.")]
        [SerializeField] private BattleSystem _battleSystem;
        [Tooltip("The world camera that is attached to the Player GameObject.")]
        [SerializeField] private Camera _worldCamera;

        private BattlerController _battler;
        private GameState _state;
        private GameState _prevState;

        private void Awake()
        {
            Instance = this;
            ConditionDB.Init();
        }

        private void Start()
        {
            GameSignals.GAME_PAUSE.AddListener(OnPause);
            GameSignals.GAME_RESUME.AddListener(OnResume);
            GameSignals.GAME_QUIT.AddListener(OnQuit);
            GameSignals.PORTAL_ENTER.AddListener(OnPortalEnter);
            GameSignals.PORTAL_EXIT.AddListener(OnPortalExit);
            GameSignals.DIALOG_OPEN.AddListener(OnDialogOpen);
            GameSignals.DIALOG_CLOSE.AddListener(OnDialogClose);
            GameSignals.BATTLE_LOS.AddListener(OnBattlerEncounter);
            GameSignals.BATTLE_START.AddListener(OnBattleStart);
            //_battleSystem.OnBattleOver += EndBattle;
        }
        
        private void OnDestroy()
        {
            GameSignals.GAME_PAUSE.RemoveListener(OnPause);
            GameSignals.GAME_RESUME.RemoveListener(OnResume);
            GameSignals.GAME_QUIT.RemoveListener(OnQuit);
            GameSignals.PORTAL_ENTER.RemoveListener(OnPortalEnter);
            GameSignals.PORTAL_EXIT.RemoveListener(OnPortalExit);
            GameSignals.DIALOG_OPEN.RemoveListener(OnDialogOpen);
            GameSignals.DIALOG_CLOSE.RemoveListener(OnDialogClose);
            GameSignals.BATTLE_LOS.RemoveListener(OnBattlerEncounter);
            GameSignals.BATTLE_START.RemoveListener(OnBattleStart);
            //_battleSystem.OnBattleOver -= EndBattle;
        }

        private void Update()
        {
            if (_state == GameState.World)
            {
                _playerController.HandleUpdate();
            }
            else if (_state == GameState.Battle)
            {
                _battleSystem.HandleUpdate();
            }
        }

        /// <summary>
        /// Starts a battle with a wild monster after Encounter collider is triggered.
        /// </summary>
        public void StartWildBattle()
        {
            _state = GameState.Battle;
            _battleSystem.gameObject.SetActive(true);
            _worldCamera.gameObject.SetActive(false);

            var playerParty = _playerController.GetComponent<MonsterParty>();
            //TODO - refactor the way we handle this. maybe a dictionary with the scenes and map areas in it?
            MonsterObj wildMonster = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomMonster();
            var enemyMonster = new MonsterObj(wildMonster.Base, wildMonster.Level);

            _battleSystem.StartWildBattle(playerParty, enemyMonster);
        }

        private void OnBattlerEncounter(BattlerEncounter encounter)
        {
            _state = GameState.Cutscene;
            StartCoroutine(encounter.Battler.TriggerEncounter(_playerController));
        }

        private void OnBattleStart(BattlerEncounter encounter)
        {
            _state = GameState.Battle;
        }

        private void OnBattleFinish()
        {
            _state = GameState.World;
        }

        /// <summary>
        /// Start a battle with enemy character.
        /// </summary>
        /// <param name="battler">Character to do battle with.</param>
        public void StartCharBattle(BattlerController battler)
        {
            _state = GameState.Battle;
            _battleSystem.gameObject.SetActive(true);
            _worldCamera.gameObject.SetActive(false);

            _battler = battler;
            var playerParty = _playerController.GetComponent<MonsterParty>();
            var battlerParty = battler.GetComponent<MonsterParty>();

            _battleSystem.StartCharBattle(playerParty, battlerParty);
        }

        private void OnPause(bool pause)
        {
            _prevState = _state;
            _state = GameState.Pause;
            Time.timeScale = 0;
            StartCoroutine(SceneLoader.Instance.LoadSceneNoUnload("UI.Popup.Pause", true));
        }
        
        private void OnResume(bool resume)
        {
            _state = _prevState;
            _prevState = GameState.Menu;
            Time.timeScale = 1;
            StartCoroutine(SceneLoader.Instance.UnloadScene("UI.Popup.Pause", true));
        }

        private void OnQuit(bool quit)
        {
            _state = GameState.Menu;
            StartCoroutine(SceneLoader.Instance.UnloadWorldScenes());
            StartCoroutine(SceneLoader.Instance.LoadScene("UI.Menu.Main"));
        }

        private void OnPortalEnter(bool entered)
        {
            _prevState = _state;
            _state = GameState.Cutscene;
        }

        private void OnPortalExit(bool exited)
        {
            _state = _prevState;
        }
        
        private void OnDialogOpen(DialogItem dialog)
        {
            _state = GameState.Dialog;
            StartCoroutine(ShowDialog(dialog));
        }

        private void OnDialogClose(string speakerName)
        {
            _state = GameState.World;
            StartCoroutine(SceneLoader.Instance.UnloadScene("UI.Popup.Dialog", true));
        }

        private IEnumerator ShowDialog(DialogItem dialog)
        {
            // We need to wait until the UI.Popup.Dialog scene is loaded before we can dispatch the signal, otherwise
            // the DialogController won't be enabled to listen for it.
            yield return SceneLoader.Instance.LoadSceneNoUnload("UI.Popup.Dialog", true);
            yield return YieldHelper.END_OF_FRAME;
            GameSignals.DIALOG_SHOW.Dispatch(dialog);
        }

        private void EndBattle(BattleResult result, bool isCharBattle)
        {
            _state = GameState.World;
            if (_battler != null && result == BattleResult.Won)
            {
                _battler.SetBattlerState(BattlerState.Defeated);
                _battler = null;
            }
            else if (_battler != null && result == BattleResult.Lost)
            {
                //TODO - handle a loss
            }
            else
            {
                //TODO - handle error
            }

            _battleSystem.gameObject.SetActive(false);
            _worldCamera.gameObject.SetActive(true);
        }
    }
}