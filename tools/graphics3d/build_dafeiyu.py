"""Rebuild the Dafeiyu GLB from the supplied three-view character sheet."""

import argparse
from pathlib import Path

import numpy as np
from PIL import Image
from scipy import ndimage
from scipy.interpolate import LinearNDInterpolator, NearestNDInterpolator

from dafeiyu_mesh import HEIGHT, color, curve, extend, gaussian, loft, mix, polygon, relief, sample, sdf, smooth, write_glb


class Reference:
    def __init__(self, sheet, left, right, holes=()):
        self.rgb = np.asarray(sheet.crop((left, 0, right, HEIGHT)), dtype=np.float32)
        ink = (self.rgb.min(axis=2) < 187) | (self.rgb.max(axis=2) < 225)
        ink[744:] &= self.rgb[744:].min(axis=2) < 155
        ink = ndimage.binary_closing(ink)
        self.silhouette = ndimage.binary_fill_holes(ink)
        for seed_x, seed_y in holes:
            seed = np.zeros_like(ink)
            seed[seed_y, seed_x] = True
            self.silhouette &= ~ndimage.binary_propagation(seed, mask=~ink)

    def area(self, path):
        return polygon(self.silhouette.shape, path)

    def cut(self, path):
        return self.silhouette & self.area(path)

    def palette(self, mask, soften=False):
        return filled_palette(self.rgb, mask, soften)


def filled_palette(rgb, mask, soften=False):
    bank = extend(rgb, mask)
    if soften:
        feather = smooth(0, 9, ndimage.distance_transform_edt(~mask))
        bank = mix(bank, ndimage.gaussian_filter(bank, (6, 6, 0)), feather)
    return bank


def fullness(distance, width):
    return np.sqrt(1 - np.clip(1 - distance / width, 0, 1)**2)


def width_profile(mask):
    rows = np.arange(mask.shape[0])
    occupied = rows[mask.any(axis=1)]
    borders = (mask.argmax(axis=1), mask.shape[1] - 1 - mask[:, ::-1].argmax(axis=1))
    return tuple(ndimage.gaussian_filter1d(np.interp(rows, occupied, border[occupied]), 5) for border in borders)


def projection_facing(depth):
    slope_y, slope_x = np.gradient(ndimage.gaussian_filter(depth, 1.8))
    front = 1 / np.sqrt(1 + slope_x**2 + slope_y**2)
    return front, np.abs(slope_x) * front


def plate(name, rgb, mask, center, depth, width=17, *, back=None, paint_mask=None, step=4, smoothing=1.1):
    radius = ndimage.distance_transform_edt(mask)
    interior = radius > min(4.5, radius.max() / 3)
    palette = filled_palette(rgb, interior if paint_mask is None else interior & paint_mask)
    rim_color = ndimage.gaussian_filter(palette, (4, 4, 0))

    def sculpt(x, y, distance):
        axis = center(x, y)
        radius = (depth(x, y) if callable(depth) else depth) * fullness(distance, width)
        return axis - radius, axis + radius

    def paint(x, y, distance, profile, rear):
        original = mix(color(rim_color, x, y), color(palette, x, y), smooth(0, 7, distance))
        if rear:
            interior = smooth(1, width * .8, distance)
            shade = original * .83 if back is None else back(x, y, profile, original)
            return mix(original, shade, interior)
        return original

    return relief(name, mask, sculpt, paint, step, smoothing)


def headdress_mask(front):
    outline = front.cut('123,230 129,202 137,176 153,147 168,123 190,100 211,80 243,65 270,55 310,54 '
                        '332,59 355,54 382,58 413,73 438,82 468,107 487,139 491,168 493,195 486,223 '
                        '461,226 448,181 431,149 410,127 387,114 361,104 326,98 299,98 265,109 233,127 '
                        '207,150 185,185 164,221 148,239')
    red, green, blue = np.moveaxis(front.rgb, -1, 0)
    white = outline & (red > 129) & (blue - green < 52) & (red + 26 > green)
    return outline & ndimage.binary_fill_holes(ndimage.binary_dilation(white, iterations=2))


def fin_bow_axis(x):
    return 221 + .42 * (np.abs(x - 314) - 154)


def headdress_axis(x, y):
    crown = curve(y, [(54, 171), (78, 177), (103, 191), (129, 210), (160, 229), (189, 239)])
    crown += .045 * np.abs(x - 314)
    at_bows = smooth(165, 220, y) * smooth(100, 152, np.abs(x - 314))
    return crown + (fin_bow_axis(x) - crown) * at_bows


def fin_bow_masks(front):
    outlines = [
        ('Left fin bow', '127,218 143,216 154,224 163,226 173,222 181,228 183,247 174,259 159,264 '
         '151,254 141,260 128,255 123,240'),
        ('Right fin bow', '452,217 467,215 483,223 494,222 507,210 521,208 527,216 526,231 531,247 523,255 '
         '508,253 494,246 483,249 478,261 467,273 459,257 460,245 454,238'),
    ]
    blue = (front.rgb[..., 2] > front.rgb[..., 0] + 45) & (front.rgb[..., 1] > 98)
    for name, outline in outlines:
        mask = front.cut(outline)
        yield name, ndimage.binary_fill_holes(ndimage.binary_dilation(blue & mask, iterations=2)) & mask


