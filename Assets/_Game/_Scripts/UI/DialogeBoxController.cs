using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogeBoxController : MonoBehaviour
{
    [Header("Dialoge Box")]
    [SerializeField] private PanelUI dialogeBox;
    [Header("Dialoge Box Settings")]
    [SerializeField] private float lifeTime = 5;
    [SerializeField] private float timeToShow = 1.5f;


    private void Start()
    {
        StartCoroutine(ShowDialogeBox());
    }

    private IEnumerator ShowDialogeBox()
    {
        yield return new WaitForSeconds(timeToShow);
        dialogeBox.Show();
        StartCoroutine(HideDialogeBox());
    }

    private IEnumerator HideDialogeBox()
    {
        yield return new WaitForSeconds(lifeTime);
        dialogeBox.Hide();

    }

}
