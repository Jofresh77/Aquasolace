using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI
{
    public class PopupWindow : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<PopupWindow> { }

        private VisualElement _windowContainer;
        private VisualElement _visualsWindow;
        private VisualElement _window;
        private bool _isLastWindow = false;

        #region uss classes
        
        private const string _popupContainerClass = "popup-container";
        private const string _windowContainerClass = "window-container";
        private const string _popupClass = "popup-window";
        private const string _visualsWindowClass = "visuals-window";
        private const string _textContainerClass = "text-container";
        private const string _btnContainerClass = "btn-container";
        private const string _msgLabelClass = "popup-msg";
        private const string _btnClass = "popup-btn";
        private const string _btnSkipClass = "btn-skip";
        private const string _btnCloseClass = "btn-close";
        private const string _btnPreviousClass = "btn-previous";
        private const string _btnNextClass = "btn-next";
        private const string _centerContainerClass = "center-container";
        private const string _mascotContainerClass = "mascot-container";
        private const string _mascotClass = "mascot";
        private const string _mascotFullBodyClass = "mascot-full-body";

        #endregion
        
        #region mascot

        private bool _showFullBodyMascot = false;
        
        #endregion
        
        #region window size variables

        private int _windowWidth = 550;
        private int _windowHeight = 300;
        private bool _windowHeightAuto = true;
        
        #endregion

        #region msg, btn and other variables

        private string _msgText;
        private string _previousBtnText;
        private string _skipBtnText;
        private string _nextBtnText;

        private bool _showPreviousBtn = true;
        private bool _showSkipBtn = true;
        private bool _showNextBtn = true;

        private bool _addVisualsWindow = false;
        
        #endregion

        #region building popup window

        public PopupWindow()
        {
            AddToClassList(_popupContainerClass);
            
            _visualsWindow = new VisualElement();
            _visualsWindow.AddToClassList(_popupClass);
            _visualsWindow.AddToClassList(_visualsWindowClass);
            hierarchy.Add(_visualsWindow);
            
            _window = new VisualElement();
            _window.AddToClassList(_popupClass);
            hierarchy.Add(_window);

            // _addVideoWindow = true;
            // _msgText =
            //     "Hier siehst du den aktuellen Grundwasserspiegel. Das ist die Hauptquelle unseres Trinkwassers. Leider ist da ziemlich wenig Wasser drin. Deshalb müssen wir was tun!";
            // _msgText =
            //     "Here you can see the groundwater level. This is the main source of our drinking water. It's pretty low right now. We have to take action!";
            // _nextBtnText = "Weiter";
            // _previousBtnText = "Zurück";
            // _skipBtnText = "Tutorial überspringen";
            // // ShowFullBodyMascot();
            // SetCenter();
            // // SetBottom(100);
            // Build();


            // SetTop(150);
            // SetLeft(55);
            // Build();
        }
        
        public PopupWindow Build()
        {
            if (_addVisualsWindow)
            {
                _visualsWindow.style.height = _windowHeight;
                _visualsWindow.style.width = _windowWidth;
            }
            else
            {
                _visualsWindow.style.display = DisplayStyle.None;
            }
            
            // set window width and height
            _window.style.width = _windowWidth;

            if (_windowHeightAuto)
            {
                _window.style.height = StyleKeyword.Auto;
            }
            else
            {
                _window.style.height = _windowHeight;
            }
            
            // text section
            VisualElement horizontalContainerText = new VisualElement();
            horizontalContainerText.AddToClassList(_textContainerClass);
            _window.Add(horizontalContainerText);

            Label msgLabel = new Label();
            msgLabel.text = _msgText;
            msgLabel.AddToClassList(_msgLabelClass);
            
            horizontalContainerText.Add(msgLabel);
            
            // button section
            VisualElement horizontalContainerButton = new VisualElement();
            horizontalContainerButton.AddToClassList(_btnContainerClass);
            _window.Add(horizontalContainerButton);

            if (_showPreviousBtn)
            {
                Button previousBtn = new Button() { text = _previousBtnText };
                previousBtn.AddToClassList(_btnClass);
                previousBtn.AddToClassList(_btnPreviousClass);
                previousBtn.clicked += OnPrevious;
                horizontalContainerButton.Add(previousBtn);
            }

            if (_showSkipBtn)
            {
                Button skipBtn = new Button() { text = _skipBtnText };
                skipBtn.AddToClassList(_btnClass);
                skipBtn.AddToClassList(_btnSkipClass);
                skipBtn.clicked += OnSkip;
                horizontalContainerButton.Add(skipBtn);   
            }

            if (_showNextBtn)
            {
                Button nextBtn = new Button() { text = _nextBtnText };
                nextBtn.AddToClassList(_btnClass);
                
                if (_isLastWindow)
                {
                    nextBtn.AddToClassList(_btnCloseClass);
                }
                else
                {
                    nextBtn.AddToClassList(_btnNextClass);
                }
            
                nextBtn.clicked += OnNext;
                horizontalContainerButton.Add(nextBtn);
            }

            // mascot section
            VisualElement mascotContainer = new VisualElement();
            mascotContainer.AddToClassList(_mascotContainerClass);
            
            if (_showFullBodyMascot)
            {
                mascotContainer.AddToClassList(_mascotFullBodyClass);
            }
            else
            {
                mascotContainer.AddToClassList(_mascotClass);
            }
            
            _window.Add(mascotContainer);
            
            return this;
        }

        #endregion

        #region set window size

        public PopupWindow SetWindowWidth(int width)
        {
            _windowWidth = width;
            return this;
        }

        public PopupWindow SetWindowHeight(int height)
        {
            _windowHeight = height;
            return this;
        }

        public PopupWindow SetWindowHeightAuto(bool isAuto)
        {
            _windowHeightAuto = isAuto;
            return this;
        }
        
        #endregion
        
        #region position setters

        public PopupWindow SetTop(int value, bool setAuto = false, bool ignoreVisualsWindow = false)
        {
            if (setAuto)
            {
                _window.style.top = StyleKeyword.Auto;
                _visualsWindow.style.top = ignoreVisualsWindow ? StyleKeyword.Auto : _visualsWindow.style.top = 1080 / 2.0f - _windowHeight * 1.5f;
                return this;
            }

            if (value >= _windowHeight + 20)
            {
                _visualsWindow.style.top = value - _windowHeight - 20;
            }
            else
            {
                _visualsWindow.style.top = value + _windowHeight;
            }
            
            _window.style.top = value;
            return this;
        }
        
        public PopupWindow SetVisualsWindowBottom(int value)
        {
            _visualsWindow.style.bottom = value;
            return this;
        }
        
        public PopupWindow SetRight(int value, bool setAuto = false)
        {
            if (setAuto)
            {
                _window.style.right = StyleKeyword.Auto;
                return this;
            }

            _visualsWindow.style.right = value;
            _window.style.right = value;
            return this;
        }

        public PopupWindow SetBottom(int value, bool setAuto = false, bool ignoreVisualsWindow = false)
        {
            if (setAuto)
            {
                _window.style.bottom = StyleKeyword.Auto;
                return this;
            }

            if (!ignoreVisualsWindow)
            {
                _visualsWindow.style.bottom = _windowHeightAuto ? value + _windowHeight : value + _windowHeight + 20;
            }
            
            _window.style.bottom = value;
            return this;
        }
        
        public PopupWindow SetLeft(int value, bool setAuto = false)
        {
            if (setAuto)
            {
                _window.style.left = StyleKeyword.Auto;
                return this;
            }

            _visualsWindow.style.left = value;
            _window.style.left = value;
            return this;
        }
        
        public PopupWindow SetCenter(bool ignoreVisualsWindow = false)
        {
            SetTop(0, true, ignoreVisualsWindow).SetRight(0, true).SetBottom(0, true).SetLeft(0, true);
            
            AddToClassList(_centerContainerClass);
            return this;
        }
        
        #endregion

        #region text setters and showBtn bools

        public PopupWindow SetMsgText(string text)
        {
            _msgText = text;
            return this;
        }
        
        public PopupWindow SetPreviousBtnText(string text)
        {
            _previousBtnText = text;
            return this;
        }
        
        public PopupWindow SetSkipBtnText(string text)
        {
            _skipBtnText = text;
            return this;
        }
        
        public PopupWindow SetNextBtnText(string text)
        {
            _nextBtnText = text;
            return this;
        }
        
        public PopupWindow ShowPreviousBtn()
        {
            _showPreviousBtn = false;
            return this;
        }
        
        public PopupWindow HidePreviousBtn()
        {
            _showPreviousBtn = false;
            return this;
        }
        
        public PopupWindow ShowSkipBtn()
        {
            _showSkipBtn = false;
            return this;
        }
        
        public PopupWindow HideSkipBtn()
        {
            _showSkipBtn = false;
            return this;
        }

        public PopupWindow HideNextBtn()
        {
            _showNextBtn = false;
            return this;
        }

        public PopupWindow HideAllBtns()
        {
            HidePreviousBtn();
            HideSkipBtn();
            HideNextBtn();

            return this;
        }

        public PopupWindow AddVisualsWindow(string className)
        {
            _addVisualsWindow = true;

            var visualsContainer = new VisualElement();
            visualsContainer.AddToClassList(className);
            _visualsWindow.Add(visualsContainer);
            return this;
        }
        
        #endregion

        #region button clicks and actions

        public event Action Previous;
        public event Action Skip;
        public event Action Next;
        
        private void OnPrevious()
        {
            Previous?.Invoke();
        }
        
        private void OnSkip()
        {
            Skip?.Invoke();
        }
        
        private void OnNext()
        {
            Next?.Invoke();
        }

        #endregion
        
        #region mascot setter

        public PopupWindow ShowFullBodyMascot()
        {
            _showFullBodyMascot = true;
            return this;
        }
        
        #endregion

        public PopupWindow SetIsLastWindow()
        {
            _isLastWindow = true;
            return this;
        }
    }
}
