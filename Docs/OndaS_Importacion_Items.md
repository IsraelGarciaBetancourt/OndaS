# Importación de Items de OndaS

Origen: `/Users/israelgarcia/Downloads/Onda S Descargas/Items`.

Los recursos seleccionados están en `Assets/OndaS/Items`, organizados en **Good Items** y **Bad Items**. Se generaron **72 prefabs**: **64 Good Items** y **8 Bad Items**. Los números incluyen variantes de modelo y de textura. El catálogo técnico está en `Assets/OndaS/Items/Catalogo_Items.json`.

Esta clasificación sirve al diseño de la mochila del juego. Por indicación del usuario, la comida preparada queda en Bad Items. La llave inglesa se ha considerado útil para tareas de emergencia; la clasificación podrá ajustarse cuando se definan los efectos y pesos de inventario.

[Vista general de los modelos principales](OndaS_Items_Resumen.png)

## Selección por carpeta

| Carpeta de origen | Selección | Destino | Prefabs |
|---|---|---|---:|
| Agua | `Water` y la mochila `Backpack`; texturas `Water.png` y `Backpack.png` | Good Items/Agua y Backpack | 2 |
| Alimentos | 52 alimentos envasados/conservas; 2 cajas de hamburguesa y 1 caja de pizza | Good Items/Alimentos y Bad Items/Alimentos | 55 |
| Libros | `Book1`, `Book2`, `Book3`; textura `books.png` | Bad Items/Libros | 3 |
| Linterna y Radio | Linterna con botón y radio con antena, mandos y altavoz; excluidos cuchillo, llave y candado | Good Items/Linterna y Radio_Horror | 2 |
| Mantas | Malla base del .blend; variantes Horror y Azul | Good Items/Manta | 2 |
| Mascarilla | Un modelo; variantes sucia y limpia con sus difusas suministradas | Good Items/Mascarilla | 2 |
| PS1 | Solamente la consola y su textura SCPH-5501_on del RAR | Bad Items/PS1 | 1 |
| Pilas y Botiquin | `battery_aa_pack` y `first_aid_box` con tapa; atlas e iconos correspondientes | Good Items/Pilas y Botiquin | 2 |
| TV y Radio | Únicamente `CRTTV` y `RADIO`; excluidos libros, muebles, ventilador y demás accesorios | Bad Items/Televisor_CRT y Good Items/Radio_Retro | 2 |
| Wrench | Llave inglesa y Wrench Texture.png | Good Items/Llave_Inglesa | 1 |

## Preparación de los recursos

- Cada objeto tiene prefab, mallas y materiales preparados. Las partes de linterna, radio, botiquín y vasos de fideos permanecen agrupadas en el prefab correspondiente.
- Texturas con filtrado Point, sin mipmaps y sin compresión. Se conservaron los colores de vértice de los recursos que los usan.
- El shader URP `OndaS/ItemLit`, dentro de `_Shared/Shaders`, combina textura, color y luces de la escena.
- Los modelos GLB se convirtieron a mallas Unity con sus transformaciones, UV y texturas integradas. Se corrigió la orientación vertical de las UV para Unity.
- Los objetos de alimentos estaban agrupados por material: se separaron por geometría y se reunieron las piezas de vasos/tapas y caja de pizza. Se excluyeron ocho bandejas/envases grises sin alimento identificable ni textura.
- La consola PS1 necesitó corregir normales y caras inconsistentes para evitar bandas oscuras en la iluminación.
- Se normalizó la escala por objeto y se colocó su pivote en la base. Son tamaños iniciales de trabajo, no pesos ni dimensiones de inventario definitivas.
- La manta se obtuvo de la malla base del .blend. El objeto original es una tela/alfombra plana y tiene **15 409 triángulos**; conserva más detalle que los demás items. Sus dos variantes comparten la misma malla. Puede simplificarse en una fase posterior si hace falta.
- Las mascarillas utilizan sus texturas difusas; los mapas planos de normal/altura y las imágenes de previsualización no se añadieron como texturas de juego.
- `_Shared` guarda texturas comunes y shader; `_Sources` guarda atribuciones y textos originales. Los paquetes completos y archivos temporales no forman parte de las dependencias finales.
- Los prefabs son recursos visuales. No se han añadido lógica de recogida, inventario, físicas ni efectos de uso.

## Comprobación