def head_and_hair(front, side, rear):
    mane = front.cut('130,210 147,142 180,113 221,96 270,92 314,89 393,92 451,99 495,132 529,195 '
                     '513,243 522,309 548,351 600,378 643,441 642,509 546,535 547,570 557,597 569,613 '
                     '550,653 479,687 430,690 412,688 394,660 348,668 345,649 282,646 222,661 203,697 '
                     '69,687 22,641 1,550 1,465 27,409 87,370 140,337 142,306')
    def hair_pigment(rgb):
        red, green, blue = np.moveaxis(rgb, -1, 0)
        return (blue - red > 49) & (green - red > 13) & (blue - green > 25) & (blue > 94)

    rows, cols = np.indices(mane.shape)
    lace_seam = curve(cols, [(128, 243), (150, 225), (169, 199), (190, 170), (216, 141), (245, 116),
                             (275, 102), (313, 92), (350, 97), (380, 111), (410, 130), (433, 155),
                             (450, 183), (466, 210), (482, 230), (512, 247), (548, 253)])
    crown_seam = sdf(rows >= lace_seam, 2)
    mane &= crown_seam > -14
    hair_color = hair_pigment(front.rgb)
    ribbon_region = (rows >= 212) & (rows <= 274) & ((cols < 159) | (cols > 448))
    front_area = mane & hair_color & ~ribbon_region & front.area(
        '83,91 547,91 540,205 483,210 480,321 534,351 634,459 628,525 541,534 546,587 564,614 '
        '535,660 443,695 423,656 483,609 523,586 503,547 531,513 484,481 465,448 404,403 394,384 '
        '456,315 443,182 198,174 163,324 220,405 171,468 124,486 93,507 143,554 113,584 122,633 '
        '223,665 200,705 59,699 0,593 0,419 127,339')
    back_area = rear.cut('73,204 98,149 122,110 198,95 285,91 367,94 421,101 467,132 481,176 489,253 '
                         '488,308 507,352 543,380 576,435 577,492 541,531 510,520 486,493 437,489 '
                         '422,542 446,576 427,588 398,584 366,537 337,499 305,534 272,547 249,501 '
                         '217,495 194,554 171,595 121,620 61,624 0,536 1,423 61,357 91,320')
    side_area = side.cut('141,88 235,67 314,80 363,120 402,187 411,296 411,337 446,370 486,404 503,451 '
                         '487,498 437,526 401,480 381,488 372,558 401,581 387,610 337,615 299,581 299,546 '
                         '267,530 271,485 234,479 225,425 206,378 232,343 296,333 371,321 373,292 310,244 '
                         '273,204 244,151 222,105')
    side_area &= hair_pigment(side.rgb)
    side_area &= ~side.area('196,191 303,191 308,238 322,258 294,270 199,269')
    back_area &= hair_pigment(rear.rgb) & ~rear.area('0,206 146,206 150,280 0,280')
    back_area &= ~rear.area('443,210 613,210 613,277 438,277')

    def end_height(x):
        return curve(288 - (x - 314) * .92, [(0, 511), (49, 599), (93, 622), (132, 596), (178, 566),
                     (209, 525), (240, 512), (275, 545), (298, 548), (329, 521), (354, 517), (384, 551),
                     (414, 567), (447, 591), (482, 577), (521, 565), (566, 517)])

    rear_ends = end_height(cols)
    overhang = mane & (rows > rear_ends - 22) & ((cols < 224) | (cols > 415))
    mane &= rows <= rear_ends
    front_paint = front.palette(front_area, soften=True)
    back_paint = rear.palette(back_area, soften=True)
    side_paint = side.palette(side_area, soften=True)
    side_confidence = ndimage.gaussian_filter(side_area.astype(np.float32), 11)
    back_confidence = ndimage.gaussian_filter(back_area.astype(np.float32), 4)
    front_confidence = ndimage.gaussian_filter(front_area.astype(np.float32), 4)
    left, right = width_profile(mane)

    def mane_sculpt(x, y, distance):
        center = 315 + 5 * np.sin(y / 108)
        span = np.where(x < center, center - np.interp(y, np.arange(HEIGHT), left),
                        np.interp(y, np.arange(HEIGHT), right) - center)
        transverse = np.clip((x - center) / np.maximum(12, span), -1, 1)
        silhouette_volume = fullness(distance, 33)
        section_volume = (1 - transverse**2)**.57
        transition = np.maximum(0, .14 - np.abs(silhouette_volume - section_volume))
        volume = np.maximum(0, np.minimum(silhouette_volume, section_volume) - transition**2 / .56)
        near = curve(y, [(88, 187), (106, 164), (160, 119), (224, 99), (290, 130), (350, 129), (386, 168),
                         (433, 227), (485, 235), (518, 246), (556, 226), (606, 213), (652, 207), (703, 205)])
        far = curve(y, [(88, 257), (109, 307), (155, 349), (228, 392), (313, 390), (353, 415), (397, 456),
                        (443, 488), (494, 462), (541, 445), (586, 415), (626, 350), (672, 266), (705, 219)])
        axis = near + (far - near) * (.28 + .24 * smooth(322, 465, y))
        crown_falloff = smooth(.36, .92, np.abs(transverse)) * (1 - smooth(216, 324, y))
        crown_rim = curve(y, [(88, 248), (116, 263), (152, 281), (192, 297), (228, 303)])
        axis += np.maximum(0, crown_rim - axis) * crown_falloff
        seam_distance = sample(crown_seam, x, y)
        lace_contact = (1 - smooth(-10, 16, seam_distance)) * (1 - smooth(221, 257, y))
        axis += (headdress_axis(x, y) - axis) * lace_contact
        volume *= 1 - .91 * lace_contact
        section = np.sin((x + (25 + .16 * np.abs(x - 313)) * np.sin(y / 49)) / 22)
        groove = (3 + 10 * smooth(280, 452, y)) * section * (1 - smooth(642, 687, y))
        gap = 100 * gaussian(x, y, 317, 533, 79, 45)
        front_depth = axis - np.maximum(6, axis - near + groove) * volume
        back_depth = axis + np.maximum(8, far - axis + groove - gap) * volume
        under_fringe = smooth(0, 13, seam_distance) * smooth(0, 9, distance) * (1 - smooth(238, 273, y))
        bow_clearance = smooth(118, 143, np.abs(x - 314)) * smooth(205, 220, y)
        under_fringe *= 1 - bow_clearance
        fringe_join = curve(y, [(106, 143), (146, 116), (187, 104), (230, 102), (264, 115)])
        return front_depth - under_fringe * np.maximum(0, front_depth - fringe_join), back_depth

    def mane_paint(x, y, distance, depth, is_rear):
        rear_x = 288 - (x - 314) * .92
        projected = color(back_paint, rear_x, y) if is_rear else color(front_paint, x, y)
        confidence = sample(back_confidence, rear_x, y) if is_rear else sample(front_confidence, x, y)
        profile = color(side_paint, depth, y)
        front_facing, side_facing = projection_facing(depth)
        spread = smooth(190, 268, depth) * smooth(.34, .87, side_facing) * sample(side_confidence, depth, y)
        pleat = .5 + .5 * np.sin((x + (25 + .16 * np.abs(x - 313)) * np.sin(y / 49)) / 22)
        roots = mix(np.array([35, 54, 113]), np.array([63, 90, 163]), pleat)
        ends = mix(np.array([48, 77, 151]), np.array([112, 163, 226]), pleat)
        body = mix(roots, ends, smooth(332, 548, y))
        visible = smooth(.025, .57, confidence) * smooth(1, 13, distance) * smooth(.15, .55, front_facing)
        projection = mix(body, projected, visible)
        return mix(projection, profile, spread * .83 * smooth(0, 12, distance))

    meshes = [relief('Swept ocean-blue mane', mane, mane_sculpt, mane_paint, step=5.5, smoothing=1.35)]
    tips = overhang & ndimage.binary_dilation(hair_color, iterations=3)
    islands, count = ndimage.label(tips)
    roots = np.bincount(islands[mane & tips], minlength=count + 1)
    tips &= roots[islands] >= 22

    def circulating_ends(x, y):
        near, far = mane_sculpt(x, y, np.full_like(x, 32))
        root = .77 * near + .23 * far
        tip = 251 + .16 * (np.abs(x - 313) - 147) + 22 * np.sin(y / 56)
        return root + (tip - root) * smooth(-6, 65, y - end_height(x))

    for label, half in [('Left', cols < 224), ('Right', cols > 415)]:
        lock = tips & half
        meshes.append(plate(f'{label} long curled ends', front.rgb, lock,
                            circulating_ends,
                            26, 23, back=lambda x, y, p, rgb: rgb * np.array([.84, .89, .97]), step=4.5))

    visible_skin = front.cut('226,262 245,211 262,266 286,299 307,313 332,300 348,257 348,202 360,234 379,253 '
                             '401,278 421,309 412,343 391,356 405,362 394,374 369,389 329,398 280,396 '
                             '242,388 223,375 218,362 231,356 216,343 204,320 208,300')
    face = front.area('209,178 243,136 302,121 369,140 410,179 433,233 439,292 431,337 420,357 402,381 '
                      '369,393 336,399 290,400 253,394 230,383 210,367 194,333 190,279 192,221')
    skin_color = front.rgb[..., 0] > front.rgb[..., 2] + 1
    visible_skin &= skin_color | ((rows >= 271) & (rows <= 348) & (cols >= 206) & (cols <= 416))
    skin = front.palette(visible_skin)
    skin_validity = ndimage.gaussian_filter(visible_skin.astype(np.float32), 1.2)

    def face_axis(x, y):
        return curve(y, [(120, 157), (213, 152), (277, 145), (323, 143), (359, 137), (383, 135), (399, 118)])

    def face_sculpt(x, y, distance):
        axis = face_axis(x, y)
        span = np.sqrt(np.clip(1 - (np.abs(x - 313) / 132)**2.2, 0, 1))
        roundness = np.minimum(fullness(distance, 40), span)
        profile = curve(y, [(120, 123), (205, 56), (260, 56), (302, 65), (328, 58), (352, 56),
                            (369, 64), (385, 84), (399, 111)])
        nose = 10 * gaussian(x, y, 313, 337, 15, 14)
        eyes = 4 * (gaussian(x, y, 250, 320, 24, 21) + gaussian(x, y, 369, 311, 25, 23))
        cheeks = 5 * (gaussian(x, y, 241, 349, 36, 23) + gaussian(x, y, 388, 343, 35, 23))
        mouth = 4 * gaussian(x, y, 313, 364, 15, 11)
        turn = 41 * smooth(15, 123, np.abs(x - 313)) * gaussian(x, y, 313, 316, 220, 57)
        return axis + (profile - axis - nose - eyes - cheeks + mouth + turn) * roundness, axis + 103 * roundness

    def face_paint(x, y, distance, depth, is_rear):
        blush = gaussian(x, y, 219, 352, 30, 25) + gaussian(x, y, 411, 348, 28, 27)
        base = mix(np.array([253, 229, 220]), np.array([249, 178, 169]), .48 * blush)
        shaded = mix(base, np.array([238, 184, 176]), .42 * smooth(102, 202, depth) + .16 * smooth(375, 400, y))
        return shaded if is_rear else mix(shaded, color(skin, x, y), sample(skin_validity, x, y))

    meshes.append(relief('Face and cheek volume', face, face_sculpt, face_paint, step=5))
    fringe = [
        ('Left swept fringe', '140,309 141,277 155,217 174,176 201,137 239,108 271,95 302,97 311,117 '
         '295,155 271,196 245,214 235,249 219,275 207,298 210,331 220,345 236,350 226,357 239,366 '
         '224,371 202,370 183,361 166,345 157,323 144,341 128,342 120,338 135,329'),
        ('Right swept fringe', '315,97 346,95 384,112 419,143 449,187 466,216 462,257 474,296 468,327 455,350 '
         '438,365 419,370 395,371 380,367 393,356 403,347 414,326 410,307 397,283 379,260 365,239 '
         '345,206 329,172 307,113'),
        ('Center bangs', '247,209 254,166 268,134 284,114 304,107 321,111 335,128 347,169 348,205 345,228 '
         '340,254 332,276 320,295 338,302 326,307 306,305 289,313 293,306 277,301 262,284 251,264 245,235'),
    ]

    def fringe_axis(x, y):
        front_curve = curve(y, [(95, 160), (107, 130), (139, 94), (194, 58), (244, 44), (295, 49), (333, 76), (371, 106)])
        roll = 84 * np.clip(np.abs(x - 314) / 174, 0, 1)**2
        temple = 81 * smooth(65, 117, np.abs(x - 314)) * gaussian(x, y, 314, 308, 270, 58)
        return front_curve + roll + temple - 14 * gaussian(x, y, 301, 257, 48, 71)

    for name, outline in fringe:
        cut = front.cut(outline)
        bang_color = (front.rgb[..., 2] > front.rgb[..., 0] + 22) & (front.rgb[..., 2] > front.rgb[..., 1] + 17)
        mask = cut & ndimage.binary_dilation(bang_color & ~ribbon_region, iterations=1)
        mask = ndimage.binary_fill_holes(ndimage.binary_closing(mask, iterations=2)) & cut
        meshes.append(plate(name, front.rgb, mask, fringe_axis, 32, 29, step=4.5,
                            back=lambda x, y, p, rgb: mix(rgb * .91, np.array([49, 71, 134]), .35)))

    locks = [
        ('Left cheek ringlet', '165,340 193,350 217,367 229,389 238,412 227,427 223,438 225,450 235,461 '
         '225,466 210,466 199,458 194,445 198,434 207,419 209,407 201,395 187,386 176,370'),
        ('Right cheek ringlet', '453,345 464,350 453,373 433,394 423,409 429,422 432,439 423,453 416,465 '
         '403,472 393,467 400,453 398,440 390,427 393,412 403,395 419,377 429,356'),
    ]
    for name, outline in locks:
        mask = front.cut(outline) & ndimage.binary_dilation(hair_color, iterations=2)
        meshes.append(plate(name, front.rgb, mask,
                            lambda x, y: curve(y, [(340, 153), (373, 115), (399, 110), (427, 106), (473, 107)])
                            + .095 * np.abs(x - 315), 23, 16, step=3))
    return meshes


