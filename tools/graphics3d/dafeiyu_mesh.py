"""Mesh and texture baking primitives for the Dafeiyu gallery asset."""

import io
import json
import struct
from dataclasses import dataclass

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage
from scipy.interpolate import PchipInterpolator


HEIGHT = 785
GROUND = 768.5
CENTER = 312
DEPTH_ORIGIN = 225
SCALE = 190
GUTTER = 8


def smooth(start, end, values):
    fraction = np.clip((values - start) / (end - start), 0, 1)
    return fraction**2 * (3 - 2 * fraction)


def curve(values, keys):
    positions, levels = np.asarray(keys).T
    return PchipInterpolator(positions, levels)(np.clip(values, positions[0], positions[-1]))


def gaussian(x, y, center_x, center_y, radius_x, radius_y):
    return np.exp(-((x - center_x) / radius_x)**2 - ((y - center_y) / radius_y)**2)


def polygon(shape, coordinates):
    points = [tuple(map(float, pair.split(','))) for pair in coordinates.split()]
    mask = Image.new('1', (shape[1], shape[0]))
    ImageDraw.Draw(mask).polygon(points, fill=1)
    return np.asarray(mask)


def sdf(mask, smoothing):
    interior = ndimage.distance_transform_edt(mask)
    exterior = ndimage.distance_transform_edt(~mask)
    return ndimage.gaussian_filter(interior - exterior, smoothing)


def sample(image, x, y):
    return ndimage.map_coordinates(image, (y, x), order=1, mode='nearest', prefilter=False)


def color(image, x, y):
    return np.stack([sample(image[..., channel], x, y) for channel in range(3)], axis=-1)


def extend(image, mask):
    indices = ndimage.distance_transform_edt(~mask, return_distances=False, return_indices=True)
    return image[tuple(indices)]


def mix(base, other, amount):
    return base + (other - base) * np.asarray(amount)[..., None]


def bitmap(rgb):
    return Image.fromarray(np.rint(np.clip(rgb, 0, 255)).astype(np.uint8))


@dataclass
class Mesh:
    name: str
    positions: np.ndarray
    faces: np.ndarray
    texels: np.ndarray
    patches: list[Image.Image]
    seams: tuple[np.ndarray, np.ndarray]

    def normals(self):
        triangles = self.positions[self.faces]
        normals = np.zeros_like(self.positions)
        cross = np.cross(triangles[:, 1] - triangles[:, 0], triangles[:, 2] - triangles[:, 0])
        for corner in range(3):
            np.add.at(normals, self.faces[:, corner], cross)
        left, right = self.seams
        normals[left] += normals[right]
        normals[right] = normals[left]
        return normals / np.maximum(np.linalg.norm(normals, axis=1, keepdims=True), 1e-12)


def positions(x, y, depth):
    return np.stack((x - CENTER, GROUND - y, DEPTH_ORIGIN - depth), axis=-1).reshape(-1, 3) / SCALE


def grid_faces(grid):
    upper_left, upper_right, lower_left, lower_right = [corner.ravel() for corner in
        (grid[:-1, :-1], grid[:-1, 1:], grid[1:, :-1], grid[1:, 1:])]
    return np.concatenate((np.stack((upper_left, lower_left, upper_right), axis=1),
                           np.stack((lower_left, lower_right, upper_right), axis=1)))


