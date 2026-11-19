"""Animate the sculpted pieces of Dafeiyu around their attachment points."""

import numpy as np
from scipy.spatial.transform import Rotation

from dafeiyu_mesh import CENTER, DEPTH_ORIGIN, GROUND, positions, smooth


class Rig:
    def __init__(self, document):
        self.nodes = document['nodes']
        self.scene = document['scenes'][document['scene']]
        self.meshes = {self.nodes[index]['name']: index for index in self.scene['nodes']}
        self.pivots = {}
        self.root = self.group('Dafeiyu rig', (CENTER, GROUND, DEPTH_ORIGIN))
        self.scene['nodes'] = [self.root]

    def group(self, name, anchor, meshes=(), parent=None):
        # The generator puts vertices in model space. Offsetting each mesh by its group's pivot
        # leaves the rest pose intact while letting the group turn around its attachment point.
        pivot = positions(*anchor)[0]
        children = []
        for mesh in meshes:
            index = self.meshes.pop(mesh)
            self.nodes[index]['translation'] = (-pivot).tolist()
            children.append(index)

        index = len(self.nodes)
        origin = 0 if parent is None else self.pivots[parent]
        self.nodes.append({'name': name, 'translation': (pivot - origin).tolist(), 'children': children})
        self.pivots[index] = pivot
        if parent is not None:
            self.nodes[parent]['children'].append(index)
        return index

    def finish(self):
        self.nodes[self.root]['children'].extend(self.meshes.values())


def add_dafeiyu_motion(document, accessor):
    rig = Rig(document)
    head = rig.group('Head', (314, 395, 163), [
        'Face and cheek volume', 'Swept ocean-blue mane', 'Left swept fringe', 'Right swept fringe', 'Center bangs',
        'Left cheek ringlet', 'Right cheek ringlet', 'Pleated lace headdress', 'Crescent cowlick',
    ], parent=rig.root)
    left_hair = rig.group('Left curl', (100, 515, 264), ['Left long curled ends'], parent=head)
    right_hair = rig.group('Right curl', (498, 513, 253), ['Right long curled ends'], parent=head)
    left_fin = rig.group('Left fin', (145, 244, 229), ['Left flipper', 'Left fin bow'], parent=head)
    right_fin = rig.group('Right fin', (478, 246, 232), ['Right flipper', 'Right fin bow'], parent=head)
    left_arm = rig.group('Left shoulder', (218, 440, 179), ['Left puff sleeve with lace cuff', 'Left hand'], parent=rig.root)
    right_arm = rig.group('Right shoulder', (405, 441, 178), ['Right puff sleeve with lace cuff'], parent=rig.root)
    wrist = rig.group('Right wrist', (494, 536, 126), ['Right hand'], parent=right_arm)
    tail = rig.group('Tail', (356, 617, 262), ['Curved whale tail and paired flukes'], parent=rig.root)
    rig.finish()

    clips = document['animations'] = []

    def sample_time(duration):
        time = np.linspace(0, duration, round(duration * 24) + 1)
        return time, time * (2 * np.pi / duration)

    def clip(name, time, rotations, translation=None):
        animation = {'name': name, 'channels': [], 'samplers': []}
        clock = accessor(time.astype('<f4'), 5126, 'SCALAR', limits=True)

        def channel(node, path, values, kind):
            # Exact matching keys at the seam also preserve the rest pose when switching clips.
            values[-1] = values[0]
            output = accessor(values.astype('<f4'), 5126, kind)
            animation['channels'].append({'sampler': len(animation['samplers']), 'target': {'node': node, 'path': path}})
            animation['samplers'].append({'input': clock, 'output': output, 'interpolation': 'LINEAR'})

        for node, angles in rotations.items():
            degrees = np.stack([np.broadcast_to(angle, time.shape) for angle in angles], axis=1)
            channel(node, 'rotation', Rotation.from_euler('xyz', degrees, degrees=True).as_quat(), 'VEC4')
        if translation is not None:
            channel(rig.root, 'translation', np.stack(translation, axis=1), 'VEC3')
        clips.append(animation)

    time, phase = sample_time(5.2)
    sway = np.sin(phase)
    flutter = np.sin(phase * 2) * .36 + sway
    clip('Idle', time, {
        head: (1.3 * sway, 3 * sway, -2.5 * sway),
        left_hair: (2.3 * flutter, 0, 2 * sway), right_hair: (-2.3 * flutter, 0, 2 * sway),
        left_fin: (0, 4 * flutter, -6 * flutter), right_fin: (0, -4 * flutter, 7 * flutter),
        left_arm: (2 * sway, 0, -2 * sway), right_arm: (-2 * sway, 0, 2 * sway),
        tail: (3 * flutter, 7 * sway, 3.5 * flutter),
    })

    time, phase = sample_time(3.6)
    stroke = np.sin(phase * 2)
    glide = 1 - np.cos(phase)
    sway = np.sin(phase)
    clip('Swim', time, {
        rig.root: (-3 * glide, 4 * sway, -3 * sway),
        head: (1.5 * glide, -2 * sway, 2 * sway),
        left_hair: (4 * stroke, 3 * sway, 0), right_hair: (4 * stroke, -3 * sway, 0),
        left_fin: (0, 7 * stroke, -12 * stroke), right_fin: (0, -7 * stroke, 12 * stroke),
        left_arm: (5 * stroke, 0, -6 * glide), right_arm: (5 * stroke, 0, 6 * glide),
        tail: (7 * stroke, 16 * stroke, 9 * sway),
    }, translation=(.06 * sway, .16 * glide, .04 * sway))

    time, phase = sample_time(5.4)
    lift = smooth(0, 1.05, time) * (1 - smooth(3.45, 4.75, time))
    wave = np.sin((time - 1.05) * (2 * np.pi * 1.55)) * lift
    clip('Wave', time, {
        head: (-2.5 * lift, -3 * lift, -7 * lift),
        left_fin: (0, 0, 7 * lift), right_fin: (0, -6 * lift, 4 * wave),
        right_arm: (0, -8 * lift, 92 * lift + 5 * wave),
        wrist: (0, 9 * wave, 23 * wave),
        tail: (0, 6 * np.sin(phase), 7 * lift),
    })
