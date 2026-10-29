# Interfaz y sistema de stamina

Este documento describe la interfaz y el sprint implementados en la escena de desarrollo `Assets/OndaS/Art/Scenes/OndaS_RetroLookDev.unity`. El jugador utiliza actualmente `OndaSLookDevWalkthrough`, un controlador de prueba; la stamina está integrada en ese controlador y debe trasladarse o reutilizarse cuando se sustituya por el controlador definitivo.

## Controles y elementos en pantalla

| Control | Acción |
| --- | --- |
| `W`, `A`, `S`, `D` | Desplazarse. |
| `Shift` izquierdo o derecho | Correr mientras el jugador se desplaza, está de pie y tiene stamina disponible. |
| `C` | Alternar entre agachado y de pie. No se puede correr agachado ni mientras se solicita agacharse. |
| `E` | Interactuar con la puerta u objeto al que apunta la cámara. |
| `1`–`5` | Seleccionar un espacio de la mochila. |
| `Q` | Soltar el objeto del espacio seleccionado. |
| `Esc` / clic izquierdo | Liberar / volver a capturar el cursor. |

Los avisos de puertas y recolección aparecen cerca del centro, en amarillo y sin fondo negro. La interfaz también muestra mensajes de mochila llena, exceso de peso y resultado al soltar objetos. El inventario aparece en el centro inferior con cinco espacios, el peso y los controles; el espacio seleccionado lleva `>`. La barra de stamina aparece en la esquina inferior derecha con su porcentaje y cambia el texto a `AGOTADO` al alcanzar cero.

Las interacciones, selección y suelta requieren que el cursor esté capturado. La distancia inicial del raycast de interacción es 2,5 m y se puede ajustar en `OndaSDoorInteractor`. El funcionamiento de la mochila y la configuración de los ítems se detallan en [OndaS_Inventario.md](OndaS_Inventario.md).

## Tipografía

La UI usa TextMeshPro y **DHF Story Brush** como fuente predeterminada. Las fuentes y sus assets SDF están en `Assets/Fonts/DHFStoryBrush/`; la configuración de TMP está en `Assets/TextMesh Pro/Resources/TMP Settings.asset`. La variante Slanted está disponible mediante `Assets/OndaS/Resources/OndaSTypography.asset`, que contiene ambas referencias.

**Cobertura:** las versiones proporcionadas de DHF Story Brush carecen de números y de varios caracteres españoles, incluidos `á é í ó ú ñ Ñ ü Ü ¿ ¡`. Los assets utilizan Liberation Sans SDF como fuente de respaldo; esos caracteres se pueden mostrar, pero tendrán una apariencia diferente. Para una tipografía uniforme se necesita una versión de DHF Story Brush que incluya los glifos faltantes y regenerar sus assets TMP.

El archivo `Assets/Fonts/DHFStoryBrush/License.txt` indica que las fuentes suministradas son solo para uso personal y pide contactar al autor para uso comercial. Los originales de Descargas se conservaron.

### Aplicar el estilo a textos nuevos

TMP usa la fuente normal configurada por defecto cuando se crea un texto nuevo. Para aplicar además el amarillo `#FFDA41` y añadir el componente Scribble, los scripts de UI deben llamar a `OndaSTypography.Apply`:

```csharp
using OndaS.Gameplay.UI;
using TMPro;

// label es un TMP_Text o TextMeshProUGUI ya creado.
OndaSTypography.Apply(label);

// Para la variante inclinada:
OndaSTypography.Apply(label, useSlanted: true);
```

El color y la animación no se añaden automáticamente a todos los textos TMP arbitrarios: deben aplicarse con este método o configurarse explícitamente. `Apply` no duplica el componente si ya existe. Los avisos, el inventario y la etiqueta de stamina lo utilizan. Los campos `Regular` y `Slanted` de `OndaSTypography.Instance` también pueden consultarse desde otros scripts.

## Animación Scribble

`Assets/OndaS/Gameplay/UI/OndaSScribbleText.cs` desplaza cada carácter y deforma ligeramente las esquinas de sus vértices mediante `OnPreRenderText` de TMP. Esto incluye los caracteres que utilizan materiales de respaldo. No cambia el texto ni sus posiciones de layout y no modifica un material compartido.

