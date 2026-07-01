using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  ROUNDED IMAGE — redondea las esquinas de un Image en runtime.
//  ----------------------------------------------------------------------------
//  Los sprites generados por código (TextureFactory) NO se guardan en la escena,
//  por eso se asignan al iniciar (mismo patrón que CalleRuntime). El color del
//  Image se conserva: el sprite es blanco y se tiñe con Image.color.
// ============================================================================
[RequireComponent(typeof(Image))]
public class RoundedImage : MonoBehaviour
{
    public int radius = 18;

    void Awake()
    {
        var img = GetComponent<Image>();
        img.sprite = TextureFactory.RoundedRect(radius);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1f;   // que el radio quede en px reales
    }
}
