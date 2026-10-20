#!/usr/bin/env python3
"""Renders the KarmaWarnings icon (256x256). The game's Karma blue, drawn by hand, with a hint of a warning.

Usage: make_icon.py [--option 1..5] [out.png]
The option defaults to 3, the one Jacob chose. With no output path it overwrites thunderstore/icon.png.
"""
import math
import os
import sys
from PIL import Image, ImageChops, ImageDraw, ImageFilter

SIZE = 256
SS = 4  # supersampling factor
W = SIZE * SS

BACKGROUND = (14, 8, 22, 255)
KARMA = (104, 140, 250)
KARMA_LIGHT = (184, 196, 255)  # the game's Karma text colour, #b8c4ff
KARMA_DEEP = (36, 52, 170)
KARMA_GLOW = (64, 88, 255)
MOON = (206, 214, 255)
MOON_SHADE = (138, 156, 236)
ALERT = (255, 178, 36)
ALERT_DARK = (70, 30, 8)

DEFAULT_OPTION = 3
DEFAULT_OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "thunderstore", "icon.png")


def s(v):
    return v * SS


def blank_mask():
    return Image.new("L", (W, W), 0)


def solid(color, alpha):
    return Image.merge("RGBA", (*[Image.new("L", (W, W), c) for c in color], alpha))


def paint(art, mask, color, strength=1.0):
    alpha = mask if strength == 1.0 else mask.point(lambda v: int(v * strength))
    art.alpha_composite(solid(color, alpha))


def glow(art, mask, color, blur, strength=1.0):
    paint(art, mask.filter(ImageFilter.GaussianBlur(s(blur))), color, strength)


def outline(mask, width):
    return mask.filter(ImageFilter.MaxFilter(s(width) * 2 | 1))


def disc(cx, cy, r):
    mask = blank_mask()
    ImageDraw.Draw(mask).ellipse([s(cx - r), s(cy - r), s(cx + r), s(cy + r)], fill=255)
    return mask


def ring(cx, cy, r, thickness):
    return ImageChops.subtract(disc(cx, cy, r), disc(cx, cy, r - thickness))


def rounded_rect(x0, y0, x1, y1, r):
    mask = blank_mask()
    ImageDraw.Draw(mask).rounded_rectangle([s(x0), s(y0), s(x1), s(y1)], radius=s(r), fill=255)
    return mask


def polygon(points):
    mask = blank_mask()
    ImageDraw.Draw(mask).polygon([(s(x), s(y)) for x, y in points], fill=255)
    return mask


def rounded(mask, radius):
    """Blurring then re-thresholding rounds corners, as in the other mods' icons."""
    return mask.filter(ImageFilter.GaussianBlur(s(radius))).point(lambda v: 255 if v > 110 else 0)


def exclamation(cx, top, height, width):
    bar = rounded_rect(cx - width / 2, top, cx + width / 2, top + height * 0.62, width / 2)
    dot = disc(cx, top + height * 0.9, width * 0.58)
    return ImageChops.lighter(bar, dot)


def triangle(cx, cy, radius):
    pts = [(cx + radius * math.cos(math.radians(a)), cy + radius * math.sin(math.radians(a))) for a in (-90, 30, 150)]
    return rounded(polygon(pts), radius * 0.08)


def karma_ring(art, cx, cy, r, thickness):
    """The glowing blue ring from the game's Karma icon."""
    mask = ring(cx, cy, r, thickness)
    glow(art, mask, KARMA_GLOW, 9, 0.9)
    paint(art, outline(mask, 2), KARMA_DEEP)
    paint(art, mask, KARMA)
    paint(art, ring(cx, cy, r - 2, thickness * 0.34), KARMA_LIGHT, 0.75)


def orb(art, cx, cy, r, color=KARMA_LIGHT):
    mask = disc(cx, cy, r)
    glow(art, mask, KARMA_GLOW, 6, 0.9)
    paint(art, outline(mask, 2), KARMA_DEEP)
    paint(art, mask, color)


def moon(art, cx, cy, r):
    mask = disc(cx, cy, r)
    glow(art, mask, KARMA_GLOW, 12, 0.8)
    paint(art, outline(mask, 2), KARMA_DEEP)
    paint(art, mask, MOON)
    # Craters: soft darker patches kept inside the disc.
    craters = blank_mask()
    d = ImageDraw.Draw(craters)
    for dx, dy, cr in ((-0.32, -0.28, 0.22), (0.30, -0.05, 0.16), (-0.08, 0.38, 0.19), (0.36, 0.34, 0.09)):
        d.ellipse([s(cx + dx * r - cr * r), s(cy + dy * r - cr * r), s(cx + dx * r + cr * r), s(cy + dy * r + cr * r)], fill=255)
    craters = ImageChops.multiply(craters.filter(ImageFilter.GaussianBlur(s(1.5))), mask)
    paint(art, craters, MOON_SHADE, 0.75)


