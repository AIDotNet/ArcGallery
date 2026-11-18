#!/usr/bin/env python3
"""Builds gallery/TKCode.Arc.Gallery.Browser/wwwroot/fonts/NotoEmoji-Subset.ttf, the emoji font of the WebAssembly host.

A browser has no system fonts for Skia, so an emoji (a reaction in a comment thread, a party popper in a toast) is a box unless
the app brings a font. This cuts the monochrome Noto Emoji (SIL OFL 1.1, a variable font) to the Regular weight and to the
emoji blocks: regional indicators, pictographs, emoticons, transport, supplemental symbols, dingbats and the variation selector, about 0.7 MB, loaded
only when an emoji is first drawn (EmojiFont.cs). The glyphs are outlines in the text colour, not the colour emoji of the
phone: a colour font is 10 MB, and a bitmap font does not scale with the theme.

  python3 -m venv /tmp/ftvenv && /tmp/ftvenv/bin/pip install fonttools brotli
  /tmp/ftvenv/bin/python tools/fonts/subset-noto-emoji.py [NotoEmoji.ttf]

Without an argument the source font is downloaded from github.com/google/fonts.
"""
import os
import sys
import tempfile
import urllib.request

from fontTools import subset
from fontTools.ttLib import TTFont
from fontTools.varLib import instancer

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUTPUT = os.path.join(ROOT, "gallery", "TKCode.Arc.Gallery.Browser", "wwwroot", "fonts", "NotoEmoji-Subset.ttf")
SOURCE_URL = "https://github.com/google/fonts/raw/main/ofl/notoemoji/NotoEmoji%5Bwght%5D.ttf"
# Keep in step with EmojiFont.Ranges (the code points that ask this font).
UNICODES = "U+1F1E6-1F1FF,U+1F300-1F64F,U+1F680-1F6FF,U+1F900-1F9FF,U+2600-27BF,U+2B50,U+2B55,U+200D,U+FE0F"


def main():
    if len(sys.argv) > 1:
        source = sys.argv[1]
    else:
        source = os.path.join(tempfile.gettempdir(), "NotoEmoji[wght].ttf")
        if not os.path.exists(source):
            print("downloading", SOURCE_URL)
            urllib.request.urlretrieve(SOURCE_URL, source)

    font = instancer.instantiateVariableFont(TTFont(source), {"wght": 400}, updateFontNames=True)
    options = subset.Options()
    options.layout_features = []
    options.hinting = False
    options.desubroutinize = True
    options.notdef_outline = True
    subsetter = subset.Subsetter(options)
    subsetter.populate(unicodes=subset.parse_unicodes(UNICODES))
    subsetter.subset(font)
    font.save(OUTPUT)
    print(OUTPUT, os.path.getsize(OUTPUT), "bytes,", len(font.getBestCmap()), "characters")


if __name__ == "__main__":
    main()
