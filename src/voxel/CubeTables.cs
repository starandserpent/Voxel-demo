using System.Numerics;
public class CubeTables
{
    // The following tables respect the following conventions
//
//    7-------6
//   /|      /|
//  / |     / |  Corners
// 4-------5  |
// |  3----|--2
// | /     | /     y z
// |/      |/      |/   OpenGL axis convention
// 0-------1    x--o
//
//
//     o---10----o
//    /|        /|
//  11 7       9 6   Edges
//  /  |      /  |
// o----8----o   |
// |   o---2-|---o
// 4  /      5  /
// | 3       | 1
// |/        |/
// o----0----o
//
// Sides are ordered according to the Voxel::Side enum.
// Edges are ordered according to the Voxel::Edge enum (only g_edge_inormals!).
//

    public static readonly int MOORE_NEIGHBORING_3D_COUNT = 26;


    // Index convention used in some lookup tables
    enum Side{
        SIDE_LEFT = 0,
	    SIDE_RIGHT,
	    SIDE_BOTTOM,
	    SIDE_TOP,
	    SIDE_BACK,
	    SIDE_FRONT,

	    SIDE_COUNT
    }

    enum Edge {
	    EDGE_BOTTOM_BACK = 0,
	    EDGE_BOTTOM_RIGHT,
    	EDGE_BOTTOM_FRONT,
    	EDGE_BOTTOM_LEFT,
    	EDGE_BACK_LEFT,
    	EDGE_BACK_RIGHT,
	    EDGE_FRONT_RIGHT,
    	EDGE_FRONT_LEFT,
    	EDGE_TOP_BACK,
	    EDGE_TOP_RIGHT,
	    EDGE_TOP_FRONT,
	    EDGE_TOP_LEFT,

	    EDGE_COUNT
    };

    // Index convention used in some lookup tables
    enum Corner {
	    CORNER_BOTTOM_BACK_LEFT = 0,
    	CORNER_BOTTOM_BACK_RIGHT,
    	CORNER_BOTTOM_FRONT_RIGHT,
    	CORNER_BOTTOM_FRONT_LEFT,
    	CORNER_TOP_BACK_LEFT,
    	CORNER_TOP_BACK_RIGHT,
    	CORNER_TOP_FRONT_RIGHT,
    	CORNER_TOP_FRONT_LEFT,

    	CORNER_COUNT
    };

    // Ordered as per the cube corners diagram
    public static Vector3i[] cornerPosition = {
        new Vector3i(1, 0, 0),
	    new Vector3i(0, 0, 0),
	    new Vector3i(0, 0, 1),
	    new Vector3i(1, 0, 1),
        new Vector3i(1, 1, 0),
        new Vector3i(0, 1, 0),
	    new Vector3i(0, 1, 1),
	    new Vector3i(1, 1, 1)
    };

