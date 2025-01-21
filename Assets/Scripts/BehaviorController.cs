using UnityEngine;

public class BehaviorController : MonoBehaviour
{
    public GameObject characterPrefab; // Prefab for the character
    public GameObject targetPrefab;    // Prefab for the target (e.g., power-up for seeking)
    public GameObject enemyPrefab;     // Prefab for the enemy (for fleeing)

    private GameObject character;
    private GameObject targetOrEnemy; // Holds the target (seek) or enemy (flee)

    public float speed = 5f; // Movement speed
    private RectTransform canvasRect; // Reference to the Canvas RectTransform

    private enum BehaviorState { None, Seeking, Fleeing }
    private BehaviorState currentState = BehaviorState.None;

    void Start()
    {
        // Find the Canvas and get its RectTransform
        canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
    }

    void Update()
    {
        // Handle key presses for switching behaviors
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchBehavior(BehaviorState.Seeking);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchBehavior(BehaviorState.Fleeing);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetScene();
        }

        // Execute the current behavior
        if (currentState == BehaviorState.Seeking && character != null && targetOrEnemy != null)
        {
            SeekTarget();
        }
        else if (currentState == BehaviorState.Fleeing && character != null && targetOrEnemy != null)
        {
            FleeFromEnemy();
        }
    }

    void ResetScene()
    {
        // Destroy existing objects
        DestroyExistingObjects();

        // Reset the behavior state to None
        currentState = BehaviorState.None;

        // Optionally, display a message or instruction that the scene has been reset
        Debug.Log("Scene reset. Press 1, 2, etc., to start a behavior.");
    }

    void SwitchBehavior(BehaviorState newState)
    {
        // Destroy existing objects
        DestroyExistingObjects();

        // Set the new state
        currentState = newState;

        // Spawn objects based on the new behavior
        switch (newState)
        {
            case BehaviorState.Seeking:
                SpawnCharacterAndTarget();
                break;

            case BehaviorState.Fleeing:
                SpawnCharacterAndEnemy();
                break;

            case BehaviorState.None:
                // Do nothing
                break;
        }
    }

    void DestroyExistingObjects()
    {
        if (character != null)
        {
            Destroy(character);
            character = null;
        }
        if (targetOrEnemy != null)
        {
            Destroy(targetOrEnemy);
            targetOrEnemy = null;
        }
    }

    void SpawnCharacterAndTarget()
    {
        Vector2 characterPos = GetRandomPosition();
        Vector2 targetPos = GetRandomPosition();

        character = Instantiate(characterPrefab, canvasRect);
        targetOrEnemy = Instantiate(targetPrefab, canvasRect);

        SetPosition(character, characterPos);
        SetPosition(targetOrEnemy, targetPos);
    }

    void SpawnCharacterAndEnemy()
    {
        Vector2 characterPos = GetRandomPosition();
        Vector2 enemyPos = GetRandomPosition();

        character = Instantiate(characterPrefab, canvasRect);
        targetOrEnemy = Instantiate(enemyPrefab, canvasRect);

        SetPosition(character, characterPos);
        SetPosition(targetOrEnemy, enemyPos);
    }

    Vector2 GetRandomPosition()
    {
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        float padding = 50f;

        return new Vector2(
            Random.Range(-canvasWidth / 2f + padding, canvasWidth / 2f - padding),
            Random.Range(-canvasHeight / 2f + padding, canvasHeight / 2f - padding)
        );
    }

    void SetPosition(GameObject obj, Vector2 position)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.rotation = Quaternion.identity;
    }

    void SeekTarget()
    {
        RectTransform charRect = character.GetComponent<RectTransform>();
        RectTransform targetRect = targetOrEnemy.GetComponent<RectTransform>();

        Vector2 direction = (targetRect.anchoredPosition - charRect.anchoredPosition).normalized;
        Vector2 newPosition = charRect.anchoredPosition + direction * speed * Time.deltaTime;

        // Ensure the new position is within the Canvas bounds
        newPosition = ClampToCanvasBounds(newPosition, charRect);

        charRect.anchoredPosition = newPosition;
        RotateTowardsDirection(charRect, direction);
    }

    void FleeFromEnemy()
    {
        RectTransform charRect = character.GetComponent<RectTransform>();
        RectTransform enemyRect = targetOrEnemy.GetComponent<RectTransform>();

        Vector2 direction = (charRect.anchoredPosition - enemyRect.anchoredPosition).normalized;
        Vector2 newPosition = charRect.anchoredPosition + direction * speed * Time.deltaTime;

        // Ensure the new position is within the Canvas bounds
        newPosition = ClampToCanvasBounds(newPosition, charRect);

        charRect.anchoredPosition = newPosition;
        RotateTowardsDirection(charRect, direction);
    }
    Vector2 ClampToCanvasBounds(Vector2 position, RectTransform rectTransform)
    {
        // Get the canvas size and the character's dimensions
        float canvasWidth = canvasRect.rect.width / 2f;
        float canvasHeight = canvasRect.rect.height / 2f;
        float charWidth = rectTransform.rect.width / 2f;
        float charHeight = rectTransform.rect.height / 2f;

        // Clamp the position to ensure the character stays within the visible canvas area
        position.x = Mathf.Clamp(position.x, -canvasWidth + charWidth, canvasWidth - charWidth);
        position.y = Mathf.Clamp(position.y, -canvasHeight + charHeight, canvasHeight - charHeight);

        return position;
    }

    void RotateTowardsDirection(RectTransform rect, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0, 0, angle);
    }
}

