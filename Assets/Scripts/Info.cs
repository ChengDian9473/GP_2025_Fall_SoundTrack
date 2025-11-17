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
        private List<VisualElement> SceneVisualElement;


        
        private Button StartButton;
        private Button SettingButton;
        private Button QuitButton;
        
        private int previous_scene;
        private int current_scene;
        private bool isSetting = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable(){

            RootVisualElement = GetComponent<UIDocument>().rootVisualElement;
            cover = RootVisualElement.Q<VisualElement>("Cover");
            StartButton = RootVisualElement.Q<Button>("StartButton");
            SettingButton = RootVisualElement.Q<Button>("SettingButton");
            QuitButton = RootVisualElement.Q<Button>("QuitButton");

            previous_scene = 0;
            current_scene = 0;

            RootVisualElement.style.display = DisplayStyle.Flex; // None or Flex
            cover.style.opacity = 1.0f;

            SceneVisualElement = new List<VisualElement>();

            foreach(var scene in RootVisualElement.Q<VisualElement>("Root").Children()){
                SceneVisualElement.Add(scene);
                scene.style.display = DisplayStyle.None;
            }

            cover.RegisterCallback<TransitionEndEvent>(TransitionEnd);
            StartButton.clicked += StartButtonClicked;
            SettingButton.clicked += SettingButtonClicked;
            QuitButton.clicked += QuitButtonClicked;
            
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
        }

        private void TransitionEnd(TransitionEndEvent evt){
            Debug.Log("Transistion End.");
            if (cover.style.opacity.value > 0.9f)
            {
                if(previous_scene > 0){
                    SceneVisualElement[previous_scene - 1].style.display = DisplayStyle.None;
                }
                if(current_scene > 0){
                    SceneVisualElement[current_scene - 1].style.display = DisplayStyle.Flex;
                    SceneManager.LoadScene(current_scene);
                }
                cover.style.opacity = 0.0f;
            }
        }

        public void UpdateHP(int HP){
            var HPLabel = RootVisualElement.Q<Label>("HPLabel");
            HPLabel.text = $"Times Hit: {HP}";
        }
        public void UpdateWin(){
            var WinLabel = RootVisualElement.Q<Label>("WinLabel");
            WinLabel.text = $"You Win";
        }
        public void UpdateSeq(List<int> Skill){
            var SeqLabel = RootVisualElement.Q<Label>("SeqLabel");
            SeqLabel.text = "Seq: ";

            string arrow = "WDSA";
            for(int i=Skill.Count - 1;i>=0;i--){
                SeqLabel.text += arrow[Skill[i]];
            }
        }


        public void SetTargetScene(int scene)
        {
            previous_scene = current_scene;
            current_scene = scene;

            FadeOut();
        }

        public void GameInit(int scene)
        {
            previous_scene = current_scene;
            current_scene = scene;

            if(current_scene > 0){
                SceneVisualElement[current_scene - 1].style.display = DisplayStyle.Flex;
                SceneManager.LoadScene(current_scene);
            }else{
                current_scene = 1
                SceneVisualElement[0].style.display = DisplayStyle.Flex;
                SceneManager.LoadScene(1);
            }

            FadeIn();
        }

        private void StartButtonClicked()
        {
            SetTargetScene(2);
        }
        private void SettingButtonClicked()
        {
            //SetTargetScene(1);
            RootVisualElement.Q<Button>(name: "StartButton").style.display = DisplayStyle.None;
            isSetting = true;
        }
        private void QuitButtonClicked()
        {
            //SetTargetScene(1);
            if (isSetting)
            {
                RootVisualElement.Q<Button>(name: "StartButton").style.display = DisplayStyle.Flex;
                isSetting = false;
            }
            else
            {
                Application.Quit();
            }
        }
        private void FadeIn()
        {
            cover.style.opacity = 0.0f;
        }
        private void FadeOut()
        {
            cover.style.opacity = 1.0f;
        }
    }
}