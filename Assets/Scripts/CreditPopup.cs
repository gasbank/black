using UnityEngine;

[DisallowMultipleComponent]
public class CreditPopup : MonoBehaviour
{
  public static CreditPopup Instance;

  [SerializeField]
  Subcanvas subcanvas;

  void Start()
  {
    subcanvas.Close();
  }

  internal void Open()
  {
    subcanvas.Open();
  }

  public void Close()
  {
    subcanvas.Close();
  }

  void OpenPopup()
  {
  }

  void ClosePopup()
  {
  }
}
