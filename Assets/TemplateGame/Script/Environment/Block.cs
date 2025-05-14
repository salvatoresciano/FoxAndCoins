using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Block : MonoBehaviour, IPlayerRespawnListener
{

    public LayerMask enemiesLayer;

    public int maxHit = 1;
    public float pushEnemyUp = 7f;
    public float sizeDetectEnemies = 0.25f;
    public int pointToAdd = 100;

    [Header("Destroyable")]
    public GameObject DestroyEffect;

    [Header("HidenTreasure")]
    public GameObject[] Treasure;

    [Header("Sound")]
    public AudioClip soundDestroy;
    [Range(0, 1)]
    public float soundDestroyVolume = 0.5f;
    public AudioClip soundSpawn;
    [Range(0, 1)]
    public float soundSpawnVolume = 0.5f;

    int currentHitLeft;

    static int blockCounter = 0;

    public Tilemap tilemap; 

    public Vector2 cellOffset;

    void Start()
    {
        currentHitLeft = maxHit;

        // Assicurati che la Tilemap sia correttamente assegnata
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }

        currentHitLeft = Mathf.Clamp(maxHit, 1, int.MaxValue);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica che l'oggetto che ha colpito sia il Player
        var player = other.gameObject.GetComponent<Player>();
        if (player == null)
            return;

        if (currentHitLeft <= 0)
            return;

       
        Vector3 playerPosition = other.bounds.center;

        Vector3Int cellPosition = tilemap.WorldToCell(playerPosition);

        cellPosition = new Vector3Int(cellPosition.x, (int)(cellPosition.y + cellOffset.y), cellPosition.z);

     
        //Debug.Log($"Posizione della cella colpita: {cellPosition}");

        if (tilemap.HasTile(cellPosition))
        {
            Debug.Log("Tile at " + cellPosition);
            HandleTileHit(cellPosition);
        }
        else
        {
            Debug.Log("No tile.");
            return;
        }

        

        CheckEnemiesOnTop(cellPosition);

        GameObject random = null;

        if (blockCounter >= 4)
        {
            blockCounter = 0;
            QuizManager.instance?.ShowQuiz();
            SoundManager.PlaySfx(soundSpawn, soundSpawnVolume);

        }
        else
        {



            Vector3 worldPosition = tilemap.CellToWorld(cellPosition);

            Vector3 spawnPosition = new Vector3(worldPosition.x + 0.5f, worldPosition.y + 0.5f, worldPosition.z);

            random = (Treasure.Length > 0) ? Treasure[Random.Range(0, Treasure.Length)] : null;
            if (random != null)
            {
                Instantiate(random, spawnPosition, Quaternion.identity); //anm
                blockCounter++;
                SoundManager.PlaySfx(soundSpawn, soundSpawnVolume);
            }

        }

        currentHitLeft = maxHit;
    }

    void HandleTileHit(Vector3Int cellPosition)
    {
        currentHitLeft--;

        // Se i colpi rimanenti sono esauriti, distruggi la tile
        if (currentHitLeft <= 0)
        {

            SoundManager.PlaySfx(soundDestroy, soundDestroyVolume);


            if (DestroyEffect != null)
            {
                Vector3 worldPosition = tilemap.CellToWorld(cellPosition);
                Vector3 spawnPosition = new Vector3(worldPosition.x + 0.5f, worldPosition.y + 0.5f, worldPosition.z);
                Instantiate(DestroyEffect, spawnPosition, Quaternion.identity);
            }
              

            if (pointToAdd != 0)
                GameManager.Instance.ShowFloatingText(pointToAdd.ToString(), transform.position, Color.white);

            tilemap.SetTile(cellPosition, null);
            Debug.Log("Tile distrutta.");

        }
        else
        {
            Debug.Log($"Colpi rimanenti per il blocco: {currentHitLeft}");
        }
    }


    void CheckEnemiesOnTop(Vector3Int cellPosition)
    {
        // Imposta la posizione sopra la tile (se la tua tile è grande 1x1, basta incrementare di 1 unità)
        Vector3 checkPosition = new Vector3(cellPosition.x, cellPosition.y + 1f, cellPosition.z);

        // DEBUG: Controlla la posizione in cui stai cercando i nemici
        Debug.Log($"Verifica nemici sopra la tile in posizione: {checkPosition}");

        // Controlla se ci sono nemici sopra la tile
        var hits = Physics2D.CircleCastAll(checkPosition, sizeDetectEnemies, Vector2.zero, 0, 1 << LayerMask.NameToLayer("Enemies"));

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                var damage = hit.collider.gameObject.GetComponent<ICanTakeDamage>();
                if (damage != null)
                {
                    // Applica danno al nemico
                    damage.TakeDamage(10000, Vector2.up * pushEnemyUp, gameObject);
                    Debug.Log("Nemico trovato e danneggiato.");
                }
            }
        }
        else
        {
            // DEBUG: Nessun nemico trovato
            Debug.Log("Nessun nemico trovato sopra la tile.");
        }
    }


    public void OnPlayerRespawnInThisCheckPoint(CheckPoint checkpoint, Player player)
    {
        //spriteRenderer.sprite = oldSprite;
        currentHitLeft = Mathf.Clamp(maxHit, 1, int.MaxValue);
        //gameObject.SetActive(true);
        //should reset all destroyed tiles

    }

    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPoint.position, sizeDetectEnemies);
    }*/
}