def triangulate(field, bounds, step):
    left, top, right, bottom = bounds
    x_mesh, y_mesh = np.meshgrid(np.arange(left, right + step, step), np.arange(top, bottom + step, step))
    points = list(zip(x_mesh.ravel(), y_mesh.ravel()))
    signed = sample(field, x_mesh, y_mesh).ravel()
    faces = grid_faces(np.arange(x_mesh.size).reshape(x_mesh.shape))
    faces = faces[np.any(signed[faces] > 0, axis=1)]
    edges = {}
    indices = []

    def crossing(first, second):
        key = (min(first, second), max(first, second))
        if key not in edges:
            fraction = signed[first] / (signed[first] - signed[second])
            start_x, start_y = points[first]
            end_x, end_y = points[second]
            edges[key] = len(points)
            points.append((start_x + (end_x - start_x) * fraction, start_y + (end_y - start_y) * fraction))
        return edges[key]

    for triangle in faces:
        clipped = []
        for first, second in zip(triangle, np.roll(triangle, -1)):
            inside = signed[first] > 0
            if inside:
                clipped.append(first)
            if inside != (signed[second] > 0):
                clipped.append(crossing(first, second))
        for index in range(1, len(clipped) - 1):
            indices.append((clipped[0], clipped[index], clipped[index + 1]))

    used, remapped = np.unique(indices, return_inverse=True)
    xy = np.asarray(points)[used]
    rim = np.flatnonzero(np.isin(used, list(edges.values())))
    distance = np.maximum(0, sample(field, *xy.T))
    distance[rim] = 0
    return xy, distance, rim, remapped.reshape(-1, 3)


def relief(name, mask, sculpt, paint, step=4, smoothing=1.1):
    """Close two sculpted surfaces along one traced contour, with separate UV charts."""
    field = sdf(mask, smoothing)
    occupied_y, occupied_x = np.nonzero(field > 0)
    left, top = int(occupied_x.min()) - GUTTER, int(occupied_y.min()) - GUTTER
    right, bottom = int(occupied_x.max()) + GUTTER + 1, int(occupied_y.max()) + GUTTER + 1
    xy, distance, rim, faces = triangulate(field, (left, top, right, bottom), step)
    count = len(xy)
    near, far = sculpt(*xy.T, distance)
    xyz = positions(np.tile(xy[:, 0], 2), np.tile(xy[:, 1], 2), np.r_[near, far])
    x, y = np.meshgrid(np.arange(left, right), np.arange(top, bottom))
    distance = sample(field, x, y)
    near, far = sculpt(x, y, np.maximum(0, distance))
    patches = [bitmap(extend(paint(x, y, np.maximum(0, distance), depth, rear), distance > 0))
               for rear, depth in enumerate((near, far))]
    return Mesh(name, xyz, np.vstack((faces, faces[:, ::-1] + count)), np.tile(xy - (left, top), (2, 1)),
                patches, (rim, rim + count))


def loft(name, shape, paint, rings, sides, texture_size):
    """Build a capped, periodic section loft with a continuous wraparound UV chart."""
    angles, levels = np.meshgrid(np.linspace(0, 2 * np.pi, sides + 1), np.linspace(0, 1, rings + 1))
    xyz = positions(*shape(angles, levels))
    grid = np.arange(angles.size).reshape(angles.shape)
    caps = np.array([xyz[grid[0, :-1]].mean(axis=0), xyz[grid[-1, :-1]].mean(axis=0)])
    top = np.column_stack((np.full(sides, len(xyz)), grid[0, :-1], grid[0, 1:]))
    bottom = np.column_stack((np.full(sides, len(xyz) + 1), grid[-1, 1:], grid[-1, :-1]))
    width, height = texture_size
    coordinates = np.column_stack((angles.ravel() * (width - 1) / (2 * np.pi), levels.ravel() * (height - 1)))
    coordinates = np.vstack((coordinates, [(width / 2, 0), (width / 2, height - 1)])) + GUTTER
    texture_angle, texture_level = np.meshgrid(np.linspace(0, 2 * np.pi, width), np.linspace(0, 1, height))
    image = paint(texture_angle, texture_level, *shape(texture_angle, texture_level))
    patch = bitmap(np.pad(image, ((GUTTER, GUTTER), (GUTTER, GUTTER), (0, 0)), mode='edge'))
    return Mesh(name, np.concatenate((xyz, caps)), np.vstack((grid_faces(grid), top, bottom)), coordinates,
                [patch], (grid[:, 0], grid[:, -1]))


