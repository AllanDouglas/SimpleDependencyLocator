using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;
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
        private VisualElement tabsContainer;
        private VisualElement contentContainer;
        private VisualElement rootElement;
        private DropdownField tabDropdown;

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
            var container = new ScrollView();
            container.style.flexGrow = 1;

            // Tab bar
            var tabBarContainer = new VisualElement();
            tabBarContainer.style.flexDirection = FlexDirection.Row;
            tabBarContainer.style.marginBottom = 10;
            tabBarContainer.style.paddingLeft = 5;
            tabBarContainer.style.paddingRight = 5;
            tabBarContainer.style.paddingTop = 5;
            tabBarContainer.style.borderBottomWidth = 1;
            tabBarContainer.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 1);
            tabBarContainer.style.alignItems = Align.Center;

            tabsContainer = new VisualElement();
            tabsContainer.style.flexDirection = FlexDirection.Row;
            // tabBarContainer.Add(tabsContainer);

            // Add tab dropdown selector
            tabDropdown = new DropdownField();
            tabDropdown.style.width = 250;
            tabDropdown.style.marginLeft = 10;
            tabDropdown.label = "Select:";
            tabDropdown.style.display = DisplayStyle.Flex; // Hidden by default, shown when tabs exist
            tabDropdown.RegisterValueChangedCallback(evt =>
            {
                var index = int.Parse(evt.newValue.AsSpan()[0].ToString());
                if (index >= 0 && index < config.ServicesEntry.Length)
                {
                    selectedTabIndex = index;
                    ShowTabContent(selectedTabIndex);
                    // RefreshTabs();
                }
            });

            tabBarContainer.Add(tabDropdown);

            // Add tab button
            var addTabButton = new Button(() => AddNewTab())
            {
                text = "+"
            };
            addTabButton.style.width = 30;
            addTabButton.style.marginLeft = 10;
            tabBarContainer.Add(addTabButton);

            container.Add(tabBarContainer);

            // Content container
            contentContainer = new VisualElement();
            contentContainer.style.flexGrow = 1;
            contentContainer.style.paddingLeft = 5;
            contentContainer.style.paddingRight = 5;
            container.Add(contentContainer);

            RefreshTabs();

            return container;
        }

        private void RefreshTabs()
        {
            tabsContainer.Clear();

            if (config.ServicesEntry == null || config.ServicesEntry.Length == 0)
            {
                selectedTabIndex = 0;
                contentContainer.Clear();
                contentContainer.Add(new Label("No service entries. Click '+' to add one."));
                if (tabDropdown != null)
                    tabDropdown.style.display = DisplayStyle.None;
                return;
            }

            // Update dropdown options
            if (tabDropdown != null)
            {
                var dropdownOptions = new List<string>();
                for (int i = 0; i < config.ServicesEntry.Length; i++)
                {
                    var entry = config.ServicesEntry[i];
                    var label = $"{i} - Entry {i + 1}";
                    if (entry.service != null)
                    {
                        label = $"{i} - {entry.service.GetType().Name}";
                    }
                    dropdownOptions.Add(label);
                }
                tabDropdown.choices = dropdownOptions;
                tabDropdown.formatListItemCallback = static index => index.AsSpan()[4..].ToString();
                //tabDropdown.value = dropdownOptions[selectedTabIndex];
                tabDropdown.formatSelectedValueCallback = null;
                tabDropdown.formatSelectedValueCallback = value =>
                {
                    if (string.IsNullOrEmpty(value))
                        return string.Empty;

                    var index = int.Parse(value.AsSpan()[0].ToString());
                    return dropdownOptions[index].AsSpan()[4..].ToString();
                };
                tabDropdown.SetValueWithoutNotify(dropdownOptions[selectedTabIndex]);
                tabDropdown.style.display = DisplayStyle.Flex;
            }

            // Clamp selected tab index
            if (selectedTabIndex >= config.ServicesEntry.Length)
            {
                selectedTabIndex = config.ServicesEntry.Length - 1;
            }

            // Create tabs
            // for (int i = Mathf.Max(0, selectedTabIndex - 1); i < Mathf.Min(selectedTabIndex + 3, config.ServicesEntry.Length); i++)
            // {
            //     int tabIndex = i;
            //     var tabButton = CreateTabButton(tabIndex);
            //     tabsContainer.Add(tabButton);
            // }

            // Show selected tab content
            ShowTabContent(selectedTabIndex);
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

                // tabsContainer.Query<Button>($"tabButton_{tabIndex}").First().style.backgroundColor = new Color(0.2f, 0.5f, 1, 1);
                // tabsContainer.Query<Button>($"tabButton_{selectedTabIndex}").First().style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);

                selectedTabIndex = tabIndex;
                ShowTabContent(tabIndex);
                RefreshTabs();
            })

            {
                text = tabLabel,
                name = $"tabButton_{tabIndex}"
            };
            tabButton.style.width = 100;
            tabButton.style.maxWidth = 100;
            tabButton.style.minWidth = 100;
            tabButton.style.alignContent = Align.FlexStart;
            tabButton.style.alignItems = Align.FlexStart;
            tabButton.style.unityTextAlign = TextAnchor.MiddleLeft;
            tabButton.style.marginRight = 5;
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
                singleField.RegisterValueChangeCallback(evt =>
                {
                    serializedConfig.ApplyModifiedProperties();
                    onChanged?.Invoke();
                    RefreshTabs(); // Update tab labels to show the new service type
                });
            }

            // if (serviceProp != null)
            // {
            //     var serviceField = new PropertyField(serviceProp);
            //     serviceField.style.marginBottom = 10;

            //     // Bind changes to save the config
            //     serviceField.RegisterValueChangeCallback(evt =>
            //     {
            //         serializedConfig.ApplyModifiedProperties();
            //         onChanged?.Invoke();
            //         RefreshTabs(); // Update tab labels to show the new service type
            //     });

            //     inspectorContainer.Add(serviceField);
            // }

            // // Show types field as read-only
            // var typesProp = singleEntryProperty.FindPropertyRelative("types");
            // if (typesProp != null)
            // {
            //     var typesField = new PropertyField(typesProp, "Implemented Interfaces");
            //     typesField.SetEnabled(false);
            //     inspectorContainer.Add(typesField);
            // }

            contentContainer.Add(inspectorContainer);
        }

        private void AddNewTab()
        {
            var entries = new List<ServiceContainerConfig.ServiceEntry>(config.ServicesEntry ?? System.Array.Empty<ServiceContainerConfig.ServiceEntry>())
            {
                new ServiceContainerConfig.ServiceEntry()
            };

            config.ServicesEntry = entries.ToArray();

            selectedTabIndex = entries.Count - 1;

            serializedConfig.ApplyModifiedProperties();
            onChanged?.Invoke();

            RefreshTabs();
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