| Campo del componente | Valor inicial | Función |
| --- | ---: | --- |
| `Displacement` | 1 | Intensidad del desplazamiento en unidades locales del texto. |
| `Corner Distortion` | 0,45 | Proporción de deformación adicional de las esquinas. |
| `Drawings Per Second` | 8 | Número de variaciones por segundo. |

El efecto usa tiempo no escalado, por lo que puede seguir animándose cuando `Time.timeScale` es cero. Los componentes de estas interfaces se crean durante Play; sus valores por defecto se definen en el script, aunque los valores de una instancia pueden ajustarse en el Inspector durante la ejecución.

## Stamina

`OndaSLookDevWalkthrough` inicia con la stamina llena. Solo consume stamina mientras hay una dirección de movimiento solicitada, Shift está pulsado y el jugador puede correr. Mantener Shift sin desplazarse no la consume. Si se deja de correr —incluido al agacharse— comienza la espera de recuperación. El cálculo usa `Time.deltaTime`, por lo que se detiene con el tiempo del juego.

| Campo del Inspector | Valor inicial | Función |
| --- | ---: | --- |
| `Max Stamina` | 100 | Capacidad máxima. |
| `Sprint Drain Per Second` | 20 | Consumo por segundo de sprint: aproximadamente 5 s desde la barra llena. |
| `Stamina Recovery Per Second` | 15 | Recuperación por segundo una vez terminada la espera. |
| `Stamina Recovery Delay` | 1 s | Tiempo sin correr antes de recuperar stamina. |
| `Exhausted Recovery Fraction` | 0,25 | Fracción de la capacidad necesaria para salir del agotamiento. |

Al llegar a cero se detiene el sprint. Para volver a correr se requiere **soltar Shift al menos una vez** y recuperar el umbral configurado; una vez cumplidas ambas condiciones se puede volver a pulsar Shift. La recuperación también ocurre mientras se camina o se permanece quieto. `IsSprinting` sigue siendo la señal utilizada por la animación del personaje; `Stamina`, `MaxStamina` e `IsExhausted` están disponibles para otros componentes.

Para ajustar los valores, selecciona el jugador de la escena y busca la sección **Stamina** del componente `OndaSLookDevWalkthrough`. La barra se crea en tiempo de ejecución mediante `OndaSStaminaHud`; su etiqueta usa la tipografía y el Scribble comunes. La UI de stamina y la de inventario usan `CanvasScaler` con referencia de 1920 × 1080.

## Archivos y alcance actual

| Archivo | Responsabilidad |
| --- | --- |
| `Assets/OndaS/Gameplay/UI/OndaSTypography.cs` | Referencias de fuentes y aplicación del estilo a textos. |
| `Assets/OndaS/Gameplay/UI/OndaSScribbleText.cs` | Animación de vértices TMP. |
| `Assets/OndaS/Gameplay/UI/OndaSStaminaHud.cs` | Barra y texto de stamina. |
| `Assets/OndaS/Gameplay/Items/OndaSInventoryHud.cs` | Nombres de los cinco espacios, selección y peso. |
| `Assets/OndaS/Gameplay/Doors/OndaSDoorInteractor.cs` | Avisos, interacción compartida con puertas e ítems y creación del HUD de inventario. |
| `Assets/OndaS/Art/Rendering/OndaSLookDevWalkthrough.cs` | Movimiento de prueba, sprint, stamina y creación de su HUD. |

Los canvas de avisos, mochila y stamina se crean en Play, por lo que no aparecen como objetos permanentes de la escena en modo edición. La interfaz actual de inventario muestra nombres, no iconos. El sistema no implementa todavía pulso, efectos del agua o la mascarilla sobre la stamina, guardado entre escenas ni las demás fases del juego.

Durante la implementación se comprobó en Unity la compilación, la cobertura de caracteres con la fuente de respaldo, la variación de vértices Scribble, la recogida y suelta, los límites de inventario y las transiciones principales de stamina. Se revisó la presencia de los HUD en Game; su legibilidad y equilibrio deben seguir probándose durante el desarrollo.
