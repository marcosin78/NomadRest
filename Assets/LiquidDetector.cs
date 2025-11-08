using UnityEngine;

public class LiquidDetector : MonoBehaviour
{
    private int totalDrops = 0;
    private int dropsInZone = 0;

    public float GetFillPercent()
    {
        if (totalDrops == 0) return 0f;
        return (float)dropsInZone / totalDrops;
    }

    void OnTriggerEnter(Collider other)
    {
        // Asegúrate de que los cubos de cerveza tengan un tag, por ejemplo "BeerDrop"
        if (other.CompareTag("BeerDrop"))
        {
            dropsInZone++;
            Debug.Log($"¡Gota detectada! Porcentaje actual: {GetFillPercent() * 100f}%");
            Destroy(other.gameObject); // Opcional: elimina la gota al entrar
        }
    }
    public void setTotalDrops(int total)
    {
        totalDrops = total;
        dropsInZone = 0;
        Debug.Log($"Total de gotas establecido a: {totalDrops}");
    }
}