def headwear(front, side, rear):
    fabric = headdress_mask(front)
    back_fabric = rear.cut('81,220 94,160 128,105 175,66 220,52 373,53 424,72 467,117 493,180 '
                           '496,227 472,222 455,166 425,123 385,100 330,88 248,93 184,116 145,154 119,222')
    side_fabric = side.cut('125,102 127,66 145,54 172,64 182,61 193,70 209,59 224,62 237,76 235,88 '
                           '250,99 252,119 249,130 260,142 266,155 272,181 268,193 270,207 255,216 '
                           '231,218 226,196 217,167 197,139 172,113 155,101')

    def lace_palette(reference, mask):
        red, green, blue = np.moveaxis(reference.rgb, -1, 0)
        cloth = mask & (red > 149) & (green > 139) & ((red > 213) | (blue - green > 8))
        return reference.palette(cloth, soften=True), ndimage.gaussian_filter(cloth.astype(np.float32), 3)

    front_band, front_confidence = lace_palette(front, fabric)
    back_band, back_confidence = lace_palette(rear, back_fabric)
    side_band, side_confidence = lace_palette(side, side_fabric)

    def band_sculpt(x, y, distance):
        axis = headdress_axis(x, y)
        folds = 1.4 * np.sin(x / 7.5 + y / 12)
        width = fullness(distance, 13)
        front_span = curve(y, [(54, 29), (89, 34), (135, 30), (173, 23), (217, 15)])
        back_span = curve(y, [(54, 28), (89, 33), (135, 27), (173, 20), (217, 15)])
        return axis - (front_span + folds) * width, axis + (back_span + folds) * width

    def band_paint(x, y, distance, depth, is_rear):
        if is_rear:
            chart_x = 288 - (x - 314) * .99
            palette, confidence = back_band, back_confidence
        else:
            chart_x = x
            palette, confidence = front_band, front_confidence
        front_facing, side_facing = projection_facing(depth)
        rim = 1 - smooth(0, 10, distance)
        fold = .5 + .5 * np.cos(x / 7.5 + y / 12 + depth / 19)
        fabric_color = mix(np.array([251, 247, 252]), np.array([203, 198, 226]), (.22 + .4 * rim) * fold)
        profile_coverage = .75 * sample(side_confidence, depth, y) * smooth(.18, .8, side_facing) * (1 - rim)
        profile = mix(fabric_color, color(side_band, depth, y), profile_coverage)
        coverage = (1 - rim) * smooth(.22, .62, front_facing) * smooth(.08, .73, sample(confidence, chart_x, y))
        return mix(profile, color(palette, chart_x, y), coverage)

    meshes = [relief('Pleated lace headdress', fabric, band_sculpt, band_paint, step=3.5, smoothing=.85)]
    crest = front.cut('206,3 346,3 346,52 339,66 328,89 317,89 318,65 306,54 265,54 222,78 206,75')
    meshes.append(plate('Crescent cowlick', front.rgb, crest,
                        lambda x, y: 70 + .95 * (x - 222) + .38 * (y - 18), 15, 12, step=3, smoothing=.7))

    fins = [
        ('Left flipper', '49,292 72,278 101,260 128,238 148,226 153,254 138,278 137,307 129,324 109,332 '
         '83,331 59,318 46,308', 141, -1),
        ('Right flipper', '467,234 501,238 522,250 550,275 582,294 597,302 598,316 571,332 548,335 514,336 '
         '491,330 474,322 462,305 467,275', 479, 1),
    ]
    for name, outline, root_x, direction in fins:
        mask = front.cut(outline)
        meshes.append(plate(name, front.rgb, mask,
                            lambda x, y, root=root_x, sign=direction: 235 + 1.13 * (x - root) * sign + .20 * (y - 255),
                            lambda x, y, root=root_x: 13 + 13 * gaussian(x, y, root, 268, 35, 39), 18, step=3))
    for name, mask in fin_bow_masks(front):
        meshes.append(plate(name, front.rgb, mask, lambda x, y: fin_bow_axis(x), 26, 15,
                            step=2.5, smoothing=.65))
    return meshes