def warning_triangle(art, cx, cy, radius):
    tri = triangle(cx, cy, radius)
    paint(art, outline(tri, 3), ALERT_DARK)
    paint(art, tri, ALERT)
    paint(art, exclamation(cx, cy - radius * 0.34, radius * 0.74, radius * 0.17), ALERT_DARK)


def option_1(art):
    """Karma ring and orb, with an exclamation mark where the disc is."""
    cx, cy, r = 128, 134, 92
    karma_ring(art, cx, cy, r, 15)
    orb(art, cx, cy - r, 17)
    mark = exclamation(cx, cy - 46, 92, 20)
    paint(art, outline(mark, 2), KARMA_DEEP)
    paint(art, mark, KARMA_LIGHT)


def option_2(art):
    """A cratered moon with an amber warning triangle."""
    moon(art, 118, 122, 84)
    warning_triangle(art, 178, 178, 54)


def option_3(art):
    """A crescent moon inside the Karma ring, with an alert badge."""
    cx, cy, r = 122, 134, 92
    karma_ring(art, cx, cy, r, 14)
    crescent = ImageChops.subtract(disc(cx, cy, 54), disc(cx + 20, cy - 8, 46))
    glow(art, crescent, KARMA_GLOW, 6, 0.9)
    paint(art, outline(crescent, 2), KARMA_DEEP)
    paint(art, crescent, MOON)
    badge = disc(196, 66, 34)
    paint(art, outline(badge, 3), ALERT_DARK)
    paint(art, badge, ALERT)
    paint(art, exclamation(196, 46, 40, 9), ALERT_DARK)


def option_4(art):
    """The Karma ring with an upward arrow: Karma going up. The orb is amber."""
    cx, cy, r = 128, 134, 92
    karma_ring(art, cx, cy, r, 15)
    orb(art, cx, cy - r, 17, ALERT)
    arrow = polygon([(cx, cy - 56), (cx + 46, cy - 6), (cx + 17, cy - 6), (cx + 17, cy + 52),
                     (cx - 17, cy + 52), (cx - 17, cy - 6), (cx - 46, cy - 6)])
    arrow = rounded(arrow, 2.5)
    glow(art, arrow, KARMA_GLOW, 5, 0.9)
    paint(art, outline(arrow, 2), KARMA_DEEP)
    paint(art, arrow, KARMA_LIGHT)


def option_5(art):
    """A moon in a crosshair, with an amber alert above it: the banner sits above the crosshair."""
    cx, cy = 128, 146
    ringed = ring(cx, cy, 82, 8)
    glow(art, ringed, KARMA_GLOW, 8, 0.9)
    paint(art, outline(ringed, 2), KARMA_DEEP)
    paint(art, ringed, KARMA)
    for dx, dy in ((0, -1), (0, 1), (-1, 0), (1, 0)):
        x0, y0 = cx + dx * 56, cy + dy * 56
        x1, y1 = cx + dx * 106, cy + dy * 106
        tick = rounded_rect(min(x0, x1) - 4, min(y0, y1) - 4, max(x0, x1) + 4, max(y0, y1) + 4, 4)
        paint(art, tick, KARMA)
    moon(art, cx, cy, 38)
    pill = rounded_rect(86, 26, 170, 66, 20)
    paint(art, outline(pill, 3), ALERT_DARK)
    paint(art, pill, ALERT)
    paint(art, exclamation(128, 33, 26, 7), ALERT_DARK)


OPTIONS = {1: option_1, 2: option_2, 3: option_3, 4: option_4, 5: option_5}


def centered(art):
    """Moves the artwork so the middle of its solid parts, not its glow, is the middle of the icon."""
    solid_bits = art.getchannel("A").point(lambda v: 255 if v > 200 else 0)
    box = solid_bits.getbbox()
    if box:
        dx = W // 2 - (box[0] + box[2]) // 2
        dy = W // 2 - (box[1] + box[3]) // 2
        art = ImageChops.offset(art, dx, dy)
    return art


def render(option):
    art = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    OPTIONS[option](art)
    art = centered(art)
    img = Image.new("RGBA", (W, W), BACKGROUND)
    img.alpha_composite(art)
    return img.resize((SIZE, SIZE), Image.LANCZOS)


def main(argv):
    option = DEFAULT_OPTION
    out = DEFAULT_OUT
    args = list(argv)
    if "--option" in args:
        i = args.index("--option")
        option = int(args[i + 1])
        del args[i:i + 2]
    if args:
        out = args[0]
    if option not in OPTIONS:
        sys.exit("--option must be 1..%d." % len(OPTIONS))
    render(option).save(out)


if __name__ == "__main__":
    main(sys.argv[1:])
