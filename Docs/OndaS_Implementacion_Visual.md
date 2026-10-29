# OndaS — Estado de la implementación visual

**Actualizado:** 28 de octubre de 2026.  
**Estado:** primera prueba visual en Unity, centrada en el cuarto piso.

Este documento describe lo que ya está implementado y cómo probarlo. La dirección artística y las ideas para etapas posteriores están en [OndaS_Direccion_Visual_PSX.md](OndaS_Direccion_Visual_PSX.md); ese archivo es un plan, por lo que no debe interpretarse como una lista de funciones terminadas.

## Propósito y alcance

El objetivo es explorar una estética **PSX Horror / Retro Low-Poly Horror**: imagen pixelada, colores algo apagados, superficies menos brillantes, polvo y una iluminación que pasa de un ambiente doméstico tenue a la oscuridad con linterna.

La zona elegida para la prueba es `Structure_03/Interior/4th Floor`: incluye el departamento, pasillos, puertas y objetos bajo esa jerarquía. La conversión de materiales y el ajuste de luces se aplicaron a ese grupo. La cámara retro y las condiciones ambientales afectan la vista completa de la escena de prueba; el control de apagón también actúa sobre las luces de toda esa escena durante Play.

| Escena | Ruta | Uso |
| --- | --- | --- |
| Original del kit | `Assets/Brick Project Studio/Apartment Kit/Scenes/Scene_02.unity` | Punto de partida |
| Prueba visual | `Assets/OndaS/Art/Scenes/OndaS_RetroLookDev.unity` | Copia donde se hicieron los cambios y se ubica al jugador en el cuarto piso |

La prueba conserva el edificio completo como contexto. Las variantes de materiales y texturas son archivos propios de OndaS; no se editaron los materiales originales del kit para conseguir este acabado.

## Cómo abrir y recorrer la prueba

1. Abrir `Assets/OndaS/Art/Scenes/OndaS_RetroLookDev.unity` en Unity.
2. Entrar en **Play** y seleccionar la pestaña **Game**. El jugador comienza en el departamento del cuarto piso.
3. Usar los controles de la tabla para comparar la iluminación y recorrer la muestra.
4. Salir de Play al terminar. Los objetos temporales de presentación y linterna se crean durante la ejecución.

| Control | Acción en la prueba |
| --- | --- |
| `W`, `A`, `S`, `D` | Desplazarse |
| Ratón | Girar la vista mientras el cursor está capturado |
| `B` | Alternar entre preparación y apagón; el apagón enciende la linterna |
| `F` | Encender o apagar la linterna de prueba |
| `E` | Abrir o cerrar una puerta animada cercana a la que se esté apuntando |
| `Esc` | Liberar el cursor; hacer clic en Game para capturarlo otra vez |

También se puede alternar el apagón desde **OndaS > Alternar apagón de prueba** mientras el juego está en Play. Estos controles son herramientas temporales para observar el arte, no las mecánicas definitivas del juego.

Capturas de referencia de esta iteración:

- [Preparación](../Assets/OndaS/Art/Captures/Resultado_Preparacion.png).
- [Apagón y linterna](../Assets/OndaS/Art/Captures/Resultado_Apagon.png).
- [Vista tras corregir la cámara de salida](../Assets/OndaS/Art/Captures/Camara_corregida.png).

Las capturas representan encuadres y ajustes del momento en que se tomaron; la valoración del estilo debe hacerse también en movimiento dentro de Unity.

## Recursos creados

| Recurso | Ruta | Función |
| --- | --- | --- |
| Escena | `Assets/OndaS/Art/Scenes/OndaS_RetroLookDev.unity` | Copia del edificio con los cambios de prueba |
| Cámara retro | `Assets/OndaS/Art/Rendering/OndaSRetroCamera.cs` | Renderiza el mundo a baja resolución y lo presenta en pantalla |
| Recorrido | `Assets/OndaS/Art/Rendering/OndaSLookDevWalkthrough.cs` | Movimiento de prueba, ratón, linterna y cambio de iluminación |
| Shader de composición | `Assets/OndaS/Art/Shaders/OndaSRetroComposite.shader` | Color, dithering y viñeta sobre la imagen del mundo |
| Material de composición | `Assets/OndaS/Art/Materials/RetroComposite.mat` | Parámetros visibles del shader de composición |
| Shader de polvo | `Assets/OndaS/Art/Shaders/OndaSDust.shader` | Partículas translúcidas de polvo |
| Material de polvo | `Assets/OndaS/Art/Materials/RetroDust.mat` | Material asignado a los emisores de prueba |
| Materiales y texturas | `Assets/OndaS/Art/Materials/` y `Assets/OndaS/Art/Textures/` | Variantes usadas en el cuarto piso |
| Herramienta de editor | `Assets/OndaS/Editor/OndaSLookDevSetup.cs` | Creación y actualización de la prueba desde el menú OndaS |

