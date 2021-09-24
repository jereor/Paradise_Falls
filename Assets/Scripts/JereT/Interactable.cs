using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent itemEvent;

    private void Start()
    {
        if (itemEvent == null)
        {
            itemEvent = new UnityEvent();
        }
        
        itemEvent.AddListener(Interact);
    }

    public virtual void Interact()
    {
        Debug.Log("Interacted with: " + gameObject.transform.name);
    }
}