def pack_patches(meshes):
    width = 2048
    patches = [patch for mesh in meshes for patch in mesh.patches]
    positions = [None] * len(patches)
    shelves = []
    for index in sorted(range(len(patches)), key=lambda index: -patches[index].height):
        patch = patches[index]
        for shelf in shelves:
            if shelf[0] + patch.width <= width:
                break
        else:
            top = shelves[-1][1] + shelves[-1][2] if shelves else 0
            shelf = [0, top, patch.height]
            shelves.append(shelf)
        positions[index] = (shelf[0], shelf[1])
        shelf[0] += patch.width
    atlas = Image.new('RGB', (width, shelves[-1][1] + shelves[-1][2]), '#17223e')
    for patch, position in zip(patches, positions):
        atlas.paste(patch, position)
    return atlas, np.asarray(positions)


def write_glb(path, meshes):
    atlas, origins = pack_patches(meshes)
    buffer = bytearray()
    document = {'asset': {'version': '2.0', 'generator': 'ArcGallery Dafeiyu'},
                'scene': 0, 'scenes': [{'name': 'Dafeiyu', 'nodes': []}], 'nodes': [], 'meshes': [],
                'bufferViews': [], 'accessors': [], 'extensionsUsed': ['KHR_materials_unlit']}

    def store(data, target=None):
        view = {'buffer': 0, 'byteOffset': len(buffer), 'byteLength': len(data)}
        if target is not None:
            view['target'] = target
        document['bufferViews'].append(view)
        buffer.extend(data)
        buffer.extend(b'\0' * (-len(buffer) % 4))
        return len(document['bufferViews']) - 1

    def accessor(data, components, kind, target, limits=False):
        entry = {'bufferView': store(data.tobytes(), target), 'componentType': components, 'count': len(data), 'type': kind}
        if limits:
            entry |= {'min': data.min(axis=0).tolist(), 'max': data.max(axis=0).tolist()}
        document['accessors'].append(entry)
        return len(document['accessors']) - 1

    patch_index = 0
    for index, mesh in enumerate(meshes):
        count = len(mesh.patches)
        offsets = np.repeat(origins[patch_index:patch_index + count], len(mesh.positions) // count, axis=0)
        uv = (mesh.texels + offsets + .5) / atlas.size
        patch_index += count
        document['scenes'][0]['nodes'].append(index)
        document['nodes'].append({'name': mesh.name, 'mesh': index})
        attributes = {'POSITION': accessor(mesh.positions.astype('<f4'), 5126, 'VEC3', 34962, True),
                      'NORMAL': accessor(mesh.normals().astype('<f4'), 5126, 'VEC3', 34962),
                      'TEXCOORD_0': accessor(uv.astype('<f4'), 5126, 'VEC2', 34962)}
        index_format, index_type = ('<u2', 5123) if len(mesh.positions) <= 65536 else ('<u4', 5125)
        indices = accessor(mesh.faces.flatten().astype(index_format), index_type, 'SCALAR', 34963)
        document['meshes'].append({'name': mesh.name, 'primitives': [{'attributes': attributes, 'indices': indices, 'material': 0}]})

    stream = io.BytesIO()
    atlas.save(stream, format='JPEG', quality=94, subsampling=0, optimize=True)
    document['images'] = [{'bufferView': store(stream.getvalue()), 'mimeType': 'image/jpeg'}]
    document['samplers'] = [{'magFilter': 9729, 'minFilter': 9987, 'wrapS': 33071, 'wrapT': 33071}]
    document['textures'] = [{'source': 0, 'sampler': 0}]
    document['materials'] = [{'name': 'Painted character colors', 'extensions': {'KHR_materials_unlit': {}},
                              'pbrMetallicRoughness': {'baseColorTexture': {'index': 0}, 'metallicFactor': 0}}]
    document['buffers'] = [{'byteLength': len(buffer)}]
    header = json.dumps(document, separators=(',', ':')).encode('utf-8')
    header += b' ' * (-len(header) % 4)
    with path.open('wb') as output:
        output.write(struct.pack('<4sII', b'glTF', 2, 28 + len(header) + len(buffer)))
        output.write(struct.pack('<I4s', len(header), b'JSON'))
        output.write(header)
        output.write(struct.pack('<I4s', len(buffer), b'BIN\0'))
        output.write(buffer)
