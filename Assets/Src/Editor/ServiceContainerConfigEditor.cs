using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
namespace Injector
{

    public class ServiceContainerConfigWindow : EditorWindow
    {
        private const string AssetName = "ServiceContainerConfig.asset";
        private const string ResourcesFolderPath = "Assets/Resources";
        private const string AssetFullPath = ResourcesFolderPath + "/" + AssetName;

        private ServiceContainerConfig config;

        [MenuItem("Simple Dependency Locator/Service Container Config")]
        public static void Open()
        {
            var window = GetWindow<ServiceContainerConfigWindow>();
            window.titleContent = new GUIContent("Service Container Config");
            window.minSize = new Vector2(400, 300);
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

            var inspector = new InspectorElement(config);
            inspector.style.flexGrow = 1;

            rootVisualElement.Add(inspector);

            var saveButton = new Button(() =>
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            })
            {
                text = "Save"
            };

            saveButton.style.marginTop = 10;

            rootVisualElement.Add(saveButton);
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
}