def dress_section(angle, level, ruffle=False):
    facing = np.cos(angle)
    positive = np.maximum(0, facing)
    negative = np.maximum(0, -facing)
    flow = angle * 9 + .6 * level
    edge = 603 + 54 * positive**.88 + 46 * negative**.91
    radius = 65 + 132 * level**.92 + 6 * level * np.cos(flow)
    near = 55 + 87 * level**.96
    far = 61 + 94 * level**1.19
    gathers = level**2.5 * (2.1 * np.cos(flow) + 1.5 * np.sin(2 * flow))
    if ruffle:
        hem = level**2.7
        edge += 22 + 4.9 * np.cos(angle * 24 + .24)
        radius += hem * (19 + 3.9 * np.cos(angle * 24 + .24))
        near += hem * (6 + 3.3 * np.cos(angle * 24))
        far += hem * (12 + 3.3 * np.cos(angle * 24))
    x = 314 + radius * np.sin(angle)
    y = 486 + (edge - 486) * level + gathers
    depth = 170 - np.where(facing > 0, near, far) * facing - 4.8 * level * np.cos(flow) * facing
    return x, y, depth


def dress_volume(angle, level, ruffle=False):
    x, y, depth = dress_section(angle, np.minimum(1, level / .91), ruffle)
    tuck = smooth(.91, 1, level)
    return 314 + (x - 314) * (1 - .89 * tuck), y - (9 + .32 * (y - 605)) * tuck, 170 + (depth - 170) * (1 - .89 * tuck)


