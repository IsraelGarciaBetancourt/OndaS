# OndaS — Plan de dirección visual PSX Horror / Retro Low-Poly Horror

Fecha: 28 de octubre de 2026. Estado: propuesta para implementación.

Para conocer lo que ya se implementó y cómo probarlo, consultar [OndaS_Implementacion_Visual.md](OndaS_Implementacion_Visual.md). Este plan conserva las propuestas y no indica que todas estén terminadas.

## 1. Dirección artística

La propuesta es un edificio residencial reconocible, usado y deteriorado, con superficies pixeladas, formas sencillas, colores apagados y luz localizada. La tensión procede de la arquitectura, el polvo, las sombras y la pérdida de iluminación durante el desastre.

De la primera referencia aportada se toman las texturas ásperas, el detalle de baja resolución y la sensación de materiales pintados. De la segunda se toman la separación entre luz y oscuridad, la linterna como centro de atención y los acentos de color. Estas son lecturas de las imágenes; no se presupone conocer los shaders internos de esos juegos.

El acabado inicial será retro moderado: textura visible, imagen de baja resolución y geometría legible. La deformación de vértices y de UV tendrá controles independientes para explorar un PSX más marcado después de fijar la imagen base.

Paleta propuesta:

| Función | Color orientativo | Aplicación |
| --- | --- | --- |
| Base residencial | Beige envejecido, marrón, gris verdoso | Paredes, muebles, concreto |
| Sombras | Azul grisáceo o verde oscuro | Profundidad de habitaciones y pasillos |
| Luz doméstica | Ámbar apagado | Lámparas, cocina, departamento |
| Servicio | Blanco verdoso | Escaleras y luminarias fluorescentes |
| Emergencia | Rojo contenido | Balizas y puntos de peligro |
| Orientación | Verde de señalética | Salidas y rutas legibles |

Los colores de orientación conservan contraste suficiente tras el tratamiento de color. La paleta es una propuesta artística que se ajustará con capturas del escenario.

## 2. Diagnóstico del proyecto

Revisión de archivos y configuración; queda pendiente observar y validar el resultado en el Editor y en una compilación.

| Elemento | Estado encontrado | Implicación |
| --- | --- | --- |
| Unity | 6000.5.9f1 | Implementar para Unity 6.5 |
| Render pipeline | URP 17.5.0 | Mantener URP como base |
| Calidad PC | `PC_RPAsset` y `PC_Renderer` | Crear configuración propia para el estilo |
| Renderer PC | Forward+ | Conservar inicialmente y medir con luces locales |
| Renderer Features | SSAO habilitado | Ajustar su intensidad tras fijar la luz |
| Renderizado PC | HDR activo, Render Scale 1, MSAA 1 | MSAA está desactivado en el asset; falta revisar AA efectivo de cámara |
| Escena | `Assets/Brick Project Studio/Apartment Kit/Scenes/Scene_02.unity` | Base para la escena visual de prueba |
| Luces serializadas en la escena | 50 componentes: 32 Point, 17 Spot y 1 Directional; modo Realtime | Auditar activación, alcance, sombras y solapamiento en Editor |
| Ambiente | Niebla desactivada y skybox asignado | Rediseñar ambiente y profundidad |
| Datos de iluminación | Referencias a LightingData y LightingSettings | Comprobar su contribución efectiva antes de reutilizarlos |
| Materiales muestreados | URP/Lit, normales y superficies de brillo alto | Crear variantes mates y simplificadas |
| Texturas de arquitectura | PNG fuente de 4096 × 4096; muestra con máximo de importación 2048 y filtrado bilineal | Son atlas que requieren reducción controlada |

El número de componentes serializados no demuestra cuántos están activos o visibles simultáneamente; tampoco constituye una medición de rendimiento. Las referencias de iluminación no demuestran por sí mismas que haya luz horneada visible.

El edificio de cinco pisos indicado por el usuario sirve para desarrollar la estética. El GDD propone comenzar en el piso 8: la ampliación o adaptación del recorrido se resolverá al planificar el nivel, después de validar el estilo.

## 3. Materiales y geometría

### Materiales retro

Primero se ajustarán variantes de materiales URP/Lit en una zona pequeña para evaluar cuánto aporta la dirección artística. Después se construirá un shader común `OndaS/RetroLit` para consolidar el acabado.

Propiedades previstas:

- Textura base, tinte y emisión controlada por material.
- Respuesta difusa sencilla, compatible con luz principal y luces adicionales de Forward+.
- Recepción y emisión de sombras; pases de profundidad y normales coherentes con el renderer y el SSAO.
- Brillo bajo en paredes, tela y madera; respuesta especular localizada en vidrio y metal.
- Normal maps desactivados o muy atenuados según la superficie.
- Niebla y soporte de iluminación precalculada si finalmente se conserva esa técnica.
- Controles opcionales de cuantización de iluminación, ajuste de vértices a rejilla e interpolación afín de UV.

Comenzar con iluminación continua simplificada; evaluar bandas suaves solo si mejoran el acabado. Un sombreado de pocas bandas muy duras puede llevar la escena hacia un aspecto de caricatura distinto al buscado.

Mantener variantes específicas para materiales opacos, recortados, vidrio y emisivos. La sustitución debe preservar texturas, tiling, offset, colores, transparencia, caras visibles y emisión de los materiales originales.

### Texturas

- Objetos pequeños: probar texturas de 64–256 píxeles por lado.
- Superficies individuales: probar 128–512 píxeles según el tamaño visible.
- Atlas de arquitectura: comparar 1024 y 512 antes de considerar resoluciones menores.
- Probar filtrado Point en variantes, conservando mipmaps inicialmente y revisando el parpadeo a distancia.
- Igualar densidad visual entre objetos cercanos. El tamaño del archivo por sí solo no determina cuán pixelada se ve una superficie.
- Añadir suciedad y desgaste de escala grande: manchas, humedad, pintura gastada y juntas. Evitar ruido fino uniforme en todas las superficies.

Reducir un atlas entero a 128 puede destruir detalles de muchas piezas a la vez. La resolución final se decidirá observando marcos, bordes, señalética y muebles desde la distancia de juego.

### Geometría

Reutilizar inicialmente arquitectura y mobiliario. Revisar siluetas de sillas, grifos, lámparas y adornos; simplificar o sustituir las piezas cuyo detalle contraste con el resto. Usar normales planas de forma selectiva en escombros y objetos nuevos. Aplicarlas indiscriminadamente a paredes o muebles puede producir cortes de sombreado no deseados.

El shader modifica la apariencia, pero la reducción real de polígonos requiere trabajo en la malla o un modelo alternativo. Las mallas nuevas de escombros tendrán formas angulares y pocos detalles pequeños.

## 4. Imagen final y arquitectura de renderizado

### Resolución interna

Renderizar el mundo a una resolución interna fija y ampliarlo con muestreo Point. Preservar la relación de aspecto; para otras pantallas, adaptar dimensiones y encuadre con una política explícita de escala y bandas cuando haga falta.

| Perfil | Resolución inicial a comparar | Uso |
| --- | --- | --- |
| Retro suave | 960 × 540 | Más definición y lectura a distancia |
| Retro base | 640 × 360 | Punto de partida recomendado |
| Retro fuerte | 480 × 270 | Pixel visible más marcado |

La elección depende de pruebas en movimiento. La resolución visual es independiente de los FPS: mantener cámara y movimiento fluidos, con objetivo inicial de 60 FPS en un equipo de referencia que se definirá durante implementación.

### Orden propuesto

1. Mundo opaco, iluminación y sombras en el target interno.
2. Vidrio, partículas y otros transparentes en ese mismo espacio visual.
3. Tratamiento de color y bloom tenue, si se utiliza.
4. Cuantización de color y dithering en la rejilla interna.
5. Presentación a pantalla con escalado Point.
6. HUD y menús a resolución de salida, con tipografía y gráficos acordes al estilo.

El prototipo puede usar una cámara de mundo con RenderTexture y una presentación a pantalla sin volver a renderizar el mundo. Para efectos sobre la imagen se usará Full Screen Pass; si el control de resolución y presentación requiere código propio, se implementará una Renderer Feature compatible con Render Graph de URP 17.5. El paquete local marca las antiguas opciones de compatibilidad de Render Graph como obsoletas desde 6000.4.

La integración debe comprobar el orden real con Frame Debugger: efectos finales, escalado automático o AA posteriores pueden suavizar el píxel. Una pixelación aplicada únicamente al final sobre una imagen renderizada a resolución completa sirve para explorar el aspecto, pero no equivale a renderizar a baja resolución ni garantiza ahorro de GPU.

### Efectos previstos

