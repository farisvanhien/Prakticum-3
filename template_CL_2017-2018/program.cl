// #define GLINTEROP


__kernel void device_function( __global int* a, float t, __global uint* pattern, __global uint* second, uint pw)
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	if(idx > 5) return;
	if(idy > 5) return;
	pattern[idx] *= 2;
	second[idx] *= 12;

}

// helper function for setting one bit in the pattern buffer
void BitSet(uint x, uint y, __global uint* pattern, uint pw) 
{ 
	pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31); 
}
// helper function for getting one bit from the secondary pattern buffer
uint GetBit(uint x, uint y, __global uint* second, uint pw) 
{ 
	return (second[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U; 
}