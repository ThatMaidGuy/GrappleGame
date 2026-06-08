using UnityEngine;

public class AttackHook : MonoBehaviour
{
    // Ссылка на скрипт игрока, чтобы управлять состоянием крюка
    [SerializeField] private PlayerScript playerScript;

    private void Start()
    {
        // Если забыл перетащить ссылку в инспекторе, попробуем найти её в родительских объектах
        if (playerScript == null)
        {
            playerScript = GetComponentInParent<PlayerScript>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что врезались именно в дрона
        if (other.TryGetComponent(out DroneController drone))
        {
            // Наносим урон дрону (первый удар отбросит, второй уничтожит)
            drone.Hurt();

            // Если игрок в этот момент куда-то притягивался — останавливаем трос
            if (playerScript != null)
            {
                playerScript.StopGrapple();
            }
        }
    }
}