Se revisaron las vistas previas de los modelos y se comprobó en Unity que cada prefab tenga malla, UV, materiales, al menos una superficie texturizada y ninguna dependencia de la carpeta temporal de importación. Las tapas de los vasos conservan el color plano que tenían en el original. La validación inicial revisó 71 prefabs y 90 renderers, con 0 errores de referencias. La escena abierta se mantuvo guardada y sin cambios.

## Créditos

Consultar `Assets/OndaS/Items/_Sources/CREDITOS_Y_PROCEDENCIA.txt` y los textos originales que lo acompañan. Los créditos deberán integrarse en la presentación final del juego según la procedencia de cada recurso.

## Catálogo completo

| Prefab | Categoría | Triángulos | Ruta |
|---|---|---:|---|
| Radio_Retro | Good Items | 347 | `Assets/OndaS/Items/Good Items/Radio_Retro/Prefabs/Radio_Retro.prefab` |
| Televisor_CRT | Bad Items | 98 | `Assets/OndaS/Items/Bad Items/Televisor_CRT/Prefabs/Televisor_CRT.prefab` |
| Linterna | Good Items | 604 | `Assets/OndaS/Items/Good Items/Linterna/Prefabs/Linterna.prefab` |
| Radio_Horror | Good Items | 1464 | `Assets/OndaS/Items/Good Items/Radio_Horror/Prefabs/Radio_Horror.prefab` |
| Sopas_y_Caldos_01 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Sopas_y_Caldos_01/Prefabs/Sopas_y_Caldos_01.prefab` |
| Sopas_y_Caldos_02 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Sopas_y_Caldos_02/Prefabs/Sopas_y_Caldos_02.prefab` |
| Sopas_y_Caldos_03 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Sopas_y_Caldos_03/Prefabs/Sopas_y_Caldos_03.prefab` |
| Sopas_y_Caldos_04 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Sopas_y_Caldos_04/Prefabs/Sopas_y_Caldos_04.prefab` |
| Sopas_y_Caldos_05 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Sopas_y_Caldos_05/Prefabs/Sopas_y_Caldos_05.prefab` |
| Sopas_y_Caldos_06 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Sopas_y_Caldos_06/Prefabs/Sopas_y_Caldos_06.prefab` |
| Sopas_y_Caldos_07 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Sopas_y_Caldos_07/Prefabs/Sopas_y_Caldos_07.prefab` |
| Sopas_y_Caldos_08 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Sopas_y_Caldos_08/Prefabs/Sopas_y_Caldos_08.prefab` |
| Hamburguesa_01 | Bad Items | 18 | `Assets/OndaS/Items/Bad Items/Alimentos/Hamburguesa_01/Prefabs/Hamburguesa_01.prefab` |
| Hamburguesa_02 | Bad Items | 18 | `Assets/OndaS/Items/Bad Items/Alimentos/Hamburguesa_02/Prefabs/Hamburguesa_02.prefab` |
| Pasta_01 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Pasta_01/Prefabs/Pasta_01.prefab` |
| Pasta_02 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Pasta_02/Prefabs/Pasta_02.prefab` |
| Pasta_03 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Pasta_03/Prefabs/Pasta_03.prefab` |
| Pasta_04 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Pasta_04/Prefabs/Pasta_04.prefab` |
| Cereales_01 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Cereales_01/Prefabs/Cereales_01.prefab` |
| Cereales_02 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Cereales_02/Prefabs/Cereales_02.prefab` |
| Cereales_03 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Cereales_03/Prefabs/Cereales_03.prefab` |
| Cereales_04 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Cereales_04/Prefabs/Cereales_04.prefab` |
| Cereales_05 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Cereales_05/Prefabs/Cereales_05.prefab` |
| Cereales_06 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Cereales_06/Prefabs/Cereales_06.prefab` |
| Leche_01 | Good Items | 68 | `Assets/OndaS/Items/Good Items/Alimentos/Leche_01/Prefabs/Leche_01.prefab` |
| Leche_02 | Good Items | 84 | `Assets/OndaS/Items/Good Items/Alimentos/Leche_02/Prefabs/Leche_02.prefab` |
| Leche_03 | Good Items | 68 | `Assets/OndaS/Items/Good Items/Alimentos/Leche_03/Prefabs/Leche_03.prefab` |
| Leche_04 | Good Items | 84 | `Assets/OndaS/Items/Good Items/Alimentos/Leche_04/Prefabs/Leche_04.prefab` |
| Leche_05 | Good Items | 76 | `Assets/OndaS/Items/Good Items/Alimentos/Leche_05/Prefabs/Leche_05.prefab` |
| Leche_06 | Good Items | 60 | `Assets/OndaS/Items/Good Items/Alimentos/Leche_06/Prefabs/Leche_06.prefab` |
| Galletas_01 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Galletas_01/Prefabs/Galletas_01.prefab` |
| Galletas_02 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Galletas_02/Prefabs/Galletas_02.prefab` |
| Galletas_03 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Galletas_03/Prefabs/Galletas_03.prefab` |
| Galletas_04 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Galletas_04/Prefabs/Galletas_04.prefab` |
| Galletas_05 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Galletas_05/Prefabs/Galletas_05.prefab` |
| Galletas_06 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Galletas_06/Prefabs/Galletas_06.prefab` |
| Galletas_07 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Galletas_07/Prefabs/Galletas_07.prefab` |
| Conserva_01 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Conserva_01/Prefabs/Conserva_01.prefab` |
| Conserva_02 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Conserva_02/Prefabs/Conserva_02.prefab` |
| Conserva_03 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Conserva_03/Prefabs/Conserva_03.prefab` |
| Conserva_04 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Conserva_04/Prefabs/Conserva_04.prefab` |
| Conserva_05 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Conserva_05/Prefabs/Conserva_05.prefab` |
| Conserva_06 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Conserva_06/Prefabs/Conserva_06.prefab` |
| Conserva_07 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Conserva_07/Prefabs/Conserva_07.prefab` |
| Conserva_08 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Conserva_08/Prefabs/Conserva_08.prefab` |
| Conserva_09 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Conserva_09/Prefabs/Conserva_09.prefab` |
| Salsa_01 | Good Items | 236 | `Assets/OndaS/Items/Good Items/Alimentos/Salsa_01/Prefabs/Salsa_01.prefab` |
| Salsa_02 | Good Items | 236 | `Assets/OndaS/Items/Good Items/Alimentos/Salsa_02/Prefabs/Salsa_02.prefab` |
| Cafe_y_Snacks_01 | Good Items | 84 | `Assets/OndaS/Items/Good Items/Alimentos/Cafe_y_Snacks_01/Prefabs/Cafe_y_Snacks_01.prefab` |
| Cafe_y_Snacks_02 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Cafe_y_Snacks_02/Prefabs/Cafe_y_Snacks_02.prefab` |
| Cafe_y_Snacks_03 | Good Items | 12 | `Assets/OndaS/Items/Good Items/Alimentos/Cafe_y_Snacks_03/Prefabs/Cafe_y_Snacks_03.prefab` |
| Cafe_y_Snacks_04 | Good Items | 16 | `Assets/OndaS/Items/Good Items/Alimentos/Cafe_y_Snacks_04/Prefabs/Cafe_y_Snacks_04.prefab` |
| Cafe_y_Snacks_05 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Cafe_y_Snacks_05/Prefabs/Cafe_y_Snacks_05.prefab` |
| Cafe_y_Snacks_06 | Good Items | 108 | `Assets/OndaS/Items/Good Items/Alimentos/Cafe_y_Snacks_06/Prefabs/Cafe_y_Snacks_06.prefab` |
| Fideos_Instantaneos_01 | Good Items | 244 | `Assets/OndaS/Items/Good Items/Alimentos/Fideos_Instantaneos_01/Prefabs/Fideos_Instantaneos_01.prefab` |
| Fideos_Instantaneos_02 | Good Items | 244 | `Assets/OndaS/Items/Good Items/Alimentos/Fideos_Instantaneos_02/Prefabs/Fideos_Instantaneos_02.prefab` |
| Fideos_Instantaneos_03 | Good Items | 244 | `Assets/OndaS/Items/Good Items/Alimentos/Fideos_Instantaneos_03/Prefabs/Fideos_Instantaneos_03.prefab` |
| Fideos_Instantaneos_04 | Good Items | 244 | `Assets/OndaS/Items/Good Items/Alimentos/Fideos_Instantaneos_04/Prefabs/Fideos_Instantaneos_04.prefab` |
| Pizza_01 | Bad Items | 44 | `Assets/OndaS/Items/Bad Items/Alimentos/Pizza_01/Prefabs/Pizza_01.prefab` |
| Manta | Good Items | 15409 | `Assets/OndaS/Items/Good Items/Manta/Prefabs/Manta.prefab` |
| PS1 | Bad Items | 296 | `Assets/OndaS/Items/Bad Items/PS1/Prefabs/PS1.prefab` |
| Agua | Good Items | 80 | `Assets/OndaS/Items/Good Items/Agua/Prefabs/Agua.prefab` |
| Llave_Inglesa | Good Items | 244 | `Assets/OndaS/Items/Good Items/Llave_Inglesa/Prefabs/Llave_Inglesa.prefab` |
| Libro_01 | Bad Items | 12 | `Assets/OndaS/Items/Bad Items/Libros/Prefabs/Libro_01.prefab` |
| Libro_02 | Bad Items | 12 | `Assets/OndaS/Items/Bad Items/Libros/Prefabs/Libro_02.prefab` |
| Libro_03 | Bad Items | 12 | `Assets/OndaS/Items/Bad Items/Libros/Prefabs/Libro_03.prefab` |
| Mascarilla | Good Items | 546 | `Assets/OndaS/Items/Good Items/Mascarilla/Prefabs/Mascarilla.prefab` |
| Pilas | Good Items | 124 | `Assets/OndaS/Items/Good Items/Pilas/Prefabs/Pilas.prefab` |
| Botiquin | Good Items | 252 | `Assets/OndaS/Items/Good Items/Botiquin/Prefabs/Botiquin.prefab` |
| Manta_Azul | Good Items | 15409 | `Assets/OndaS/Items/Good Items/Manta/Prefabs/Manta_Azul.prefab` |
| Mascarilla_Limpia | Good Items | 546 | `Assets/OndaS/Items/Good Items/Mascarilla/Prefabs/Mascarilla_Limpia.prefab` |

