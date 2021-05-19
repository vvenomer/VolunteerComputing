
extern "C" __global__ void collatz(int start, int count, int* stepsArray, int max) {
	int i = blockDim.x * blockIdx.x + threadIdx.x;

	if (i < count) {
		int n = start + i;

		for (int steps = 1; steps < max; steps++)
		{
			if (n & 1)
				n = (n << 1) + n + 1;
			else
				n >>= 1;

			if (n == 1)
			{
				stepsArray[i] = steps;
				return;
			}
		}
		stepsArray[i] = -1;
	}
}
