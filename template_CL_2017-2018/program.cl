// #define GLINTEROP

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

__kernel void device_function( __global int* a, float t, __global uint* pattern, __global uint* second, uint pw, uint ph)
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	if(idx > 5) return;
	if(idy > 5) return;
	pattern[idx] *= 2;
	second[idx] *= 12;

	// clear destination pattern
    for (int i = 0; i < pw * ph; i++) pattern[i] = 0;
    // process all pixels, skipping one pixel boundary
    uint w = pw * 32, h = ph;
    for (uint y = 1; y < h - 1; y++) for (uint x = 1; x < w - 1; x++)
        {
            // count active neighbors
            uint n = GetBit(x - 1, y - 1, second, pw) + GetBit(x, y - 1, second, pw) + GetBit(x + 1, y - 1, second, pw) + GetBit(x - 1, y, second, pw) +
                        GetBit(x + 1, y, second, pw) + GetBit(x - 1, y + 1, second, pw) + GetBit(x, y + 1, second, pw) + GetBit(x + 1, y + 1, second, pw);
            if ((GetBit(x, y, second, pw) == 1 && n == 2) || n == 3) BitSet(x, y, pattern, pw);
        }
    // swap buffers
    for (int i = 0; i < pw * ph; i++) second[i] = pattern[i];

}