| Backpack | Good Items | 304 | `Assets/OndaS/Items/Good Items/Backpack/Prefabs/Backpack.prefab` |

## Resaltado de ítems

El shader compartido `OndaS/ItemLit` añade un borde claro y una iluminación tenue sobre la textura para distinguir los ítems cercanos en interiores oscuros. Se aplica a los 72 prefabs de la biblioteca mediante sus materiales existentes. Good Items y Bad Items usan el mismo efecto para que su categoría no quede revelada por el color.

El efecto depende de la distancia a la cámara que renderiza la imagen. Tiene intensidad completa hasta 3,5 m y se desvanece hasta desaparecer a 5 m. El pulso es leve y el resaltado utiliza la profundidad normal del modelo, por lo que queda oculto detrás de superficies opacas. No crea luces sobre el escenario ni necesita colliders o scripts en los prefabs.

Los parámetros están disponibles en cada material:

| Parámetro | Valor inicial | Uso |
|---|---:|---|
| Highlight Color | Blanco marfil | Color del borde |
| Highlight Intensity | 0,65 | Intensidad; 0 desactiva el efecto |
| Highlight Distance | 5 m | Distancia máxima |
| Distance Fade | 1,5 m | Tramo final de desvanecimiento |
| Pulse Speed | 1,5 rad/s | Ritmo del pulso; 0 lo deja constante |

[Comparación de agua, botiquín y libro en poca luz](OndaS_Items_Resaltado.png). La prueba de render comparó el efecto desactivado, a corta distancia y fuera de alcance; este último coincidió con el aspecto sin resaltado. Unity comprobó los parámetros en los 73 materiales de la biblioteca y no reportó errores de compilación. La vista Scene también calcula el efecto desde su propia cámara.

### Mochila Backpack

Se añadió como Good Item el objeto `Backpack` de la colección Survival. La segunda variante `Backpack.001` no forma parte del prefab. La mochila tiene 304 triángulos, conserva las UV y usa `Backpack.png` con filtrado Point, sin mipmaps y sin compresión. Su dimensión mayor es 0,45 m y el pivote está en la base. Usa `OndaS/ItemLit` con el resaltado común. Se extrajo la malla a un asset independiente para evitar importar los demás objetos del paquete. El prefab tiene collider, Rigidbody y `OndaSBackpack`; su funcionamiento se describe en [OndaS_Inventario.md](OndaS_Inventario.md).
