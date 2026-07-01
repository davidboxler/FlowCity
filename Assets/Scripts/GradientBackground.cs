using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  GRADIENT BACKGROUND — pinta un Image con un gradiente vertical en runtime.
//  ----------------------------------------------------------------------------
//  Da profundidad a cielos y fondos sin costo de rendimiento. Los colores se
//  setean desde el builder (abajo/arriba). Se aplica al iniciar porque el sprite
//  se genera por código (no se serializa en la escena).
// ============================================================================
[RequireComponent(typeof(Image))]
public class GradientBackground : MonoBehaviour
{
    public Color abajo = Color.black;
    public Color arriba = Color.white;

    void Awake()
    {
        var img = GetComponent<Image>();
        img.sprite = TextureFactory.GradienteVertical(abajo, arriba);
        img.type = Image.Type.Simple;
        img.color = Color.white;   // mostrar el gradiente con sus colores
    }
}
