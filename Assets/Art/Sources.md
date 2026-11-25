# Art sources

Where each generated file came from, as `docs/ART.md` asks. Model licences were read from the
licence text, not remembered.

## Model

Stable Diffusion 1.5 (`stable-diffusion-v1-5/stable-diffusion-v1-5`, revision
`451f4fe16113bff5a5d2269ed5ad43b0592e9a14`), the fp16 safetensors weights, each checked
against its published sha256. Run on a CPU with diffusers 0.31.0 and
PyTorch 2.5.1, at 512 x 512, 25 steps of DPM-Solver multistep, guidance 7, with circular
padding in every convolution so the output tiles.

Licence: CreativeML OpenRAIL-M. Section 6: "Licensor claims no rights in the Output You
generate using the Model. You are accountable for the Output you generate and its subsequent
uses." It allows commercial use and names no countries; its use restrictions (Attachment A)
are about harmful uses, which game textures aren't.

Negative prompt for all: "perspective, shadows, vignette, text, watermark, logo, people,
objects, furniture, blurry, frame, border"

## Files

| File | Prompt | Seed | Finished with |
|---|---|---|---|
| `Resources/Art/Floors/Carpet.png` | seamless tileable texture, close-up of short pile commercial office carpet, grey-blue, fine uniform fibres, top-down, flat even lighting, no pattern | 29 | `tools/art/tiles.py carpet` |
| `Resources/Art/Places/office/Ceiling.png` | seamless tileable texture, close-up of a white mineral fibre ceiling tile surface, small fissures and pinholes, uniform, flat even lighting, straight on | 17 | `tools/art/tiles.py ceiling` |
| `Resources/Art/Places/office/Wall.png` | seamless tileable texture, off-white painted plasterboard office wall, faint scuffs, flat lighting, straight on | 11 | `tools/art/tiles.py plain` |
| `Resources/Art/Floors/Linoleum.png` | seamless tileable texture, close-up of speckled vinyl school corridor floor, pale grey-green with small flecks, top-down, flat even lighting, no pattern | 7 | `tools/art/tiles.py plain` at 0.6 |
| `Resources/Art/Places/school/Wall.png` | seamless tileable texture, close-up of painted concrete block surface, pale cream paint, slight pores, flat even lighting, straight on, no lines | 7 | `tools/art/tiles.py blocks` |
| `Resources/Art/Places/school/Ceiling.png` | the office ceiling's prompt | 29 | `tools/art/tiles.py ceiling` |
| `Resources/Art/Floors/Boards.png` | seamless tileable texture, close-up of worn oak wood grain, warm mid brown, top-down, flat even lighting, straight grain | 7 | `tools/art/tiles.py boards` |
| `Resources/Art/Places/house/Wall.png` | seamless tileable texture, faded old wallpaper, small muted floral pattern, pale yellow and grey, flat lighting, straight on | 31 | `tools/art/tiles.py plain` at 0.7 |
| `Resources/Art/Places/house/Ceiling.png` | seamless tileable texture, close-up of off-white textured plaster ceiling, gentle stipple, flat even lighting, straight up | 7 | `tools/art/tiles.py plain` at 0.82 |

Each was saved fresh by `tiles.py`, so it carries no generator metadata.
