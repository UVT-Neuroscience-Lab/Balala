using Gtec.UnityInterface;
using System;
using Unity.VisualScripting;
using UnityEngine;
using static Gtec.UnityInterface.BCIManager;

public class ClassSelectionAvailableExample : MonoBehaviour
{
    private Player playerObj;

    private uint _selectedClass = 0;
    private bool _update = false;
    void Start()
    {
        playerObj = GameObject.FindFirstObjectByType<Player>();
        //attach to class selection available event
        BCIManager.Instance.ClassSelectionAvailable += OnClassSelectionAvailable;
    }

    void OnApplicationQuit()
    {
        //detach from class selection available event
        BCIManager.Instance.ClassSelectionAvailable -= OnClassSelectionAvailable;
    }

    void Update()
    {
        if(_update)
        {
            int index = (int)_selectedClass;
            if (index > 0)
            {
                PlayingCard currentCard = playerObj.GetCardInHand(index - 1);
                Debug.Log("Current card suit: " + currentCard.suit);
                currentCard.SelectCard();
            }
            _update = false;
        } 
    }

    /// <summary>
    /// This event is called whenever a new class selection is available. Th
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnClassSelectionAvailable(object sender, EventArgs e)
    {
        ClassSelectionAvailableEventArgs ea = (ClassSelectionAvailableEventArgs)e;
       _selectedClass = ea.Class;
        _update = true;
        Debug.Log(string.Format("Selected class: {0}", ea.Class));
    }
}
