"""Finish a generated surface as a game texture.

Usage: python3 tools/art/tiles.py <kind> <in.png> <out.png> [brightness]

Kinds: carpet (tiles laid a quarter-turn apart, with seams), ceiling (a T-bar grid), blocks
(concrete blocks in running bond), boards (planks along the corridor), plain (as is). Each is first brought to a mean brightness that reads in the game's dim light. Lines a
model draws come out wavy, so the straight ones are drawn here. Every output is a 512 px RGB PNG
saved fresh, so no generator metadata survives, and it still tiles: tiles and grid lines sit on
the same lattice at each edge. Needs Pillow.
"""
import random
import sys

from PIL import Image, ImageDraw, ImageEnhance, ImageStat

SIZE = 512
CELLS = 4  # Each texture covers 2 m in the game, so a cell is 0.5 m

# Mean brightness each kind ends at: the scenes are dim and fogged, and dark textures vanish.
LEVEL = {"carpet": 0.42, "ceiling": 0.82, "blocks": 0.78, "boards": 0.45, "plain": 0.8}


# A gamma curve rather than a gain, so a dark source is lifted without clipping its highlights.
def level(image, target):
    low, high = 0.05, 20.0
    for _ in range(30):
        gamma = (low * high) ** 0.5
        table = [round(255 * (v / 255) ** gamma) for v in range(256)]
        mean = ImageStat.Stat(image.convert("L").point(table)).mean[0] / 255
        low, high = (low, gamma) if mean < target else (gamma, high)
    return image.point(table * 3)


def carpet(surface, seed=7):
    rng = random.Random(seed)
    cell = SIZE // CELLS
    out = Image.new("RGB", (SIZE, SIZE))
    for row in range(CELLS):
        for col in range(CELLS):
            piece = surface.crop((col * cell, row * cell, (col + 1) * cell, (row + 1) * cell))
            if (row + col) % 2:
                piece = piece.rotate(90)
            piece = ImageEnhance.Brightness(piece).enhance(1 + rng.uniform(-0.05, 0.05))
            out.paste(piece, (col * cell, row * cell))
    draw = ImageDraw.Draw(out)
    for i in range(CELLS):
        # One column either side of the lattice line, wrapping, so edge seams match inner ones.
        for x in (i * cell - 1, i * cell):
            draw.line([(x % SIZE, 0), (x % SIZE, SIZE)], fill=(40, 46, 56))
            draw.line([(0, x % SIZE), (SIZE, x % SIZE)], fill=(40, 46, 56))
    return out


def ceiling(surface, seed=7):
    rng = random.Random(seed)
    cell = SIZE // CELLS
    out = surface.copy()
    for row in range(CELLS):
        for col in range(CELLS):
            box = (col * cell, row * cell, (col + 1) * cell, (row + 1) * cell)
            piece = ImageEnhance.Brightness(out.crop(box)).enhance(1 + rng.uniform(-0.03, 0.03))
            out.paste(piece, box[:2])
    draw = ImageDraw.Draw(out)
    for i in range(CELLS):
        at = i * cell
        # A bar centred on the lattice line, wrapping, so it crosses the edge intact.
        for x in range(at - 3, at + 4):
            draw.line([(x % SIZE, 0), (x % SIZE, SIZE)], fill=(205, 207, 210))
            draw.line([(0, x % SIZE), (SIZE, x % SIZE)], fill=(205, 207, 210))
    return out


def blocks(surface, seed=7):
    # 0.5 x 0.25 m blocks, each row half a block along from the last.
    rng = random.Random(seed)
    wide, high = SIZE // 4, SIZE // 8
    out = surface.copy()
    draw = ImageDraw.Draw(out)
    for row in range(8):
        shift = (wide // 2) * (row % 2)
        for col in range(4):
            left = (col * wide + shift) % SIZE
            shade = 1 + rng.uniform(-0.04, 0.04)
            for part in ((left, min(left + wide, SIZE)), (0, max(0, left + wide - SIZE))):
                if part[1] > part[0]:
                    box = (part[0], row * high, part[1], (row + 1) * high)
                    out.paste(ImageEnhance.Brightness(out.crop(box)).enhance(shade), box[:2])
        # Bold enough to read across the corridor at speed: 4 px is 16 mm at 2 m a texture.
        for joint in range(4):
            for x in range(shift + joint * wide - 2, shift + joint * wide + 2):
                draw.line([(x % SIZE, row * high), (x % SIZE, (row + 1) * high - 1)], fill=(112, 108, 100))
        for y in range(row * high - 2, row * high + 2):
            draw.line([(0, y % SIZE), (SIZE, y % SIZE)], fill=(112, 108, 100))
    return out


def boards(surface, seed=7):
    # 0.25 m planks running along the corridor, each with one end joint where it meets the next.
    rng = random.Random(seed)
    wide = SIZE // 8
    out = surface.copy()
    draw = ImageDraw.Draw(out)
    for plank in range(8):
        box = (plank * wide, 0, (plank + 1) * wide, SIZE)
        out.paste(ImageEnhance.Brightness(out.crop(box)).enhance(1 + rng.uniform(-0.08, 0.08)), box[:2])
        joint = rng.randrange(SIZE)
        for y in (joint - 1, joint):
            draw.line([(plank * wide, y % SIZE), ((plank + 1) * wide - 1, y % SIZE)], fill=(48, 34, 24))
        for x in (plank * wide - 1, plank * wide):
            draw.line([(x % SIZE, 0), (x % SIZE, SIZE)], fill=(48, 34, 24))
    return out


def main():
    kind, source, target = sys.argv[1:4]
    brightness = float(sys.argv[4]) if len(sys.argv) > 4 else LEVEL[kind]
    surface = Image.open(source).convert("RGB").resize((SIZE, SIZE), Image.LANCZOS)
    finish = {"carpet": carpet, "ceiling": ceiling, "blocks": blocks, "boards": boards,
              "plain": lambda s: s}[kind]
    image = finish(level(surface, brightness))
    clean = Image.new("RGB", image.size)
    clean.putdata(list(image.getdata()))
    clean.save(target, format="PNG", optimize=True)


if __name__ == "__main__":
    main()
