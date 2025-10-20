using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.Build;

namespace SoundTrack{
    public class Info : MonoBehaviour
    {
        public static Info Instance { get; private set; }

        private VisualElement RootVisualElement;
        private VisualElement cover;
        private List<VisualElement> SceneVisualElement;
        
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

        public void OnEnable()
        {
            previous_scene = 0;
            current_scene = 0;

            RootVisualElement = GetComponent<UIDocument>().rootVisualElement;
            cover = RootVisualElement.Q<VisualElement>("Cover");

            RootVisualElement.style.display = DisplayStyle.Flex; // None or Flex
            cover.style.opacity = 1.0f;

            SceneVisualElement = new List<VisualElement>();

            foreach(var scene in RootVisualElement.Q<VisualElement>("Root").Children()){
                SceneVisualElement.Add(scene);
                scene.style.display = DisplayStyle.None;
                // Debug.Log(scene.name);
            }
            
            var StartButton = RootVisualElement.Q<Button>("StartButton");
            StartButton.clicked += StartButtonClicked;
            var SettingButton = RootVisualElement.Q<Button>("SettingButton");
            SettingButton.clicked += SettingButtonClicked;
            var QuitButton = RootVisualElement.Q<Button>("QuitButton");
            QuitButton.clicked += QuitButtonClicked;
            // var tButton = RootVisualElement.Q<Button>("TalentButton");
            // tButton.clicked += OntButtonClicked;
            // var MenuButton = RootVisualElement.Q<Button>("MenuButton");
            // MenuButton.clicked += OnMenuButtonClicked;

            cover.RegisterCallback<TransitionEndEvent>(evt =>
            {
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
            });

            // StartCoroutine(FadeIn());
        }

        public void SetTargetScene(int scene)
        {
            previous_scene = current_scene;
            current_scene = scene;
            FadeOut();
        }

        public void GameInit()
        {
            previous_scene = current_scene;
            current_scene = 1;

            SceneVisualElement[current_scene - 1].style.display = DisplayStyle.Flex;
            SceneManager.LoadScene(current_scene);

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