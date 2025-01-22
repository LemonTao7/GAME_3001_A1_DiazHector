using UnityEngine;

public class BehaviorController : MonoBehaviour
{
    public GameObject characterPrefab; 
    public GameObject targetPrefab;    
    public GameObject enemyPrefab;     
    public GameObject obstaclePrefab;  

    private GameObject character;
    private GameObject targetOrEnemy; 
    private GameObject obstacle;      

    public float speed = 5f; 
    private RectTransform canvasRect; 

    private enum BehaviorState { None, Seeking, Fleeing, Arrival, Avoidance }
    private BehaviorState currentState = BehaviorState.None;

    void Start()
    {
       
        canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchBehavior(BehaviorState.Seeking);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchBehavior(BehaviorState.Fleeing);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchBehavior(BehaviorState.Arrival);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchBehavior(BehaviorState.Avoidance);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetScene();
        }

        
        if (currentState == BehaviorState.Seeking && character != null && targetOrEnemy != null)
        {
            SeekTarget();
        }
        else if (currentState == BehaviorState.Fleeing && character != null && targetOrEnemy != null)
        {
            FleeFromEnemy();
        }
        else if (currentState == BehaviorState.Arrival && character != null && targetOrEnemy != null)
        {
            ArriveAtTarget();
        }
        else if (currentState == BehaviorState.Avoidance && character != null && targetOrEnemy != null && obstacle != null)
        {
            AvoidObstacle();
        }
    }

    void ResetScene()
    {
        
        DestroyExistingObjects();

        
        currentState = BehaviorState.None;

        Debug.Log("Scene reset. Press 1, 2, etc., to start a behavior.");
    }

    void SwitchBehavior(BehaviorState newState)
    {
        
        DestroyExistingObjects();

        
        currentState = newState;

        
        switch (newState)
        {
            case BehaviorState.Seeking:
                SpawnCharacterAndTarget();
                break;

            case BehaviorState.Fleeing:
                SpawnCharacterAndEnemy();
                break;

            case BehaviorState.Arrival:
                SpawnCharacterAndTarget();
                break;

            case BehaviorState.Avoidance:
                SpawnCharacterTargetAndObstacle();
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
        if (obstacle != null)
        {
            Destroy(obstacle);
            obstacle = null;
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

    void SpawnCharacterTargetAndObstacle()
{
    
    Vector2 characterPos = GetRandomPosition();
    Vector2 targetPos = GetRandomPosition();

    Vector2 obstaclePos = Vector2.Lerp(characterPos, targetPos, 0.5f); 

    
    character = Instantiate(characterPrefab, canvasRect);
    targetOrEnemy = Instantiate(targetPrefab, canvasRect);
    obstacle = Instantiate(obstaclePrefab, canvasRect);

    SetPosition(character, characterPos);
    SetPosition(targetOrEnemy, targetPos);
    SetPosition(obstacle, obstaclePos);
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

        newPosition = ClampToCanvasBounds(newPosition, charRect);

        charRect.anchoredPosition = newPosition;
        RotateTowardsDirection(charRect, direction);
    }

    void ArriveAtTarget()
    {
        RectTransform charRect = character.GetComponent<RectTransform>();
        RectTransform targetRect = targetOrEnemy.GetComponent<RectTransform>();

        Vector2 direction = targetRect.anchoredPosition - charRect.anchoredPosition;
        float distance = direction.magnitude;

        if (distance > 10f)
        {
            direction.Normalize();

            float slowingSpeed = Mathf.Lerp(0, speed, distance / 200f);
            Vector2 newPosition = charRect.anchoredPosition + direction * slowingSpeed * Time.deltaTime;

            newPosition = ClampToCanvasBounds(newPosition, charRect);

            charRect.anchoredPosition = newPosition;
            RotateTowardsDirection(charRect, direction);
        }
        else
        {
            charRect.anchoredPosition = targetRect.anchoredPosition;
        }
    }

    void AvoidObstacle()
    {
        RectTransform charRect = character.GetComponent<RectTransform>();
        RectTransform targetRect = targetOrEnemy.GetComponent<RectTransform>();
        RectTransform obstacleRect = obstacle.GetComponent<RectTransform>();

        Vector2 toTarget = targetRect.anchoredPosition - charRect.anchoredPosition;
        Vector2 toObstacle = obstacleRect.anchoredPosition - charRect.anchoredPosition;
        float obstacleDistance = toObstacle.magnitude;

        Vector2 avoidance = Vector2.zero;
        if (obstacleDistance < 150f)
        {
            avoidance = -toObstacle.normalized * (150f - obstacleDistance) / 150f;
        }

        Vector2 combinedDirection = (toTarget.normalized + avoidance).normalized;

        Vector2 newPosition = charRect.anchoredPosition + combinedDirection * speed * Time.deltaTime;

        newPosition = ClampToCanvasBounds(newPosition, charRect);

        charRect.anchoredPosition = newPosition;
        RotateTowardsDirection(charRect, combinedDirection);

        if (toTarget.magnitude < 10f)
        {
            charRect.anchoredPosition = targetRect.anchoredPosition;
        }
    }

    Vector2 ClampToCanvasBounds(Vector2 position, RectTransform rectTransform)
    {
        float canvasWidth = canvasRect.rect.width / 2f;
        float canvasHeight = canvasRect.rect.height / 2f;
        float charWidth = rectTransform.rect.width / 2f;
        float charHeight = rectTransform.rect.height / 2f;

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
