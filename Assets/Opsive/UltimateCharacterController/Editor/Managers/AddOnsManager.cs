/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Draws the inspector for an add-on that has been installed.
    /// </summary>
    public abstract class AddOnInspector
    {
        protected MainManagerWindow m_MainManagerWindow;
        public MainManagerWindow MainManagerWindow { set { m_MainManagerWindow = value; } }

        /// <summary>
        /// Draws the add-on inspector.
        /// </summary>
        /// <param name="container">The parent VisualElement container.</param>
        public abstract void ShowInspector(VisualElement container);
    }

    /// <summary>
    /// Draws a list of all of the available add-ons.
    /// </summary>
    [OrderedEditorItem("Add-Ons", 11)]
    public class AddOnsManager : Manager
    {
        private string[] m_ToolbarStrings = { "Installed Add-Ons", "Available Add-Ons" };
        [SerializeField] private bool m_ShowInstalledAddOns;

        private AddOnInspector[] m_AddOnInspectors;
        private string[] m_AddOnNames;

        /// <summary>
        /// Stores the information about the add-on.
        /// </summary>
        private class AvailableAddOn
        {
            private int m_ID;
            private string m_Name;
            private string m_AddOnURL;
            private string m_Description;
            private bool m_Installed;
            private Texture2D m_Icon;

            private UnityEngine.Networking.UnityWebRequest m_IconRequest;
            private UnityEngine.Networking.DownloadHandlerTexture m_TextureDownloadHandler;

            private VisualElement m_Container;

            /// <summary>
            /// Constructor for the AvailableAddOn class.
            /// </summary>
            public AvailableAddOn(int id, string name, string iconURL, string addOnURL, string description, string type)
            {
                m_ID = id;
                m_Name = name;
                m_AddOnURL = addOnURL;
                m_Description = description;
                // The add-on is installed if the type exists.
                m_Installed = !string.IsNullOrEmpty(type) && Shared.Utility.TypeUtility.GetType(type) != null;

                // Start loading the icon as soon as the url is retrieved.
                m_TextureDownloadHandler = new UnityEngine.Networking.DownloadHandlerTexture();
                m_IconRequest = UnityEngine.Networking.UnityWebRequest.Get(iconURL);
                m_IconRequest.downloadHandler = m_TextureDownloadHandler;
                m_IconRequest.SendWebRequest();

                EditorApplication.update += WaitForIconWebRequest;
            }

            /// <summary>
            /// Retrieves the icon for the add-on.
            /// </summary>
            private void WaitForIconWebRequest()
            {
                if (m_IconRequest.isDone) {
                    if (string.IsNullOrEmpty(m_IconRequest.error)) {
                        m_Icon = m_TextureDownloadHandler.texture;
                    }
                    m_IconRequest = null;
                    ShowAddOn(null);

                    EditorApplication.update -= WaitForIconWebRequest;
                }
            }

            /// <summary>
            /// Draws the inspector for the available add-on.
            /// </summary>
            /// <param name="container">The parent VisualElement container.</param>
            public void ShowAddOn(VisualElement container)
            {
                if (container != null) {
                    m_Container = container;
                } else {
                    m_Container.Clear();
                }

                // Draw the add-on details.
                var horizontalLayout = new VisualElement();
                horizontalLayout.AddToClassList("horizontal-layout");
                horizontalLayout.style.marginTop = 10;
                horizontalLayout.style.marginLeft = 5;
                horizontalLayout.style.marginBottom = 10;
                horizontalLayout.style.marginRight = 5;
                m_Container.Add(horizontalLayout);

                if (m_Icon != null) {
                    var iconImage = new Image();
                    iconImage.image = m_Icon;
                    iconImage.style.flexShrink = 0;
                    horizontalLayout.Add(iconImage);
                }

                var verticalLayout = new VisualElement();
                verticalLayout.style.marginLeft = 5;
                horizontalLayout.Add(verticalLayout);

                var name = m_Name;
                if (m_Installed) {
                    name += " (INSTALLED)";
                }
                var nameLabel = new Label(name);
                nameLabel.AddToClassList("large-title");
                verticalLayout.Add(nameLabel);

                var buttonHorizontalLayout = new VisualElement();
                buttonHorizontalLayout.AddToClassList("horizontal-layout");
                buttonHorizontalLayout.style.flexGrow = 0;
                verticalLayout.Add(buttonHorizontalLayout);

                if (!string.IsNullOrEmpty(m_AddOnURL)) {
                    var overviewButton = new Button();
                    overviewButton.text = "Overview";
                    overviewButton.style.width = 120;
                    buttonHorizontalLayout.Add(overviewButton);

                    overviewButton.clicked += () =>
                    {
                        Application.OpenURL(m_AddOnURL);
                    };
                }

                if (m_ID > 0) {
                    var assetStoreButton = new Button();
                    assetStoreButton.text = "Asset Store";
                    assetStoreButton.style.width = 120;
                    buttonHorizontalLayout.Add(assetStoreButton);

                    assetStoreButton.clicked += () =>
                    {
                        Application.OpenURL("https://opsive.com/asset/UltimateCharacterController/AssetRedirect.php?asset=" + m_ID);
                    };
                }

                var descriptionLabel = new Label(m_Description);
                descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
                verticalLayout.Add(descriptionLabel);
            }
        }

        private UnityEngine.Networking.UnityWebRequest m_AddOnsReqest;
        private AvailableAddOn[] m_AvailableAddOns;

        private VisualElement m_InstalledContainer;
        private VisualElement m_AvailableContainer;

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            BuildInstalledAddOns();

            m_ShowInstalledAddOns = m_AvailableAddOns != null && m_AvailableAddOns.Length > 0;
        }

        /// <summary>
        /// Finds and create an instance of the inspectors for all of the installed add-ons.
        /// </summary>
        private void BuildInstalledAddOns()
        {
            var addOnInspectors = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var addOnIndexes = new List<int>();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must implement AddOnInspector.
                    if (!typeof(AddOnInspector).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    // A valid inspector class.
                    addOnInspectors.Add(assemblyTypes[j]);
                    var index = addOnIndexes.Count;
                    if (assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                        var item = assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                        index = item.Index;
                    }
                    addOnIndexes.Add(index);
                }
            }

            // Do not reinitialize the inspectors if they are already initialized and there aren't any changes.
            if (m_AddOnInspectors != null && m_AddOnInspectors.Length == addOnInspectors.Count) {
                return;
            }

            // All of the manager types have been found. Sort by the index.
            var inspectorTypes = addOnInspectors.ToArray();
            Array.Sort(addOnIndexes.ToArray(), inspectorTypes);

            m_AddOnInspectors = new AddOnInspector[addOnInspectors.Count];
            m_AddOnNames = new string[addOnInspectors.Count];

            // The inspector types have been found and sorted. Add them to the list.
            for (int i = 0; i < inspectorTypes.Length; ++i) {
                m_AddOnInspectors[i] = Activator.CreateInstance(inspectorTypes[i]) as AddOnInspector;
                m_AddOnInspectors[i].MainManagerWindow = m_MainManagerWindow;

                var name = ObjectNames.NicifyVariableName(inspectorTypes[i].Name);
                if (addOnInspectors[i].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                    var item = inspectorTypes[i].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                    name = item.Name;
                }
                m_AddOnNames[i] = name;
            }
        }

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            m_InstalledContainer = new VisualElement();
            m_AvailableContainer = new VisualElement();

            var tabToolbar = new TabToolbar(m_ToolbarStrings, m_ShowInstalledAddOns ? 0 : 1, (int selected) =>
            {
                m_ShowInstalledAddOns = selected == 0;
                if (m_ShowInstalledAddOns) {
                    m_AvailableContainer.Clear();
                    ShowInstalledAddOns();
                } else {
                    m_InstalledContainer.Clear();
                    ShowAvailableAddOns();
                }
            }, true);
            m_ManagerContentContainer.Add(tabToolbar);

            var scrollView = new ScrollView();
            scrollView.Add(m_InstalledContainer);
            scrollView.Add(m_AvailableContainer);
            m_ManagerContentContainer.Add(scrollView);

            if (m_ShowInstalledAddOns) {
                ShowInstalledAddOns();
            } else {
                ShowAvailableAddOns();
            }
        }

        /// <summary>
        /// Shows the installed add-ons.
        /// </summary>
        private void ShowInstalledAddOns()
        {
            m_InstalledContainer.Clear();

            if (m_AddOnInspectors == null || m_AddOnInspectors.Length == 0) {
                var helpBox = new HelpBox("No add-ons are currently installed.\n\nSelect the \"Available Add-Ons\" tab to see a list of all of the available add-ons.", HelpBoxMessageType.Info);
                m_InstalledContainer.Add(helpBox);
                return;
            }

            for (int i = 0; i < m_AddOnInspectors.Length; ++i) {
                var nameLabel = new Label(m_AddOnNames[i]);
                nameLabel.AddToClassList("large-title");
                nameLabel.style.marginTop = 10;
                m_InstalledContainer.Add(nameLabel);
                m_AddOnInspectors[i].ShowInspector(m_InstalledContainer);
            }
        }

        /// <summary>
        /// Shows the available add-ons.
        /// </summary>
        private void ShowAvailableAddOns()
        {
            m_AvailableContainer.Clear();

            if (m_AvailableAddOns == null && m_AddOnsReqest == null) {
                m_AddOnsReqest = UnityEngine.Networking.UnityWebRequest.Get("https://opsive.com/asset/UltimateCharacterController/Version3AddOnsList.txt");
                m_AddOnsReqest.SendWebRequest();
                EditorApplication.update += WaitForAddOnWebRequest;
            }

            // Draw the add-ons once they are loaded.
            if (m_AvailableAddOns != null && m_AvailableAddOns.Length > 0) {
                // Draw each add-on.
                for (int i = 0; i < m_AvailableAddOns.Length; ++i) {
                    var container = new VisualElement();
                    m_AvailableAddOns[i].ShowAddOn(container);
                    m_AvailableContainer.Add(container);
                }
            } else {
                if (m_AddOnsReqest != null && m_AddOnsReqest.isDone && !string.IsNullOrEmpty(m_AddOnsReqest.error)) {
                    var helpbox = new HelpBox("Error: Unable to retrieve add-ons.", HelpBoxMessageType.Error);
                    m_AvailableContainer.Add(helpbox);
                } else {
                    var helpbox = new HelpBox("Retrieveing the list of current add-ons...", HelpBoxMessageType.Info);
                    m_AvailableContainer.Add(helpbox);
                }
            }
        }

        /// <summary>
        /// Retrieves the list of available add-ons.
        /// </summary>
        private void WaitForAddOnWebRequest()
        {
            if (m_AvailableAddOns == null && m_AddOnsReqest.isDone && string.IsNullOrEmpty(m_AddOnsReqest.error)) {
                var splitAddOns = m_AddOnsReqest.downloadHandler.text.Split('\n');
                m_AvailableAddOns = new AvailableAddOn[splitAddOns.Length];
                var count = 0;
                for (int i = 0; i < splitAddOns.Length; ++i) {
                    if (string.IsNullOrEmpty(splitAddOns[i])) {
                        continue;
                    }

                    // The data must contain info on the add-on name, id, icon, add-on url, description, and type.
                    var addOnData = splitAddOns[i].Split(',');
                    if (addOnData.Length < 6) {
                        continue;
                    }

                    m_AvailableAddOns[count] = new AvailableAddOn(int.Parse(addOnData[0].Trim()), addOnData[1].Trim(), addOnData[2].Trim(), addOnData[3].Trim(), addOnData[4].Trim(), addOnData[5].Trim());
                    count++;
                }

                if (count != m_AvailableAddOns.Length) {
                    Array.Resize(ref m_AvailableAddOns, count);
                }
                m_AddOnsReqest = null;
                if (!m_ShowInstalledAddOns) {
                    ShowAvailableAddOns();
                }

                EditorApplication.update -= WaitForAddOnWebRequest;
            }
        }
    }
}