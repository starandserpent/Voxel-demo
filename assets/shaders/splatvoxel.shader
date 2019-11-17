shader_type spatial;
render_mode skip_vertex_transform;

uniform vec4 albedo : hint_color;
uniform float voxelSize;
uniform vec2 screen_size;
uniform vec2 viewport_pos;

varying vec4 col;

varying mat4 ViewProjectionMatrix;
varying vec4 pos_Box;
varying vec3 radius_Box;
varying vec3 invradius_Box;
varying mat3 rotation_Box;

varying vec3 min_pos_Box;
varying vec3 max_pos_Box;



void vertex() {
	float padding = 2.1f;
	mat4 mvp = PROJECTION_MATRIX * MODELVIEW_MATRIX;
	vec4 gl_Position = mvp * vec4(VERTEX, 1.0);
	ViewProjectionMatrix = mvp;
   //quadricProj(VERTEX,voxelSize,mvp,screen_size/2.0,gl_Position,POINT_SIZE);
	POINT_SIZE = screen_size.y * PROJECTION_MATRIX[1][1] * (voxelSize*padding)/gl_Position.w;

	
	col = albedo;
	pos_Box = WORLD_MATRIX * vec4(VERTEX,1.0);
	
	VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    NORMAL = (MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz;
	
	
	radius_Box = vec3(voxelSize);
	invradius_Box = 1.0/radius_Box;
	rotation_Box=mat3(1.0f);
	
	min_pos_Box = pos_Box.xyz - vec3(voxelSize);
	max_pos_Box = pos_Box.xyz + vec3(voxelSize);
	//PROJECTION_MATRIX = mat4(1.0);
}

vec2 intersectAABB(vec3 rayOrigin, vec3 rayDir, vec3 boxMin, vec3 boxMax) {
    vec3 tMin = (boxMin - rayOrigin) / rayDir;
    vec3 tMax = (boxMax - rayOrigin) / rayDir;
    vec3 t1 = min(tMin, tMax);
    vec3 t2 = max(tMin, tMax);
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
    return vec2(tNear, tFar);
}
vec3 getNormal(vec3 box_min, vec3 box_max, vec3 pos, float epsilon){
	vec3 d1 = abs(box_min - pos);
	vec3 d2 = abs(box_max - pos);
	
	vec3 n = -1.0 * vec3(lessThan(d1, vec3(epsilon)));
	n += vec3(lessThan(d2, vec3(epsilon)));

    return normalize(n);
}

void fragment() {
	float epsilon = 0.00001f;
	vec2 p = 2.0 * vec2(FRAGCOORD.xy)/(screen_size.xy-viewport_pos.xy) - vec2(1.0);
	vec3 ro = CAMERA_MATRIX[3].xyz;//ray origin
	vec4 rdh = (CAMERA_MATRIX * INV_PROJECTION_MATRIX )* vec4(p,-1.0,1.0);
	vec3 rd = rdh.xyz/rdh.w - ro;//ray direction
	

	vec3 color = albedo.rgb;
	
	vec3 boxMin = min_pos_Box;
    vec3 boxMax = max_pos_Box;


	vec2 result = intersectAABB(ro,rd,boxMin,boxMax);
    bool rayIntersectionTest = result.y >= result.x;
	if(rayIntersectionTest == false){
//		color = vec3(1.0);
	discard;
	}
	color = sqrt( color );
	vec3 pos = ro + result.x * rd;
//	DEPTH = result.x/8192.0f;

//	NORMAL = getNormal(boxMin,boxMax,pos,epsilon);
//
	vec4 PClip = ViewProjectionMatrix * vec4(pos,1.0);
	float ndc_depth = PClip.z / PClip.w;
	float win_depth = (ndc_depth + 1.0)/2.0;
//
	DEPTH = 1.0f * win_depth + 0.0f;
	
	ALBEDO =color;
	
	
	ALBEDO = texture(SCREEN_TEXTURE,POINT_COORD).rgb;

}
void light(){
//	vec4 lightW = CAMERA_MATRIX * vec4(LIGHT,0.0f);
//	DIFFUSE_LIGHT += dot(NORMAL, lightW.xyz) * ATTENUATION * ALBEDO;
}