def skirt_and_apron(front):
    navy_area = front.cut('109,570 141,543 173,507 211,487 239,489 234,531 196,566 214,609 249,635 327,638 '
                          '378,626 410,588 407,543 389,516 391,489 428,503 459,542 485,562 513,587 482,609 '
                          '444,629 403,648 347,662 285,663 225,650 180,635 143,613 115,596')
    navy_fit = (front.rgb[..., 2] > front.rgb[..., 0] + 4) & (front.rgb.max(axis=2) < 140)
    navy_colors = front.palette(navy_area & ndimage.binary_dilation(navy_fit, iterations=3), soften=True)
    navy_projection = ndimage.gaussian_filter(navy_area.astype(np.float32), 4)

    def skirt_paint(angle, level, x, y, depth):
        level = np.minimum(1, level / .91)
        folds = .5 + .5 * np.cos(9 * angle + .6 * level)
        base = mix(np.array([29, 34, 66]), np.array([63, 72, 124]), .21 + .51 * folds)
        base = mix(base, np.array([22, 26, 54]), .46 * smooth(.93, 1, level))
        thread = 1 - smooth(.004, .011, np.abs(level - .923))
        spacing = (angle * 25 / (2 * np.pi) + .5) % 1 - .5
        stitch_y = (level - .86) * 2.8
        stitches = np.maximum(1 - smooth(.011, .031, np.abs(spacing + stitch_y)),
                              1 - smooth(.011, .031, np.abs(spacing - stitch_y)))
        stitches *= (1 - smooth(.091, .137, np.abs(stitch_y))) * (1 - smooth(.11, .17, np.abs(spacing)))
        painted = mix(base, np.array([182, 136, 91]), np.maximum(thread * .83, stitches * .87))
        source = color(navy_colors, x, y)
        alpha = .96 * smooth(.19, .78, np.cos(angle)) * sample(navy_projection, x, y)
        return mix(painted, source, alpha)

    def ruffle_paint(angle, level, x, y, depth):
        fold = .5 + .5 * np.cos(24 * angle + 1.1 * level)
        wave = np.sin(9 * angle - .82 * level)
        lilac = .15 + .72 * fold**2 + .08 * wave
        fabric = mix(np.array([253, 250, 253]), np.array([182, 177, 210]), lilac)
        return mix(fabric, np.array([125, 122, 161]), smooth(.92, 1, level))

    meshes = [loft('Pleated navy skirt', dress_volume, skirt_paint, 26, 108, (912, 200)),
              loft('Scalloped petticoat', lambda angle, level: dress_volume(angle, level, True),
                   ruffle_paint, 26, 120, (752, 150))]
    apron = front.cut('218,500 246,495 271,499 309,505 350,503 374,492 394,496 399,512 405,525 399,538 '
                      '408,555 407,569 398,582 400,595 391,610 382,619 365,628 348,634 326,638 307,639 '
                      '289,635 269,632 245,621 226,613 210,601 200,586 193,567 193,549 198,534 196,520 208,517')
    cream = (front.rgb.min(axis=2) > 160) & apron
    trim = ndimage.binary_fill_holes(ndimage.binary_dilation(cream, iterations=2)) & apron

    angles, levels = np.meshgrid(np.linspace(-np.pi / 2, np.pi / 2, 73), np.linspace(0, 1, 39))
    surface_x, surface_y, surface_depth = dress_section(angles, levels)
    points = np.column_stack((surface_x.ravel(), surface_y.ravel()))
    linear = LinearNDInterpolator(points, surface_depth.ravel())
    nearest = NearestNDInterpolator(points, surface_depth.ravel())

    def skirt_surface(x, y):
        depth = linear(x, y)
        return np.where(np.isnan(depth), nearest(x, y), depth)

    meshes.append(plate('Lace apron with whale embroidery', front.rgb, trim, lambda x, y: skirt_surface(x, y) - 7,
                        lambda x, y: 7 + 2 * np.sin((x - y) / 7), 12, step=4, smoothing=.85))
    for name, outline in [
        ('Left gold skirt bow', '146,593 151,579 160,576 173,588 185,583 194,589 189,604 178,605 183,619 '
         '173,623 167,607 160,615 154,609 158,599 148,602'),
        ('Right gold skirt bow', '401,590 419,594 431,584 440,584 445,597 437,603 427,603 435,617 425,624 '
         '419,609 414,621 405,616 413,602 400,601'),
    ]:
        mask = front.cut(outline)
        gold = (front.rgb[..., 0] > front.rgb[..., 2] + 20) & (front.rgb[..., 0] > 91)
        mask &= ndimage.binary_fill_holes(ndimage.binary_dilation(gold & mask, iterations=2))
        meshes.append(plate(name, front.rgb, mask, lambda x, y: skirt_surface(x, y) - 4, 5, 6, step=2.5))
    return meshes


