using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
//  NAVEGACIÓN DE MENÚS — FlowCity
//  ----------------------------------------------------------------------------
//  Guarda qué tipo de examen eligió el usuario (Moto / Auto) y maneja la
//  navegación entre las escenas:  MenuPrincipal → SeleccionVehiculo → FlowCity
//
//  La elección se guarda en una variable estática (sobrevive el cambio de escena)
//  para usarla más adelante (por ahora ambos llevan al mismo examen).
// ============================================================================
public enum TipoExamen { Auto, Moto }

public class SeleccionExamen : MonoBehaviour
{
    [SerializeField] private string escenaSeleccion = "SeleccionVehiculo";
    [SerializeField] private string escenaJuego = "FlowCity";

    // Elegido por el usuario, accesible desde cualquier escena.
    public static TipoExamen ExamenElegido = TipoExamen.Auto;

    // --- Botones del menú principal ---
    public void IrASeleccion() { SceneManager.LoadScene(escenaSeleccion); }
    public void Salir()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- Botones del selector de vehículo ---
    public void ElegirAuto() { ExamenElegido = TipoExamen.Auto; SceneManager.LoadScene(escenaJuego); }
    public void ElegirMoto() { ExamenElegido = TipoExamen.Moto; SceneManager.LoadScene(escenaJuego); }

    // --- Volver atrás (del selector al menú) ---
    public void VolverAlMenu() { SceneManager.LoadScene("MenuPrincipal"); }
}
