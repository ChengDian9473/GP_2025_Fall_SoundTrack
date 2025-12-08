using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace SoundTrack{
    public class Info : MonoBehaviour
    {
        public static Info Instance { get; private set; }

        private VisualElement RootVisualElement;
        private VisualElement cover;
        private List<VisualElement> pages;

        private VisualElement tutorial_page;
        private VisualElement setting_page;
        private VisualElement end_page;

        private Label TutorialLabel;

        private Button StartButton;
        private Button SettingButton;
        private Button QuitButton;

        private List<Button> LevelButton;

        private Button QuitSettingButton;
        private Slider VolumeSlider;

        private Button MenuButton;
        private Button LevelSelectButton;
        private Button ReplayButton;
        private Button NextButton;
        
        private Label HPLabel;
        private VisualElement KeyContainer;
        private VisualElement StarContainer;

        [SerializeField] private Sprite keySprite;
        [SerializeField] private Sprite keySprite_black;

        [SerializeField] private Sprite starSprite;
        [SerializeField] private Sprite starSprite_black;
        
        private string[] tutorialLines;
        private int currentIndex = 0;
        private Coroutine typingCoroutine;
        private bool isTyping = false;

        private int current_scene;
        private int current_page;

        private static int LEVEL_MIN = 2;
        private static int LEVEL_MAX = 8;

        private bool firstTime;

        public event Action OnTutorialEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            firstTime = true;
        }

        private void OnEnable(){
            RootVisualElement = GetComponent<UIDocument>().rootVisualElement;
            cover = RootVisualElement.Q<VisualElement>("Cover");
            StartButton = RootVisualElement.Q<Button>("StartButton");
            SettingButton = RootVisualElement.Q<Button>("SettingButton");
            QuitButton = RootVisualElement.Q<Button>("QuitButton");

            LevelButton = RootVisualElement.Query<Button>("LevelButton").ToList();

            VolumeSlider = RootVisualElement.Q<Slider>("VolumeSlider");
            QuitSettingButton = RootVisualElement.Q<Button>("QuitSettingButton");

            MenuButton = RootVisualElement.Q<Button>("MenuButton");
            LevelSelectButton = RootVisualElement.Q<Button>("LevelSelectButton");
            ReplayButton = RootVisualElement.Q<Button>("ReplayButton");
            NextButton = RootVisualElement.Q<Button>("NextButton");

            HPLabel = RootVisualElement.Q<Label>("HPLabel");
            KeyContainer = RootVisualElement.Q<VisualElement>("KeyContainer");
            StarContainer = RootVisualElement.Q<VisualElement>("StarContainer");

            current_scene = 0;
            current_page = 0;

            RootVisualElement.style.display = DisplayStyle.Flex; // None or Flex
            cover.style.opacity = 1.0f;

            pages = new List<VisualElement>();

            pages.Add(RootVisualElement.Q<VisualElement>("MainMenu")); // Page 0
            pages.Add(RootVisualElement.Q<VisualElement>("LevelSelect")); // Page 1
            pages.Add(RootVisualElement.Q<VisualElement>("Level")); // Page 2

            tutorial_page = RootVisualElement.Q<VisualElement>("Tutorial");
            pages[2].RegisterCallback<ClickEvent>(OnClickAnywhere);
            TutorialLabel = RootVisualElement.Q<Label>("TutorialLabel");

            setting_page = RootVisualElement.Q<VisualElement>("Setting");
            end_page = RootVisualElement.Q<VisualElement>("End");
            
            tutorial_page.style.display = DisplayStyle.None;
            setting_page.style.display = DisplayStyle.None;
            end_page.style.display = DisplayStyle.None;

            foreach(var page in pages){
                page.style.display = DisplayStyle.None;
            }

            cover.RegisterCallback<TransitionEndEvent>(TransitionEnd);

            StartButton.clicked += StartButtonClicked;
            SettingButton.clicked += SettingButtonClicked;
            QuitButton.clicked += QuitButtonClicked;

            foreach(var btn in LevelButton){
                btn.clicked += () => LevelButtonClicked(btn);
            }

            VolumeSlider.RegisterValueChangedCallback(VolumeSliderChanged);
            QuitSettingButton.clicked += QuitSettingButtonClicked;

            MenuButton.clicked += MenuButtonClicked;
            LevelSelectButton.clicked += LevelSelectButtonClicked;
            ReplayButton.clicked += ReplayButtonClicked;
            NextButton.clicked += NextButtonClicked;
            
            Scene current = SceneManager.GetActiveScene();
            GameInit(Utils.GetSceneNames().IndexOf(current.name));
        }


        private void OnClickAnywhere(ClickEvent evt)
        {
            HandleNextStep();
        }

        private void HandleNextStep()
        {
            if (isTyping)
            {
                FinishTypingCurrentLine();
            }
            else
            {
                ShowTutorial(currentIndex + 1);
            }
        }

        public void StartTutorial(string[] lines){
            GameManager.Instance.TurtorialStart();
            tutorial_page.style.display = DisplayStyle.Flex;
            tutorialLines = lines;
            ShowTutorial(0);
        }
        private void ShowTutorial(int index)
        {
            if (index >= tutorialLines.Length)
            {
                EndTutorial();
                return;
            }

            currentIndex = index;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(tutorialLines[index]));
        }

        private IEnumerator TypeText(string content)
        {
            isTyping = true;
            TutorialLabel.text = "";

            foreach (char c in content)
            {
                TutorialLabel.text += c;
                yield return new WaitForSeconds(0.1f);
            }

            isTyping = false;
        }

        private void FinishTypingCurrentLine()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            TutorialLabel.text = tutorialLines[currentIndex];
            isTyping = false;
        }

        private void EndTutorial()
        {
            OnTutorialEnded?.Invoke();
            // 教學結束，隱藏整個教學 UI
            tutorial_page.style.display = DisplayStyle.None;
            if(GameManager.Instance != null)
                GameManager.Instance.TurtorialEnd();
        }

        private void OnDisable(){
            // DI for check cover != null
            cover?.UnregisterCallback<TransitionEndEvent>(TransitionEnd);
            if(StartButton != null)
                StartButton.clicked -= StartButtonClicked;
            if(SettingButton != null)
                SettingButton.clicked -= SettingButtonClicked;
            if(QuitButton != null)
                QuitButton.clicked -= QuitButtonClicked;
            if(LevelButton != null)
                foreach(var btn in LevelButton){
                    btn.clicked -= () => LevelButtonClicked(btn);
                }

            if(QuitSettingButton != null)
                QuitSettingButton.clicked -= QuitSettingButtonClicked;

            if(MenuButton != null)
                MenuButton.clicked -= MenuButtonClicked;
            if(LevelSelectButton != null)
                LevelSelectButton.clicked -= LevelSelectButtonClicked;
            if(ReplayButton != null)
                ReplayButton.clicked -= ReplayButtonClicked;
            if(NextButton != null)
                NextButton.clicked -= NextButtonClicked;
        }

        private void TransitionEnd(TransitionEndEvent evt){
            Debug.Log("Transistion End.");
            if (cover.style.opacity.value > 0.9f)
            {
                Display(true);
            }
        }

        public void UpdateHP(int currentBeat = 0, int hit = 0){
            HPLabel.text = $"Timer: {currentBeat + hit * 4} ({hit})";
        }
        public void UpdateWin(int starCount){
            UpdateStar(starCount);
            end_page.style.display = DisplayStyle.Flex;
        }
        public void UpdateKey(int keyCount, int maxKeyCount){
            KeyContainer.Clear();

            Debug.Log($"KC MKC {keyCount} {maxKeyCount}");

            for (int i = 0; i < keyCount; i++)
            {
                VisualElement key = new VisualElement();
                key.AddToClassList("key-icon");
                key.style.backgroundImage = new StyleBackground(keySprite);

                KeyContainer.Add(key);
            }
            for(int i = keyCount; i < maxKeyCount; i++){
                VisualElement key = new VisualElement();
                key.AddToClassList("key-icon");
                key.style.backgroundImage = new StyleBackground(keySprite_black);

                KeyContainer.Add(key);
            }
        }
        public void UpdateStar(int starCount,int maxStarCount = 3){
            StarContainer.Clear();

            for (int i = 0; i < starCount; i++)
            {
                VisualElement star = new VisualElement();
                star.AddToClassList("star-icon");
                star.style.backgroundImage = new StyleBackground(starSprite);

                StarContainer.Add(star);
            }
            for(int i = starCount; i < maxStarCount; i++){
                VisualElement star = new VisualElement();
                star.AddToClassList("star-icon");
                star.style.backgroundImage = new StyleBackground(starSprite_black);

                StarContainer.Add(star);
            }
        }

        public void SetTargetScene(int scene, int page)
        {
            current_scene = scene;
            current_page = page;
            FadeOut();
        }

        public void GameInit(int scene)
        {
            current_scene = scene;
    
            if(current_scene == 0){
                current_scene = 1;
            }
            
            if(current_scene >= LEVEL_MIN){
                firstTime = false;
                current_page = 2;
            }
            
            Display(false);
        }

        private void StartButtonClicked()
        {
            if(firstTime){
                SetTargetScene(LEVEL_MIN,2);
                firstTime = false;
            }else{
                SetTargetScene(1,1);
            }

            
        }
        private void SettingButtonClicked()
        {
            setting_page.style.display = DisplayStyle.Flex;
        }
        private void QuitButtonClicked()
        {
            Debug.Log("EXIT");
            Application.Quit();
        }
        private void LevelButtonClicked(Button btn)
        {
            SetTargetScene(LEVEL_MIN + int.Parse(btn.text) - 1, 2);
        }


        public void Home()
        {
            LevelManager.Instance.GameEnd();
            SetTargetScene(1, 0);
        }
        private void QuitSettingButtonClicked()
        {
            setting_page.style.display = DisplayStyle.None;
        }
        private void VolumeSliderChanged(ChangeEvent<float> evt){
            Debug.Log($"Volume {evt.newValue}");
        }
        
        private void MenuButtonClicked()
        {
            SetTargetScene(1, 0);
        }
        private void LevelSelectButtonClicked()
        {
            SetTargetScene(1, 1);
        }
        private void ReplayButtonClicked()
        {
            SetTargetScene(current_scene, current_page);
        }
        private void NextButtonClicked()
        {
            if(current_scene < LEVEL_MAX)
                SetTargetScene(current_scene + 1, current_page);
        }
        private void FadeIn()
        {
            cover.style.opacity = 0.0f;
        }
        private void FadeOut()
        {
            cover.style.opacity = 1.0f;
        }
        private void Display(bool load)
        {
            for(int i=0;i<pages.Count;i++){
                if(i == current_page){
                    pages[i].style.display = DisplayStyle.Flex;
                }else{
                    pages[i].style.display = DisplayStyle.None;
                }
            }
            EndTutorial();
            end_page.style.display = DisplayStyle.None;
            if(load)
                SceneManager.LoadScene(current_scene);
            cover.style.opacity = 0.0f;
        }
    }
}