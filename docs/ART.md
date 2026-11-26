# Art pipeline

How generated textures and props get into the game. The direction and budget are decided in
`docs/DIRECTION.md`; this is the how. Generation runs on the owner's machine (it needs the GPU),
in a Claude session there or by hand. Everything after the files exist runs anywhere.

## The rules

- Commit the assets. Nothing is generated at runtime: the exit rule needs a lap that looks the
  same every time unless something is deliberately wrong.
- No art is required. A floor kind or a place with no files keeps the procedural look, so every
  slot can be filled one file at a time.
- The WebGL build stays at 25 MB or under. `scripts/build-web.sh` fails past it and lists the
  largest assets.
- Liminal places of our own. Prompt for the office, school and house as they are described in
  `Places.cs`; never for the Backrooms or another game's look.

## Where files go

Unity loads these by path, so the names matter. Each file needs its `.meta`, which Unity writes
the first time the project opens or builds with the file in place.

```
Assets/Resources/Art/Floors/<Floor>.png       Linoleum, Carpet, Tile or Boards: used wherever
                                              that floor is laid, in any run
Assets/Resources/Art/Places/<id>/Wall.png     id is the place's Id: office, school, house
Assets/Resources/Art/Places/<id>/Ceiling.png
Assets/Resources/Art/Places/<id>/Props/*      prefabs, one per prop
Assets/Art/Sources.md                         where each file came from (below)
```

A place's walls and ceiling appear in its story runs and in a daily wearing its look. Free
runs keep the open void.

## Textures

- 512 x 512 PNG, tileable in both directions, albedo only. 1024 only where 512 visibly blurs.
- Check a texture tiles before committing it: offset it by half its size and look for seams.
- Import settings: max size 512 (1024 for the exceptions), default compression, mip maps on.
  Unity's defaults on import are close; set the max size.
- Mid-grey to light values: the scenes are dim and fogged, and dark textures vanish.

Every texture covers 2 x 2 m and repeats: four times across the 8 m floor slab, about five
times along a 10 m wall, and so on (`PlaceArt`). So a 512 texture is 256 px a metre, and a
pattern in it should be drawn at real size: four 0.5 m carpet tiles across, for instance.

### Making one

Models draw textures well but straight lines badly: a grid comes out wavy. So generate the
surface material alone, as a close-up, and let `tools/art/tiles.py` draw the straight parts:

```
python3 tools/art/tiles.py carpet  surface.png Assets/Resources/Art/Floors/Carpet.png
python3 tools/art/tiles.py ceiling surface.png Assets/Resources/Art/Places/office/Ceiling.png
python3 tools/art/tiles.py plain   surface.png Assets/Resources/Art/Places/office/Wall.png
```

`carpet` lays the surface as 0.5 m tiles a quarter-turn apart with seams; `ceiling` draws a
0.5 m T-bar grid; `plain` keeps it as it is. Each saves a fresh PNG, so no metadata survives.

Turn circular padding on in the model's convolutions (in diffusers, `padding_mode =
"circular"` on every `Conv2d` in the UNet and VAE) and the output tiles by itself.

### Prompts for the office

What worked, with Stable Diffusion 1.5 at 512 px, 25 steps, guidance 7:

- Carpet surface: "seamless tileable texture, close-up of short pile commercial office carpet,
  grey-blue, fine uniform fibres, top-down, flat even lighting, no pattern"
- Wall: "seamless tileable texture, off-white painted plasterboard office wall, faint scuffs,
  flat lighting, straight on"
- Ceiling surface: "seamless tileable texture, close-up of a white mineral fibre ceiling tile
  surface, small fissures and pinholes, uniform, flat even lighting, straight on"
- Negative, for all: "perspective, shadows, vignette, text, watermark, logo, people, objects,
  furniture, blurry, frame, border"

Generate several seeds and pick by eye; record the seed in `Sources.md`.

## Props

- Landmarks a player would remember along a corridor: for the office a water cooler, a
  photocopier, a potted plant, a filing cabinet, a coat stand.
- Make a concept image first, then a model from it with a local image-to-3D model.
- Under 3,000 triangles, one material, one 512 texture. Real-world scale in metres, the pivot
  at the base, facing +Z.
- Import the model (OBJ or FBX import with no package; GLB needs one, so convert it in Blender
  first), make a prefab of it in `Props/`, and give it no collider or Rigidbody: props are scenery,
  a collider would kill or snag the player, and a Rigidbody would drop it through the floor.
- Until then each place has stand-ins built from primitives by
  `Assets/Scripts/Editor/PropPrefabs.cs` (Liminal Leap > Generate Stand-in Props). A generated
  prefab with the same name replaces one; delete its line from the script so a rerun doesn't
  bring it back.
- A place gets 3 to 5 props to start. Each piece may stand one beside the track, chosen from
  the piece's seed, so a loop replays them where they were.

## Before committing

1. Strip generator metadata from every PNG and JPEG with max-kit's asset-metadata skill.
2. Record each file in `Assets/Art/Sources.md`: the file, the model and its version, the model's
   licence, and the prompt or source image.
3. Check each model's licence yourself, for use in the UK and in a game sold or given away, and
   write down what it says. Some image-to-3D models exclude the UK; don't rely on memory.
4. `scripts/build-web.sh` passes, within the budget.
5. Load the build in a browser, start a run in the place, and look: the art reads at running
   speed, the anomalies still read against it, and the console is clean. The story starts in
   the office, so reach another place through the daily, which wears a place picked from the
   date: shift the page's clock to a day whose daily is that place, and press Enter without
   clicking first, since the title takes a click as any key and starts the story.

## On the owner's machine

Before a batch: max-kit's gpu-preflight skill (power, GPU, nothing else on it), and its
game-mode skill's lock if the machine might be wanted. A generation batch is a heavy run.