def bodice_and_limbs(front, side, rear):
    top = front.cut('214,460 223,428 243,416 253,398 285,395 319,402 350,395 378,390 389,400 400,431 '
                    '411,458 405,483 384,502 348,510 272,510 243,504 229,480')

    def top_sculpt(x, y, distance):
        center = 174 + .08 * (y - 431)
        radius = fullness(distance, 33)
        fold = 2.8 * np.cos(x / 7.5) * gaussian(x, y, 316, 453, 59, 49)
        return center - (71 + fold) * radius, center + 69 * radius

    bib = front.palette(top)
    meshes = [relief('Gathered bodice and waist cincher', top, top_sculpt,
                     lambda x, y, d, p, back: color(bib, x, y) if not back else mix(
                         color(bib, x, y), np.array([41, 47, 81]), smooth(1, 26, d)), step=4.75)]
    chest_bow = front.cut('268,398 284,397 311,407 329,400 346,396 356,401 358,411 352,427 358,447 '
                         '337,457 330,446 319,432 311,428 301,442 297,455 271,446 278,428 268,422 262,407')
    meshes.append(plate('Sapphire brooch and neck bow', front.rgb, chest_bow, lambda x, y: 97 + .0034 * (x - 314)**2,
                        10, 12, step=3, smoothing=.75))
    arms = [
        ('Left', '126,496 143,489 151,480 169,466 187,459 200,433 221,426 239,435 245,456 236,478 '
         '219,493 198,507 182,523 162,541 142,548 127,540 117,527',
         '92,510 101,511 118,511 128,503 140,507 148,520 147,536 136,540 125,546 115,547 '
         '106,543 98,537 88,535 88,527 85,521 86,513'),
        ('Right', '378,449 386,432 405,426 429,429 445,443 447,461 469,466 481,480 485,492 506,498 510,520 '
         '500,539 488,554 469,548 449,534 430,513 415,504 398,487 388,472',
         '504,497 512,503 513,513 530,512 536,519 533,528 532,535 519,540 514,541 515,548 504,553 '
         '490,552 480,549 475,539 475,525 488,513'),
    ]
    for label, sleeve_outline, hand_outline in arms:
        sleeve = front.cut(sleeve_outline)
        shoulder_x = 215 if label == 'Left' else 405

        def sleeve_axis(x, y, shoulder=shoulder_x):
            return 182 - .59 * np.abs(x - shoulder) - .07 * (y - 453)

        meshes.append(plate(f'{label} puff sleeve with lace cuff', front.rgb, sleeve, sleeve_axis,
                            lambda x, y, shoulder=shoulder_x: 32 - .10 * np.abs(x - shoulder), 15, step=3.5))
        skin = (front.rgb[..., 0] > front.rgb[..., 2] + 19) & (front.rgb[..., 0] > 119)
        hand = front.cut(hand_outline)
        hand &= ndimage.binary_fill_holes(ndimage.binary_dilation(skin & hand, iterations=1))
        meshes.append(plate(f'{label} hand', front.rgb, hand, lambda x, y: sleeve_axis(x, y) - 6 - .14 * (y - 526),
                            12, 9, step=2, smoothing=.65))

    rear_bow = rear.cut('202,515 227,507 256,514 273,521 288,516 303,508 325,510 348,500 363,504 371,518 '
                        '367,535 355,548 366,564 381,583 365,589 340,593 324,569 313,556 301,557 289,591 '
                        '280,622 280,640 258,642 227,631 205,625 216,596 228,572 214,556 204,544')
    white = (rear.rgb.min(axis=2) > 151) & rear_bow
    bow_mask = ndimage.binary_fill_holes(ndimage.binary_dilation(white, iterations=2)) & rear_bow
    shift = 314 - (rear.rgb.shape[1] - 1 - 288)
    back_rgb = np.roll(np.fliplr(rear.rgb), shift, axis=1)
    back_bow = np.roll(np.fliplr(bow_mask), shift, axis=1)
    meshes.append(plate('Tied satin sash', back_rgb, back_bow,
                        lambda x, y: curve(y, [(497, 272), (516, 336), (551, 340), (606, 329), (643, 303)]),
                        20, 15, back=lambda x, y, p, rgb: rgb, step=4))
    return meshes


