using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  CALLE RUNTIME — pinta la calle en perspectiva al iniciar el juego.
//  ----------------------------------------------------------------------------
//  El sprite de la calle (trapecio) se genera por código en runtime, porque
//  TextureFactory crea texturas que no conviene generar desde el editor.
// ============================================================================
[RequireComponent(typeof(Image))]
public class CalleRuntime : MonoBehaviour
{
    void Start()
    {
        var img = GetComponent<Image>();
        img.sprite = TextureFactory.Calle(512, 760);
        img.type = Image.Type.Simple;
        img.color = Color.white; // que se vea el sprite con sus colores
    }
}
