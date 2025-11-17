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

        private VisualElement home_page;
        private VisualElement setting_page;
        private VisualElement end_page;

        private Button StartButton;
        private Button SettingButton;
        private Button QuitButton;

        private List<Button> LevelButton;

        private Button HomeButton;

        private Button QuitSettingButton;
        private Slider VolumeSlider;

        private Button MenuButton;
        private Button LevelSelectButton;
        private Button ReplayButton;
        
        private int current_scene;
        private int current_page;

        public static int LEVEL_START = 2;

        public bool firstTime;

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

            HomeButton = RootVisualElement.Q<Button>("HomeButton");

            VolumeSlider = RootVisualElement.Q<Slider>("VolumeSlider");
            QuitSettingButton = RootVisualElement.Q<Button>("QuitSettingButton");

            MenuButton = RootVisualElement.Q<Button>("MenuButton");
            LevelSelectButton = RootVisualElement.Q<Button>("LevelSelectButton");
            ReplayButton = RootVisualElement.Q<Button>("ReplayButton");

            current_scene = 0;
            current_page = 0;

            RootVisualElement.style.display = DisplayStyle.Flex; // None or Flex
            cover.style.opacity = 1.0f;

            pages = new List<VisualElement>();

            pages.Add(RootVisualElement.Q<VisualElement>("MainMenu")); // Page 0
            pages.Add(RootVisualElement.Q<VisualElement>("LevelSelect")); // Page 1
            pages.Add(RootVisualElement.Q<VisualElement>("Level")); // Page 2

            home_page = RootVisualElement.Q<VisualElement>("Home");
            setting_page = RootVisualElement.Q<VisualElement>("Setting");
            end_page = RootVisualElement.Q<VisualElement>("End");

            home_page.style.display = DisplayStyle.None;
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

            HomeButton.clicked += HomeButtonClicked;

            VolumeSlider.RegisterValueChangedCallback(VolumeSliderChanged);
            QuitSettingButton.clicked += QuitSettingButtonClicked;

            MenuButton.clicked += MenuButtonClicked;
            LevelSelectButton.clicked += LevelSelectButtonClicked;
            ReplayButton.clicked += ReplayButtonClicked;
            
            Scene current = SceneManager.GetActiveScene();
            GameInit(Utils.GetSceneNames().IndexOf(current.name));
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

            if(HomeButton != null)
                HomeButton.clicked -= HomeButtonClicked;

            if(QuitSettingButton != null)
                QuitSettingButton.clicked -= QuitSettingButtonClicked;

            if(MenuButton != null)
                MenuButton.clicked -= MenuButtonClicked;
            if(LevelSelectButton != null)
                LevelSelectButton.clicked -= LevelSelectButtonClicked;
            if(ReplayButton != null)
                ReplayButton.clicked -= ReplayButtonClicked;
        }

        private void TransitionEnd(TransitionEndEvent evt){
            Debug.Log("Transistion End.");
            if (cover.style.opacity.value > 0.9f)
            {
                Display(true);
            }
        }

        public void UpdateHP(int HP){
            var HPLabel = RootVisualElement.Q<Label>("HPLabel");
            HPLabel.text = $"Times Hit: {HP}";
        }
        public void UpdateWin(){
            var WinLabel = RootVisualElement.Q<Label>("WinLabel");
            WinLabel.text = $"You Win";
            end_page.style.display = DisplayStyle.Flex;
        }
        public void UpdateSeq(List<int> Skill){
            var SeqLabel = RootVisualElement.Q<Label>("SeqLabel");
            SeqLabel.text = "Seq: ";

            string arrow = "WDSA";
            for(int i=Skill.Count - 1;i>=0;i--){
                SeqLabel.text += arrow[Skill[i]];
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
            
            if(current_scene >= LEVEL_START){
                firstTime = false;
                current_page = 2;
            }
            
            Display(false);
        }

        private void StartButtonClicked()
        {
            if(firstTime){
                SetTargetScene(LEVEL_START,2);
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
            SetTargetScene(LEVEL_START + int.Parse(btn.text.Replace("Level","")), 2);
        }


        private void HomeButtonClicked()
        {
            GameManager.Instance.GameEnd();
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
            if(current_scene == 1){
                home_page.style.display = DisplayStyle.None;
            }else{
                home_page.style.display = DisplayStyle.Flex;
            }

            end_page.style.display = DisplayStyle.None;
            if(load)
                SceneManager.LoadScene(current_scene);
            cover.style.opacity = 0.0f;
        }
    }
}