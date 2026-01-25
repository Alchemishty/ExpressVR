using System.Collections.Generic;
using UnityEngine;

namespace VREmoteWheel
{
    /// <summary>
    /// Radial menu UI component for displaying emote options.
    /// Dynamically generates menu items based on configuration.
    /// </summary>
    public class RadialEmoteMenu : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EmoteWheelConfig config;

        [Header("Menu Item Template")]
        [SerializeField] private EmoteMenuItem itemTemplate;
        [SerializeField] private Transform itemContainer;

        [Header("Layout")]
        [SerializeField] private float radiusOverride = -1f;

        private List<EmoteMenuItem> menuItems = new List<EmoteMenuItem>();
        private bool isInitialized;
        private int currentSelection = -1;

        private float Radius => radiusOverride > 0 ? radiusOverride : (config != null ? config.menuRadius : 100f);

        private void Awake()
        {
            if (itemTemplate != null)
            {
                itemTemplate.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (config == null && EmoteWheelManager.Instance != null)
            {
                config = EmoteWheelManager.Instance.Config;
            }

            Initialize();
            Hide();
        }

        /// <summary>
        /// Initializes the radial menu by creating menu items.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;
            if (config == null)
            {
                Debug.LogError("[VREmoteWheel] RadialEmoteMenu: No EmoteWheelConfig assigned");
                return;
            }
            if (itemTemplate == null)
            {
                Debug.LogError("[VREmoteWheel] RadialEmoteMenu: No item template assigned");
                return;
            }

            ClearMenuItems();
            CreateMenuItems();
            isInitialized = true;
        }

        /// <summary>
        /// Re-initializes the menu with new configuration.
        /// </summary>
        public void Reinitialize(EmoteWheelConfig newConfig)
        {
            config = newConfig;
            isInitialized = false;
            Initialize();
        }

        private void ClearMenuItems()
        {
            foreach (var item in menuItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            menuItems.Clear();
        }

        private void CreateMenuItems()
        {
            int emoteCount = config.EmoteCount;
            float angleStep = 360f / emoteCount;

            for (int i = 0; i < emoteCount; i++)
            {
                var emoteData = config.GetEmote(i);
                if (emoteData == null) continue;

                // Calculate position for this item (starting from top, going clockwise)
                float angle = 90f - (i * angleStep); // Start from top (90 degrees)
                float angleRad = angle * Mathf.Deg2Rad;

                Vector2 position = new Vector2(
                    Mathf.Cos(angleRad) * Radius,
                    Mathf.Sin(angleRad) * Radius
                );

                // Create menu item
                var itemGO = Instantiate(itemTemplate.gameObject, itemContainer);
                itemGO.name = $"EmoteItem_{i}_{emoteData.emoteName}";
                itemGO.SetActive(true);

                var rectTransform = itemGO.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = position;
                }

                var menuItem = itemGO.GetComponent<EmoteMenuItem>();
                if (menuItem != null)
                {
                    menuItem.Initialize(i, emoteData, config);
                    menuItems.Add(menuItem);
                }
            }
        }

        /// <summary>
        /// Shows the radial menu.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            currentSelection = -1;

            // Reset all items to unselected state
            foreach (var item in menuItems)
            {
                item.SetSelected(false, true);
            }
        }

        /// <summary>
        /// Hides the radial menu.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            currentSelection = -1;
        }

        /// <summary>
        /// Sets the selected menu item.
        /// </summary>
        public void SetSelection(int newIndex, int previousIndex = -1)
        {
            // Deselect previous
            if (previousIndex >= 0 && previousIndex < menuItems.Count)
            {
                menuItems[previousIndex].SetSelected(false);
            }
            else if (currentSelection >= 0 && currentSelection < menuItems.Count)
            {
                menuItems[currentSelection].SetSelected(false);
            }

            // Select new
            if (newIndex >= 0 && newIndex < menuItems.Count)
            {
                menuItems[newIndex].SetSelected(true);
            }

            currentSelection = newIndex;
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection(int previousIndex = -1)
        {
            int indexToClear = previousIndex >= 0 ? previousIndex : currentSelection;

            if (indexToClear >= 0 && indexToClear < menuItems.Count)
            {
                menuItems[indexToClear].SetSelected(false);
            }

            currentSelection = -1;
        }

        /// <summary>
        /// Gets the menu item at the specified index.
        /// </summary>
        public EmoteMenuItem GetMenuItem(int index)
        {
            if (index < 0 || index >= menuItems.Count)
            {
                return null;
            }
            return menuItems[index];
        }
    }
}
