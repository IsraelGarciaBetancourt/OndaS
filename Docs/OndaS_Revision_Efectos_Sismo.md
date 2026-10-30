# Revisión de la escena para simular el sismo

Revisión mediante MCP de Unity de `OndaS_RetroEdificio`, el 30 de octubre de 2026. Alcance: efectos físicos y visuales del primer sismo. No se modificó la escena ni se ejecutó la simulación.

**Actualización posterior:** se implementó un primer prototipo en `8th Floor` mediante `OndaSFirstEarthquake`. Comienza 60 segundos después de entrar en Play, dura 35 segundos y permite iniciarlo con **F8** durante la preparación. Incluye cámara, objetos seleccionados, dos paneles de vidrio, polvo, luces y audio provisional. La temporización todavía es independiente de un sistema de fases o HUD. El collider de la hoja de ventana permanece para evitar que el jugador salga del edificio. Los fragmentos se eliminan si descienden por debajo del piso 8.

## Hallazgos en el Editor

La escena estaba en Edit Mode, guardada y sin errores en la consulta inicial de consola. Se inspeccionaron los grupos de decoración y muebles de las cinco habitaciones del departamento del piso 8, sus luces, los elementos del pasillo y las ventanas exteriores. Se tomaron capturas actuales en `Temp/QuakeReview`.

El departamento tiene muchos objetos separados y aprovechables. La consulta de `Rigidbody` devolvió una sola coincidencia en la escena: la mochila. Tener un collider no implica que un objeto ya pueda caer. Los objetos revisados necesitan configuración de simulación; no basta con activar una gravedad global.

| Elementos existentes | Comportamiento propuesto | Preparación necesaria |
| --- | --- | --- |
| `Canvas_Painting_01`, `02`, `03` | Vibran, se ladean y uno o dos se desprenden. | Ya tienen BoxCollider. Añadir cuerpo físico inicialmente cinemático; liberar con impulso y giro al superar su umbral. |
| `Vase_apt_01`, `Vase_Apt_01` | Se balancean y vuelcan; uno puede romperse al golpear el suelo. | En las raíces inspeccionadas no aparece collider. Añadir formas simples; en la variante con flores mantener el conjunto unido. La rotura requiere una variante fragmentada. |
| `Wall Wine Rack_Apt_01_Filled/WineBottles` | Algunas botellas vibran, resbalan y caen de forma escalonada. | Hay siete raíces de botella separadas. Añadir collider y cuerpo por botella elegida, conservando sus hijos; no soltar todo el botellero. |
| `DrinkingGlass_01`, `DrinkingGlass_02` y duplicados | Deslizamiento, caída y sonido de vidrio. | Ya tienen BoxCollider. Preparar cuerpo y un efecto de rotura por impacto, si se requiere. |
| `Plate_Apt_01_Stacked`, `Plate_Apt_02_Stacked`, `Bowl_Apt_01_Stacked` | La pila vibra o cae como conjunto. | Cada pila inspeccionada es un único objeto sin hijos. Para platos individuales hay que reemplazarla por piezas separadas. |
| `Toaster_Apt`, `CoffeeMaker_Apt_01`, `Blender_Apt_01` | Se desplazan sobre la encimera; solo uno cae en el pico. | Ya tienen BoxCollider en la raíz. Ajustar masa y fricción y mover todos sus hijos como conjunto. |
| Cuatro `Chair_Apt_01` | Se desplazan poco y una puede volcar. | El collider encontrado está en el hijo `Legs Chair`; añadir la simulación al conjunto y completar el volumen del asiento/respaldo si hace falta. |
| `Lamp_Floor_01` y lámparas de mesa | Balanceo y vuelco, seguido de golpe y posible apagado. | La lámpara de pie inspeccionada tiene BoxCollider y carece de Rigidbody. Revisar que la luz y el material luminoso respondan junto al modelo. |
| `TV_Apt_01` | Vibra en el soporte; un desprendimiento puede servir como evento destacado. | El soporte `Mount` y la pantalla `Screen` son hijos separados. Mantener el soporte fijado y preparar el cuerpo del televisor como conjunto móvil. |
| `Shelf_Apt_01`, armarios y refrigeradora | Movimiento muy pequeño o vuelco preparado en una segunda entrega. | La estantería inspeccionada usa MeshCollider no convexo. Para caída física necesita un collider adecuado; no convertir todos los muebles directamente en cuerpos dinámicos. |
| Espejos y accesorios del baño | Vibración, golpeteo y caída de un objeto pequeño. | Elegir pocos eventos: vaso de cepillos, espejo u otro accesorio, sin desprender toda la instalación. |

## Ventanas: solución concreta

En `Structure_03/Exterior/Walls/8th Floor/Windows` hay 14 grupos de marco. Se siguió la jerarquía de dos muestras. La estructura observada es:

```text
Window_Frame_Apt_01
└── Window_Base
    ├── Window_01
    │   └── Glass.007
    └── Window_02
        └── Glass.006
```

Los cristales de la muestra tienen MeshFilter y MeshRenderer independientes. Se puede conservar la carpintería y sustituir únicamente el cristal. No se identificó una variante rota dentro de esta jerarquía.

Propuesta de rotura:

1. Vibración pequeña del conjunto de ventana y traqueteo localizado.
2. En un umbral de intensidad propio de cada ventana, sonido de tensión y, opcionalmente, una textura de grietas.
3. Ocultar el cristal intacto y activar una versión de fragmentos en su misma posición, orientación y escala. Una primera versión puede usar entre 8 y 16 fragmentos físicos por panel, más partículas pequeñas; son cifras iniciales para medir rendimiento.
4. Lanzar los fragmentos con dispersión moderada y gravedad; conservar algunos bordes rotos para que el hueco no parezca una ventana simplemente abierta.
5. Ajustar colisiones y asentar los restos después del impacto. Separar fragmentos visibles, fragmentos con colisión y daño jugable.

El BoxCollider de `Window_01` cubre la hoja completa: ocultar `Glass.007` no lo elimina. Si queremos que objetos atraviesen el cristal roto, hay que separar la colisión del vidrio y la del marco. Conservar la protección contra caída del jugador como una decisión explícita del nivel.

`Window_01` también tiene Animator y `SojaExiles.opencloseWindowApt`, cuyo código utiliza el Input antiguo. Antes de añadir vibración, desactivar o integrar ese control en la variante de OndaS y evitar que animación y sismo escriban sobre el mismo transform. No modificar el prefab compartido del kit para todas las escenas.

## Cómo hacer que se sienta que todo tiembla

Usar una intensidad central que crece y disminuye, con varias sacudidas y cambios de dirección. Cada elemento responde con un pequeño desfase y un umbral propio.

- **Cámara:** desplazamiento corto y rotación limitada, en un pivote independiente de la mirada y del agachado. La imagen comunica el movimiento general del edificio.
- **Objetos pequeños:** cuerpos físicos que reciben impulsos laterales coordinados. Masa, fricción y geometría deben diferenciar una botella, una silla y un electrodoméstico.
- **Elementos fijados:** oscilación controlada alrededor de su anclaje: cuadros, lámparas, puertas de armario y ventanas. Algunos se liberan durante el pico.
- **Arquitectura:** conservar suelo, paredes y escaleras estables para las colisiones. La cámara, los objetos y el sonido producen la sensación conjunta. Si se añade deformación visual de arquitectura, medir amplitudes y evitar separaciones entre módulos y marcos.
- **Polvo:** emisiones puntuales desde techo, juntas y lugares de impacto, reutilizando el estilo del polvo existente. Los dos emisores ambientales actuales no bastan para representar esos impactos.
- **Luces:** variaciones breves sincronizadas con sacudidas concretas. En el departamento hay siete Point Lights y dos Reflection Probes; el brillo de los modelos de lámpara y sus luces requiere coordinación.
- **Audio:** rumor estructural continuo, traqueteo cercano y golpes ligados a caídas reales. Cada ventana se oye en su ubicación; cada objeto registra su primer impacto fuerte para no repetir sonidos por cada contacto.

Conviene evitar dos efectos acumulados en el mismo objeto: cuando un cuadro pasa a caída física, deja de recibir la animación de balanceo. Al desaparecer el sismo, los objetos se quedan caídos; la cámara vuelve a reposo.

## Primer montaje recomendado

Concentrar la primera prueba en la sala/comedor visible desde la posición inicial:

- Los dos cuadros de la sala vibran; uno cae.
- El jarrón alto junto al banco se balancea y vuelca.
- Tres de las siete botellas del botellero se desprenden en momentos distintos.
- Dos sillas se desplazan y una lámpara se vuelca.
- Una ventana cercana traquetea y rompe un panel.
- Polvo de techo y golpes acompañan esos eventos; las habitaciones contiguas aportan sonidos.

Estos son objetivos de montaje, no comportamientos ya implementados. Empezar con aproximadamente 10–20 cuerpos activos cerca del jugador y ampliar solo después de medir. No activar simultáneamente física y rotura en los ocho pisos.

Después extender a la cocina con vasos y un electrodoméstico, a dormitorios con lámparas/cuadros y al pasillo con una luminaria y polvo. Las roturas principales deberían repartirse en el tiempo, en lugar de ocurrir todas al inicio.

## Orden de implementación y comprobación

1. Crear variantes o configuración propia de OndaS para los objetos elegidos y un perfil de intensidad común.
2. Preparar colliders, pivotes y estados iniciales; comprobar que al empezar Play no caen antes del sismo ni se expulsan por solapamiento con mesas o paredes.
3. Implementar vibración y caída de sala/comedor y validar la cámara.
4. Construir un panel de cristal fragmentado y verificar marco, collider, Animator y repetición del evento.
5. Añadir sonido de impacto, polvo y reacción de luces.
6. Probar desde varios lugares del departamento: eventos fuera de vista deben seguir siendo comprensibles por sonido, el inventario debe permanecer intacto y la salida debe ser transitable.
7. Medir física y renderizado con fragmentos activos; reducir colisiones entre restos, sombras o cantidad de cuerpos según los resultados. Reiniciar Play debe restaurar el estado inicial sin cambios guardados en la escena.

La lectura de componentes y las capturas confirman que esta escena permite el montaje propuesto. El movimiento, la rotura y el rendimiento quedan pendientes de implementar y comprobar en Play.

Una consulta auxiliar con `execute_code` falló porque el compilador utilizado por esa herramienta no manejó correctamente la ruta de Unity con espacios. La inspección continuó mediante recursos de componentes y jerarquías. Ese intento dejó un mensaje de error del MCP en consola; no se crearon scripts de proyecto ni se alteró la escena. La comprobación final confirmó `isDirty: false`.