def legs_and_shoes(front, side, rear):

    shoes = [
        ('Left', '217,667 242,670 280,664 285,687 286,713 293,739 289,756 279,770 237,773 220,767 212,742 210,719', 251),
        ('Right', '348,669 361,662 397,658 414,668 419,686 422,714 431,718 429,743 417,763 403,774 355,774 '
         '343,759 344,739 353,717 352,692', 386),
    ]
    back_boots = rear.palette(rear.cut('195,678 369,678 379,779 190,779'), soften=True)
    side_boot = side.palette(side.cut('142,660 217,654 221,694 229,729 223,770 109,770 118,737 146,722'), soften=True)
    meshes = []
    for name, outline, foot_x in shoes:
        mask = front.cut(outline)
        mask &= ~front.area('200,664 447,664 447,687 413,674 382,682 354,688 278,693 211,688')
        mask |= front.cut('223,683 273,683 281,703 218,706') if name == 'Left' else front.cut('355,679 410,673 417,702 356,704')
        boot = front.palette(mask & (ndimage.distance_transform_edt(mask) > 2.4), soften=True)

        def shoe_shape(angle, level):
            y = 618 + 150.4 * level
            width = curve(y, [(618, 25), (658, 28), (688, 28), (704, 29), (710, 32), (719, 28),
                              (731, 31), (745, 35), (758, 34), (765, 30), (769, 23)])
            center = 180 + curve(y, [(618, 7), (677, 6), (716, 1), (742, -7), (762, -8), (770, -5)])
            front_radius = curve(y, [(618, 24), (684, 26), (705, 27), (718, 29), (741, 50), (758, 51), (769, 44)])
            back_radius = curve(y, [(618, 22), (688, 23), (718, 31), (747, 41), (761, 39), (769, 36)])
            facing = np.cos(angle)
            lace = gaussian(y, level, 705, .58, 12, 1) * (1.8 * np.cos(angle * 11))
            return (foot_x + (width + lace) * np.sin(angle), y + lace * .63,
                    center - (np.where(facing > 0, front_radius, back_radius) + lace) * facing)

        def shoe_paint(angle, level, x, y, depth):
            copied = color(boot, x, y)
            rear_coords = (238 if name == 'Right' else 335) - (x - foot_x) * .86
            heel = color(back_boots, rear_coords, y)
            profile = color(side_boot, depth, y)
            side_weight = smooth(.49, .96, np.abs(np.sin(angle)))
            shell = mix(mix(heel, copied, smooth(-.29, .29, np.cos(angle))), profile, side_weight)
            skin = mix(np.array([255, 227, 217]), np.array([224, 161, 161]), .37 * (1 - np.cos(angle)))
            return mix(skin, shell, smooth(677, 692, y))

        meshes.append(loft(f'{name} leg, lace sock and Mary Jane', shoe_shape, shoe_paint, 34, 52, (323, 196)))
    return meshes


