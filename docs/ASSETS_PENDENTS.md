# Assets pendents — Xuixo 2026

Llista d'art, so i UI que falten per tenir el joc complet. Pensada per al dissenyador.

---

## 1. Què ja existeix (no cal repetir)

| Asset | Detall |
| --- | --- |
| Fons | `Assets/Backgrounds/Escena1.jpg` … `Escena6.jpg` (2750–3168 px d'ample) |
| Massa (estats) | `Assets/Sprites/` — 8 sprites de 1254×1254 px: `dough`, `kneaded_dough`, `stretched_dough`, `stretched_dough_with_custard`, `rolled_dough_with_custard`, `fried_rolled_dough`, `heavily_sugared_fried_rolled_dough`, `finished_croissant` |
| Ombra | Generada per codi (`Assets/Art/Escena1/ombra.png`) |
| Escena 1 | Muntada i jugable: massa dins el bol pintat → corró pintat |

---

## 2. Estil i especificacions tècniques

- **Estil:** pixel art, la mateixa paleta càlida i el mateix gruix de píxel que els fons.
- **Densitat:** els fons estan pintats a **160 px per unitat de joc** (blocs de
  píxel de ~4 px). Tots els sprites de mon es generen a aquesta densitat:
  es dissenyen a 40 px/unitat i s'escalen ×4 (`tools/genera_art_mon.ps1`).
  - La massa s'ha normalitzat a `Assets/Sprites/160/` (els originals queden intactes).
  - Referència: el bol pintat fa **0,7 unitats** d'ample i el corró **0,9 unitats**.
  - La massa de l'escena 1 fa **0,4 unitats** d'ample.
- **Format:** PNG-24 amb alfa, sense fons, sense ombres pintades (les ombres les posa el joc).
- **Pivot:** centrat a la base de l'objecte (per poder-lo recolzar a la taula).
- **Mides:** potència de 2 no és obligatòria; sí que ho és mantenir la densitat de píxel.
- **Noms:** `snake_case`, sense accents ni espais (`bol_darrere.png`, `corro.png`).
- **Animacions:** spritesheet horitzontal (`nom_anim_6frames.png`) o fitxers separats numerats.
- **UI:** dissenyada a **1920×1080**; panells i botons en **9-slice** (marges de 16–32 px).
- **So:** música en `.ogg`; efectes en `.wav` mono.

> Nota: els sprites actuals de la massa són d'alta resolució (1254 px per 0,4 unitats), molt més fins que el fons. Si volem un acabat pixel-art coherent, caldria **refer la massa a la densitat del fons** (uns 64–96 px per a l'etapa 1). Decidir-ho abans de començar.

---

## 3. Prioritari per jugar de principi a fi (P0)

### 3.1 La massa (personatge)

Per cada etapa de la cadena (dough → kneaded → stretched → custard → rolled → fried → sugared → xuixo):

| Animació / posa | Frames | Notes |
| --- | --- | --- |
| Idle | 2–4 | Respirar/panteixar suaument |
| Rodar / moure's | 4–6 | Gir de la bola; pot ser rotació del mateix sprite |
| Salt / aire | 2 | Estirada a l'aire |
| Caiguda / aterrar | 2 | Aixafada |
| Aixafada pel corró | 1 per etapa | Versió plana (el joc l'escala, però una versió dibuixada queda millor) |
| Dany / rebot (opcional) | 1 | Si hi ha obstacles que fan perdre |

**Mínim per a la jam:** 1 sprite extra per etapa (salt) + 1 d'aixafada. La resta es pot fer amb transformacions (escalar/girar).

### 3.2 Eines i objectes interactius

**Fet:** `Assets/Art/Compartit/eines/` (corro, bol, sac de farina, pot, cullera,
cistella, tapa, safata, pinces, sortidor, plat + `taula_gran`, `bol_gran_*` i
`corro_gran` per a la taula del davant). Es regenera amb `tools/genera_art_mon.ps1`
i es veu a `docs/preview_mon.png`.

Aquests no poden ser part del fons perquè s'han de moure o col·lidir:

| Objecte | Unitats aproximades | Per a què |
| --- | --- | --- |
| Bol (darrere + vora davantera) | 0,7 × 0,35 | Poder balancejar el bol de veritat a l'escena 1 |
| Corró (objecte solt) | 0,9 × 0,25 | Que giri i aplasti la massa |
| Mànega/sortidor de crema | 0,6 × 0,8 | Escena de la crema (Escena4) |
| Rajolí de crema (sprite o spritesheet) | 0,2 × 0,6 | VFX de la crema |
| Bol de sucre + cullera | 0,6 × 0,4 | Ensucre |
| Pinces / espàtula | 0,5 × 0,2 | Treure el xuixo de l'oli |
| Safata / plat | 1,2 × 0,3 | Objectiu final |
| Paella gran (si no es fa servir la del fons) | 2,0 × 1,0 | Fregit (Escena6) |

### 3.3 Plataformes i obstacles (sprites solts)

**Fet:** els objectes d'eines fan també de plataformes (sacs, pots, culleres,
cistelles, tapes, taules). Caldrà afegir variants de color quan es muntin les
escenes 2–6.

Per muntar plataformes sobre els fons cal art separat, no pintat:

| Asset | Mides | Notes |
| --- | --- | --- |
| Sac de farina | 0,7 × 0,9 | Plataforma / obstacle |
| Pot de vidre | 0,35 × 0,5 | Plataforma petita |
| Cullera de fusta | 1,2 × 0,15 | Plataforma inclinada |
| Corró (obstacle) | 0,9 × 0,25 | Roda i empeny |
| Cistella de pa | 0,9 × 0,4 | Plataforma |
| Tapa/olla | 0,6 × 0,2 | Plataforma metàl·lica |
| Taula petita | 2,0 × 0,4 | Plataforma gran |
| Corda/trap | variable | Decoració/obstacle |

Es poden fer **un parell de variants de color** de cada família (fusta, metall, vidre) i reutilitzar-les.

### 3.4 Interfície (UI)

**Fet:** kit base generat a `Assets/Art/UI/` (vegeu `docs/preview_ui.png` i
`docs/preview_hud.png`). Es regenera amb `tools/genera_art_ui.ps1`.

| Asset | Estat |
| --- | --- |
| Logotip `logo_xuixo` | ✅ fet |
| Panell 9-slice `panell_9slice` | ✅ fet |
| Botons normal / hover / premut | ✅ fet |
| Barra d'impuls (marc + farciment) | ✅ fet |
| Icones de fletxes, A/D i stick | ✅ fet |
| Indicadors d'escena | ✅ fet |
| Tipografia pixel amb accents catalans | ❌ pendent (ara s'usa la font per defecte) |
| Pantalla de menú (fons + botons) | ❌ pendent |
| Pantalla de configuració / crèdits | ❌ pendent |
| Icona del joc (512/1024) | ❌ pendent |
| Portada 1920×1080 | ❌ pendent |

### 3.5 Efectes (VFX)

**Fet:** `Assets/Art/Compartit/vfx/` en spritesheets (farina, sucre, oli, esquitx,
crema, estrelles, vapor), a `docs/preview_mon.png`.

| Efecte | Format | Notes |
| --- | --- | --- |
| Farina | spritesheet 6–8 frames | En rodar, en caure, en aixafar-se |
| Polsim de sucre | spritesheet 6 frames | Final |
| Bombolles d'oli | spritesheet 6–8 frames | Escena del fregit |
| Esquitx d'oli | 4 frames | En entrar a la paella |
| Rajolí de crema | 4 frames o sprite allargat | Escena de la crema |
| Estrelles / espurnes | 4–6 frames | En completar |
| Vapor | spritesheet 8 frames | Cuina, paella |

### 3.6 So

**Fet:** `Assets/Audio/` (8 efectes + música de joc de 19 s). Es regenera amb
`tools/genera_audio.ps1`. Integrat a l'escena 1 (`AudioEscena1`), música en bucle,
volum de música i efectes ajustables, i tecla `M` per silenciar.

| Asset | Estat |
| --- | --- |
| Música de joc | ✅ feta (`musica_joc.wav`) |
| SFX massa (remenar, sortida, rodar, aterrar) | ✅ fets |
| SFX eines (corró) | ✅ fet |
| SFX farina / victòria / clic UI | ✅ fets |
| Música de menú | ❌ pendent (es pot fer una variant) |

---

## 4. Per escena (segons els fons que ja tens)

| Escena | Fons | Què és | Art específic que cal |
| --- | --- | --- | --- |
| 1 | `Escena2.jpg` | El bol: la massa surt i arriba al corró | Bol i corró solts (P1), farina |
| 2 | `Escena3.jpg` | Taula vista de dalt: estirar la massa | Corró objecte, masa estirada animada, farina |
| 3 | `Escena4.jpg` | Cuina amb el sortidor: omplir de crema | Rajolí de crema, broquetes, gotes |
| 4 | `Escena5.jpg` | Obrador: donar forma / enfarinar | Corró, farina, plataformes de sacs |
| 5 | `Escena6.jpg` | Paella gegant: fregir | Paella, oli, esquitxos, pinces |
| 6 / final | (reutilitzar) | Ensucre i xuixo acabat | Sucre, plat, confeti, logo final |

---

## 5. Ordre de treball suggerit

1. **UI mínima** (logotip, font, panell, botons, barra d'impuls, icones de fletxes) → desbloqueja el HUD i les pantalles.
2. **Massa: salt i aixafada** per etapa → fa que el joc se senti viu.
3. **Bol i corró solts** → l'escena 1 guanya la mecànica de balanceig visible.
4. **Efectes de farina, oli, sucre i crema** → el joc sembla acabat.
5. **Plataformes i obstacles** → contingut de les escenes 2–6.
6. **So** → música i SFX.
7. **Branding** (icona, portada) → publicació.

---

## 6. Format d'entrega

```
Assets/Art/Compartit/
  ui/            logotip, panells, botons, icones, barra
  eines/         bol, corró, crema, sucre, pinces, safata
  plataformes/   sacs, pots, culleres, cistelles
  vfx/           farina, oli, sucre, crema, estrelles, vapor

Assets/Audio/
  musica/        menu.ogg, joc.ogg
  sfx/           massa_*.wav, ui_*.wav, ...
```

Cada entrega: PNG amb alfa (o spritesheet), nom en `snake_case`, i una nota de la mida en unitats de joc.
