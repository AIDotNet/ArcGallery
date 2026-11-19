# Dafeiyu gallery model

`Dafeiyu.glb` is the self-contained character in the Model viewer gallery preview. It has separate meshes for the face, hair, headwear, dress, shoes and flukes. Its texture atlas is embedded, with painted lighting to preserve the illustrated colors.

## Rebuild

Use the original character sheet supplied for this demo (the same crop and proportions, with the three full-body views in the top row). It is not checked in. The generator needs Python, NumPy, SciPy and Pillow. No Python or Blender install is needed to run the gallery.

```bash
python3 tools/graphics3d/build_dafeiyu.py /path/to/character-sheet.png \
  gallery/TKCode.Arc.Gallery/Assets/Graphics3D/Dafeiyu.glb
```

The workflow follows the blockout, separate outfit pieces, UV texturing and turntable checks shown in Blender Studio's [Stylized Character Workflow](https://studio.blender.org/training/stylized-character-workflow/). The generator uses the sheet's front silhouette for contours and its profile for section depths. It models important overlaps separately and bakes the visible painted details into UV charts, with wraparound charts for the dress and shoes.

The sheet is not a geometrically consistent orthographic set: the tail is drawn on the viewer's right in both the front and back. The mesh favors the front pose and has one fixed tail, so turning it around puts the tail on the viewer's left. The model is a static reconstruction, not a rig or a pixel-identical rendering from all views.
