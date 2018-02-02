#pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable
#pragma OPENCL EXTENSION cl_khr_global_int32_extended_atomics : enable

// helper function for setting one bit in the pattern buffer
void BitSet(uint x, uint y, volatile __global uint* pattern, uint pw) 
{ 
	//pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31);
	atomic_or(&(pattern[y * pw + (x >> 5)]), (1U << (int)(x & 31)) );
}
// helper function for getting one bit from the secondary pattern buffer
uint GetBit(uint x, uint y, __global uint* second, uint pw) 
{ 
	return (second[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U; 
}

__kernel void device_function(__global uint* pattern, __global uint* second, uint pw, uint ph)
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	if(idx >= pw*32 - 1 || idx < 1) return;
	if(idy >= ph - 1 || idy < 1) return;

	// count active neighbors
    uint n = GetBit(idx - 1, idy - 1, second, pw) + GetBit(idx, idy - 1, second, pw) + GetBit(idx + 1, idy - 1, second, pw) + GetBit(idx - 1, idy, second, pw) +
                GetBit(idx + 1, idy, second, pw) + GetBit(idx - 1, idy + 1, second, pw) + GetBit(idx, idy + 1, second, pw) + GetBit(idx + 1, idy + 1, second, pw);
    if ((GetBit(idx, idy, second, pw) == 1 && n == 2) || n == 3) BitSet(idx, idy, pattern, pw);
	


	
	
}
