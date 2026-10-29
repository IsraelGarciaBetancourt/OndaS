PSX OLD OBJECTS - FREE PACK (90s TV room)
=========================================

10 low-poly PlayStation 1 style models, ready for any engine.

Format ........... GLB (binary glTF 2.0), texture embedded in each file
Textures ......... PNG 128 or 256 px, 15-bit colour, up to 256 colours (PS1 CLUT style),
                   NEAREST filtering (crisp pixels)
Scale ............ real-world metres (1 unit = 1 m)
Axes ............. glTF standard: Y up, the object's front faces +Z
Origin ........... on the floor, centred (exceptions below)

MODELS (models\)
----------------
 1  CRTTV.glb ........... 29" CRT TV                        98 tris   0.80 x 0.53 x 0.60 m
 2  TVREMOTE.glb ........ TV remote                         68 tris   0.05 x 0.18 x 0.02 m
 3  POLTRONA.glb ........ old armchair                     252 tris   0.89 x 0.84 x 0.95 m
 4  RUG.glb ............. Persian rug                       14 tris   2.10 x 1.40 m
 5  WALLCLOCK.glb ....... wall clock                       330 tris   Ø 0.31 m
 6  RADIO.glb ........... portable radio                   347 tris   0.30 x 0.14 x 0.40 m
 7  CEILINGFAN.glb ...... ceiling fan                      374 tris   Ø 1.04 m
 8  BOOK.glb ............ old book (3 covers)               64 tris   0.04 x 0.17 x 0.24 m
    BOOK_B.glb / BOOK_C.glb  = same book, the other two covers
 9  NIGHTSTAND.glb ...... carved nightstand               568 tris   0.52 x 0.39 x 0.56 m
10  DESKLAMP.glb ........ desk lamp                        548 tris   0.26 x 0.42 x 0.45 m

MOVING PARTS (separate nodes inside the GLB, ready to animate)
--------------------------------------------------------------
NIGHTSTAND_DRAWER_1..3 ... drawers (bottom to top). Open by moving along local +Z (forward), up to ~0.25 m.
WALLCLOCK_HAND_HOUR / _MINUTE / _SECOND ... clock hands, pivot at the clock centre. Rotate around local Z;
                           at angle 0 they point at 12. Set to 10:10:35. Clockwise (seen from the front)
                           = negative rotation around Z.
CEILINGFAN_BLADES ........ blades, pivot on the motor axis. Spin around local Y.

SPECIAL ORIGINS
---------------
WALLCLOCK .... origin at the bottom BACK (put the origin against the wall).
CEILINGFAN ... origin at the TOP (ceiling mount point); the fan hangs 0.37 m down.
TVREMOTE ..... lying with the buttons up.

TIPS
----
- Keep texture filtering on "Point"/"Nearest" for the PS1 look.
- For vertex jitter / affine texture warping, use a PSX shader in your engine.

LICENSE
-------
Free to use in personal and commercial projects. Credit is appreciated but not required.
You may modify the models. You may not resell or redistribute the models themselves,
on their own or as part of another asset pack.

Made by ghinem - https://ghinem.itch.io
