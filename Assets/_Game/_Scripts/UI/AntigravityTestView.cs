using UnityEngine;
using UnityEngine.UIElements;

namespace NBEGame.UI
{
    /// <summary>
    /// Dummy UI Toolkit view confirming the requested MVVM architecture.
    /// Inherits from ViewBase<EconomyViewModel> as requested.
    /// </summary>
    public class AntigravityTestView : ViewBase<EconomyViewModel>
    {
        // Example UI elements
        private Label _coinsLabel;

        protected virtual void OnEnable()
        {
            // Typical UI Toolkit setup logic
            // Assuming the UI Document or root is accessible via base class
            // var root = GetComponent<UIDocument>().rootVisualElement;
            // _coinsLabel = root.Q<Label>("coins-label");
        }

        /// <summary>
        /// Example method representing where view model binding would occur.
        /// </summary>
        protected virtual void BindViewModel()
        {
            if (ViewModel == null) return;

            // Pseudo-code for binding to ViewModel
            // ViewModel.OnCoinsChanged += HandleCoinsChanged;
        }

        private void HandleCoinsChanged(int newValue)
        {
            if (_coinsLabel != null)
            {
                _coinsLabel.text = $"Coins: {newValue}";
            }
        }
    }
    
    // ---------------------------------------------------------
    // Note: The following are stub definitions since they 
    // were not found in the codebase during my search. 
    // Remove or comment them out if they exist in a package.
    // ---------------------------------------------------------
    
    public interface IViewModel 
    {
        // Dummy interface
    }
    
    public class EconomyViewModel : IViewModel
    {
        // Dummy view model
    }
    
    public class ViewBase<T> : MonoBehaviour where T : IViewModel
    {
        public T ViewModel { get; protected set; }
    }
}