El proyecto usa Unity **6000.5.9f1** y **URP 17.5.0**. La escena de prueba utiliza la configuración de renderizado existente del proyecto.

## Materiales y texturas del cuarto piso

La herramienta de preparación recorre los renderers bajo `4th Floor` y crea copias de los materiales cuyo shader es `Universal Render Pipeline/Lit`. En la generación inicial se actualizaron **1004 renderers**, con **75 variantes de materiales** y **41 entradas de texturas** procesadas. Los valores son el resultado registrado en esa generación, no una garantía para futuras modificaciones de la escena.

Para materiales opacos se aplica un tinte algo más cálido y apagado, se limita el brillo (`Smoothness`) a un máximo de `0.25` y se reduce la intensidad del mapa de normales al 35 % de su valor original. Las variantes conservan el shader URP/Lit y las demás propiedades copiadas del material. Los materiales transparentes se tratan por separado y no reciben esos cambios de brillo, normales o tinte opaco.

Cuando la textura base es PNG, JPG, JPEG o TGA, se crea una copia con filtrado **Point**, mipmaps activados y anisotropía en cero. La herramienta usa una resolución máxima de importación de `1024` para texturas que detecta de al menos 2048 píxeles en alguna dimensión, y `256` para las demás. Estos son límites de importación, no una reducción del tamaño del archivo fuente. Conviene revisar el resultado y los posibles ajustes por plataforma en el Inspector, especialmente en atlas de arquitectura con muchas piezas.

Los nombres de los archivos generados incluyen el GUID del recurso original para evitar colisiones. Otros shaders y formatos de textura que no coinciden con las condiciones de la herramienta pueden seguir utilizando sus recursos originales.

## Cámara, pixelado y color

`OndaSRetroCamera` está en la cámara del jugador. Durante Play:

1. Crea una `RenderTexture` para el mundo, con altura interna predeterminada de **360 píxeles**. El ancho se calcula según la relación de aspecto de la ventana; por ejemplo, a 16:9 resulta aproximadamente **640 × 360**.
2. Renderiza el mundo en esa textura con filtrado Point y sin MSAA en la textura de salida.
3. La muestra con un `RawImage` en un Canvas `Screen Space - Overlay`, usando el material `RetroComposite`.
4. Crea una cámara de presentación con máscara de renderizado vacía para que exista una cámara dirigida a la pantalla. Esta cámara corrige el aviso **“Display 1 / No cameras rendering”** que aparecía cuando la cámara del mundo solo dibujaba en la textura.
5. Al desactivar el componente restaura el destino anterior de la cámara y libera la textura y los objetos de presentación.

El Canvas de presentación tiene orden `-100`. Los elementos de interfaz superpuestos con un orden posterior pueden quedar nítidos sobre la imagen pixelada. Al cambiar el tamaño de la ventana, la textura se vuelve a crear con la nueva relación de aspecto. Esta primera versión estira la presentación para llenar la pantalla; no implementa una política de escalado entero o bandas.

En `RetroComposite.mat` se pueden ajustar:

| Propiedad | Valor inicial | Efecto |
| --- | --- | --- |
| `Color levels per channel` | `32` | Niveles usados para cuantizar cada canal de color |
| `Dither strength` | `0.08` | Intensidad del patrón Bayer 4 × 4 |
| `Color reduction strength` | `0.3` | Mezcla entre color original y cuantizado |
| `Desaturation` | `0.12` | Reducción de saturación |
| `Vignette` | `0.08` | Oscurecimiento suave en los bordes |

