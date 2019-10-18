using UnityEngine;
using Unity.Rendering;

public class Settings : MonoBehaviour
{
	public GameObject PlayerPrefab;

    public int[] SpellIDCoolDowns;
    public int[] PlayerSpellIDLevels;
/*    public AudioClip PlayerDeathClip;
    public AudioClip EnemyDeathClip;
*/

    public int ScytheLevel;
    public int BowLevel;
    public int SwordLevel;
    
    public bool isPaused;

    public float CamRayLen = 100f;
    public float CamSmoothing = 5f;
}
