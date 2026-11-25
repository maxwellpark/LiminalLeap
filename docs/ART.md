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

The floor texture covers one 8 x 10 m slab once, so a 512 texture with about 8 tiles across
reads as carpet tiles or floor tiles at the right size. Walls are 10 m long and 9.5 m high,
the ceiling 17 x 10 m.

### Prompts for the office

Start from these and keep what works in `Sources.md`:

- Carpet: "seamless tileable texture, worn grey-blue office carpet tiles, top-down, flat even
  lighting, no shadows, no perspective"
- Wall: "seamless tileable texture, off-white painted plasterboard office wall with a faint
  scuffed skirting line, flat lighting, straight on"
- Ceiling: "seamless tileable texture, office suspended ceiling tiles, white mineral fibre with
  pinholes, thin grey grid, flat lighting, straight up"

Generate several, pick one, then make it tileable if the model didn't (an img2img pass with
tiling on, or a seamless-texture node).

## Props

- Landmarks a player would remember along a corridor: for the office a water cooler, a
  photocopier, a potted plant, a filing cabinet, a coat stand.
- Make a concept image first, then a model from it with a local image-to-3D model.
- Under 3,000 triangles, one material, one 512 texture. Real-world scale in metres, the pivot
  at the base, facing +Z.
- Import the model (OBJ or FBX import with no package; GLB needs one, so convert it in Blender
  first), make a prefab of it in `Props/`, and give it no collider or Rigidbody: props are scenery,
  a collider would kill or snag the player, and a Rigidbody would drop it through the floor.
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
   speed, the anomalies still read against it, and the console is clean.

## On the owner's machine

Before a batch: max-kit's gpu-preflight skill (power, GPU, nothing else on it), and its
game-mode skill's lock if the machine might be wanted. A generation batch is a heavy run.