La altura interna se ajusta en **Vertical Resolution** del componente `OndaSRetroCamera` de la cámara del jugador. La cuantización se mezcló parcialmente con la imagen original después de observar bandas de color demasiado marcadas en la linterna. La presentación actual usa Canvas y RenderTexture; todavía no se ha implementado la Renderer Feature o Full Screen Pass descrita como opción en el plan.

## Iluminación y efectos

En la escena de prueba se tiñeron las luces descendientes de `4th Floor` hacia un tono cálido, se multiplicó su intensidad inicial por `0.8` y se desactivaron sus sombras. La iluminación ambiental de la escena usa `Trilight` con tonos grises cálidos y se activó niebla exponencial cuadrática con densidad `0.012`.

Se añadieron dos emisores **OndaS - Polvo ambiental** bajo el cuarto piso. Usan `ParticleSystem` y el shader `OndaS/Dust`, con partículas pequeñas y translúcidas. Es polvo de ambientación, sin simulación de derrumbes ni efectos de salud.

En el estado de apagón, el recorrido guarda y desactiva las luces encontradas en la escena al iniciar Play, reduce la luz ambiental y enciende una luz Spot como linterna. Al volver al estado anterior restaura las luces y los ajustes ambientales guardados. La luz tiene alcance y ángulos configurados en el código del recorrido. Este apagón sirve para evaluar una imagen: no gestiona fases, batería, emisiones, reflejos ni posibles contribuciones precalculadas como lo haría un sistema definitivo.

## Controlador temporal e Input System

El controlador del kit utiliza la API antigua `UnityEngine.Input`, pero el proyecto está configurado para el paquete **Input System**. En la prueba original aparecieron excepciones al ejecutar los scripts de movimiento y mirada del kit.

En la copia de la escena se desactivaron los componentes `SojaExiles.PlayerMovement` y `SojaExiles.MouseLook` del jugador y se agregó `OndaSLookDevWalkthrough`, que usa `Keyboard` y `Mouse` del Input System. Es un controlador simple de recorrido, con `CharacterController` y gravedad; su finalidad es revisar la escena. Otros scripts interactivos del kit siguen existiendo y algunos también usan la API antigua, por lo que deberán revisarse si se quieren usar en el juego.

Las puertas animadas de la escena de prueba también tienen interacción con el nuevo Input System. `Assets/OndaS/Gameplay/Doors/OndaSDoorInteractor.cs` lanza un raycast desde la cámara con alcance de `2.5` unidades y muestra la indicación de abrir o cerrar; `OndaSDoor.cs` reproduce los estados de animación del kit y mantiene el estado de la puerta. Los controles antiguos de las puertas convertidas están desactivados en la copia. Para poder cerrar una puerta abierta hay que apuntar a la hoja en su nueva posición. La interacción requiere tener el cursor capturado en Game.

El comando **OndaS > Configurar interacción con puertas** instala estos componentes en la escena de prueba para las puertas con los scripts y animadores compatibles del kit. Guarda la escena, por lo que debe usarse con cuidado si hay cambios en curso. Ventanas y otros objetos interactivos no están incluidos en esta conversión.

## Menú de editor y precauciones

La herramienta `OndaSLookDevSetup.cs` añade tres comandos:

- **OndaS > Crear prueba visual del cuarto piso:** genera la copia y sus variantes a partir de `Scene_02`. Exige que la escena original esté abierta y sin cambios pendientes; se detiene si la escena de prueba ya existe. Su propósito es la generación inicial, no regenerar el trabajo existente.
- **OndaS > Actualizar controles de la prueba visual:** configura el controlador de prueba en la escena de look development, aplica los valores ambientales definidos en el código, ajusta dithering y viñeta del material y guarda la escena. Tener en cuenta que puede sobrescribir esos ajustes si el equipo ya los ha personalizado.
- **OndaS > Alternar apagón de prueba:** cambia el estado de iluminación durante Play.

Para seguir iterando, editar la escena y los materiales de OndaS en lugar de aplicar cambios masivos al kit. Guardar los cambios de escena en Edit Mode y mantener los archivos `.meta` junto con sus recursos al compartir el proyecto para conservar sus referencias.

## Comprobaciones realizadas y trabajo pendiente

