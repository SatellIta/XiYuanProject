using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TherapyLevelManager : MonoBehaviour
{

    [Header("引导箭头")]
    [SerializeField] private List<GameObject> guideArrows;
    [SerializeField] private GameObject arrowTowardsMeditationRoom;
    [SerializeField] private GameObject arrowTowardsMusicRoom;

    public void showGuideTowardsLevel(TherapyLevelID levelID)
    {
        
        if (levelID == TherapyLevelID.Mindfulness)
        {
            arrowTowardsMeditationRoom.SetActive(true);
        }
        else if (levelID == TherapyLevelID.MusicalJourney)
        {
            arrowTowardsMusicRoom.SetActive(true); 
        }
        else
        {
            return;
        }
        foreach (var arrow in guideArrows)
        {
            if (arrow != null)
            {
                arrow.SetActive(true);
            }
        }
    }
}
