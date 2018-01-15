// #define GLINTEROP


__kernel void device_function( __global int* a, float t, __global uint* temp )
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	if(idx > 5) return;
	if(idy > 5) return;
	temp[idx] *= 2;
}