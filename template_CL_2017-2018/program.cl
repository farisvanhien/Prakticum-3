// #define GLINTEROP


__kernel void device_function( __global int* a, float t, __global uint* pattern, __global uint* second )
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	if(idx > 5) return;
	if(idy > 5) return;
	pattern[idx] *= 2;
	second[idx] *= 12;

}