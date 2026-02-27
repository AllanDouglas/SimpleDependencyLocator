using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Injector
{

    public class ServiceContainerConfigWindow : EditorWindow
    {
        private const string AssetName = "ServiceContainerConfig.asset";
        private const string ResourcesFolderPath = "Assets/Resources";
        private const string AssetFullPath = ResourcesFolderPath + "/" + AssetName;

        private ServiceContainerConfig config;
        private ServiceContainerConfigInspector configInspector;

        [MenuItem("Simple Dependency Locator/Service Container Config")]
        public static void Open()
        {
            var window = GetWindow<ServiceContainerConfigWindow>();
            window.titleContent = new GUIContent("Service Container Config");
            window.minSize = new Vector2(500, 400);
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            LoadOrCreateConfig();

            if (config == null)
            {
                rootVisualElement.Add(new Label("Failed to load config."));
                return;
            }

            configInspector = new ServiceContainerConfigInspector(config, () =>
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            });

            var mainContainer = new VisualElement();
            mainContainer.style.flexGrow = 1;
            mainContainer.Add(configInspector.GetRootElement());

            rootVisualElement.Add(mainContainer);

            var footerContainer = new VisualElement();
            footerContainer.style.flexDirection = FlexDirection.Row;
            footerContainer.style.marginTop = 10;
            footerContainer.style.paddingLeft = 5;
            footerContainer.style.paddingRight = 5;
            footerContainer.style.borderTopWidth = 1;
            footerContainer.style.borderTopColor = new Color(0.2f, 0.2f, 0.2f, 1);
            footerContainer.style.paddingTop = 5;
            footerContainer.style.alignItems = Align.Center;

            // Spacer to push save button to the right
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            footerContainer.Add(spacer);

            var saveButton = new Button(() =>
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            })
            {
                text = "Save"
            };
            saveButton.style.paddingLeft = 20;
            saveButton.style.paddingRight = 20;
            saveButton.style.marginRight = 5;

            footerContainer.Add(saveButton);
            rootVisualElement.Add(footerContainer);
        }

        private void Update()
        {
            if (configInspector != null)
            {
                configInspector.Update();
            }
        }

        private void LoadOrCreateConfig()
        {
            // Garante que a pasta Resources existe
            if (!Directory.Exists(ResourcesFolderPath))
            {
                Directory.CreateDirectory(ResourcesFolderPath);
                AssetDatabase.Refresh();
            }

            // Tenta carregar via Resources
            config = Resources.Load<ServiceContainerConfig>("ServiceContainerConfig");

            if (config == null)
            {
                // Tenta carregar diretamente pelo caminho
                config = AssetDatabase.LoadAssetAtPath<ServiceContainerConfig>(AssetFullPath);

                if (config == null)
                {
                    // Cria novo asset
                    config = CreateInstance<ServiceContainerConfig>();
                    AssetDatabase.CreateAsset(config, AssetFullPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log("ServiceContainerConfig criado em: " + AssetFullPath);
                }
            }
        }
    }

    public class ServiceContainerConfigInspector
    {
        private ServiceContainerConfig config;
        private SerializedObject serializedConfig;
        private System.Action onChanged;
        private int selectedTabIndex = 0;
        private VisualElement leftPanel;               // holds tab buttons
        private VisualElement splitter;
        private VisualElement contentContainer;
        private VisualElement rootElement;

        public ServiceContainerConfigInspector(ServiceContainerConfig config, System.Action onChanged)
        {
            this.config = config;
            this.onChanged = onChanged;
            this.serializedConfig = new SerializedObject(config);
            this.rootElement = CreateUI();
        }

        public VisualElement GetRootElement() => rootElement;

        public void Update()
        {
            if (serializedConfig != null && config != null)
            {
                serializedConfig.Update();
            }
        }

        private VisualElement CreateUI()
        {
            // root row holding left panel, splitter and content
            var rootRow = new VisualElement();
            rootRow.style.flexGrow = 1;
            rootRow.style.flexDirection = FlexDirection.Row;

            // left panel with tabs (scrollable)
            leftPanel = new ScrollView();
            leftPanel.style.flexDirection = FlexDirection.Column;
            leftPanel.style.width = new StyleLength(new Length(30, LengthUnit.Percent)); // default 30%
            leftPanel.style.minWidth = 100;
            leftPanel.style.borderRightWidth = 1;
            leftPanel.style.borderRightColor = new Color(0.2f, 0.2f, 0.2f, 1);
            leftPanel.style.paddingLeft = 5;
            leftPanel.style.paddingRight = 5;
            leftPanel.style.marginTop = 10;
            leftPanel.style.flexGrow = 0;

            // draggable splitter
            splitter = new VisualElement();
            splitter.style.width = 4;
            splitter.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
            // cursor style unsupported in this Unity version, omit it
            // splitter.style.cursor = new StyleCursor(MouseCursor.ResizeHorizontal);

            bool dragging = false;
            float startMouseX = 0;
            float startWidth = 0;

            splitter.RegisterCallback<MouseDownEvent>(evt =>
            {
                dragging = true;
                startMouseX = evt.mousePosition.x;
                startWidth = leftPanel.resolvedStyle.width;
                evt.StopPropagation();
            });
            splitter.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (dragging)
                {
                    float delta = evt.mousePosition.x - startMouseX;
                    float newWidth = startWidth + delta;
                    float parentWidth = rootRow.resolvedStyle.width;
                    if (parentWidth > 0)
                    {
                        float pct = Mathf.Clamp(newWidth / parentWidth, 0.1f, 0.9f);
                        leftPanel.style.width = new StyleLength(new Length(pct * 100, LengthUnit.Percent));
                    }
                    else
                    {
                        leftPanel.style.width = newWidth;
                    }
                    evt.StopPropagation();
                }
            });
            splitter.RegisterCallback<MouseUpEvent>(evt =>
            {
                dragging = false;
                evt.StopPropagation();
            });
            splitter.RegisterCallback<MouseLeaveEvent>(evt => { dragging = false; });

            // right/content panel (scrollable)
            contentContainer = new ScrollView();
            contentContainer.style.flexGrow = 1;
            contentContainer.style.paddingLeft = 5;
            contentContainer.style.paddingRight = 5;

            rootRow.Add(leftPanel);
            rootRow.Add(splitter);
            rootRow.Add(contentContainer);

            RefreshTabs();

            return rootRow;
        }

        private void RefreshTabs()
        {
            // rebuild vertical button list on left panel
            leftPanel.Clear();

            if (config.ServicesEntry == null || config.ServicesEntry.Length == 0)
            {
                selectedTabIndex = 0;
                contentContainer.Clear();
                contentContainer.Add(new Label("No service entries. Click '+' to add one."));
                DrawAddEntryBtn();
                return;
            }

            // clamp selected index
            if (selectedTabIndex >= config.ServicesEntry.Length)
            {
                selectedTabIndex = config.ServicesEntry.Length - 1;
            }

            // create buttons
            for (int i = 0; i < config.ServicesEntry.Length; i++)
            {
                var btn = CreateTabButton(i);
                btn.style.width = new StyleLength(new Length(95, LengthUnit.Percent));
                btn.style.marginBottom = 2;
                leftPanel.Add(btn);
            }

            // add new-tab button at bottom
            DrawAddEntryBtn();

            // show the currently selected content
            ShowTabContent(selectedTabIndex);

            void DrawAddEntryBtn()
            {
                var addTabBtn = new Button(() => AddNewTab()) { text = "+" };
                addTabBtn.style.marginTop = 5;
                addTabBtn.style.width = new StyleLength(new Length(95, LengthUnit.Percent));
                leftPanel.Add(addTabBtn);
            }
        }

        private Button CreateTabButton(int tabIndex)
        {
            var entry = config.ServicesEntry[tabIndex];
            var tabLabel = $"Entry {tabIndex + 1}";
            if (entry.service != null)
            {
                tabLabel = $"{entry.service.GetType().Name}";
            }

            var tabButton = new Button(() =>
            {
                selectedTabIndex = tabIndex;
                ShowTabContent(tabIndex);
                RefreshTabs();
            })

            {
                text = tabLabel,
                name = $"tabButton_{tabIndex}"
            };
            // make the button expand horizontally in left panel;
            tabButton.style.alignContent = Align.FlexStart;
            tabButton.style.alignItems = Align.FlexStart;
            tabButton.style.unityTextAlign = TextAnchor.MiddleLeft;
            tabButton.style.paddingLeft = 10;
            tabButton.style.paddingRight = 10;


            // Highlight selected tab
            if (tabIndex == selectedTabIndex)
            {
                tabButton.style.backgroundColor = new Color(0.2f, 0.5f, 1, 1);
            }
            else
            {
                tabButton.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
            }

            return tabButton;
        }

        private void ShowTabContent(int tabIndex)
        {
            contentContainer.Clear();

            if (tabIndex < 0 || tabIndex >= config.ServicesEntry.Length)
                return;

            var entry = config.ServicesEntry[tabIndex];
            var tabLabelText = $"Entry {tabIndex + 1}";
            if (entry.service != null)
            {
                tabLabelText += $" ({entry.service.GetType().Name})";
            }

            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.marginBottom = 15;
            headerContainer.style.marginTop = 10;

            var label = new Label(tabLabelText);

            label.style.fontSize = 14;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.flexGrow = 1;
            headerContainer.Add(label);

            var removeButton = new Button(() => RemoveTab(tabIndex))
            {
                text = "Remove"
            };

            removeButton.style.backgroundColor = new Color(1, 0.3f, 0.3f, 1);
            removeButton.style.paddingLeft = 15;
            removeButton.style.paddingRight = 15;
            headerContainer.Add(removeButton);

            contentContainer.Add(headerContainer);

            // Show inspector for this entry
            var servicesProperty = serializedConfig.FindProperty("_servicesEntry");
            if (servicesProperty == null || !servicesProperty.isArray)
            {
                Debug.LogError("Could not find _servicesEntry property");
                return;
            }

            var singleEntryProperty = servicesProperty.GetArrayElementAtIndex(tabIndex);

            var inspectorContainer = new VisualElement();
            inspectorContainer.style.borderLeftWidth = 2;
            inspectorContainer.style.borderLeftColor = new Color(0.2f, 0.5f, 1, 1);
            inspectorContainer.style.paddingLeft = 10;
            inspectorContainer.style.paddingTop = 5;
            inspectorContainer.style.paddingBottom = 5;

            // Service field
            // var serviceProp = singleEntryProperty.FindPropertyRelative("service");
            if (singleEntryProperty != null)
            {
                var singleField = new PropertyField(singleEntryProperty);
                inspectorContainer.Add(singleField);
                singleField.Bind(serializedConfig);
                singleField.RegisterCallback<ChangeEvent<string>>(evt =>
                {
                    if (evt.newValue == evt.previousValue)
                    {
                        return;
                    }

                    serializedConfig.ApplyModifiedProperties();
                    onChanged?.Invoke();
                    RefreshTabs(); // Update tab labels to show the new service type
                });
            }

            contentContainer.Add(inspectorContainer);
        }

        private void AddNewTab()
        {
            var entries = new List<ServiceContainerConfig.ServiceEntry>(config.ServicesEntry ?? System.Array.Empty<ServiceContainerConfig.ServiceEntry>())
            {
                new ServiceContainerConfig.ServiceEntry()
            };

            config.ServicesEntry = entries.ToArray();
            serializedConfig.ApplyModifiedProperties();

            Task.Delay(100).ContinueWith(_ =>
            {
                selectedTabIndex = entries.Count - 1;
                onChanged?.Invoke();
                RefreshTabs();
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void RemoveTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= config.ServicesEntry.Length)
                return;

            serializedConfig.ApplyModifiedProperties();

            var entries = new List<ServiceContainerConfig.ServiceEntry>(config.ServicesEntry);
            entries.RemoveAt(tabIndex);
            config.ServicesEntry = entries.ToArray();

            if (selectedTabIndex >= config.ServicesEntry.Length && selectedTabIndex > 0)
            {
                selectedTabIndex--;
            }

            onChanged?.Invoke();
            RefreshTabs();
        }
    }
}