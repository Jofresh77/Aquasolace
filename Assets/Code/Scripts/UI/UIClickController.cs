using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.UI
{
    public static class UIClickController
    {
        public static bool IsClickOnUi(IPanel panel)
        {
            if (panel == null) return false;
            
            var height = Screen.height;
            Vector3 mousePos = Input.mousePosition;
            // panel pos start from bottom but mouse pos start from top
            // so we need to get screen height - mousePos.y
            var vector3 = new Vector3(mousePos.x, height - mousePos.y);
            var element = panel.Pick(vector3);

            return element != null && element.visible && element.style.display != DisplayStyle.None;
        }
    }
}