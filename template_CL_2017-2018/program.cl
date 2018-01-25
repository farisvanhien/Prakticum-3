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

__kernel void device_function(__global uint* pattern, __global uint* second, uint pw, uint ph)
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	if(idx >= pw*32 - 1 || idx < 1) return;
	if(idy >= ph - 1 || idy < 1) return;

	/*
	//===========================================================================
	if(idx > 1) return;
	if(idy > 1) return;
	
	uint w = 512, h = 512;
	for (uint y = 1; y < h - 1; y++)
		for (uint x = 1; x < w - 1; x++)
		{
			// count active neighbors
			uint n = GetBit(x - 1, y - 1, second, pw) + GetBit(x, y - 1, second, pw) + GetBit(x + 1, y - 1, second, pw) + GetBit(x - 1, y, second, pw) +
						GetBit(x + 1, y, second, pw) + GetBit(x - 1, y + 1, second, pw) + GetBit(x, y + 1, second, pw) + GetBit(x + 1, y + 1, second, pw);
			if ((GetBit(x, y, second, pw) == 1 && n == 2) || n == 3) BitSet(x, y, pattern, pw);
		}
	//===========================================================================
    */

	/*
	//===========================================================================
	uint w = 512, h = 512;
	//uint x = idx;
	//for (uint y = 1; y < w - 1; y++)
	uint y = idy;
	for (uint x = 1; x < w - 1; x++)
	{
		// count active neighbors
		uint n = GetBit(x - 1, y - 1, second, pw) + GetBit(x, y - 1, second, pw) + GetBit(x + 1, y - 1, second, pw) + GetBit(x - 1, y, second, pw) +
					GetBit(x + 1, y, second, pw) + GetBit(x - 1, y + 1, second, pw) + GetBit(x, y + 1, second, pw) + GetBit(x + 1, y + 1, second, pw);
		if ((GetBit(x, y, second, pw) == 1 && n == 2) || n == 3) BitSet(x, y, pattern, pw);
	}
	//===========================================================================
	*/

	
	
	//===========================================================================
	// count active neighbors
    uint n = GetBit(idx - 1, idy - 1, second, pw) + GetBit(idx, idy - 1, second, pw) + GetBit(idx + 1, idy - 1, second, pw) + GetBit(idx - 1, idy, second, pw) +
                GetBit(idx + 1, idy, second, pw) + GetBit(idx - 1, idy + 1, second, pw) + GetBit(idx, idy + 1, second, pw) + GetBit(idx + 1, idy + 1, second, pw);
    if ((GetBit(idx, idy, second, pw) == 1 && n == 2) || n == 3) BitSet(idx, idy, pattern, pw);
	//===========================================================================
	


	
	

	/*
	//===========================================================================
	for(int i = 1; i < 32; i++)
	{
		uint x = (idx * 32) + i;
		uint y = idy;
		// count active neighbors
		uint n = GetBit(x - 1, y - 1, second, pw) + GetBit(x, y - 1, second, pw) + GetBit(x + 1, y - 1, second, pw) + GetBit(x - 1, y, second, pw) +
					GetBit(x + 1, y, second, pw) + GetBit(x - 1, y + 1, second, pw) + GetBit(x, y + 1, second, pw) + GetBit(x + 1, y + 1, second, pw);
		//if ((GetBit(x, y, second, pw) == 1 && n == 2) || n == 3) 
		BitSet(x, y, pattern, pw);
	}
	//===========================================================================
	*/
	
}