| Efecto | Ajuste inicial | Prioridad |
| --- | --- | --- |
| Cuantización de color | Comparar 32 niveles por canal con mezcla regulable; no significa 32 colores totales | Alta |
| Dithering | Matriz Bayer 4 × 4, amplitud baja, alineada con píxeles internos | Alta |
| Viñeta | Leve; mantener visible la periferia útil | Media |
| Bloom | Muy bajo y reservado a emisivos | Media |
| Grano | Bajo; evaluar después de texturas y dithering | Baja |
| Aberración cromática | Desactivada al inicio; prueba leve en bordes como variante | Baja |
| Vertex snapping | Desactivado al inicio; probar desplazamiento aparente menor a un píxel interno | Opcional |
| UV afín | Intensidad baja, por material; comparar en paredes largas | Opcional |
| Scanlines / distorsión CRT | Variante independiente si aporta a la dirección final | Opcional |

La interpolación afín reproduce deformación de texturas al cambiar la perspectiva; el ajuste de vértices reproduce el movimiento escalonado de la silueta. Son efectos distintos de la pixelación. Se evaluarán por separado para controlar grietas visuales, vibración de barandas y pérdida de estabilidad durante el movimiento.

El preset inicial tendrá motion blur, profundidad de campo, TAA y suavizados de imagen desactivados para poder juzgar la rejilla de píxeles. Las variantes más suaves se decidirán con comparaciones en movimiento.

## 5. Iluminación por estado visual

Se crearán tres presets de ambiente previsualizables desde el Inspector. Al principio serán controles de arte independientes; las futuras mecánicas podrán activarlos después.

| Estado | Imagen objetivo | Recursos |
| --- | --- | --- |
| Preparación | Departamento habitable y ligeramente incómodo; objetos reconocibles | Luz doméstica cálida, exteriores fríos moderados, sombras legibles |
| Impacto / evacuación | Edificio inestable, visibilidad fragmentada | Luminarias con fallos puntuales, polvo localizado y acentos de emergencia |
| Apagón / escombros | Espacios oscuros que se descubren con la linterna | Luz ambiental mínima controlada, señales y balizas localizadas, polvo en el haz |

Ajustar el cielo y la luz exterior para que el interior no quede lavado. La hora del día se fijará durante la prueba visual; el GDD no obliga a que todo ocurra de noche.

Planificar el apagón desde el principio: las luminarias que se apagarán deben tener una contribución eliminable. Usar Realtime para esas fuentes durante el prototipo. Si se introduce iluminación baked, reservarla para contribuciones permanentes o diseñar un cambio completo de datos. Apagar un componente Light no elimina su contribución ya horneada. Revisar también emisión y reflection probes para que no permanezca una imagen diurna tras el corte.

Reducir alcances y solapamientos de luces, activar por zonas y reservar sombras dinámicas para las fuentes que definen la imagen. Empezar con la linterna y pocas luminarias cercanas con sombras; medir el resultado antes de ampliar el presupuesto. Conservar SSAO solo si aporta contacto sin ensuciar las superficies.

La linterna de prueba será un Spot Light con cookie irregular y borde definido, evitando un círculo perfecto y uniforme. Su alcance se calibrará con la profundidad del pasillo. La batería y el inventario pertenecen a la implementación posterior de mecánicas.

## 6. Niebla y VFX

URP no incluye el mismo sistema de niebla volumétrica local de HDRP. La propuesta inicial combina niebla de distancia, partículas y geometría sencilla para los haces. Un sistema volumétrico personalizado se valorará únicamente después de ver esta solución.

| VFX | Tratamiento visual | Implementación inicial |
| --- | --- | --- |
| Polvo ambiental | Partículas pequeñas, discretas y de textura pixelada | Particle System en zonas, material compatible con luces si se necesita |
| Polvo de impacto | Nube localizada de expansión breve | Flipbook de baja resolución; duración y opacidad regulables |
| Fragmentos | Trozos angulares y pocos tamaños reconocibles | Partículas de malla o piezas de prueba; sin simulación de derrumbe todavía |
| Chispas | Trazos cortos, amarillos y blancos | Partículas aditivas, ráfagas breves y luz puntual limitada |
| Fuego | Formas legibles de pocos fotogramas | Flipbook estilizado y emisión; moderar bloom |
| Luz en polvo | Haz visible solo donde aporta profundidad | Cono de malla con ruido y desvanecimiento por profundidad |
| Grietas y suciedad | Marcas grandes y de baja frecuencia | Variantes de textura, tarjetas o decals según la superficie |

La niebla de distancia no sustituye una nube local dentro de una habitación. El haz simulado necesita pruebas de intersección y oclusión para evitar atravesar muros. Controlar el overdraw de partículas transparentes, especialmente durante el apagón con linterna.

