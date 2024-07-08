import pyvista as pv
import tetgen

# handType = 'LeftHand'
handType = 'RightHand'
savefile = True

if handType == "LeftHand":
    inputFilePath = "./LeftHandMesh.obj"
    outputFilePath = "TetLeftHandMesh.vtk"
elif handType == "RightHand":
    inputFilePath = "./RightHandMesh.obj"
    outputFilePath = "TetRightHandMesh.vtk"

trimesh = pv.read(inputFilePath)
# 1360, 2314
print(f"{len(trimesh.points)} vertices in the trimesh")
print(f"{trimesh.n_cells} triangles in the trimesh")

tet = tetgen.TetGen(trimesh)
# nodes, elems = tet.tetrahedralize(order=1, mindihedral=20, minratio=1.5)
nodes, elems = tet.tetrahedralize(switches="pq1.1a0.000000018")
# switches="pq1.414a0.0001Y"
# pq1.5/20Y
grid = tet.grid

# grid.ex

# left and right hand are not consistent
print(f"{len(nodes)} vertices in the tetmesh")
print(f"{len(elems)} tetrahedrons in the tetmesh")
if savefile: 
    tet.write(outputFilePath)
# grid.plot(show_edges=True)

cell_center = grid.cell_centers().points
mask = cell_center[:, 2] < 0.005
cell_ind = mask.nonzero()[0]
subgrid = grid.extract_cells(cell_ind)

# advanced plotting
plotter = pv.Plotter()

# show the triangular mesh
# pl.add_mesh(trimesh, show_edges=True)

# show the tetrahedron mesh
plotter.add_mesh(trimesh, "r", "wireframe")
plotter.add_mesh(subgrid, show_edges=True)

check_quality = False
if (not check_quality):
    plotter.show()
else:
    cell_qual = subgrid.compute_cell_quality()["CellQuality"]
    # plot quality
    subgrid.plot(
        scalars=cell_qual,
        scalar_bar_args={"title": "Quality"},
        cmap="bwr",
        clim=[0, 1],
        flip_scalars=True,
        show_edges=True,
    )

# print all the tetrahedron elements 
print(elems)