def whale_tail(front):
    traced = front.cut('407,664 433,646 461,633 498,605 507,612 514,633 534,641 553,636 572,624 '
                       '582,610 566,600 553,587 548,574 547,550 550,532 555,520 565,515 574,522 '
                       '581,540 591,546 602,559 610,543 621,532 639,523 650,516 660,503 669,503 '
                       '681,520 683,554 666,593 631,614 618,631 611,675 593,705 556,724 489,731 421,723 395,706')
    blue = (front.rgb[..., 2] > front.rgb[..., 0] + 16) & (front.rgb[..., 1] < 169)
    visible = traced & ndimage.binary_fill_holes(ndimage.binary_dilation(blue & traced, iterations=2))
    root = front.area('338,580 366,571 405,592 429,614 455,637 490,643 475,701 440,717 411,703 382,678 355,646')
    mask = visible | root

    def axis(x, y):
        return curve(x, [(335, 251), (385, 273), (459, 325), (527, 363), (596, 422), (672, 499)]) + .13 * (636 - y)

    def tail_back(x, y, depth, original):
        belly = 682 + 33 * np.cos(np.clip((x - 500) / 128, -1.6, 1.6)) - 72 * smooth(546, 623, x)
        light = gaussian(x, y, 524, 658, 95, 24) + .59 * gaussian(x, y, 633, 551, 29, 30)
        base = mix(np.array([41, 61, 118]), np.array([67, 101, 178]), .16 + .58 * light)
        return mix(base, np.array([150, 166, 221]), smooth(belly - 40, belly - 30, y) * (1 - smooth(588, 627, x)))

    return plate('Curved whale tail and paired flukes', front.rgb, mask, axis,
                 lambda x, y: 16 + 34 * gaussian(x, y, 444, 675, 152, 72), 23, back=tail_back,
                 paint_mask=visible, step=4, smoothing=1.1)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('reference', type=Path)
    parser.add_argument('output', type=Path)
    arguments = parser.parse_args()
    with Image.open(arguments.reference) as original:
        sheet = original.convert('RGB').resize((1824, HEIGHT), Image.Resampling.LANCZOS)
    front = Reference(sheet, 0, 687, [(274, 49), (109, 426), (557, 407), (606, 507), (559, 614)])
    side = Reference(sheet, 689, 1205, [(123, 51), (413, 365), (468, 447), (390, 582)])
    rear = Reference(sheet, 1208, 1824, [(351, 51), (75, 409), (496, 410), (491, 337)])
    meshes = head_and_hair(front, side, rear) + headwear(front, side, rear) + skirt_and_apron(front)
    meshes += bodice_and_limbs(front, side, rear) + legs_and_shoes(front, side, rear) + [whale_tail(front)]
    write_glb(arguments.output, meshes)
    print(f'{arguments.output}: {len(meshes)} meshes; {sum(len(mesh.faces) for mesh in meshes):,} triangles; '
          f'{arguments.output.stat().st_size:,} bytes')


if __name__ == '__main__':
    main()
