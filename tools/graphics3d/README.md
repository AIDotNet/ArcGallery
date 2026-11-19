# Dafeiyu gallery model

`Dafeiyu.glb` is the self-contained character in the Model viewer gallery preview. It has separate meshes for the face, hair, headwear, dress, shoes and flukes. Its texture atlas is embedded, with painted lighting to preserve the illustrated colors. It also contains three glTF clips: Idle, Swim and Wave.

## Rebuild

Use the original character sheet supplied for this demo (the same crop and proportions, with the three full-body views in the top row). It is not checked in. The generator needs Python, NumPy, SciPy and Pillow. No Python or Blender install is needed to run the gallery.

```bash
python3 tools/graphics3d/build_dafeiyu.py /path/to/character-sheet.png \
  gallery/TKCode.Arc.Gallery/Assets/Graphics3D/Dafeiyu.glb
```

The workflow follows the blockout, separate outfit pieces, UV texturing and turntable checks shown in Blender Studio's [Stylized Character Workflow](https://studio.blender.org/training/stylized-character-workflow/). The generator uses the sheet's front silhouette for contours and its profile for section depths. It models important overlaps separately and bakes the visible painted details into UV charts, with wraparound charts for the dress and shoes.

The sheet draws the tail on the viewer's right in both the front and back. The mesh favors the front pose, so turning it around puts the tail on the viewer's left. `dafeiyu_motion.py` groups the separate meshes around their attachment points and writes looping translation and rotation tracks. Every clip starts and ends in the same rest pose.