Se abrió la copia con el MCP de Unity, se ejecutó la escena y se capturaron la vista de preparación y la de apagón. Se detectó y resolvió la incompatibilidad del movimiento de prueba con el Input System. Tras añadir la cámara de presentación se comprobó que ambas cámaras existían durante Play, se obtuvo una captura de la vista compuesta y la consola consultada no mostró errores. Estas comprobaciones se hicieron en el Editor de macOS; no equivalen a una prueba de compilación Windows ni a una medición de rendimiento.

Quedan por revisar con el equipo:

- Validar el acabado en movimiento, en distintos encuadres y con varias resoluciones de Game View.
- Ajustar la lectura de puertas, objetos y sombras; revisar atlas, transparencia y parpadeo de texturas a distancia.
- Evaluar iluminación de impacto, luces de emergencia, polvo de impacto, chispas, fuego y escombros.
- Decidir si se necesita un shader propio de superficies, deformación de vértices o UV afín. La muestra usa **URP/Lit** para los materiales del escenario; `OndaS/RetroLit` sigue siendo una propuesta.
- Integrar el apagón y la linterna con los sistemas reales cuando se desarrollen las mecánicas, contemplando iluminación horneada, reflejos y materiales emisivos.
- Medir rendimiento en un equipo de referencia y probar una compilación para Windows antes de extender la solución.

### Personaje principal de la prueba

En `OndaS_RetroLookDev`, el jugador de primera persona incorpora `Office Worker` como hijo del controlador existente. La cápsula visual anterior está desactivada; el `CharacterController`, la cámara retro, la navegación y el sistema de puertas se mantienen. Los pies del modelo se alinean con la base del controlador. El esqueleto usa un avatar **Generic** y el Animator reproduce `idle` y `walk` según la velocidad horizontal del controlador. Las diez animaciones originales permanecen en el FBX para uso futuro. El movimiento sigue a cargo del controlador, sin root motion.

Durante Play, el cuerpo se configura para proyectar sombras sin dibujarse en la cámara de primera persona, evitando que la cabeza obstruya la vista. En la escena de Unity el modelo puede inspeccionarse seleccionando `Base Scene/First Person Player/Office Worker`. La iluminación del escenario debe permitir sombras para que estas aparezcan. La integración todavía no incluye animaciones de recoger objetos ni de caminar agachado.

El material utiliza el atlas con filtrado Point y sin mipmaps, junto con los colores de vértice que vienen en el modelo. Los recursos están en `Assets/OndaS/Characters/OfficeWorker`; el menú **OndaS > Configurar personaje principal** permite volver a configurar la escena de prueba guardada.

Crédito del recurso: **PSX People Free by heyheythere** — https://heyheythere.itch.io/psx-people-free — CC BY 4.0. La licencia completa acompaña al modelo en `Assets/OndaS/Characters/OfficeWorker/LICENSE.txt`; este crédito también debe aparecer en los créditos del juego publicado.


Los antebrazos y manos visibles en primera persona se extrajeron de la malla original del personaje y conservan su atlas y colores de vértice. Están bajo `Camera/OndaS First Person Hands` y `OndaSFirstPersonHands` hace subir y bajar los brazos de forma alternada al caminar, ajusta el ritmo a la velocidad y los baja suavemente al detenerse; la frecuencia y amplitud se pueden ajustar en el Inspector. Son mallas independientes con oscilación sencilla, no animaciones completas de manos o interacción. El material del personaje también recibe las luces adicionales de la escena. El menú **OndaS > Configurar manos en primera persona** regenera las mallas y la configuración de la escena.

El objeto temporal `OndaS Retro Display` presenta el resultado de la cámara retro en Game. Durante Play se oculta únicamente en la vista Scene del editor para que el lienzo no cubra la escena.

En la escena de prueba, `Shift` sostenido permite correr y `C` alterna entre agacharse y levantarse. Al agacharse se reducen la altura del CharacterController y de la cámara; la velocidad también disminuye. Si hay un obstáculo sobre el jugador, permanece agachado hasta que haya espacio para levantarse. Correr utiliza el clip `run`; aún no tiene consumo de estamina. El paquete incluye `cower`, pero muestra al personaje cubriéndose la cabeza y temblando, por lo que no se usa como animación de desplazamiento agachado. El cuerpo completo y su sombra todavía no cuentan con una animación específica de agachado.
