using UnityEngine;

namespace SimplePieMenu
{
    public class PieMenuInputController : MonoBehaviour
    {
        [SerializeField] private PieMenu pieMenu;
        [SerializeField] private KeyCode toggleKey = KeyCode.Q;
        [SerializeField] private bool pauseOnOpen = true;

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                bool isActive = pieMenu.PieMenuInfo != null && pieMenu.PieMenuInfo.IsActive;
                PieMenuShared.References.PieMenuToggler.SetActive(pieMenu, !isActive);
            }
        }
    }
}
