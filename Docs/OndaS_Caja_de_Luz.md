# Caja de luz e interruptores

## Función en el juego

La caja de luz está en una pared de la sala del departamento del **piso 8**, cerca de la mesa del comedor, en la escena `Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity`. Contiene cinco interruptores que el jugador puede bajar y subir individualmente.

La intención es que cortar la corriente reduzca el riesgo de los cables expuestos durante el descenso. **Los cables, los pasillos bloqueados y sus consecuencias todavía no están implementados**: actualmente la caja registra el estado de los interruptores y lo expone para conectarlo con esos sistemas en una etapa posterior.

## Interacción y controles

1. La caja comienza cerrada y los cinco interruptores comienzan arriba.
2. Acércate, apunta a la caja y pulsa **E** cuando aparezca `[E] Para abrir`.
3. La cámara se acerca, la puerta se abre y el cursor queda libre. Haz **clic izquierdo** sobre cada interruptor para bajarlo o subirlo.
4. Pulsa **E** o **Esc** para cerrar y recuperar el movimiento y la mirada. **F** permite encender o apagar la linterna mientras se inspecciona la caja.

La caja también se puede abrir antes del sismo y los interruptores pueden cambiarse en ese momento. Si el primer sismo comienza mientras el jugador la inspecciona, la inspección se interrumpe y se recuperan los controles para buscar refugio.

Después de sobrevivir al primer sismo aparece el objetivo `BAJA LOS INTERRUPTORES`, con el número que ya está abajo. Al bajar los cinco se muestra brevemente `CORRIENTE CORTADA`; si se sube alguno, el objetivo vuelve a aparecer. Si los cinco ya estaban abajo antes de que terminara el sismo, el objetivo no obliga al jugador a repetir la acción.

El apagón del sismo y el estado de la caja son sistemas distintos. Bajar o subir los interruptores **no restablece las luces del edificio** ni cambia todavía el comportamiento de los peligros eléctricos.

## Implementación

| Elemento | Responsabilidad |
| --- | --- |
| `Assets/OndaS/Gameplay/Electrical/OndaSFuseBox.cs` | Inicializa la puerta, el fondo metálico, las zonas de interacción y los cinco interruptores; controla la vista cercana, el objetivo y el estado de la corriente. |
| `Assets/OndaS/Gameplay/Doors/OndaSDoorInteractor.cs` | Detecta la caja al apuntar con la cámara, muestra el aviso de E y delega los controles durante la inspección. |
| `Assets/OndaS/Gameplay/Earthquake/OndaSFirstEarthquake.cs` | Interrumpe la inspección al comenzar el sismo y expone `HasFinished`, que permite mostrar el objetivo tras sobrevivir. |
| `Assets/OndaS/Art/Rendering/OndaSLookDevWalkthrough.cs` | Controla el movimiento del jugador y la linterna. Durante la inspección el movimiento se suspende y se restaura al salir. |
| `Assets/OndaS/Art/Rendering/OndaSFlashlightAutoDim.cs` | Reduce el brillo de la linterna ante superficies cercanas y recupera su potencia al alejarse; también funciona durante la inspección. |
| `Assets/OndaS/Art/Rendering/OndaSRetroCamera.cs` | Renderiza el mundo a baja resolución y lo presenta en pantalla completa. La selección de los interruptores usa coordenadas normalizadas de pantalla para respetar esta conversión. |

El objeto de escena `OndaS - Caja de luz` tiene el componente `OndaSFuseBox`. El modelo importado está en `Assets/OndaS/Art/Models/CajaDeLuz/` y usa el OBJ `PSX_FuseBox.obj` con su MTL y texturas. El código busca como hijos directos `Puerta_Abierta`, `Caja_Metal_Cuerpo` y `Switch_01` a `Switch_05`; si se reemplaza el modelo o se cambia esa jerarquía hay que adaptar el código y comprobar los pivotes.

En ejecución se crean la bisagra de la puerta, el fondo metálico y un pivote para cada interruptor. Las palancas combinan desplazamiento y rotación para distinguir las posiciones. Sus colliders se usan para la interacción física y las consultas de proximidad; la selección con el cursor se calcula sobre un plano local de la caja, con un área que cubre las posiciones alta y baja de cada palanca.

La aproximación y el regreso de cámara duran aproximadamente **0,55 segundos**. Mientras dura la transición no se procesan clics en las palancas. Al salir se restauran la posición, la rotación y el campo de visión anteriores, además del control del jugador y el bloqueo del cursor.

## Estado para futuros peligros

`OndaSFuseBox` expone:

- `IsPowerIsolated`: es `true` únicamente cuando se encontraron los cinco interruptores y todos están abajo.
- `PowerIsolationChanged`: evento `Action<bool>` que se dispara al cambiar un interruptor; el valor indica si ahora están todos abajo.
- `IsInspecting`: indica si el jugador está utilizando la vista cercana.

Un futuro obstáculo eléctrico puede consultar `IsPowerIsolated` al iniciarse y escuchar `PowerIsolationChanged` para reaccionar a cambios posteriores. Debe cancelar su suscripción cuando deje de existir. Si alguno de los cinco interruptores no se encuentra al iniciar la caja, `IsPowerIsolated` permanece en `false` y se registra una advertencia en la consola.

El estado vive en la instancia de la escena durante la partida: **no hay guardado persistente ni transferencia implementada entre escenas**. Reiniciar la escena vuelve a dejar los cinco interruptores arriba. Si el descenso se separa en escenas, habrá que guardar y restaurar explícitamente este estado antes de conectar los peligros.

## Comprobación en Unity

1. Abre `OndaS_RetroEdificio` y entra en Play. Comprueba que la caja se vea cerrada y que aparezca el aviso al apuntarle de cerca.
2. Ábrela con **E**, prueba la linterna con **F** y comprueba que la puerta, el zoom y el cursor funcionen. Baja y sube cada palanca con el mouse; verifica también que un clic fuera de los interruptores no cambie ninguno.
3. Cierra con **E** y repite con **Esc**. Confirma que regresan la cámara, el movimiento y el bloqueo del cursor.
4. Inicia el sismo con **F8** durante la preparación, refúgiate y espera a que termine. Verifica el objetivo y su contador, incluida la confirmación al bajar los cinco. Si F8 se usa mientras la caja está abierta, comprueba que se cierre y que el jugador pueda buscar refugio.
5. Reinicia la escena y comprueba que los interruptores vuelvan a estar arriba. Si se modifican resolución, cámara retro, escala o modelo, repite las pruebas de clic sobre los cinco interruptores tanto arriba como abajo.