Partículas y materiales del mundo recibirán la misma resolución y tratamiento final que el escenario para conservar coherencia. Particle System es suficiente para esta primera prueba; se evaluará VFX Graph si aparece una necesidad concreta.

## 7. Organización propuesta

Crear los recursos propios en `Assets/OndaS/Art/`, con subcarpetas `Shaders`, `Materials`, `Textures`, `VFX`, `Lighting`, `Rendering` y `Scenes`. Crear una escena de prueba derivada de Scene_02 y variantes propias de los materiales y ajustes de renderizado.

Recursos previstos:

- `OndaS_RetroLookDev.unity`: habitación, pasillo y escalera de prueba.
- `OndaS_Retro_RPAsset` y `OndaS_Retro_Renderer`: configuración de renderizado del proyecto.
- `OndaS/RetroLit`: shader de superficies.
- `OndaS/RetroComposite`: cuantización y dithering de pantalla.
- `OndaS/RetroParticles`: tratamiento consistente de partículas cuando los shaders estándar no basten.
- Perfiles de Volume y presets de luz para preparación, impacto y apagón.
- Control de previsualización de estados y una linterna de prueba.
- Herramienta de remapeo de materiales, si el volumen de variantes la justifica.

Los efectos de pantalla serán parámetros globales; texturas, brillo y deformaciones tendrán ajustes por material. Mantener pocas variantes compartidas y evitar materiales instanciados por cada objeto cuando no sean necesarios.

## 8. Secuencia de implementación

| Paso | Trabajo | Resultado revisable |
| --- | --- | --- |
| 1. Línea base | Abrir Scene_02 y elegir tres encuadres; registrar luz, materiales y coste inicial | Capturas y recorrido corto del estado actual |
| 2. Dirección de materiales | Variantes de paredes, suelo, madera, tela, metal y vidrio; pruebas de atlas | Una habitación con superficies coherentes |
| 3. Luz | Preparación y apagón, linterna, ambiente y sombras | Los mismos encuadres en dos condiciones legibles |
| 4. Imagen retro | Resolución interna, escalado, color y dithering; consolidación de shaders | Comparación de perfiles suave, base y fuerte |
| 5. Atmósfera | Polvo, haz, chispas y luz de emergencia | Pasillo y escalera con el acabado completo |
| 6. Consolidación | Elegir preset, medir, ajustar materiales y extender por zonas | Guía reutilizable y edificio visualmente consistente |

El primer entregable será una pequeña muestra recorrible con los estados visuales y comparaciones. Permitirá fijar el estilo antes de convertir todos los materiales o ampliar el nivel.

## 9. Criterios de validación

- Capturar los mismos puntos de vista antes y después: habitación amueblada, pasillo largo, escalera y rincón con vidrio/metal.
- Revisar en movimiento bordes, barandas, atlas y señales: buscar parpadeo, sangrado de texturas y deformación excesiva.
- Verificar que las puertas, desniveles y objetos prioritarios se distinguen con cada preset.
- Comprobar que el apagón elimina luz doméstica, emisión y reflejos residuales según lo previsto.
- Confirmar que partículas, transparencias y HUD se componen en el orden correcto.
- Revisar Profiler y Frame Debugger; registrar resolución, equipo y ajustes para que la comparación sea útil.
- Probar en Editor sobre macOS y, cuando haya un entorno disponible, en una compilación Windows, que es la plataforma objetivo del GDD.
- Fijar el equipo de referencia antes de dar por cumplido el objetivo de 60 FPS. El coste real depende también de sombras, objetos, transparencias y CPU.

## 10. Referencias técnicas consultadas

La dirección artística se basa en las dos imágenes aportadas por el usuario. La compatibilidad se contrastó con el proyecto y el código del paquete URP instalado.

- [Unity 6.5: Full Screen Pass Renderer Feature](https://docs.unity.com/en-us/engine/6000.5/manual/post-processing-and-full-screen-effects/urp/custom-post-processing/renderer-feature-full-screen-pass): puntos de inserción de efectos de pantalla.
- [Unity 6.5: comparación de render pipelines](https://docs.unity.com/en-us/engine/6000.5/manual/render-pipelines/choose-a-render-pipeline/feature-comparison): disponibilidad de niebla volumétrica y otras funciones.
- [Unity: Light Modes](https://docs.unity.com/en-us/engine/6000.0/manual/lighting-overview/lighting-light-sources/components/configuring/light-modes/introduction): comportamiento de luces Realtime, Baked y Mixed y persistencia de iluminación precalculada.
