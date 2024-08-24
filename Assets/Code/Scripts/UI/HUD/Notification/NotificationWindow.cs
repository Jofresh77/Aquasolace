using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI.HUD.Notification
{
    public class NotificationWindow : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<NotificationWindow> { }

        private readonly VisualElement _window;

        #region uss classes

        private const string NotificationContainerClass = "notificationContainer";
        private const string RestrictionClass = "restriction";
        private const string AchievementClass = "achievement";
        private const string WindowClass = "window";
        private const string TextContainerClass = "text-container";
        private const string MsgLabelClass = "msg";
        private const string ImageContainerClass = "image-container";
        #endregion

        #region msg + showImage bool

        private string _msgText;
        private bool _showImage = true;
        private readonly VisualElement _image;
        private float _lifetime = 3f;

        #endregion

        #region building notification window

        public NotificationWindow()
        {
            AddToClassList(NotificationContainerClass);
            
            _window = new VisualElement();
            _window.AddToClassList(WindowClass);
            hierarchy.Add(_window);

            // create image element so we can use it anywhere
            _image = new VisualElement();
            
            /*
             * debug and testing
             */
            // _msgText = "You have reached a quest and earned +3 forest deciduous.";
            // _msgText = "-9 farmland";
            // ShowImage(false);
            // Build();
            // SetImage(Resources.Load<Texture2D>("farmland_preview"));
        }
        
        public NotificationWindow Build()
        {
            if (_showImage)
            {
                // picture section
                VisualElement imageContainer = new VisualElement();
                imageContainer.AddToClassList(ImageContainerClass);
                _window.Add(imageContainer);
                
                _image.AddToClassList("image");
                imageContainer.Add(_image);   
            }
            
            // text section
            VisualElement horizontalContainerText = new VisualElement();
            horizontalContainerText.AddToClassList(TextContainerClass);
            _window.Add(horizontalContainerText);

            Label msgLabel = new Label();
            msgLabel.text = _msgText;
            msgLabel.AddToClassList(MsgLabelClass);

            if (!_showImage)
            {
                // text can take the whole width
                msgLabel.style.width = Length.Percent(100);
            }
            
            horizontalContainerText.Add(msgLabel);
            
            return this;
        }

        #endregion

        #region text and image setters

        public NotificationWindow SetMsgText(string text)
        {
            _msgText = text;
            return this;
        }

        public NotificationWindow ShowImage(bool showImage)
        {
            _showImage = showImage;
            return this;
        }

        public NotificationWindow SetImage(Texture2D image)
        {
            if (!_showImage) return this;
            
            _image.style.backgroundImage = image;
            return this;
        }

        public NotificationWindow SetLifetimeInSeconds(float lifetime)
        {
            _lifetime = lifetime;
            return this;
        }

        public NotificationWindow Restriction()
        {
            _window.AddToClassList(RestrictionClass);
            return this;
        }

        public NotificationWindow Achievement()
        {
            _window.AddToClassList(AchievementClass);
            return this;
        }
            
        #endregion
        
        #region getters

        public float GetLifeTimeInSeconds()
        {
            return _lifetime;
        }
        
        #endregion
        
        #region window height setter
        
        public NotificationWindow SetWindowHeight(int height)
        {
            style.height = height;
            return this;
        }

        public NotificationWindow UseAutomaticWindowHeight()
        {
            int averageCharsPerLine = 18;
            const float lineHeightInPixels = 28.75f;

            if (_showImage)
            {
                averageCharsPerLine = 12;
            }

            if (string.IsNullOrEmpty(_msgText)) return this;
            
            var totalCharacters = _msgText.Length;
            var numberOfLines = Mathf.CeilToInt((float)totalCharacters / averageCharsPerLine);
            var textBoxHeight = numberOfLines * lineHeightInPixels;
            
            style.height = Mathf.CeilToInt(textBoxHeight);

            return this;
        }
        
        #endregion
    }
}