 public static int[][] g_side_quad_triangles = {
	new int[6]{ 0, 2, 1, 0, 3, 2 }, // LEFT (+x)
	new int[6]{ 0, 2, 1, 0, 3, 2 }, // RIGHT (-x)
	new int[6]{ 0, 2, 1, 0, 3, 2 }, // BOTTOM (-y)
	new int[6]{ 0, 2, 1, 0, 3, 2 }, // TOP (+y)
	new int[6]{ 0, 2, 1, 0, 3, 2 }, // BACK (-z)
	new int[6]{ 0, 2, 1, 0, 3, 2 } // FRONT (+z)
};

public static Vector3i[] sideNormals= {
	new Vector3i(1, 0, 0), // LEFT
	new Vector3i(-1, 0, 0), // RIGHT
	new Vector3i(0, -1, 0), // BOTTOM
	new Vector3i(0, 1, 0), // TOP
	new Vector3i(0, 0, -1), // BACK
	new Vector3i(0, 0, 1), // FRONT
};

// Corners have same winding, relative to the face's normal
public static int[][] sideCorners = {
	new int[4]{ 3, 0, 4, 7 },
	new int[4]{ 1, 2, 6, 5 },
	new int[4]{ 1, 0, 3, 2 },
	new int[4]{ 4, 5, 6, 7 },
	new int[4]{ 0, 1, 5, 4 },
	new int[4]{ 2, 3, 7, 6 }
};

public static int[][] sideEdges = {
	new int[4]{ 3, 7, 11, 4 },
	new int[4]{ 1, 6, 9, 5 },
	new int[4]{ 0, 1, 2, 3 },
	new int[4]{ 8, 9, 10, 11 },
	new int[4]{ 0, 5, 8, 4 },
	new int[4]{ 2, 6, 10, 7 }
};

// 3---2
// | / | {0,1,2,0,2,3}
// 0---1
//static const unsigned int g_vertex_to_corner[Voxel::SIDE_COUNT][6] = {
//    { 0, 3, 7, 0, 7, 4 },
//    { 2, 1, 5, 2, 5, 6 },
//    { 0, 1, 2, 0, 2, 3 },
//    { 7, 6, 5, 7, 5, 4 },
//    { 1, 0, 4 ,1, 4, 5 },
//    { 3, 2, 6, 3, 6, 7 }
//};

public static Vector3i[] cornerInormals = {
	new Vector3i(1, -1, -1),
	new Vector3i(-1, -1, -1),
	new Vector3i(-1, -1, 1),
	new Vector3i(1, -1, 1),

	new Vector3i(1, 1, -1),
	new Vector3i(-1, 1, -1),
	new Vector3i(-1, 1, 1),
	new Vector3i(1, 1, 1)
};

public static Vector3i[] edgeInormals = {
	new Vector3i(0, -1, -1),
	new Vector3i(-1, -1, 0),
	new Vector3i(0, -1, 1),
	new Vector3i(1, -1, 0),

	new Vector3i(1, 0, -1),
	new Vector3i(-1, 0, -1),
	new Vector3i(-1, 0, 1),
	new Vector3i(1, 0, 1),

	new Vector3i(0, 1, -1),
	new Vector3i(-1, 1, 0),
	new Vector3i(0, 1, 1),
	new Vector3i(1, 1, 0)
};

public static int[][] edgeCorners = {
	new int[2]{ 0, 1 }, new int[2]{ 1, 2 }, new int[2]{ 2, 3 }, 
    new int[2]{ 3, 0 }, new int[2]{ 0, 4 }, new int[2]{ 1, 5 },
    new int[2]{ 2, 6 }, new int[2]{ 3, 7 }, new int[2]{ 4, 5 }, 
    new int[2]{ 5, 6 }, new int[2]{ 6, 7 }, new int[2]{ 7, 4 }
};

// Order is irrelevant
public static Vector3i[] mooreNeighboring3d = {
	new Vector3i(-1, -1, -1),
	new Vector3i(0, -1, -1),
	new Vector3i(1, -1, -1),
	new Vector3i(-1, -1, 0),
	new Vector3i(0, -1, 0),
	new Vector3i(1, -1, 0),
	new Vector3i(-1, -1, 1),
	new Vector3i(0, -1, 1),
	new Vector3i(1, -1, 1),

	new Vector3i(-1, 0, -1),
	new Vector3i(0, 0, -1),
	new Vector3i(1, 0, -1),
	new Vector3i(-1, 0, 0),
	//Vector3i(0,0,0),
	new Vector3i(1, 0, 0),
	new Vector3i(-1, 0, 1),
	new Vector3i(0, 0, 1),
	new Vector3i(1, 0, 1),

	new Vector3i(-1, 1, -1),
	new Vector3i(0, 1, -1),
	new Vector3i(1, 1, -1),
	new Vector3i(-1, 1, 0),
	new Vector3i(0, 1, 0),
	new Vector3i(1, 1, 0),
	new Vector3i(-1, 1, 1),
	new Vector3i(0, 1, 1),
	new Vector3i(1, 1, 1),
};
}
